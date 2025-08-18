using UnityEngine;
using System.Collections;

// 적 몬스터의 AI를 제어하는 스크립트
// 좌우 순찰, 플레이어 감지 시 추적, 공격 범위 내에서 공격 수행
public class EnemyAI : MonoBehaviour
{
    // === 인스펙터에서 설정할 변수 ===
    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 2f;           // 적의 이동 속도
    [SerializeField] private float patrolDistance = 3f;      // 순찰 범위 (좌우 이동 거리)
    [SerializeField] private float patrolWaitTime = 2f;      // 순찰 지점에서 대기 시간

    [Header("감지 및 공격 설정")]
    [SerializeField] private float detectionRange = 5f;      // 플레이어 감지 범위
    [SerializeField] private float attackRange = 1f;         // 공격 범위
    [SerializeField] private float attackCooldown = 1.5f;    // 공격 쿨타임
    [SerializeField] private int attackDamage = 10;          // 공격 데미지

    [Header("체력 설정")]
    [SerializeField] private int maxHealth = 100;            // 최대 체력
    [SerializeField] private int defense = 0;                // 방어력

    [Header("컴포넌트 및 오브젝트")]
    [SerializeField] private Transform player;               // 플레이어 Transform 참조
    [SerializeField] private LayerMask groundLayer;          // 바닥 레이어 (순찰 시 사용)
    [SerializeField] private Transform groundCheck;          // 바닥 감지 위치
    [SerializeField] private GameObject damageTextPrefab;    // 데미지 텍스트 프리팹
    [SerializeField] private Vector3 damageTextOffset = new Vector3(0, 1f, 0); // 데미지 텍스트 오프셋

    [Header("사운드 설정")]
    [SerializeField] private AudioClip hitSound;             // 피격 사운드
    [SerializeField] private AudioClip deathSound;           // 사망 사운드
    [SerializeField] private AudioClip attackSound;          // 공격 사운드

    // === 내부 상태 변수 ===
    private int currentHealth;                               // 현재 체력
    private bool isDead = false;                            // 사망 상태 여부
    private bool facingRight = true;                        // 오른쪽을 향하고 있는지
    private Vector2 startPos;                               // 초기 위치 (순찰 기준)
    private bool isPatrolling = true;                       // 순찰 중인지 여부
    private float waitTimer = 0f;                           // 순찰 대기 타이머
    private float attackTimer = 0f;                         // 공격 쿨타임 타이머
    private bool isGrounded = true;                         // 바닥에 닿아 있는지

    // === 컴포넌트 참조 ===
    private Animator animator;                               // 애니메이터 컴포넌트
    private Rigidbody2D rb;                                 // Rigidbody2D 컴포넌트
    private AudioSource audioSource;                        // 사운드 재생용 AudioSource

    // === 상태 열거형 ===
    private enum EnemyState
    {
        Idle,    // 대기 (순찰 중 정지)
        Run,     // 이동 (순찰 또는 추적)
        Attack,  // 공격
        Hurt,    // 피격
        Die      // 사망
    }
    private EnemyState currentState = EnemyState.Idle;      // 현재 상태

    // 초기화 메서드
    private void Start()
    {
        // 컴포넌트 초기화
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        // 체력 초기화
        currentHealth = maxHealth;

        // 시작 위치 저장
        startPos = transform.position;

        // 플레이어 Transform가 없으면 자동으로 찾기
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
        }
    }

    // 매 프레임 호출
    private void Update()
    {
        // 사망 상태면 아무 동작도 하지 않음
        if (isDead) return;

        // 바닥 감지
        CheckGround();

        // 공격 쿨타임 관리
        if (attackTimer > 0)
        {
            attackTimer -= Time.deltaTime;
        }

        // 현재 상태에 따라 동작 처리
        switch (currentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Run:
                HandleRun();
                break;            
            case EnemyState.Attack:
                HandleAttack();
                break;
            case EnemyState.Hurt:
                // 피격 상태는 애니메이션 종료 후 RecoverFromHit에서 처리
                break;
            case EnemyState.Die:
                // 사망 상태는 Die 메서드에서 처리
                break;
        }

        // 애니메이터 파라미터 업데이트
        UpdateAnimator();
    }

    // 고정 업데이트 (물리 관련)
    private void FixedUpdate()
    {
        // 사망 상태거나 피격 상태면 이동하지 않음
        if (isDead || currentState == EnemyState.Hurt) return;

        // 플레이어와의 거리 계산
        float distanceToPlayer = player != null ? Vector2.Distance(transform.position, player.position) : float.MaxValue;

        // 플레이어 감지 및 상태 전환
        if (distanceToPlayer <= detectionRange && player != null)
        {
            isPatrolling = false; // 순찰 중지
            if (distanceToPlayer <= attackRange && attackTimer <= 0)
            {
                currentState = EnemyState.Attack; // 공격 범위 내 -> 공격 상태
            }
            else
            {
                currentState = EnemyState.Run; // 감지 범위 내 -> 추적
            }
        }
        else
        {
            isPatrolling = true; // 플레이어 미감지 -> 순찰 재개
            currentState = EnemyState.Run;
        }
    }

    // 바닥 감지 함수
    private void CheckGround()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, 0.1f, groundLayer);
    }

    // 대기 상태 처리
    private void HandleIdle()
    {
        // 대기 시간 감소
        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0)
        {
            currentState = EnemyState.Run; // 대기 종료 -> 이동
            Flip(); // 방향 전환
        }
    }

    // 이동 상태 처리 (순찰 또는 추적)
    private void HandleRun()
    {
        if (!isGrounded) return; // 바닥이 없으면 이동 중지

        if (isPatrolling)
        {
            // 순찰 로직
            float moveDir = facingRight ? 1f : -1f;
            rb.velocity = new Vector2(moveDir * moveSpeed, rb.velocity.y);

            // 순찰 범위 초과 시 방향 전환
            if (Mathf.Abs(transform.position.x - startPos.x) >= patrolDistance)
            {
                currentState = EnemyState.Idle;
                waitTimer = patrolWaitTime;
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }
        else if (player != null)
        {
            // 추적 로직
            Vector2 direction = (player.position - transform.position).normalized;
            rb.velocity = new Vector2(direction.x * moveSpeed, rb.velocity.y);

            // 플레이어 방향으로 회전
            if (direction.x > 0 && !facingRight)
                Flip();
            else if (direction.x < 0 && facingRight)
                Flip();
        }
    }

    // 공격 상태 처리
    private void HandleAttack()
    {
        // 이동 중지
        rb.velocity = new Vector2(0, rb.velocity.y);

        // 공격 애니메이션 트리거
        animator.SetTrigger("Attack");

        // 공격 쿨타임 설정
        attackTimer = attackCooldown;
    }

    // 공격 애니메이션 이벤트로 호출되는 함수
    public void PerformAttack()
    {
        if (player == null) return;

        // 플레이어와의 거리 확인
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            // 플레이어의 PlayerHealth 컴포넌트 가져오기
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage); // 데미지 적용
            }

            // 공격 사운드 재생
            if (attackSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackSound);
            }
        }
    }

    // 데미지 처리 메서드 (검, 창, 철퇴 공격에 대응)
    public void TakeDamageSword(int amount, int attackType)
    {
        if (isDead) return;

        // 데미지 계산 (ScarecrowHealth.cs와 동일한 로직)
        float damageMultiplier = Random.Range(0.8f, 1.2f);
        int finalDamage;

        if (attackType == 1) // Attack1
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) / 2f);
        }
        else // Attack2
        {
            finalDamage = Mathf.RoundToInt((amount - defense) * damageMultiplier);
        }

        finalDamage = Mathf.Max(finalDamage, 1); // 최소 데미지 1 보장
        ApplyDamage(finalDamage);
    }

    public void TakeDamageMace(int amount, int attackType)
    {
        if (isDead) return;

        float damageMultiplier = Random.Range(0.8f, 1.2f);
        int finalDamage;

        if (attackType == 1) // Attack1
        {
            finalDamage = Mathf.RoundToInt((amount - defense) * damageMultiplier);
        }
        else // Attack2
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) * 1.1f);
        }

        finalDamage = Mathf.Max(finalDamage, 1);
        ApplyDamage(finalDamage);
    }

    public void TakeDamageLance(int amount, int attackType)
    {
        if (isDead) return;

        float damageMultiplier = Random.Range(0.8f, 1.2f);
        int finalDamage;

        if (attackType == 1) // Attack1
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) / 2f);
        }
        else // Attack2
        {
            finalDamage = Mathf.RoundToInt(((amount - defense) * damageMultiplier) / 3f);
        }

        finalDamage = Mathf.Max(finalDamage, 1);
        ApplyDamage(finalDamage);
    }

    // 공통 데미지 적용 로직
    private void ApplyDamage(int finalDamage)
    {
        currentHealth -= finalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // 피격 사운드 재생
        if (hitSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(hitSound);
        }

        // 데미지 텍스트 생성
        if (damageTextPrefab != null)
        {
            Vector3 spawnPosition = transform.position + damageTextOffset;
            GameObject damageTextObj = Instantiate(damageTextPrefab, spawnPosition, Quaternion.identity, GameObject.Find("DamageTextCanvas").transform);
            DamageText damageText = damageTextObj.GetComponent<DamageText>();
            if (damageText != null)
                damageText.SetDamage(finalDamage);
        }

        // 피격 애니메이션 재생
        animator.SetTrigger("Hurt");
        currentState = EnemyState.Hurt;

        // 체력이 0 이하일 경우 사망
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // 피격 상태에서 복구
            Invoke(nameof(RecoverFromHit), 0.3f);
        }
    }

    // 피격 상태에서 복구
    private void RecoverFromHit()
    {
        if (!isDead)
        {
            currentState = EnemyState.Idle;
        }
    }

    // 사망 처리
    private void Die()
    {
        isDead = true;
        currentState = EnemyState.Die;
        rb.velocity = Vector2.zero; // 이동 중지
        animator.SetTrigger("Die");

        // 사망 사운드 재생
        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        // 사망 애니메이션 재생 후 오브젝트 파괴
        StartCoroutine(DestroyAfterAnimation());
    }

    // 사망 애니메이션 후 오브젝트 파괴
    private IEnumerator DestroyAfterAnimation()
    {
        yield return new WaitForSeconds(1f); // 사망 애니메이션 시간
        Debug.Log("적이 쓰러졌습니다.");
        Destroy(gameObject);
    }

    // 애니메이터 파라미터 업데이트
    private void UpdateAnimator()
    {
        animator.SetBool("isWalking", currentState == EnemyState.Run);
    }

    // 캐릭터 좌우 반전
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    // 디버깅용: 감지/공격 범위 시각화
    private void OnDrawGizmosSelected()
    {
        // 감지 범위
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 공격 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // 바닥 감지
        if (groundCheck != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}