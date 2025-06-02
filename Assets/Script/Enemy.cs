using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Stats")]
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement & Chase")]
    public float chaseSpeed = 2.0f;
    public float detectionRange = 5f;
    // public LayerMask playerLayer; // Masih bisa dipakai jika ingin deteksi lebih spesifik

    [Header("Attack")]
    public float attackRange = 1.5f;   // Jarak musuh bisa menyerang (lebih pendek dari detectionRange)
    public int attackDamage = 1;       // Damage serangan musuh
    public float attackCooldown = 2f;  // Waktu jeda antar serangan musuh (dalam detik)
    private float lastAttackTime = -Mathf.Infinity; // Waktu terakhir musuh menyerang, diinisialisasi agar bisa langsung attack

    private Transform playerTransform;
    private Rigidbody2D rb;
    private Vector2 originalScale;
    // private Animator enemyAnim; // Kita akan uncomment ini jika nanti pakai animasi

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        originalScale = transform.localScale;
        // enemyAnim = GetComponent<Animator>(); // Jika ada Animator

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            playerTransform = playerObject.transform;
        }
        else
        {
            Debug.LogError("Pemain dengan tag 'Player' tidak ditemukan! Musuh tidak akan bisa mengejar atau menyerang.");
        }
        lastAttackTime = -attackCooldown; // Agar musuh bisa langsung menyerang saat pertama kali bertemu
    }

    void Update()
    {
        if (playerTransform == null) return; // Jangan lakukan apa-apa jika tidak ada pemain

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= detectionRange) // Jika pemain dalam jangkauan deteksi
        {
            // Selalu hadap pemain jika dalam jangkauan deteksi
            Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
            FlipSprite(directionToPlayer.x);

            if (distanceToPlayer <= attackRange) // Jika pemain dalam jangkauan SERANG
            {
                // Berhenti bergerak dan coba serang
                // (Untuk Rigidbody Kinematic, berhenti bisa dengan tidak memanggil MovePosition)
                AttemptAttack();
            }
            else // Jika pemain dalam jangkauan deteksi TAPI di luar jangkauan serang
            {
                // Kejar pemain
                // if (enemyAnim != null) enemyAnim.SetBool("IsMoving", true); // Jika ada animasi jalan
                rb.MovePosition((Vector2)transform.position + (directionToPlayer * chaseSpeed * Time.deltaTime));
            }
        }
        // else // Jika pemain di luar jangkauan deteksi
        // {
        //     // if (enemyAnim != null) enemyAnim.SetBool("IsMoving", false); // Jika ada animasi idle
        //     // Berhenti atau patroli (jika ada logika patroli)
        // }
    }

    void FlipSprite(float horizontalDirection)
    {
        if (horizontalDirection > 0)
        {
            transform.localScale = new Vector2(originalScale.x, originalScale.y);
        }
        else if (horizontalDirection < 0)
        {
            transform.localScale = new Vector2(-originalScale.x, originalScale.y);
        }
    }

    void AttemptAttack()
    {
        if (Time.time >= lastAttackTime + attackCooldown) // Cek apakah cooldown sudah selesai
        {
            lastAttackTime = Time.time; // Catat waktu serangan terakhir
            Debug.Log(gameObject.name + " mencoba menyerang pemain!");

            // TODO: Nanti di sini kita bisa trigger animasi serangan musuh
            // if (enemyAnim != null) enemyAnim.SetTrigger("AttackTrigger");
            // else PerformDirectDamageToPlayer(); // Jika tidak ada animasi, langsung deal damage

            PerformDirectDamageToPlayer(); // Untuk sekarang, langsung deal damage
        }
    }

    // Fungsi ini akan memberikan damage ke pemain secara langsung
    void PerformDirectDamageToPlayer()
    {
        // Pastikan pemain masih dalam jangkauan saat damage diberikan (bisa saja pemain kabur sedikit)
        if (playerTransform != null && Vector2.Distance(transform.position, playerTransform.position) <= attackRange)
        {
            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log(gameObject.name + " memberikan " + attackDamage + " damage ke pemain.");
                playerHealth.TakeDamage(attackDamage);
            }
        }
    }

    public void TakeDamage(int damageAmount)
    {
        // ... (kode TakeDamage musuh yang sudah ada)
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " terkena damage: " + damageAmount + ", Sisa health: " + currentHealth);
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // ... (kode Die musuh yang sudah ada)
        Debug.Log(gameObject.name + " mati!");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        // ... (kode OnDrawGizmosSelected yang sudah ada untuk detectionRange)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Tambahkan Gizmos untuk attackRange
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}