using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sprite;
    private PlayerController playerController; // tambahkan PlayerInputActions

    // Untuk input dari button UI
    private float mobileInputX = 0f;

    private Vector2 moveInput;
    private bool isJumping = false;

    private enum MovementState { idle, walk, jump, fall, run }

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    private BoxCollider2D coll;

    // Variabel untuk interaksi
    private GameObject currentInteractable = null;
    public QuestManager questManager; // Akan kita isi nanti lewat Inspector


    [Header("Attack Settings")]
    public Transform attackPoint; // Drag GameObject AttackPoint ke sini
    public float attackRange = 0.5f; // Jangkauan serangan dari attackPoint
    public int attackDamage = 1;     // Damage serangan pemain
    public LayerMask enemyLayers;    // Layer mana saja yang dianggap musuh

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Penting!
        sprite = GetComponent<SpriteRenderer>();
        coll = GetComponent<BoxCollider2D>();

        playerController = new PlayerController();
    }

    private void OnEnable()
    {
        playerController.Enable();

        playerController.Movement.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        playerController.Movement.Move.canceled += ctx => moveInput = Vector2.zero;

        playerController.Movement.Jump.performed += ctx => Jump();

        playerController.Movement.Attack.performed += ctx => PerformAttack(); // Asumsi action 'Attack' ada di map 'Movement'
        playerController.Movement.Interact.performed += ctx => HandleContextualAction();
    }

    private void OnDisable()
    {
        playerController.Disable();
        playerController.Movement.Attack.performed -= ctx => PerformAttack();
        playerController.Movement.Interact.performed -= ctx => HandleContextualAction();

    }

    private void Update()
    {
        if (Application.isMobilePlatform || UnityEditor.EditorApplication.isRemoteConnected) // Cek juga untuk Unity Remote
        {
            // Jika di mobile atau menggunakan Unity Remote, gunakan mobileInputX
            moveInput = new Vector2(mobileInputX, 0f);
        }
        else
        {
            // Jika bukan mobile (misalnya di Editor tanpa Remote atau build PC), pakai Input System Keyboard
            if (playerController != null && playerController.asset.enabled) // Pastikan playerController aktif
            {
                moveInput = playerController.Movement.Move.ReadValue<Vector2>();
            }
            else
            {
                moveInput = Vector2.zero; // Default jika input system tidak aktif
            }
        }
        // Fungsi UpdateAnimation akan menggunakan moveInput.x untuk animasi jalan/idle
        // dan juga untuk membalik sprite.
    }


    private void FixedUpdate()
    {
        // Pastikan bug input ganda sudah diperbaiki
        // Kecepatan sekarang seharusnya hanya dikontrol oleh moveInput.x
        float currentSpeed = moveSpeed; // Nanti bisa ditambahkan logika untuk runSpeed jika ada
        rb.velocity = new Vector2(moveInput.x * currentSpeed, rb.velocity.y);

        UpdateAnimation(); // Ini akan membalik sprite dan set animasi berdasarkan moveInput.x dan rb.velocity.y

        if (isGrounded() && Mathf.Abs(rb.velocity.y) < 0.01f)
        {
            isJumping = false;
        }
    }


    private void UpdateAnimation()
    {
        MovementState state;

        // Gabungkan input dari keyboard dan mobile
        float horizontal = moveInput.x != 0 ? moveInput.x : mobileInputX;

        // Cek arah jalan
        if (horizontal > 0f)
        {
            state = MovementState.walk;
            sprite.flipX = false;
        }
        else if (horizontal < 0f)
        {
            state = MovementState.walk;
            sprite.flipX = true;
        }
        else
        {
            state = MovementState.idle;
        }

        // Cek apakah sedang lompat atau jatuh
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -0.1f)
        {
            state = MovementState.fall;
        }

        anim.SetInteger("state", (int)state);
    }


    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, .1f, jumpableGround);
    }

    private void Jump()
    {
        // Cek ulang grounded saat ini, dan jangan gunakan isJumping (karena bisa delay)
        if (isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            isJumping = true;
        }
    }

    // Fungsi ini dipanggil saat tombol kanan ditekan
    public void MoveRight(bool isPressed)
    {
        if (isPressed)
            mobileInputX = 1f;
        else if (mobileInputX == 1f)
            mobileInputX = 0f;
    }

    public void MoveLeft(bool isPressed)
    {
        if (isPressed)
            mobileInputX = -1f;
        else if (mobileInputX == -1f)
            mobileInputX = 0f;
    }

    public void PerformMobileContextualAction()
    {
        HandleContextualAction(); // Panggil fungsi HandleContextualAction yang sudah ada
    }


    // Fungsi ini dipanggil saat tombol lompat ditekan
    public void MobileJump()
    {
        if (isGrounded())
        {
            Jump();
        }
    }

    // Buat fungsi baru untuk melakukan serangan
    private void PerformAttack()
    {
        // Cek kondisi lain jika perlu (misalnya, tidak bisa attack saat sedang apa)
        anim.SetTrigger("Attack1Trigger"); // Mengaktifkan trigger di Animator
        Debug.Log("Attack performed!"); // Untuk testing
    }

    // Fungsi baru untuk menangani interaksi
    private void PerformInteraction()
    {
        if (currentInteractable != null)
        {
            ManuscriptItem manuscript = currentInteractable.GetComponent<ManuscriptItem>(); // Ambil komponen ManuscriptItem

            if (manuscript != null && !manuscript.isCollected) // Pastikan ada scriptnya dan belum dikoleksi
            {
                Debug.Log("Mengambil manuskrip: " + currentInteractable.name + " | Isi: " + manuscript.ayatContent);

                if (questManager != null)
                {
                    questManager.DisplayAyatContent(manuscript.ayatContent); // Suruh QuestManager tampilkan konten
                    // Tandai sudah dikoleksi (opsional, tergantung bagaimana kamu mau menangani jika pemain berinteraksi lagi)
                    // manuscript.isCollected = true; 
                    questManager.ManuscriptCollected(); // Baru kemudian update progres quest
                }
                else
                {
                    Debug.LogWarning("QuestManager belum di-assign di PlayerMovement!");
                }

                // Hancurkan objek manuskrip setelah kontennya ditampilkan dan progres diupdate
                // Atau bisa juga currentInteractable.SetActive(false); jika tidak mau dihancurkan total
                Destroy(currentInteractable);
                currentInteractable = null; // Kosongkan referensi
            }
            else if (manuscript != null && manuscript.isCollected)
            {
                Debug.Log("Manuskrip ini sudah pernah diambil.");
            }
            else
            {
                Debug.LogError("Objek interaksi tidak memiliki script ManuscriptItem!");
            }
        }
        else
        {
            Debug.Log("Tidak ada objek untuk berinteraksi di dekat pemain.");
        }
    }

    // Mendeteksi ketika pemain masuk ke area trigger objek lain
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Manuscript"))
        {
            Debug.Log("Dekat dengan manuskrip: " + other.name);
            currentInteractable = other.gameObject;
        }
    }


    // Mendeteksi ketika pemain keluar dari area trigger objek lain
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentInteractable)
        {
            Debug.Log("Menjauh dari manuskrip: " + other.name);
            currentInteractable = null;
        }
    }


    private void HandleContextualAction()
    {
        // Cek apakah ada objek yang bisa diinteraksi di dekat pemain
        if (currentInteractable != null)
        {
            // Jika ada, coba berinteraksi sebagai manuskrip
            ManuscriptItem manuscript = currentInteractable.GetComponent<ManuscriptItem>();
            if (manuscript != null && !manuscript.isCollected)
            {
                Debug.Log("INTERAKSI: Mengambil manuskrip - " + currentInteractable.name);
                if (questManager != null)
                {
                    questManager.DisplayAyatContent(manuscript.ayatContent);
                    questManager.ManuscriptCollected(); // Panggil setelah menampilkan konten atau sebelumnya, sesuai alur
                }
                else
                {
                    Debug.LogWarning("QuestManager belum di-assign di PlayerMovement!");
                }
                Destroy(currentInteractable); // Hancurkan manuskrip setelah diambil
                currentInteractable = null;   // Kosongkan referensi
            }
            else if (manuscript != null && manuscript.isCollected)
            {
                Debug.Log("INTERAKSI: Manuskrip ini (" + currentInteractable.name + ") sudah pernah diambil.");
                // Mungkin tidak melakukan apa-apa, atau beri feedback lain
            }
            else
            {
                // Jika currentInteractable bukan manuskrip atau script ManuscriptItem tidak ada
                Debug.LogWarning("INTERAKSI: Objek " + currentInteractable.name + " bukan manuskrip yang valid atau scriptnya hilang.");
            }
        }
        else
        {
            // Jika TIDAK ada objek untuk diinteraksi, lakukan serangan
            Debug.Log("SERANGAN: Tidak ada interaksi, melakukan serangan!");
            if (anim != null) // Pastikan Animator ada
            {
                anim.SetTrigger("Attack1Trigger"); // Memicu animasi serangan pemain
                                                // Damage aktual akan diberikan oleh ExecuteAttackDamage() via Animation Event
            }
            else
            {
                Debug.LogError("Animator belum di-assign atau tidak ada di Player!");
            }
        }
    }


    public void ExecuteAttackDamage()
    {
        if (attackPoint == null)
        {
            Debug.LogError("AttackPoint belum di-assign di PlayerMovement!");
            return;
        }
        Debug.Log("Player Attack Damage Check at frame!");
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Debug.Log("Hit: " + enemyCollider.name);
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                enemy.TakeDamage(attackDamage);
            }
        }
    }


    // (Opsional) Untuk visualisasi jangkauan serangan di Editor (tidak terlihat di game)
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
