using UnityEngine;
using TMPro; // Jangan lupa tambahkan ini jika menggunakan TextMeshPro

public class QuestManager : MonoBehaviour
{
    [Header("Quest Settings")]
    public int totalManuscriptsToFind = 3; // Atur jumlah total manuskrip di Inspector
    private int manuscriptsCollected = 0;

    [Header("UI Elements")]
    public TextMeshProUGUI questProgressText; // Untuk teks progres "Manuskrip: 0/3"
    public GameObject manuscriptDisplayPanel;  // Referensi ke Panel yang kita buat tadi
    public TextMeshProUGUI ayatDisplayText;     // Referensi ke Text di dalam Panel

    // (Opsional) Untuk pesan setelah quest selesai
    // public GameObject questCompleteMessagePanel;

    void Start()
    {
        // Nonaktifkan panel display dan pesan selesai di awal
        if (manuscriptDisplayPanel != null)
            manuscriptDisplayPanel.SetActive(false);
        // if (questCompleteMessagePanel != null)
        //     questCompleteMessagePanel.SetActive(false);

        UpdateQuestProgressUI();
    }

    // Dipanggil dari PlayerMovement saat manuskrip diambil
    public void ManuscriptCollected()
    {
        manuscriptsCollected++;
        UpdateQuestProgressUI();
        Debug.Log("QuestManager: Manuskrip dikumpulkan! Total sekarang: " + manuscriptsCollected);

        if (manuscriptsCollected >= totalManuscriptsToFind)
        {
            CompleteQuest();
        }
    }

    // Dipanggil dari PlayerMovement untuk menampilkan isi ayat
    public void DisplayAyatContent(string ayat)
    {
        if (manuscriptDisplayPanel != null && ayatDisplayText != null)
        {
            ayatDisplayText.text = ayat; // Set teks ayatnya
            manuscriptDisplayPanel.SetActive(true); // Tampilkan panelnya
            // Di sini kita bisa menambahkan logika untuk tombol tutup atau input lain untuk menutup panel
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

    void CompleteQuest()
    {
        Debug.Log("SELAMAT! Semua manuskrip telah ditemukan!");
        // if (questCompleteMessagePanel != null)
        //     questCompleteMessagePanel.SetActive(true);
        if (questProgressText != null)
            questProgressText.text = "Quest Selesai! Semua manuskrip ditemukan. Segera ke portal Tembok Reruntuhan dan lanjutkan ke level berikutnya";

        // Logika lain setelah quest selesai (misalnya beri hadiah, buka pintu, dll.)
    }

    // Fungsi ini bisa dipanggil oleh tombol "Tutup" di UI Panel nanti
    public void CloseAyatDisplayPanel()
    {
        if (manuscriptDisplayPanel != null)
        {
            manuscriptDisplayPanel.SetActive(false);
        }
    }
}