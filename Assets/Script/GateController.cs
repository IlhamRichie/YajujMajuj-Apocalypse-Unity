using UnityEngine;
using UnityEngine.SceneManagement; // Penting untuk pindah scene!

public class GateController : MonoBehaviour
{
    [Header("Sprite Gerbang")]
    public Sprite gateClosedSprite; // Drag sprite gerbang tertutup ke sini
    public Sprite gateOpenSprite;   // Drag sprite gerbang terbuka ke sini

    [Header("Pengaturan Level")]
    public string nextLevelSceneName = "Level2"; // Isi dengan nama scene level 2 kamu

    private bool isOpen = false;
    private SpriteRenderer spriteRenderer;
    private Collider2D gateCollider;

    private AudioSource audioSource; 

    void Start()
    {
        // Ambil komponen yang dibutuhkan
        spriteRenderer = GetComponent<SpriteRenderer>();
        gateCollider = GetComponent<Collider2D>();

        audioSource = GetComponent<AudioSource>();

        // Pastikan gerbang dalam keadaan tertutup di awal
        spriteRenderer.sprite = gateClosedSprite;
        gateCollider.enabled = false; // Nonaktifkan collider agar tidak bisa dimasuki sebelum terbuka
    }

    // Fungsi ini akan dipanggil oleh QuestManager saat semua manuskrip terkumpul
    public void OpenGate()
    {
        if (!isOpen)
        {
            Debug.Log("Gerbang telah terbuka!");
            isOpen = true;
            spriteRenderer.sprite = gateOpenSprite;
            gateCollider.enabled = true;

            // Mainkan suara saat gerbang terbuka
            if (audioSource != null)
            {
                audioSource.Play();
            }
        }
    }

    // Fungsi ini berjalan saat ada objek lain yang masuk ke area trigger gerbang
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah gerbang sudah terbuka DAN yang masuk adalah pemain
        if (isOpen && other.CompareTag("Player"))
        {
            Debug.Log("Pemain memasuki gerbang. Memuat " + nextLevelSceneName + "...");

            // Muat scene level berikutnya
            SceneManager.LoadScene(nextLevelSceneName);
        }
    }
}