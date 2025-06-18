using UnityEngine;

public class MainMenuController : MonoBehaviour
{
    public GameObject continueButton; // Assign tombol "Continue" di Inspector

    void Start()
    {
        // Saat Main Menu muncul, cek apakah ada data progres yang tersimpan
        if (PlayerPrefs.HasKey("LastPlayedLevel"))
        {
            // Jika ada, tampilkan tombol "Continue"
            if (continueButton != null)
            {
                continueButton.SetActive(true);
            }
        }
        else
        {
            // Jika tidak ada (pemain baru), sembunyikan tombol "Continue"
            if (continueButton != null)
            {
                continueButton.SetActive(false);
            }
        }
    }

    // Fungsi ini akan dipanggil oleh tombol "Continue"
    public void OnContinueButtonPressed()
    {
        // Ambil nama level terakhir dari data yang tersimpan
        string lastLevel = PlayerPrefs.GetString("LastPlayedLevel");

        // Pastikan nama level tidak kosong, lalu muat scene tersebut
        if (!string.IsNullOrEmpty(lastLevel))
        {
            Debug.Log("Melanjutkan ke level terakhir: " + lastLevel);
            // Gunakan SceneLoader yang sudah ada untuk memuat level
            SceneLoader.Instance.LoadSceneWithLoadingScreen(lastLevel);
        }
        else
        {
            Debug.LogWarning("Tidak ada data level terakhir. Tidak bisa melanjutkan.");
        }
    }
}