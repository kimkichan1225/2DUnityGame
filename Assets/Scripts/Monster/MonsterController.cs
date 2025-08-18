using System.Collections;
using UnityEngine;

// 몬스터의 이동, 탐지, 추적, 공격, 사운드 재생을 관리하는 스크립트
public class MonsterController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float minMoveDuration = 1f;
    [SerializeField] private float maxMoveDuration = 3f;
    [SerializeField] private float minPauseDuration = 0.5f;
    [SerializeField] private float maxPauseDuration = 2f;
    [SerializeField] private float moveRange = 5f;

    [Header("Detection Settings")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRadius = 1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Attack Settings")]
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private BoxCollider2D attackHitbox;

    [Header("Sound Settings")]
    [SerializeField] private AudioClip walkSound;

    private Rigidbody2D rb;
    private MonsterHealth monsterHealth;
    private Animator animator;
    private AudioSource audioSource;
    private Renderer rend; // 카메라에 보이는지 확인용

    private Vector2 startPosition;
    private bool isMovingRight = true;
    private bool isMoving = true;
    private Vector3 initialScale;
    private bool isChasingPlayer = false;
    private bool canAttack = true;
    private bool isAttacking = false;
    private Transform player;

    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int attackHash = Animator.StringToHash("Attack");

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        monsterHealth = GetComponent<MonsterHealth>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        rend = GetComponent<Renderer>();

        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        startPosition = transform.position;
        initialScale = transform.localScale;

        if (attackHitbox != null)
            attackHitbox.enabled = false;

        if (monsterHealth != null && animator != null)
            StartCoroutine(MovementRoutine());
    }

    void Update()
    {
        if (monsterHealth != null && monsterHealth.IsDead)
        {
            StopAllCoroutines();
            rb.velocity = Vector2.zero;
            if (animator != null)
                animator.SetFloat(speedHash, 0f);
            return;
        }

        DetectPlayer();

        if (isChasingPlayer)
        {
            ChasePlayer();
        }
        else
        {
            if (Mathf.Abs(transform.position.x - startPosition.x) >= moveRange)
            {
                isMovingRight = transform.position.x > startPosition.x ? false : true;
            }
        }

        UpdateSpriteDirection();
    }

    private void DetectPlayer()
    {
        Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, playerLayer);
        if (playerCollider != null && playerCollider.CompareTag("Player"))
        {
            player = playerCollider.transform;
            isChasingPlayer = true;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRadius && canAttack && !isAttacking)
            {
                StartCoroutine(AttackPlayer());
            }
        }
        else
        {
            isChasingPlayer = false;
            player = null;
        }
    }

    private void ChasePlayer()
    {
        if (player == null || isAttacking) return;

        Vector2 direction = (player.position - transform.position).normalized;
        float moveDir = Mathf.Sign(direction.x); // -1 또는 1

        // 스프라이트 방향과 이동 방향을 일치시킴
        float xScale = Mathf.Abs(initialScale.x);
        transform.localScale = new Vector3(xScale * moveDir, initialScale.y, initialScale.z);

        rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

        isMovingRight = moveDir > 0;
        isMoving = true;

        animator.SetFloat(speedHash, moveSpeed);
    }

    private IEnumerator AttackPlayer()
    {
        while (isChasingPlayer && player != null)
        {
            canAttack = false;
            isAttacking = true;
            isMoving = false;
            rb.velocity = Vector2.zero;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer > attackRadius)
            {
                isAttacking = false;
                isMoving = true;
                canAttack = true;
                yield break;
            }

            if (animator != null)
                animator.SetTrigger(attackHash);

            yield return new WaitForSeconds(1f);

            isAttacking = false;
            isMoving = true;

            if (player != null)
            {
                isMovingRight = player.position.x > transform.position.x;
                UpdateSpriteDirection();
            }

            yield return new WaitForSeconds(attackCooldown);
            canAttack = true;

            if (isChasingPlayer && player != null && Vector2.Distance(transform.position, player.position) <= attackRadius)
                continue;
            else
                yield break;
        }
    }

    private void UpdateSpriteDirection()
    {
        if (animator == null || isAttacking) return;

        float moveDir = isMovingRight ? 1f : -1f;

        // 스프라이트 방향 고정
        float xScale = Mathf.Abs(initialScale.x);
        transform.localScale = new Vector3(xScale * moveDir, initialScale.y, initialScale.z);

        // 정찰 이동일 경우, 이동 방향 강제
        if (!isChasingPlayer)
        {
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);
        }

        float currentSpeed = Mathf.Abs(rb.velocity.x);
        animator.SetFloat(speedHash, currentSpeed > 0.1f ? moveSpeed : 0f);
    }

    private IEnumerator MovementRoutine()
    {
        while (true)
        {
            // 플레이어를 추적 중이거나 공격 중일 땐 이 루틴을 건너뜀
            if (isChasingPlayer || isAttacking)
            {
                yield return null;
                continue;
            }

            // ➤ 이동 상태
            isMoving = true;

            // 이동 방향 랜덤 설정 (50% 확률로 방향 반전)
            if (Random.value > 0.5f)
                isMovingRight = !isMovingRight;

            // 이동 범위 검사 후 방향 조정
            if (Mathf.Abs(transform.position.x - startPosition.x) >= moveRange)
                isMovingRight = transform.position.x > startPosition.x ? false : true;

            float moveDir = isMovingRight ? 1f : -1f;
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

            // 이동 애니메이션 갱신
            UpdateSpriteDirection();

            // 이동 시간 랜덤 대기
            float moveDuration = Random.Range(minMoveDuration, maxMoveDuration);
            yield return new WaitForSeconds(moveDuration);

            // ➤ 멈춤 상태
            isMoving = false;
            rb.velocity = Vector2.zero;

            // 애니메이션 속도 갱신 (멈춤)
            animator.SetFloat(speedHash, 0f);

            // 멈추는 시간 랜덤 대기
            float pauseDuration = Random.Range(minPauseDuration, maxPauseDuration);
            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && attackHitbox != null && attackHitbox.enabled)
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    // 애니메이션 이벤트로 호출: 공격 히트박스 ON
    public void EnableAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = true;
    }

    // 애니메이션 이벤트로 호출: 공격 히트박스 OFF
    public void DisableAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.enabled = false;
    }

    // 애니메이션 이벤트로 호출: 공격 사운드 재생
    public void PlayAttackSound()
    {
        if (attackSound != null && audioSource != null)
            audioSource.PlayOneShot(attackSound);
    }

    // 애니메이션 이벤트로 호출: 걷기 사운드 재생 (카메라에 보일 때만)
    public void PlayWalkSound()
    {
        if (rend != null && !rend.isVisible)
            return;

        if (walkSound != null && audioSource != null)
            audioSource.PlayOneShot(walkSound);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }

    // 외부 접근용 읽기 전용 상태
    public bool IsMovingRight => isMovingRight;
    public bool IsMoving => isMoving;
    public bool IsChasingPlayer => isChasingPlayer;
}
