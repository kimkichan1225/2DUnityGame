using UnityEngine;
using System.Collections;

public class EnemyAI : MonoBehaviour
{
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 3f;
    [SerializeField] private float patrolWaitTime = 2f;

    [Header("감지 및 공격 설정")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int attackDamage = 10;

    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int defense = 0;

    [Header("컴포넌트 및 오브젝트")]
    [SerializeField] private Transform player;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck; // 낭떠러지 감지용
    [SerializeField] private Transform wallCheck;   // 벽 감지용
    [SerializeField] private GameObject damageTextPrefab;
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 1f, 0);

    [Header("사운드 설정")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip attackSound;

    private int currentHealth;
    private bool isDead = false;
    private bool facingRight = true;
    private Vector2 startPos;
    private bool isPatrolling = true;
    private float waitTimer = 0f;
    private float attackTimer = 0f;

    private Animator animator;
    private Rigidbody2D rb;
    private AudioSource audioSource;

    private enum EnemyState { Idle, Run, Attack, Hurt, Die }
    private EnemyState currentState = EnemyState.Idle;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();
        currentHealth = maxHealth;
        startPos = transform.position;
        if (player == null) player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (isDead) return;

        if (attackTimer > 0) attackTimer -= Time.deltaTime;

        switch (currentState)
        {
            case EnemyState.Idle: HandleIdle(); break;
            case EnemyState.Run: HandleRun(); break;
            case EnemyState.Attack: HandleAttack(); break;
        }

        UpdateAnimator();
    }

    private void FixedUpdate()
    {
        if (isDead || currentState == EnemyState.Hurt) return;

        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        if (distanceToPlayer <= detectionRange && player != null)
        {
            isPatrolling = false;
            if (distanceToPlayer <= attackRange && attackTimer <= 0)
            {
                currentState = EnemyState.Attack;
            }
            else
            {
                currentState = EnemyState.Run;
            }
        }
        else
        {
            isPatrolling = true;
            if (currentState != EnemyState.Idle) currentState = EnemyState.Run;
        }
    }

    private void HandleIdle()
    {
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            currentState = EnemyState.Run;
            Flip();
        }
    }

    private void HandleRun()
    {
        // --- 절벽 및 벽 감지 --- 
        RaycastHit2D groundInfo = Physics2D.Raycast(groundCheck.position, Vector2.down, 2f, groundLayer);
        RaycastHit2D wallInfo = Physics2D.Raycast(wallCheck.position, facingRight ? Vector2.right : Vector2.left, 0.1f, groundLayer);

        if (groundInfo.collider == false || wallInfo.collider == true)
        {
            if (isPatrolling)
            {
                currentState = EnemyState.Idle;
                waitTimer = patrolWaitTime;
                rb.velocity = new Vector2(0, rb.velocity.y);
                return;
            }
        }

        if (isPatrolling)
        {
            float moveDir = facingRight ? 1f : -1f;
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        }
        else if (player != null)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

            if (direction.x > 0 && !facingRight) Flip();
            else if (direction.x < 0 && facingRight) Flip();
        }
    }

    private void HandleAttack()
    {
        rb.velocity = new Vector2(0, rb.velocity.y);
        animator.SetTrigger("Attack");
        attackTimer = attackCooldown;
    }

    public void PerformAttack()
    {
        if (player == null) return;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(attackDamage);
            if (attackSound != null && audioSource != null) audioSource.PlayOneShot(attackSound);
        }
    }

    public void TakeDamageSword(int amount, int attackType) { /* ... */ }
    public void TakeDamageMace(int amount, int attackType) { /* ... */ }
    public void TakeDamageLance(int amount, int attackType) { /* ... */ }

    private void ApplyDamage(int finalDamage)
    {
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        if (hitSound != null && audioSource != null) audioSource.PlayOneShot(hitSound);
        if (damageTextPrefab != null)
        {
            // ... 데미지 텍스트 생성 ...
        }
        animator.SetTrigger("Hurt");
        currentState = EnemyState.Hurt;
        if (currentHealth <= 0) Die();
        else Invoke(nameof(RecoverFromHit), 0.3f);
    }

    private void RecoverFromHit()
    {
        if (!isDead) currentState = EnemyState.Idle;
    }

    private void Die()
    {
        isDead = true;
        currentState = EnemyState.Die;
        rb.velocity = Vector2.zero;
        animator.SetTrigger("Die");
        if (deathSound != null && audioSource != null) audioSource.PlayOneShot(deathSound);
        StartCoroutine(DestroyAfterAnimation());
    }

    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void UpdateAnimator()
    {
        animator.SetBool("isWalking", currentState == EnemyState.Run);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z);
        // groundCheck와 wallCheck의 위치도 함께 뒤집어주면 좋지만, 일단은 생략
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(groundCheck.position, groundCheck.position + Vector3.down * 2f);
        }
        if (wallCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (facingRight ? Vector3.right : Vector3.left) * 0.1f);
        }
    }
}