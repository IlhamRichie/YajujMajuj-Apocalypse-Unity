using UnityEngine;
using System.Collections;
using UnityEngine.UI; // <-- Tambahkan ini
using TMPro;          // <-- Tambahkan ini
using Cinemachine;

public class BossBattleManager : MonoBehaviour
{
    [Header("Boss References")]
    public Enemy yajujBoss;
    public Enemy majujBoss;
    public GameObject warningPopup;

    [Header("Battle Settings")]
    public float introDuration = 3f;

    // --- TAMBAHKAN VARIABEL UI DI SINI ---
    [Header("Boss UI")]
    public GameObject bossHealthBarUI;
    public Image bossHealthFill;
    public TextMeshProUGUI bossNameText;
    // ------------------------------------

    [Header("Victory Sequence")]
    public GameObject victoryPopup; // Assign panel "Selamat" ke sini
    public GateController finalGate; // Assign gerbang terakhir di scene ini
    public CinemachineVirtualCamera mainBattleCamera; // Assign VCam_Arena ke sini
    public CinemachineVirtualCamera gateFocusCamera; // Assign VCam untuk fokus ke gerbang (bisa VCam_Gate dari level sebelumnya)


    private int bossesDefeated = 0;
    private Enemy currentActiveBoss;

    void Start()
    {
        // Sembunyikan semua elemen yang tidak seharusnya muncul di awal
        if (yajujBoss != null) yajujBoss.gameObject.SetActive(false);
        if (majujBoss != null) majujBoss.gameObject.SetActive(false);
        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(false);
        if (victoryPopup != null) victoryPopup.SetActive(false);

        // Tampilkan panel peringatan
        if (warningPopup != null)
        {
            warningPopup.SetActive(true);
        }
        else
        {
            // Jika tidak ada panel peringatan, langsung mulai pertarungan (fallback)
            StartCoroutine(BattleIntroSequence());
        }

        // Jeda permainan agar pemain tidak bisa bergerak saat popup muncul
        Time.timeScale = 0f;
    }

    IEnumerator BattleIntroSequence()
    {
        yield return new WaitForSeconds(introDuration);

        // Munculkan Yajuj dan UI-nya
        Debug.Log("YAJUJ MUNCUL!");
        yajujBoss.gameObject.SetActive(true);
        SetupAndShowBossUI(yajujBoss, "YAJUJ");
    }

    // --- FUNGSI BARU UNTUK SETUP UI BOSS ---
    void SetupAndShowBossUI(Enemy boss, string bossName)
    {
        currentActiveBoss = boss;
        if (bossHealthBarUI != null)
        {
            bossNameText.text = bossName;
            // Update health bar ke kondisi penuh
            UpdateBossHealthBar(boss.maxHealth, boss.maxHealth); 
            bossHealthBarUI.SetActive(true);
        }
    }

    // --- FUNGSI BARU UNTUK UPDATE HEALTH BAR ---
    public void UpdateBossHealthBar(float currentHealth, float maxHealth)
    {
        if (bossHealthFill != null)
        {
            bossHealthFill.fillAmount = currentHealth / maxHealth;
        }
    }

    // --- (Fungsi BossDefeated dan VictorySequence tetap sama) ---
    public void BossDefeated(Enemy boss)
    {
        bossesDefeated++;
        if (bossHealthBarUI != null) bossHealthBarUI.SetActive(false); // Sembunyikan UI saat boss kalah

        if (boss == yajujBoss)
        {
            Debug.Log("YAJUJ TELAH DIKALAHKAN!");
            if (!majujBoss.gameObject.activeInHierarchy)
            {
                Debug.Log("MAJUJ MUNCUL!");
                majujBoss.gameObject.SetActive(true);
                SetupAndShowBossUI(majujBoss, "MAJUJ"); // Tampilkan UI untuk Majuj
            }
        }
        else if (boss == majujBoss)
        {
            Debug.Log("MAJUJ TELAH DIKALAHKAN!");
        }

        if (bossesDefeated >= 2)
        {
            StartCoroutine(VictorySequence());
        }
    }

    public void StartBossBattleFromPopup()
    {
        // Lanjutkan waktu permainan
        Time.timeScale = 1f;

        // Sembunyikan panel peringatan
        if (warningPopup != null)
        {
            warningPopup.SetActive(false);
        }

        // Mulai coroutine intro pertarungan boss
        StartCoroutine(BattleIntroSequence());
    }

    IEnumerator VictorySequence()
    {
        Debug.Log("MEMULAI SEKUENS KEMENANGAN...");

        // --- TAHAP 1: EFEK GETAR KAMERA ---
        CinemachineBasicMultiChannelPerlin noise = null;
        if (mainBattleCamera != null)
        {
            noise = mainBattleCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        if (noise != null)
        {
            Debug.Log("Kamera bergetar!");
            noise.m_AmplitudeGain = 3f; // Atur kekuatan getaran
            noise.m_FrequencyGain = 3f; // Atur kecepatan getaran
            yield return new WaitForSeconds(2.5f); // Durasi getaran
            noise.m_AmplitudeGain = 0f; // Hentikan getaran
            noise.m_FrequencyGain = 0f;
        }

        // --- TAHAP 2: FOKUS KE GERBANG ---
        if (gateFocusCamera != null)
        {
            gateFocusCamera.gameObject.SetActive(true);
            // Tunggu transisi kamera selesai
            yield return new WaitWhile(() => Camera.main.GetComponent<CinemachineBrain>().IsBlending);
            Debug.Log("Kamera fokus ke gerbang.");
        }

        // --- TAHAP 3: BUKA GERBANG & TAMPILKAN POPUP "SELAMAT" ---
        if (finalGate != null)
        {
            finalGate.OpenGate(); // Buka gerbang terakhir
        }
        if (victoryPopup != null)
        {
            victoryPopup.SetActive(true); // Tampilkan panel "Selamat"
        }

        // Jeda sejenak agar pemain bisa membaca
        yield return new WaitForSeconds(4f); 

        // Sembunyikan popup sebelum kamera kembali
        if (victoryPopup != null)
        {
            victoryPopup.SetActive(false);
        }

        // --- TAHAP 4: KAMERA KEMBALI & KEMBALIKAN KONTROL ---
        if (gateFocusCamera != null)
        {
            gateFocusCamera.gameObject.SetActive(false);
            yield return new WaitWhile(() => Camera.main.GetComponent<CinemachineBrain>().IsBlending);
            Debug.Log("Kamera kembali ke pemain.");
        }

        // Pemain sekarang bisa berjalan menuju gerbang yang sudah terbuka
        // (Pastikan pemain diaktifkan kembali jika sebelumnya dinonaktifkan)
    }

}