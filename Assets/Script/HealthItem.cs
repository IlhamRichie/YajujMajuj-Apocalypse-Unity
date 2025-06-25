using UnityEngine;

public class HealthItem : MonoBehaviour
{
    [Tooltip("Jumlah nyawa yang akan dipulihkan saat item ini diambil.")]
    public int healthToRestore = 2; // Atur di Inspector berapa nyawa yang didapat

    // Variabel untuk efek visual sederhana (opsional)
    public float floatSpeed = 0.5f; // Kecepatan item mengambang
    public float floatHeight = 0.25f; // Ketinggian item mengambang
    private Vector3 startPosition;

    void Start()
    {
        // Simpan posisi awal untuk gerakan mengambang
        startPosition = transform.position;
    }
  
    void Update()
    {
        // Membuat item bergerak naik-turun (mengambang) agar terlihat menarik
        float newY = startPosition.y + Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = new Vector3(startPosition.x, newY, startPosition.z);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cek apakah yang menyentuh item ini adalah objek dengan tag "Player"
        if (other.CompareTag("Player"))
        {
            // Cari komponen PlayerHealth pada objek pemain
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

            // Jika komponen ditemukan, panggil fungsi untuk menambah nyawa
            if (playerHealth != null)
            {
                playerHealth.RestoreHealth(healthToRestore);
                Debug.Log("Pemain mengambil item dan memulihkan " + healthToRestore + " HP.");

                // Mainkan sound effect "collect" jika ada
                PlayerMovement playerMovement = other.GetComponent<PlayerMovement>();
                if (playerMovement != null && playerMovement.sfxAudioSource != null && playerMovement.collectItemSFX != null)
                {
                    playerMovement.sfxAudioSource.PlayOneShot(playerMovement.collectItemSFX);
                }

                // Hancurkan item setelah diambil
                Destroy(gameObject);
            }
        }
    }
}