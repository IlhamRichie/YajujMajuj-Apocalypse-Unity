using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Kamu bisa membuat instance statis agar mudah diakses
    public static SceneLoader Instance;

    private void Awake()
    {
        // Logika Singleton: Pastikan hanya ada satu instance SceneLoader
        if (Instance == null)
        {
            // Jika ini yang pertama, jadikan ini sebagai instance utama
            Instance = this;
            // Jangan hancurkan objek ini saat pindah scene
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            // Jika sudah ada instance lain, hancurkan yang baru ini
            Destroy(gameObject);
        }
    }

    // Fungsi utama yang akan dipanggil dari tombol-tombol UI atau script lain
    public void LoadSceneWithLoadingScreen(string sceneName)
    {
        // Pastikan game tidak dalam keadaan pause
        Time.timeScale = 1f;

        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            // --- LOGIKA PENYIMPANAN BARU DITAMBAHKAN DI SINI ---
            // Cek apakah scene yang akan dimuat adalah sebuah "Level"
            // (berdasarkan namanya yang diawali dengan "Level")
            if (sceneName.StartsWith("Level"))
            {
                // Simpan nama scene ini sebagai level terakhir yang dimainkan
                PlayerPrefs.SetString("LastPlayedLevel", sceneName);
                PlayerPrefs.Save(); // Segera simpan perubahan ke perangkat
                Debug.Log("Progres Disimpan: Level terakhir adalah " + sceneName);
            }
            // ----------------------------------------------------

            // Lanjutkan ke proses loading scene seperti biasa
            LoadingBarController.sceneToLoad = sceneName;
            SceneManager.LoadScene("LoadingScene");
        }
        else
        {
            Debug.LogError("Scene '" + sceneName + "' tidak dapat ditemukan di Build Settings!");
        }
    }
    
    // Fungsi untuk memuat scene tanpa loading screen (jika diperlukan)
    public void LoadSceneDirectly(string sceneName)
    {
         if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            Debug.LogError("Scene '" + sceneName + "' tidak dapat ditemukan di Build Settings!");
        }
    }

    // Fungsi untuk pause dan resume
    public void PauseGame()
    {
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
    }

    // Fungsi untuk keluar dari game
    public void QuitGame()
    {
        Application.Quit();
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }
}