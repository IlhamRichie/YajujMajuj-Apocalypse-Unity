using UnityEngine;
using UnityEngine.UI; // Tambahkan ini untuk mengakses komponen Image
using TMPro;          // Tambahkan ini untuk mengakses TextMeshProUGUI
using UnityEngine.SceneManagement; // Untuk SceneManager
using System.Collections;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 10;
    private int currentHealth;

    private Animator anim;                 // Untuk Animator pemain
    private PlayerMovement playerMovement; // Untuk menonaktifkan gerakan pemain
    private bool isDead = false;           // Flag untuk memastikan Die() hanya berjalan sekali

    // Referensi ke UI health bar dan teks
    public Image healthBarFill;         // Drag Image 'HealthBarFill' ke sini
    public TextMeshProUGUI healthText;  // Drag TextMeshPro 'HealthText' ke sini

    [Header("Death Sequence Settings")]
    public float zoomOutFactor = 1.5f;     // Seberapa jauh kamera zoom out (1.5f = 50% lebih jauh)
    public float deathSequenceDuration = 3.0f; // Total durasi efek kematian sebelum pindah scene (detik)
    public string loseSceneName = "NamaSceneLoseKamu"; // ISI DENGAN NAMA SCENE LOSE KAMU

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>(); // Ambil Animator
        playerMovement = GetComponent<PlayerMovement>(); // Ambil PlayerMovement
        UpdateHealthUI();
        // isDead sudah false secara default
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return; // Jika sudah mati, jangan proses damage lagi

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (isDead) return; // Mencegah Die() dipanggil berkali-kali
        isDead = true;

        Debug.Log("Pemain Mati!");

        if (playerMovement != null)
        {
            playerMovement.enabled = false; // Matikan script gerakan pemain, ini sudah bagus
        }

        // --- PERUBAHAN PENTING UNTUK MENCEGAH JATUH ---
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.velocity = Vector2.zero;       // 1. Hentikan semua sisa kecepatan.
            rb.isKinematic = true;            // 2. Ubah Rigidbody menjadi Kinematic. 
                                              //    Ini akan membuatnya tidak lagi terpengaruh oleh gaya luar 
                                              //    seperti gravitasi atau tabrakan. Ia hanya akan bergerak jika
                                              //    kamu secara eksplisit mengubah transform.position-nya (misalnya oleh animasi).
                                              // Alternatif lain jika tidak mau Kinematic (tapi Kinematic lebih umum untuk kasus ini):
                                              // rb.gravityScale = 0f; 
        }
        // --- AKHIR PERUBAHAN PENTING ---

        Collider2D playerCollider = GetComponent<Collider2D>();
        if (playerCollider != null)
        {
            // Menonaktifkan collider setelah menjadi Kinematic biasanya aman dan baik
            // untuk mencegah interaksi fisik yang tidak diinginkan dengan "mayat" pemain.
            playerCollider.enabled = false;
        }

        // Mulai sekuens kematian (animasi, zoom, pindah scene)
        StartCoroutine(DeathSequenceCoroutine());
    }


    // Fungsi untuk update UI Health Bar dan Teks
    void UpdateHealthUI()
    {
        if (healthBarFill != null)
        {
            float fill = (float)currentHealth / maxHealth;
            healthBarFill.fillAmount = fill;
            Debug.Log("PlayerHealth UI: Updating healthBarFill.fillAmount to: " + fill);
        }
        else { Debug.LogWarning("PlayerHealth UI: healthBarFill is null!"); }

        if (healthText != null)
        {
            healthText.text = currentHealth + " / " + maxHealth + " HP";
            Debug.Log("PlayerHealth UI: Updating healthText to: " + healthText.text);
        }
        else { Debug.LogWarning("PlayerHealth UI: healthText is null!"); }
    }

    // Di dalam class PlayerHealth
    IEnumerator DeathSequenceCoroutine()
    {
        // 1. Picu Animasi Kematian
        if (anim != null)
        {
            anim.SetTrigger("DieTrigger"); // Pastikan nama trigger ini ("DieTrigger") sama dengan di Animator Controller
        }

        // Beri sedikit waktu agar animasi kematian mulai terlihat sebelum efek lain
        // yield return new WaitForSeconds(0.2f); // Opsional, sesuaikan nilainya

        // 2. Kamera Zoom Out (Untuk game 2D dengan kamera Orthographic)
        Camera mainCamera = Camera.main;
        float zoomDuration = deathSequenceDuration * 0.6f; // Alokasikan sebagian durasi untuk zoom (misal 60%)
        float timeToWaitAfterZoom = deathSequenceDuration * 0.4f; // Sisa durasi untuk menunggu

        if (mainCamera != null && mainCamera.orthographic) // Pastikan kamera utama ada dan Orthographic
        {
            float initialOrthoSize = mainCamera.orthographicSize;
            float targetOrthoSize = initialOrthoSize * zoomOutFactor;
            float zoomElapsedTime = 0f;

            while (zoomElapsedTime < zoomDuration)
            {
                mainCamera.orthographicSize = Mathf.Lerp(initialOrthoSize, targetOrthoSize, zoomElapsedTime / zoomDuration);
                zoomElapsedTime += Time.deltaTime;
                yield return null; // Tunggu frame berikutnya
            }
            mainCamera.orthographicSize = targetOrthoSize; // Pastikan mencapai target zoom
        }
        else if (mainCamera != null && !mainCamera.orthographic)
        {
            Debug.LogWarning("Kamera utama bukan Orthographic. Zoom out untuk kamera Perspective perlu implementasi berbeda (FOV atau posisi).");
            // Jika kamera Perspective (3D), kamu bisa ubah Field of View atau mundurkan posisi Z kamera.
            // Untuk sekarang, kita lewati zoom jika bukan Orthographic, tapi delay tetap berjalan.
            yield return new WaitForSeconds(zoomDuration); // Tetap berikan delay seolah-olah zoom terjadi
        }
        else
        {
            Debug.LogWarning("Main Camera tidak ditemukan. Efek zoom dilewati.");
            yield return new WaitForSeconds(zoomDuration); // Tetap berikan delay
        }

        // 3. Tunggu sisa durasi (jika ada)
        if (timeToWaitAfterZoom > 0)
        {
            yield return new WaitForSeconds(timeToWaitAfterZoom);
        }

        // 4. Muat Scene "Lose"
        Debug.Log("Memuat Scene Lose: " + loseSceneName);
        SceneLoader.Instance.LoadSceneWithLoadingScreen(loseSceneName);
    }

    public void RestoreHealth(int amount)
    {
        // Tambahkan nyawa, tapi jangan sampai melebihi maksimal
        currentHealth += amount;
        currentHealth = Mathf.Min(currentHealth, maxHealth); // Batasi agar tidak lebih dari maxHealth

        // Update UI health bar
        UpdateHealthUI();

        Debug.Log("Nyawa bertambah! HP sekarang: " + currentHealth);
    }
}