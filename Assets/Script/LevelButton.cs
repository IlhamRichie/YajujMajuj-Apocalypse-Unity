using UnityEngine;
using UnityEngine.UI; // Untuk mengakses komponen Button dan Image
using TMPro; // Jika kamu pakai TextMeshPro untuk nomor level

public class LevelButton : MonoBehaviour
{
    [Header("Level Info")]
    public int levelIndex; // Level yang diwakili tombol ini (misal: 1 untuk Level 1, 2 untuk Level 2)
    public string sceneNameToLoad; // Nama scene yang akan dimuat (misal: "Level1" atau "Level2")

    [Header("UI References")]
    public Button buttonComponent;
    public Image lockIcon; // GameObject Image untuk ikon gembok (opsional)
    public TextMeshProUGUI levelText; // GameObject Text untuk nomor/nama level

    void Start()
    {
        // Saat pertama kali menu level muncul, perbarui status semua tombol
        UpdateButtonStatus();
    }

    public void UpdateButtonStatus()
    {
        // Ambil data level tertinggi yang sudah dicapai pemain dari PlayerPrefs
        // "LevelReached" adalah key/nama data yang kita simpan. Defaultnya 1 (hanya level 1 yang terbuka)
        int levelReached = PlayerPrefs.GetInt("LevelReached", 1);

        if (levelIndex <= levelReached)
        {
            // Jika level tombol ini SUDAH atau PERNAH dicapai, buat tombol aktif
            buttonComponent.interactable = true;
            if (levelText != null) levelText.color = Color.white; // Ganti warna teks jadi terang (opsional)
            if (lockIcon != null) lockIcon.gameObject.SetActive(false); // Sembunyikan ikon gembok
        }
        else
        {
            // Jika level tombol ini BELUM dicapai, buat tombol tidak bisa diklik dan pudar
            buttonComponent.interactable = false;
            if (levelText != null) levelText.color = Color.gray; // Ganti warna teks jadi abu-abu (opsional)
            if (lockIcon != null) lockIcon.gameObject.SetActive(true); // Tampilkan ikon gembok
        }
    }

    // Fungsi ini akan dipanggil saat tombol level di-klik
    public void LoadLevel()
    {
        if (buttonComponent.interactable)
        {
            // Cek apakah SceneLoader.Instance ada
            if (SceneLoader.Instance != null)
            {
                SceneLoader.Instance.LoadSceneWithLoadingScreen(sceneNameToLoad);
            }
            else
            {
                // Jika tidak ada, kemungkinan besar karena game dijalankan dari scene yang salah.
                // Beri pesan error yang jelas di Console.
                Debug.LogError("SceneLoader.Instance tidak ditemukan! Pastikan game dijalankan dari scene MainMenu dan ada objek _SceneLoader di sana.");
                
                // Sebagai fallback, kita bisa coba memuat scene secara langsung, tapi ini tidak ideal.
                // UnityEngine.SceneManagement.SceneManager.LoadScene(sceneNameToLoad);
            }
        }
    }
}