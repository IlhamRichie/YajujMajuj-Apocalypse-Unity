using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections; // Penting untuk Coroutine

public class QuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    public int totalManuscriptsToFind = 3;
    private int manuscriptsCollected = 0;

    private bool isQuestReadyToComplete = false;

    [Header("UI Elements")]
    public TextMeshProUGUI questProgressText;
    public GameObject manuscriptDisplayPanel;
    public TextMeshProUGUI ayatDisplayText;
    public GameObject questCompletePopup;
    public GameObject questProgressPopup;

    [Header("Cinematic & Objective")]
    public GateController gateController;   // Assign objek Gerbang di Inspector
    public GameObject vcamForGate;          // Assign Virtual Camera Gerbang di Inspector
    public PlayerMovement playerMovement;   // Assign objek Player di Inspector
    public float cinematicDuration = 4.0f;  // Total durasi perkiraan cinematic

    void Start()
    {
        if (manuscriptDisplayPanel != null)
            manuscriptDisplayPanel.SetActive(false);

        UpdateQuestProgressUI();

        // Pastikan vcam untuk gerbang tidak aktif di awal
        if (vcamForGate != null)
        {
            vcamForGate.SetActive(false);
        }
    }

    public void ManuscriptCollected()
    {
        manuscriptsCollected++;
        UpdateQuestProgressUI();
        Debug.Log("QuestManager: Manuskrip dikumpulkan! Total sekarang: " + manuscriptsCollected);

        if (manuscriptsCollected >= totalManuscriptsToFind)
        {
            // JANGAN panggil CompleteQuest() di sini. Cukup set flag-nya.
            isQuestReadyToComplete = true; 
            Debug.Log("Quest siap diselesaikan, menunggu panel manuskrip ditutup.");
        }
    }

    public void DisplayAyatContent(string ayat)
    {
        if (manuscriptDisplayPanel != null && ayatDisplayText != null)
        {
            // Matikan gerakan pemain saat panel muncul
            if (playerMovement != null) playerMovement.enabled = false;

            ayatDisplayText.text = ayat;
            manuscriptDisplayPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("UI Panel atau Text untuk display ayat belum di-assign di QuestManager!");
        }
    }

    void UpdateQuestProgressUI()
    {
        if (questProgressText != null)
        {
            questProgressText.text = "Manuskrip Ditemukan: " + manuscriptsCollected + " / " + totalManuscriptsToFind;
        }
    }

    // Fungsi ini sekarang hanya bertugas untuk memulai Coroutine sekuens
    void CompleteQuest()
    {
        Debug.Log("QUEST SELESAI! Menampilkan popup...");

        // Matikan kontrol pemain agar tidak bisa bergerak saat popup muncul
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }

        // Tampilkan panel popup "Selamat"
        if (questCompletePopup != null)
        {
            questCompletePopup.SetActive(true);
        }
        else
        {
            Debug.LogError("QuestCompletePopup belum di-assign di QuestManager!");
            // Fallback: Jika popup tidak ada, langsung mulai sekuens gerbang
            StartCoroutine(GateOpenSequence());
        }
    }

    public void ToggleQuestProgressPopup()
    {
        if (questProgressPopup != null)
        {
            // Toggle: Jika aktif, nonaktifkan. Jika nonaktif, aktifkan.
            bool isActive = questProgressPopup.activeSelf;
            questProgressPopup.SetActive(!isActive);

            // Opsional: Jika popup quest muncul, hentikan gerakan pemain.
            // Jika kamu mau pemain tetap bisa bergerak, hapus bagian if/else ini.
            if (playerMovement != null)
            {
                playerMovement.enabled = isActive; // Jika popup dinonaktifkan (isActive=true), player.enabled jadi true.
            }
        }
    }

    public void StartGateSequenceFromButton()
    {
        // Sembunyikan panel popup
        if (questCompletePopup != null)
        {
            questCompletePopup.SetActive(false);
        }

        // Mulai Coroutine sekuens sinematik gerbang
        StartCoroutine(GateOpenSequence());
    }

    // Coroutine yang menjadi "sutradara" dari sekuens sinematik
    IEnumerator GateOpenSequence()
    {
        // --- TAHAP 1: PERSIAPAN ---
        // Matikan kontrol pemain
        if (playerMovement != null)
        {
            playerMovement.enabled = false;
        }
        // Update UI dengan pesan awal
        if (questProgressText != null)
        {
            questProgressText.text = "Sebuah gerbang kuno bergetar...";
        }

        // --- TAHAP 2: KAMERA FOKUS KE GERBANG ---
        // Aktifkan Virtual Camera untuk gerbang
        if (vcamForGate != null)
        {
            vcamForGate.SetActive(true);
        }
        else
        {
            Debug.LogError("vcamForGate belum di-assign di QuestManager! Cinematic dibatalkan.");
            // Fallback jika kamera tidak di-assign: langsung buka gerbang
            if (gateController != null) gateController.OpenGate();
            yield break; // Hentikan coroutine
        }

        // Tunggu sebentar agar transisi kamera dari Cinemachine Brain selesai.
        // Waktu ini bisa disesuaikan dengan setting "Default Blend" di Cinemachine Brain.
        yield return new WaitForSeconds(2.0f); 

        // --- TAHAP 3: GERBANG TERBUKA ---
        // Setelah kamera fokus, buka gerbang & mainkan suaranya
        if (gateController != null)
        {
            gateController.OpenGate();
        }

        // Tahan kamera di gerbang selama beberapa saat agar pemain bisa melihatnya terbuka
        yield return new WaitForSeconds(2.0f); // Tahan selama 2 detik (sesuaikan nilainya)

        // --- TAHAP 4: KAMERA KEMBALI KE PEMAIN ---
        // Nonaktifkan Virtual Camera gerbang, Cinemachine Brain akan otomatis kembali ke kamera pemain
        if (vcamForGate != null)
        {
            vcamForGate.SetActive(false);
        }

        // Tunggu lagi agar transisi kamera kembali ke pemain selesai
        yield return new WaitForSeconds(2.0f); 

        // --- TAHAP 5: KEMBALIKAN KONTROL ---
        // Beri instruksi baru ke pemain
        if (questProgressText != null)
        {
            questProgressText.text = "Gerbang telah terbuka! Masuklah...";
        }
        // Kembalikan kontrol ke pemain
        if (playerMovement != null)
        {
            playerMovement.enabled = true;
        }
    }

    // Fungsi untuk tombol tutup panel display manuskrip
    public void CloseAyatDisplayPanel()
    {
        if (manuscriptDisplayPanel != null)
        {
            manuscriptDisplayPanel.SetActive(false); // Selalu sembunyikan panel

            // Cek apakah quest sudah siap untuk diselesaikan (artinya ini panel dari manuskrip terakhir)
            if (isQuestReadyToComplete)
            {
                // Jika ya, JANGAN aktifkan kembali pemain.
                // Langsung panggil CompleteQuest() untuk menampilkan popup "Selamat".
                CompleteQuest(); 
            }
            else
            {
                // Jika tidak (ini bukan manuskrip terakhir), aktifkan kembali gerakan pemain seperti biasa.
                if (playerMovement != null)
                {
                    playerMovement.enabled = true;
                }
            }
        }
    }
}