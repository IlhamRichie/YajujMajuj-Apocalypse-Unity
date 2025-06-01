using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement & Chase")]
    public float chaseSpeed = 2.0f; // Kecepatan musuh saat mengejar
    public float detectionRange = 5f; // Jarak musuh bisa mendeteksi pemain
    public LayerMask playerLayer;     // Layer tempat pemain berada (untuk deteksi yang lebih spesifik)
    private Transform playerTransform; // Untuk menyimpan posisi pemain
    private Rigidbody2D rb;
    private Vector2 originalScale; // Untuk menyimpan skala awal (jika sprite perlu di-flip)

    // (Opsional) Untuk efek visual sederhana saat terkena serangan
    // public SpriteRenderer spriteRenderer;
    // public Color hitColor = Color.red;
    // private Color originalColor;
    // public float hitEffectDuration = 0.1f;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>(); // Ambil komponen Rigidbody2D
        originalScale = transform.localScale; // Simpan skala awal

        // Cari GameObject pemain berdasarkan Tag "Player"
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Pemain dengan tag 'Player' tidak ditemukan! Musuh tidak akan bisa mengejar.");
        }

        // if (spriteRenderer == null)
        // {
        //     spriteRenderer = GetComponent<SpriteRenderer>();
        // }
        // if (spriteRenderer != null)
        // {
        //     originalColor = spriteRenderer.color;
        // }
    }

    void Update()
    {
        // Hanya lakukan pengejaran jika pemain sudah ditemukan
        if (playerTransform != null)
        {
            // Hitung jarak ke pemain
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            // Jika pemain dalam jangkauan deteksi
            if (distanceToPlayer <= detectionRange)
            {
                // Tentukan arah ke pemain
                Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;

                // Gerakkan musuh menuju pemain menggunakan Rigidbody2D (cocok untuk Rigidbody Kinematic)
                rb.MovePosition((Vector2)transform.position + (directionToPlayer * chaseSpeed * Time.deltaTime));

                // Balikkan sprite musuh agar menghadap pemain
                FlipSprite(directionToPlayer.x);
            }
            // else
            // {
            //     // Opsional: Jika pemain di luar jangkauan, musuh bisa berhenti atau patroli
            //     // Untuk sekarang, biarkan dia berhenti jika tidak ada logika patroli
            //     // rb.velocity = Vector2.zero; // Jika menggunakan Rigidbody Dynamic
            // }
        }
    }

    void FlipSprite(float horizontalDirection)
    {
        if (horizontalDirection > 0) // Jika bergerak ke kanan
        {
            transform.localScale = new Vector2(originalScale.x, originalScale.y); // Arah normal (misal, sprite default hadap kanan)
        }
        else if (horizontalDirection < 0) // Jika bergerak ke kiri
        {
            transform.localScale = new Vector2(-originalScale.x, originalScale.y); // Balik sumbu X
        }
        // Jika horizontalDirection == 0, biarkan arah terakhir
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " terkena damage: " + damageAmount + ", Sisa health: " + currentHealth);

        // (Opsional) Efek visual sederhana saat terkena serangan
        // StartCoroutine(HitEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    // (Opsional) Coroutine untuk efek visual
    // System.Collections.IEnumerator HitEffect()
    // {
    //     if (spriteRenderer != null)
    //     {
    //         spriteRenderer.color = hitColor;
    //         yield return new WaitForSeconds(hitEffectDuration);
    //         spriteRenderer.color = originalColor;
    //     }
    // }

    void Die()
    {
        Debug.Log(gameObject.name + " mati!");
        Destroy(gameObject);
    }

    // (Opsional) Untuk visualisasi jangkauan deteksi di Editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}