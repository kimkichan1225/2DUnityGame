using UnityEngine;
using System.Collections; // ★ 코루틴 사용을 위해 추가

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSwimming : MonoBehaviour
{
    [Header("헤엄 설정")]
    public float swimSpeed = 4f;
    public float ascendForce = 5.5f;
    public float waterDrag = 1.5f;
    public float gravityScaleInWater = 1f;

    // --- ▼▼▼▼▼ 1. (★추가됨★) 공격/피격 시 속도 설정 ▼▼▼▼▼ ---
    [Header("공격/피격 시 속도")]
    [Tooltip("공격 모션 중에 적용될 이동 속도")]
    public float attackMoveSpeed = 0.5f; // 공격 중 속도 (매우 느리게)
    [Tooltip("적과 닿았을 때 속도 감소 비율 (예: 0.5 = 50% 감소)")]
    public float speedReductionFactor = 0.5f;
    [Tooltip("적과 닿은 후 속도가 느려져 있는 시간")]
    public float speedReductionDuration = 0.5f;
    private bool isSlowed = false; // 현재 감속 상태인지
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

    [Header("벽 감지 설정")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public LayerMask wallLayer;

    // --- ▼▼▼▼▼ 수정된 부분 (점프 카운트 관련) ▼▼▼▼▼ ---
    [Header("상승 제한")]
    [SerializeField] private int maxAscendCount = 5; // 물 속에서 연속 상승 가능한 횟수 (5회로 변경)
    private int currentAscendCount = 0;
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

    // --- ▼▼▼▼▼ 추가된 부분 (땅 감지 관련) ▼▼▼▼▼ ---
    [Header("땅 감지 설정")]
    [SerializeField] private Transform groundCheck; // 플레이어 발밑 위치 (빈 오브젝트 연결)
    [SerializeField] private float groundCheckRadius = 0.2f; // 땅 감지 범위
    [SerializeField] private LayerMask groundLayer; // 땅으로 인식할 레이어
    private bool isGrounded = false; // 현재 땅에 닿아있는지 여부
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

    private Rigidbody2D rb;
    private float originalGravityScale;
    private float moveInput;

    private PhysicsMaterial2D noFrictionMaterial;
    private PhysicsMaterial2D originalMaterial;
    private PlayerController playerController;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerController = GetComponent<PlayerController>();

        if (wallCheck == null) wallCheck = transform.Find("WallCheck");
        // --- ▼▼▼▼▼ 추가된 부분 (GroundCheck 자동 찾기) ▼▼▼▼▼ ---
        if (groundCheck == null) groundCheck = transform.Find("GroundCheck");
        // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

        noFrictionMaterial = new PhysicsMaterial2D("NoFriction_Swim");
        noFrictionMaterial.friction = 0f;
        noFrictionMaterial.bounciness = 0f;
    }

    void OnEnable()
    {
        originalGravityScale = rb.gravityScale;
        rb.gravityScale = gravityScaleInWater;
        rb.linearDamping = waterDrag;
        originalMaterial = rb.sharedMaterial;
        rb.sharedMaterial = noFrictionMaterial;
        currentAscendCount = 0; // 활성화 시 카운트 초기화
        isSlowed = false; // 감속 상태 초기화
    }

    void OnDisable()
    {
        rb.gravityScale = originalGravityScale;
        rb.linearDamping = 0f;
        rb.sharedMaterial = originalMaterial;
    }

    void Update()
    {
        // PlayerController가 비활성화(canMove=false) 상태면 입력 무시
        if (playerController != null && !playerController.canMove)
        {
            moveInput = 0;
            return;
        }

        moveInput = Input.GetAxisRaw("Horizontal");

        // 'K'키를 누르고, 아직 최대 상승 횟수에 도달하지 않았을 때
        if (Input.GetKeyDown(KeyCode.K) && currentAscendCount < maxAscendCount)
        {
            currentAscendCount++;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.8f);
            rb.AddForce(Vector2.up * ascendForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        if (playerController != null && !playerController.canMove)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        if (playerController != null && playerController.IsDashing())
        {
            return;
        }

        // --- ▼▼▼▼▼ 추가된 부분 (땅 감지 로직) ▼▼▼▼▼ ---
        // 매 프레임 땅에 닿아있는지 확인
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

        bool isTouchingWall = false;
        if (wallCheck != null)
        {
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        }

        // --- ▼▼▼▼▼ 2. (★수정됨★) 공격 중/감속 중 속도 계산 ▼▼▼▼▼ ---
        float currentMoveSpeed;

        if (playerController != null && playerController.isAttacking)
        {
            // "공격할때는 방향키를 눌러도 매우느리게 이동"
            currentMoveSpeed = attackMoveSpeed;
        }
        else if (isSlowed)
        {
            // "몬스터와 닿아있을때는 이동속도 느리게"
            currentMoveSpeed = swimSpeed * speedReductionFactor;
        }
        else
        {
            // 평상시 수영 속도
            currentMoveSpeed = swimSpeed;
        }
        // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

        if (isTouchingWall && ((moveInput > 0 && transform.localScale.x > 0) || (moveInput < 0 && transform.localScale.x < 0)))
        {
            currentMoveSpeed = 0;
        }

        rb.linearVelocity = new Vector2(moveInput * currentMoveSpeed, rb.linearVelocity.y);

        // --- ▼▼▼▼▼ 수정된 부분 (상승 횟수 초기화 조건 변경) ▼▼▼▼▼ ---
        // 플레이어가 땅(Ground)에 착지하면 상승 횟수를 초기화합니다.
        if (isGrounded)
        {
            currentAscendCount = 0;
        }
        // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---
    }

    // --- ▼▼▼▼▼ 3. (★추가됨★) 적 감지 로직 ▼▼▼▼▼ ---
    // (PlayerController의 OnTriggerEnter2D와 동일한 기능)
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 몬스터와 닿았고, 현재 감속 상태가 아니라면 감속 코루틴 시작
        if (other.CompareTag("Enemy") && !isSlowed)
        {
            StartCoroutine(HandleEnemyCollision());
        }
    }

    private IEnumerator HandleEnemyCollision()
    {
        isSlowed = true; // 감속 시작
        yield return new WaitForSeconds(speedReductionDuration); // 정해진 시간만큼 대기
        isSlowed = false; // 감속 해제
    }
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

    // --- ▼▼▼▼▼ 추가된 부분 (땅 감지 범위 시각화) ▼▼▼▼▼ ---
    // Scene 뷰에서 땅 감지 범위를 원으로 표시 (디버깅용)
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---
}