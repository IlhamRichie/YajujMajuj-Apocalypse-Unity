using UnityEngine;
using System.Collections;
using UnityEngine.UI; // Ditambahkan jika mau pakai Coroutine untuk efek, dll.

public class Enemy : MonoBehaviour
{

    [Header("UI")]
    public Image enemyHealthBarFill;

    [Header("Stats")]
    public bool isBoss = false;
    public int maxHealth = 3;
    private int currentHealth;

    [Header("Movement & Chase")]
    public float chaseSpeed = 2.0f;
    public float detectionRange = 5f;
    public float patrolSpeed = 1.0f; // Kecepatan saat patroli

    [Header("Attack")]
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    public float attackCooldown = 2f;
    private float lastAttackTime; // Tidak perlu inisialisasi -Mathf.Infinity jika di Start diatur

    [Header("Territory & Patrol")]
    public Vector2 territoryCenter; // Titik tengah area patroli (bisa diset di Inspector atau otomatis)
    public Vector2 territorySize = new Vector2(10f, 1f); // Lebar (X) dan Tinggi (Y) area patroli (Y tidak terlalu dipakai untuk patroli horizontal)
    private Vector3[] patrolPoints = new Vector3[2];
    private int currentPatrolPointIndex = 0;
    private bool patrollingInitialized = false;

    [Header("References")]
    public Transform playerTransform; // Assign Player di Inspector
    private Rigidbody2D rb;
    private Animator anim; // Variabel untuk Animator
    private Vector2 originalScale;

    private enum EnemyState { IDLE, PATROLLING, CHASING, ATTACKING }
    private EnemyState currentState = EnemyState.IDLE;

    void Start()
    {
        currentHealth = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // Ambil komponen Animator
        originalScale = transform.localScale;

        if (playerTransform == null)
        {
            Debug.LogWarning("PlayerTransform belum di-assign di Inspector untuk: " + gameObject.name + ". Mencoba mencari dengan Tag 'Player'.");
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null) {
                playerTransform = playerObject.transform;
            } else {
                Debug.LogError("PlayerTransform tidak di-assign & Player dg Tag 'Player' tidak ditemukan. Musuh tidak akan mengejar/menyerang.");
            }
        }

        // Inisialisasi health bar UI
        if (enemyHealthBarFill != null)
        {
            enemyHealthBarFill.fillAmount = (float)currentHealth / maxHealth;
        }
        else
        {
            Debug.LogWarning("EnemyHealthBarFill belum di-assign untuk: " + gameObject.name);
        }        
        
        InitializePatrolAndState();
        lastAttackTime = -attackCooldown; // Agar bisa langsung attack saat pertama kali kondisi terpenuhi
    }

    void InitializePatrolAndState()
    {
        Vector3 actualTerritoryCenter;
        if (territoryCenter == Vector2.zero && transform.parent == null) // Jika tidak diset & tidak punya parent (untuk world space)
        {
            actualTerritoryCenter = transform.position;
        }
        else if (territoryCenter != Vector2.zero) // Jika diset di inspector (relative to world or parent if any)
        {
             actualTerritoryCenter = new Vector3(territoryCenter.x, territoryCenter.y, transform.position.z);
             if(transform.parent != null) actualTerritoryCenter = transform.parent.TransformPoint(territoryCenter); // Jika center relatif ke parent
        }
        else // Jika tidak diset dan punya parent, pusat teritori relatif ke posisi awal di parent
        {
            actualTerritoryCenter = transform.localPosition; // Ambil local position jika ada parent
             if(transform.parent != null) actualTerritoryCenter = transform.parent.TransformPoint(actualTerritoryCenter); // Konversi ke world
        }


        patrolPoints[0] = new Vector3(actualTerritoryCenter.x - territorySize.x / 2, actualTerritoryCenter.y, transform.position.z);
        patrolPoints[1] = new Vector3(actualTerritoryCenter.x + territorySize.x / 2, actualTerritoryCenter.y, transform.position.z);
        
        // Tentukan titik patroli awal yang dituju
        if (Vector2.Distance(transform.position, patrolPoints[0]) < Vector2.Distance(transform.position, patrolPoints[1])) {
            currentPatrolPointIndex = 0;
        } else {
            currentPatrolPointIndex = 1;
        }
        patrollingInitialized = true;
        currentState = EnemyState.PATROLLING; // Mulai patroli setelah inisialisasi
    }

    void Update()
    {
        if (!patrollingInitialized && currentState != EnemyState.IDLE) return; // Tunggu inisialisasi selesai jika bukan IDLE awal

        float distanceToPlayer = Mathf.Infinity;
        if (playerTransform != null) {
            distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        }

        // === State Transitions ===
        switch (currentState)
        {
            case EnemyState.IDLE: // Dari IDLE biasanya langsung ke PATROLLING
                SetAnimationState("IsMoving", false);
                currentState = EnemyState.PATROLLING;
                break;

            case EnemyState.PATROLLING:
                SetAnimationState("IsMoving", true);
                if (playerTransform != null && distanceToPlayer <= detectionRange)
                {
                    currentState = EnemyState.CHASING;
                }
                break;

            case EnemyState.CHASING:
                SetAnimationState("IsMoving", true); // Atau bisa pakai "IsChasing" jika animasi beda
                if (playerTransform == null || distanceToPlayer > detectionRange || IsStrayingTooFarFromPatrol())
                {
                    currentState = EnemyState.PATROLLING;
                }
                else if (distanceToPlayer <= attackRange)
                {
                    currentState = EnemyState.ATTACKING;
                }
                break;

            case EnemyState.ATTACKING:
                SetAnimationState("IsMoving", false);
                if (playerTransform != null && distanceToPlayer > attackRange && distanceToPlayer <= detectionRange && !IsStrayingTooFarFromPatrol())
                {
                    currentState = EnemyState.CHASING;
                }
                else if (playerTransform == null || distanceToPlayer > detectionRange || IsStrayingTooFarFromPatrol())
                {
                    currentState = EnemyState.PATROLLING;
                }
                break;
        }

        // === State Actions ===
        switch (currentState)
        {
            case EnemyState.PATROLLING:
                Patrol();
                break;
            case EnemyState.CHASING:
                ChasePlayer();
                break;
            case EnemyState.ATTACKING:
                // Movement dihentikan (tidak memanggil MovePosition), logika serangan di AttemptAttack
                AttemptAttack();
                break;
            case EnemyState.IDLE:
                // Tidak melakukan apa-apa, animasi idle sudah di-set dari transisi state lain
                break;
        }
    }
    
    bool IsStrayingTooFarFromPatrol()
    {
        if (!patrollingInitialized) return false;
        // Cek apakah musuh keluar dari batas X patrolinya + sedikit toleransi
        float tolerance = 1f; // Toleransi agar tidak bolak-balik state terus menerus
        return transform.position.x < patrolPoints[0].x - tolerance || transform.position.x > patrolPoints[1].x + tolerance;
    }

    void SetAnimationState(string parameterName, bool value)
    {
        if (anim != null)
        {
            anim.SetBool(parameterName, value);
        }
    }

    void TriggerAnimation(string triggerName)
    {
        if (anim != null)
        {
            anim.SetTrigger(triggerName);
        }
    }

    void Patrol()
    {
        if (patrolPoints.Length < 2) return;

        Vector3 targetPoint = patrolPoints[currentPatrolPointIndex];
        Vector2 directionToTarget = (targetPoint - transform.position).normalized;

        if (Vector2.Distance(transform.position, targetPoint) > 0.1f)
        {
            rb.MovePosition((Vector2)transform.position + (directionToTarget * patrolSpeed * Time.deltaTime));
            FlipSprite(directionToTarget.x);
        }
        else
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
            // Opsional: currentState = EnemyState.IDLE; // Untuk berhenti sejenak di tiap titik
        }
    }

    void ChasePlayer()
    {
        if (playerTransform == null) { currentState = EnemyState.PATROLLING; return; }

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        rb.MovePosition((Vector2)transform.position + (directionToPlayer * chaseSpeed * Time.deltaTime));
        FlipSprite(directionToPlayer.x);
    }

    void FlipSprite(float horizontalDirection)
    {
        if (horizontalDirection > 0.01f) // Bergerak ke kanan
        {
            transform.localScale = new Vector2(Mathf.Abs(originalScale.x), originalScale.y);
        }
        else if (horizontalDirection < -0.01f) // Bergerak ke kiri
        {
            transform.localScale = new Vector2(-Mathf.Abs(originalScale.x), originalScale.y);
        }
    }

    void AttemptAttack()
    {
        if (playerTransform == null) return;

        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            Debug.Log(gameObject.name + " mencoba menyerang pemain!"); // Log ini sudah muncul
            FlipSprite(playerTransform.position.x - transform.position.x); 

            if (anim != null)
            {
                // Jalur Animasi
                Debug.Log(gameObject.name + ": Animator DITEMUKAN. Memicu Trigger 'Attack'.");
                TriggerAnimation("Attack"); // Ini memanggil anim.SetTrigger("Attack")
            }
            else
            {
                // Jalur Damage Langsung
                Debug.Log(gameObject.name + ": Animator TIDAK DITEMUKAN. Mencoba PerformDirectDamageToPlayer.");
                PerformDirectDamageToPlayer();
            }
        }
    }


    // Fungsi ini dipanggil oleh Animation Event dari animasi Attack musuh,
    // ATAU dipanggil langsung oleh PerformDirectDamageToPlayer jika tidak ada animasi.
    public void ExecuteEnemyAttack() 
    {
        Debug.Log(gameObject.name + " -> MEMASUKI ExecuteEnemyAttack()."); // Log #1

        if (playerTransform == null) {
            Debug.LogError(gameObject.name + ": playerTransform adalah NULL di dalam ExecuteEnemyAttack!"); // Log #2
            return;
        }

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        // Log #3: Cek jarak aktual saat damage seharusnya terjadi
        Debug.Log(gameObject.name + ": Jarak ke pemain di ExecuteEnemyAttack = " + distance + 
                ". AttackRange (dengan toleransi) = " + (attackRange + 0.5f));

        if (distance <= attackRange + 0.5f) // Menggunakan attackRange dengan sedikit toleransi
        {
            Debug.Log(gameObject.name + ": Pemain DALAM jangkauan serangan di ExecuteEnemyAttack."); // Log #4

            PlayerHealth playerHealth = playerTransform.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                Debug.Log(gameObject.name + ": Komponen PlayerHealth DITEMUKAN di pemain. Memberikan " + attackDamage + " damage..."); // Log #5
                playerHealth.TakeDamage(attackDamage); // INI YANG SEHARUSNYA MENGURANGI HP PLAYER
            }
            else
            {
                Debug.LogError(gameObject.name + ": Komponen PlayerHealth TIDAK DITEMUKAN di GameObject '" + playerTransform.name + "'!"); // Log #6
            }
        }
        else
        {
            Debug.LogWarning(gameObject.name + ": Pemain TIDAK DALAM jangkauan serangan saat ExecuteEnemyAttack dieksekusi."); // Log #7
        }
    }

    
    // Fungsi ini bisa jadi fallback jika tidak ada animasi
    void PerformDirectDamageToPlayer()
    {
        Debug.Log(gameObject.name + " -> Memanggil PerformDirectDamageToPlayer(), yang akan memanggil ExecuteEnemyAttack().");
        ExecuteEnemyAttack(); // Langsung panggil logika damage
    }


    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // --- LOGIKA BARU UNTUK MEMBEDAKAN UI ---
        if (isBoss)
        {
            // JIKA INI ADALAH BOSS: Update UI health bar utama di layar
            BossBattleManager battleManager = FindObjectOfType<BossBattleManager>();
            if (battleManager != null)
            {
                battleManager.UpdateBossHealthBar(currentHealth, maxHealth);
            }
        }
        else
        {
            // JIKA INI MUSUH BIASA: Update health bar kecil di atas kepalanya sendiri
            if (enemyHealthBarFill != null)
            {
                enemyHealthBarFill.fillAmount = (float)currentHealth / maxHealth;
            }
        }
        // -----------------------------------------

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " mati!");
        // TriggerAnimation("Die");

        // --- TAMBAHAN UNTUK MELAPOR KE QUEST MANAGER ---
        QuestManager questManager = FindObjectOfType<QuestManager>();
        if (questManager != null)
        {
            questManager.EnemyKilled();
        }
        else
        {
            Debug.LogWarning("QuestManager tidak ditemukan di scene. Kill tidak tercatat.");
        }
        // --- AKHIR TAMBAHAN ---

        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Gizmos untuk Territory dan Patrol Points
        Vector3 gizmoTerritoryCenter;
        if(Application.isPlaying && patrollingInitialized){
            // Jika sudah play dan patroli diinisialisasi, pusatnya adalah titik tengah antara patrolPoints
             gizmoTerritoryCenter = patrolPoints[0] + (patrolPoints[1] - patrolPoints[0]) / 2;
             gizmoTerritoryCenter.z = transform.position.z; // Jaga Z tetap sama
        } else if (territoryCenter == Vector2.zero && transform.parent == null) {
            gizmoTerritoryCenter = transform.position;
        } else if (territoryCenter != Vector2.zero) {
            gizmoTerritoryCenter = new Vector3(territoryCenter.x, territoryCenter.y, transform.position.z);
            if(transform.parent != null && !Application.isPlaying) gizmoTerritoryCenter = transform.parent.TransformPoint(new Vector3(territoryCenter.x, territoryCenter.y, 0));
             gizmoTerritoryCenter.z = transform.position.z;
        } else {
            gizmoTerritoryCenter = transform.position; // Fallback
        }


        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(gizmoTerritoryCenter, new Vector3(territorySize.x, territorySize.y, 0));

        if (Application.isPlaying && patrollingInitialized)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(patrolPoints[0], 0.3f);
            Gizmos.DrawSphere(patrolPoints[1], 0.3f);
            Gizmos.DrawLine(patrolPoints[0], patrolPoints[1]);
        } else if (!Application.isPlaying) { // Preview patrol points saat edit
            Vector3 p0 = new Vector3(gizmoTerritoryCenter.x - territorySize.x / 2, gizmoTerritoryCenter.y, transform.position.z);
            Vector3 p1 = new Vector3(gizmoTerritoryCenter.x + territorySize.x / 2, gizmoTerritoryCenter.y, transform.position.z);
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(p0, 0.2f);
            Gizmos.DrawSphere(p1, 0.2f);
            Gizmos.DrawLine(p0,p1);
        }
    }
}