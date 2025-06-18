using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class LoadingBarController : MonoBehaviour
{
    // Variabel statis untuk menerima nama scene dari script lain
    public static string sceneToLoad;

    [Header("UI Elements")]
    public Image loadingBarFill;
    public TextMeshProUGUI percentageText;
    
    [Header("Random Tip/Lore Text")]
    public TextMeshProUGUI tipText;
    [TextArea(3, 5)]
    public System.Collections.Generic.List<string> tipsList;

    [Header("Settings")]
    public float loadingTime = 3.0f; // Durasi "gimmick" loading

    void Start()
    {
        // Tampilkan teks random jika ada
        if (tipText != null && tipsList != null && tipsList.Count > 0)
        {
            int randomIndex = Random.Range(0, tipsList.Count);
            tipText.text = tipsList[randomIndex];
        }
        else if (tipText != null)
        {
            tipText.gameObject.SetActive(false);
        }
        
        // Mulai Coroutine untuk mengisi loading bar
        StartCoroutine(LoadAsyncGimmick());
    }

    IEnumerator LoadAsyncGimmick()
    {
        // Pastikan sceneToLoad tidak kosong sebelum memulai
        if (string.IsNullOrEmpty(sceneToLoad))
        {
            Debug.LogError("Tidak ada scene tujuan yang ditentukan (sceneToLoad is null or empty)!");
            yield break; // Hentikan coroutine
        }

        // Mulai memuat scene tujuan di background, TAPI jangan aktifkan dulu
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneToLoad);
        asyncLoad.allowSceneActivation = false;

        float elapsedTime = 0f;

        // Loop untuk gimmick loading bar
        while (elapsedTime < loadingTime)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / loadingTime);

            // Update UI
            if (loadingBarFill != null) loadingBarFill.fillAmount = progress;
            if (percentageText != null) percentageText.text = (progress * 100f).ToString("F0") + "%";

            yield return null;
        }

        // Pastikan loading asli di background sudah hampir selesai
        yield return new WaitUntil(() => asyncLoad.progress >= 0.9f);

        // Bar penuh, tampilkan 100%
        if (loadingBarFill != null) loadingBarFill.fillAmount = 1f;
        if (percentageText != null) percentageText.text = "100%";
        yield return new WaitForSeconds(0.5f); // Jeda sesaat di 100%

        // Izinkan scene baru untuk aktif dan ditampilkan
        asyncLoad.allowSceneActivation = true;
    }
}