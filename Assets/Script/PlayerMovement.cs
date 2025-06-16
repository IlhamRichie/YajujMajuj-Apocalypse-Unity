using UnityEngine;
using UnityEngine.InputSystem;

// Tidak perlu 'using System.Collections.Generic;' jika tidak digunakan
// Tidak perlu 'using System.Collections;' jika tidak ada Coroutine di sini

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

    // Komponen & Variabel Internal
    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D coll;
    private PlayerController playerController;
    private Vector2 moveInput;
    private float mobileInputX = 0f;
    private GameObject currentInteractable = null;

    private enum MovementState { idle, walk, run, jump, fall }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        coll = GetComponent<BoxCollider2D>();
        playerController = new PlayerController();
    }

    private void OnEnable()
    {
        playerController.Enable();

        // Subscribe ke semua event input
        playerController.Movement.Move.performed += OnMoveInput;
        playerController.Movement.Move.canceled += OnMoveInput;
        playerController.Movement.Jump.performed += OnJumpPerformed;
        playerController.Movement.Interact.performed += OnInteractPerformed;
    }

    private void OnDisable()
    {
        playerController.Disable();

        // Unsubscribe dari semua event
        playerController.Movement.Move.performed -= OnMoveInput;
        playerController.Movement.Move.canceled -= OnMoveInput;
        playerController.Movement.Jump.performed -= OnJumpPerformed;
        playerController.Movement.Interact.performed -= OnInteractPerformed;
    }

    // --- Event Handler Functions ---
    private void OnMoveInput(InputAction.CallbackContext context)
    {
        // Hanya proses input ini jika BUKAN dari platform mobile atau remote
        // Ini untuk mencegah keyboard menimpa input tombol UI saat testing
        if (!(Application.isMobilePlatform || UnityEditor.EditorApplication.isRemoteConnected))
        {
            moveInput = context.ReadValue<Vector2>();
        }
    }

    private void OnJumpPerformed(InputAction.CallbackContext context) => Jump();
    private void OnInteractPerformed(InputAction.CallbackContext context) => HandleContextualAction();
    
    private void Update()
    {
        // Hanya proses input mobile jika DI platform mobile atau remote
        if (Application.isMobilePlatform || UnityEditor.EditorApplication.isRemoteConnected)
        {
            // Set moveInput.x dari variabel yang diubah oleh tombol UI
            moveInput.x = mobileInputX;
        }
    }

    private void FixedUpdate()
    {
        float currentSpeed = moveSpeed;
        Vector2 newVelocity = new Vector2(moveInput.x * currentSpeed, rb.velocity.y);
        rb.velocity = newVelocity;

        // Hanya log jika ada input untuk menghindari spam
        if (moveInput.x != 0)
        {
            Debug.Log("LOG C: FixedUpdate() mengatur velocity.x menjadi = " + newVelocity.x);
        }

        UpdateAnimationAndFlip();
    }

    private void UpdateAnimationAndFlip()
    {
        // Logika Flip menggunakan transform.localScale
        if (moveInput.x > 0.01f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveInput.x < -0.01f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        // Logika state animasi
        MovementState state;
        if (rb.velocity.y > 0.1f)
        {
            state = MovementState.jump;
        }
        else if (rb.velocity.y < -0.1f && !isGrounded())
        {
            state = MovementState.fall;
        }
        else if (Mathf.Abs(moveInput.x) > 0.01f)
        {
            state = MovementState.walk;
        }
        else
        {
            state = MovementState.idle;
        }

        if (anim != null) anim.SetInteger("state", (int)state);
    }

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
                
                questManager.AnItemWasCollected(itemTag);
                Destroy(currentInteractable);
                currentInteractable = null;
            }
        }
        else // Jika tidak ada objek interaktif, lakukan serangan
        {
            if (anim != null) anim.SetTrigger("Attack1Trigger");
        }
    }
    
    public void ExecuteAttackDamage()
    {
        if (attackPoint == null) { Debug.LogError("AttackPoint belum di-assign!"); return; }
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);
        foreach (Collider2D enemyCollider in hitEnemies)
        {
            Enemy enemy = enemyCollider.GetComponent<Enemy>();
            if (enemy != null)
            {
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

    // --- Trigger Interaksi ---
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

    // --- Public Methods untuk Tombol UI Mobile ---
    public void Move(float direction)
    {
        Debug.Log("LOG A: Move() dipanggil, direction = " + direction);
        mobileInputX = direction;
    }
    public void MobileJump() => Jump();
    public void PerformMobileContextualAction() => HandleContextualAction();
    
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}