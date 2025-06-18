using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine; // <--- TAMBAHKAN BARIS INI

public class QuestManager : MonoBehaviour
{
    [Header("Primary Quest Item")]
    public string primaryItemName = "Manuskrip";
    public string primaryItemTag = "Manuscript";
    public int totalPrimaryItemsToFind = 3;
    private int primaryItemsCollected = 0;

    [Header("Secondary Quest Item")]
    public bool requireSecondaryItem = false;
    public string secondaryItemName = "Kunci Batu";
    public string secondaryItemTag = "KunciBatu";
    public int totalSecondaryItemsToFind = 3;
    private int secondaryItemsCollected = 0;

    [Header("Enemy Quest")]
    public bool requireEnemyKills = false;
    public int totalEnemiesToKill = 5;
    private int enemiesKilled = 0;

    private bool isQuestComplete = false;

    [Header("UI Elements")]
    public TextMeshProUGUI questProgressText;
    public GameObject manuscriptDisplayPanel;
    public TextMeshProUGUI ayatDisplayText;
    public GameObject questCompletePopup;
    public GameObject questProgressPopup;

    [Header("Cinematic & Objective")]
    public GateController gateController;
    public GameObject vcamForGate;
    public PlayerMovement playerMovement;
    public float cinematicDuration = 4.0f;

    void Start()
    {
        if (manuscriptDisplayPanel != null) manuscriptDisplayPanel.SetActive(false);
        if (questCompletePopup != null) questCompletePopup.SetActive(false);
        if (questProgressPopup != null) questProgressPopup.SetActive(false);
        if (vcamForGate != null) vcamForGate.SetActive(false);
        UpdateQuestProgressUI();
    }

    // Fungsi utama yang dipanggil oleh PlayerMovement saat item diambil
    public void AnItemWasCollected(string collectedItemTag)
    {
        if (isQuestComplete) return;

        if (collectedItemTag == primaryItemTag)
        {
            primaryItemsCollected++;
        }
        else if (requireSecondaryItem && collectedItemTag == secondaryItemTag)
        {
            secondaryItemsCollected++;
        }

        UpdateQuestProgressUI();
        CheckQuestCompletion();
    }

    public void EnemyKilled()
    {
        if (isQuestComplete || !requireEnemyKills) return;
        enemiesKilled++;
        UpdateQuestProgressUI();
        CheckQuestCompletion();
    }

    void CheckQuestCompletion()
    {
        bool primaryItemsDone = (primaryItemsCollected >= totalPrimaryItemsToFind);
        bool secondaryItemsDone = (secondaryItemsCollected >= totalSecondaryItemsToFind);
        bool enemiesDone = (enemiesKilled >= totalEnemiesToKill);

        if (primaryItemsDone && (!requireSecondaryItem || secondaryItemsDone) && (!requireEnemyKills || enemiesDone))
        {
            if (!isQuestComplete)
            {
                isQuestComplete = true;
                CompleteQuest();
            }
        }
    }

    void UpdateQuestProgressUI()
    {
        if (questProgressText != null)
        {
            string fullText = "";
            fullText += primaryItemName + " Ditemukan: " + primaryItemsCollected + " / " + totalPrimaryItemsToFind;
            if (requireSecondaryItem)
            {
                fullText += "\n" + secondaryItemName + " Ditemukan: " + secondaryItemsCollected + " / " + totalSecondaryItemsToFind;
            }
            if (requireEnemyKills)
            {
                fullText += "\n" + "Musuh Dikalahkan: " + enemiesKilled + " / " + totalEnemiesToKill;
            }
            questProgressText.text = fullText;
        }
    }

    public void DisplayAyatContent(string ayat)
    {
        if (manuscriptDisplayPanel != null && ayatDisplayText != null)
        {
            if (playerMovement != null) playerMovement.enabled = false;
            ayatDisplayText.text = ayat;
            manuscriptDisplayPanel.SetActive(true);
        }
    }
    
    public void CloseAyatDisplayPanel()
    {
        if (manuscriptDisplayPanel != null)
        {
            manuscriptDisplayPanel.SetActive(false);
            if (!isQuestComplete)
            {
                if (playerMovement != null) playerMovement.enabled = true;
            }
        }
    }

    void CompleteQuest()
    {
        if (playerMovement != null) playerMovement.enabled = false;
        if (questCompletePopup != null)
        {
            questCompletePopup.SetActive(true);
        }
        else
        {
            StartCoroutine(GateOpenSequence());
        }
    }

    public void StartGateSequenceFromButton()
    {
        if (questCompletePopup != null) questCompletePopup.SetActive(false);
        StartCoroutine(GateOpenSequence());
    }

    IEnumerator GateOpenSequence()
    {
        // --- TAHAP 1: PERSIAPAN ---
        if (playerMovement != null) playerMovement.enabled = false;
        if (questProgressText != null) questProgressText.text = "Gerbang kuno bergetar...";

        // Ambil index level saat ini dari SceneManager
        int thisLevelIndex = SceneManager.GetActiveScene().buildIndex;

        CinemachineBrain cinemachineBrain = Camera.main.GetComponent<CinemachineBrain>();
        if (cinemachineBrain == null)
        {
            Debug.LogError("CinemachineBrain tidak ditemukan di Main Camera. Cinematic dibatalkan.");
            if (gateController != null) gateController.OpenGate();
            if (playerMovement != null) playerMovement.enabled = true;
            yield break;
        }

        // --- TAHAP 2: KAMERA FOKUS KE GERBANG ---
        if (vcamForGate != null)
        {
            vcamForGate.SetActive(true);
        }
        yield return null; 
        yield return new WaitWhile(() => cinemachineBrain.IsBlending);
        Debug.Log("Transisi kamera ke gerbang selesai.");

        // --- TAHAP 3: GERBANG TERBUKA & JEDA DRAMATIS ---
        if (gateController != null)
        {
            gateController.OpenGate();
        }
        yield return new WaitForSeconds(2.5f);

        // --- TAHAP 4: SIMPAN PROGRES PERMAINAN (LOGIKA BARU) ---
        Debug.Log("Menyimpan progres level...");
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);
        
        // Cek apakah level yang baru selesai ini lebih tinggi dari progres yang tersimpan
        if (thisLevelIndex >= levelReached)
        {
            // Buka akses ke level berikutnya
            int nextLevel = thisLevelIndex + 1;
            PlayerPrefs.SetInt("LevelReached", nextLevel);
            PlayerPrefs.Save(); // Simpan perubahan ke perangkat
            Debug.Log("Level " + nextLevel + " unlocked!");
        }

        // --- TAHAP 5: KAMERA KEMBALI KE PEMAIN ---
        if (vcamForGate != null)
        {
            vcamForGate.SetActive(false);
        }
        yield return null;
        yield return new WaitWhile(() => cinemachineBrain.IsBlending);
        Debug.Log("Transisi kamera kembali ke pemain selesai.");

        // --- TAHAP 6: FINALISASI ---
        if (questProgressText != null) questProgressText.text = "Gerbang telah terbuka! Masuklah...";
        if (playerMovement != null) playerMovement.enabled = true;
    }

    public void ToggleQuestProgressPopup()
    {
        if (questProgressPopup != null)
        {
            bool isActive = questProgressPopup.activeSelf;
            questProgressPopup.SetActive(!isActive);
            if (playerMovement != null)
            {
                playerMovement.enabled = isActive;
            }
        }
    }
}