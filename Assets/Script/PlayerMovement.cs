using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [Header("Player Movement")]
    public float moveSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 10f;

    [Header("Attack Settings")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public int attackDamage = 1;
    public LayerMask enemyLayers;

    [Header("Jump Settings")]
    [SerializeField] private LayerMask jumpableGround;
    
    [Header("Quest Interaction")]
    public QuestManager questManager;

    [Header("Sound Effects")]
    public AudioSource sfxAudioSource; // Assign komponen AudioSource Player ke sini
    public AudioClip attackHitSFX;

    public AudioClip collectItemSFX; // Assign file audio "collect" ke sini

    // --- PERUBAHAN VARIABEL INPUT ---
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D coll;
    private PlayerController playerController;
    private Vector2 keyboardMoveInput; // Variabel khusus untuk input keyboard/gamepad
    private float mobileInputX = 0f;   // Variabel khusus untuk input tombol UI
    private GameObject currentInteractable = null;

    private enum MovementState { idle, walk, run, jump, fall }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        playerController = new PlayerController();

        if (sfxAudioSource == null)
        {
            sfxAudioSource = GetComponent<AudioSource>();
        }
    }

    private void OnEnable()
    {
        playerController.Enable();
        playerController.Movement.Move.performed += OnMoveInput;
        playerController.Movement.Move.canceled += OnMoveInput;
        playerController.Movement.Jump.performed += OnJumpPerformed;
        playerController.Movement.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        playerController.Disable();
        playerController.Movement.Move.performed -= OnMoveInput;
        playerController.Movement.Move.canceled -= OnMoveInput;
        playerController.Movement.Jump.performed -= OnJumpPerformed;
        playerController.Movement.Interact.performed -= OnInteractPerformed;
    }

    // --- FUNGSI INPUT INI SEKARANG SANGAT SEDERHANA ---
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // Fungsi ini HANYA mengisi variabel untuk keyboard/gamepad
        keyboardMoveInput = context.ReadValue<Vector2>();
    }

    private void OnJumpPerformed(InputAction.CallbackContext context) => Jump();
    private void OnInteractPerformed(InputAction.CallbackContext context) => HandleContextualAction();
    
    // Fungsi Update sekarang tidak perlu mengurus input gerakan
    private void Update() { }

    // --- LOGIKA UTAMA PINDAH KE SINI ---
    private void FixedUpdate()
    {
        // 1. Tentukan input horizontal yang akan dipakai
        float horizontalInput;

        if (mobileInputX != 0)
        {
            // PRIORITAS 1: Jika tombol mobile sedang ditekan, gunakan nilainya.
            horizontalInput = mobileInputX;
        }
        else
        {
            // PRIORITAS 2: Jika tidak, gunakan nilai dari keyboard/gamepad.
            horizontalInput = keyboardMoveInput.x;
        }

        // 2. Terapkan gerakan menggunakan input yang sudah terpilih
        float currentSpeed = moveSpeed;
        rb.velocity = new Vector2(horizontalInput * currentSpeed, rb.velocity.y);

        // 3. Panggil update animasi dengan input yang sudah terpilih
        UpdateAnimationAndFlip(horizontalInput);
    }

    // --- FUNGSI INI SEKARANG MENERIMA PARAMETER ---
    private void UpdateAnimationAndFlip(float horizontalInput)
    {
        // Logika Flip berdasarkan input final
        if (horizontalInput > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // Logika state animasi berdasarkan input final
        MovementState state;
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -0.1f && !isGrounded())
        {
            state = MovementState.fall;
        }
        else if (Mathf.Abs(horizontalInput) > 0.01f)
        {
            state = MovementState.walk;
        }
        else
        {
            state = MovementState.idle;
        }
        if (anim != null) anim.SetInteger("state", (int)state);
    }
    
    // --- Public Methods untuk Tombol UI Mobile (TIDAK PERLU DIUBAH) ---
    public void Move(float direction)
    {
        mobileInputX = direction;
    }
    public void MobileJump() => Jump();
    public void PerformMobileContextualAction() => HandleContextualAction();

    // Sisa fungsi lain (HandleContextualAction, Jump, isGrounded, dll.) tetap sama
    // ... (salin sisa fungsi lain dari script lama-mu ke sini jika perlu) ...
    private void HandleContextualAction()
    {
        if (currentInteractable != null)
        {
            if (questManager == null) questManager = FindObjectOfType<QuestManager>();
            if (questManager == null) { Debug.LogError("QuestManager tidak ditemukan!"); return; }

            string itemTag = currentInteractable.tag;

            if (itemTag == "Manuscript" || itemTag == "KunciBatu")
            {
                if (itemTag == "Manuscript")
                {
                    ManuscriptItem manuscript = currentInteractable.GetComponent<ManuscriptItem>();
                    if (manuscript != null)
                    {
                        questManager.DisplayAyatContent(manuscript.ayatContent);
                    }
                }

                if (sfxAudioSource != null && collectItemSFX != null)
                {
                    sfxAudioSource.PlayOneShot(collectItemSFX);
                }
                
                questManager.AnItemWasCollected(itemTag);
                Destroy(currentInteractable);
                currentInteractable = null;
            }
        }
        else
        {
            if (anim != null) anim.SetTrigger("Attack1Trigger");
        }
    }
    
    public void ExecuteAttackDamage()
    {
        if (attackPoint == null) { return; }

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        // Variabel untuk memastikan suara hanya diputar sekali per ayunan, meskipun mengenai banyak musuh
        bool hasHit = false; 

        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
                // Jika serangan mengenai musuh
                if (!hasHit) // Cek apakah ini pukulan pertama di ayunan ini
                {
                    // --- MAINKAN SUARA HIT DI SINI ---
                    if (sfxAudioSource != null && attackHitSFX != null)
                    {
                        sfxAudioSource.PlayOneShot(attackHitSFX);
                    }
                    hasHit = true; // Tandai sudah ada yang kena
                }

                enemy.TakeDamage(attackDamage);
            }
        }
    }

    private void Jump()
    {
        if (isGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }
    }

    private bool isGrounded()
    {
        return Physics2D.BoxCast(coll.bounds.center, coll.bounds.size, 0f, Vector2.down, 0.1f, jumpableGround);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Manuscript") || other.CompareTag("KunciBatu"))
        {
            currentInteractable = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject == currentInteractable)
        {
            currentInteractable = null;
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}