using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSwimming : MonoBehaviour
{
    [Header("헤엄 설정")]
    public float swimSpeed = 4f;
    public float ascendForce = 5.5f;
    public float waterDrag = 1.5f;
    public float gravityScaleInWater = 1f;

    [Header("벽 감지 설정")]
    public Transform wallCheck;
    public float wallCheckRadius = 0.1f;
    public LayerMask wallLayer;

    private Rigidbody2D rb;
    private float originalGravityScale;
    private float moveInput;

    private PhysicsMaterial2D noFrictionMaterial;
    private PhysicsMaterial2D originalMaterial;

    // --- ▼▼▼▼▼ 수정된 부분 (PlayerController 참조 추가) ▼▼▼▼▼ ---
    private PlayerController playerController; // PlayerController 참조
    // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        // --- ▼▼▼▼▼ 수정된 부분 (PlayerController 참조 찾기) ▼▼▼▼▼ ---
        // PlayerController 컴포넌트를 찾아서 변수에 저장
        playerController = GetComponent<PlayerController>();
        // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

        if (wallCheck == null)
        {
            wallCheck = transform.Find("WallCheck");
        }

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
    }

    void OnDisable()
    {
        rb.gravityScale = originalGravityScale;
        rb.linearDamping = 0f;
        rb.sharedMaterial = originalMaterial;
    }

    void Update()
    {
        moveInput = Input.GetAxisRaw("Horizontal");

        if (Input.GetKeyDown(KeyCode.K))
        {
            rb.AddForce(Vector2.up * ascendForce, ForceMode2D.Impulse);
        }
    }

    void FixedUpdate()
    {
        // --- ▼▼▼▼▼ 수정된 부분 (대시 중일 때 움직임 제어 방지) ▼▼▼▼▼ ---
        // PlayerController가 존재하고, 현재 대시 중(IsDashing())이라면
        // PlayerSwimming의 움직임 로직을 실행하지 않고 건너뜁니다.
        if (playerController != null && playerController.IsDashing())
        {
            return; // 대시 중에는 수영 이동 로직 실행 안 함
        }
        // --- ▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲▲ ---

        bool isTouchingWall = false;
        if (wallCheck != null)
        {
            isTouchingWall = Physics2D.OverlapCircle(wallCheck.position, wallCheckRadius, wallLayer);
        }

        float currentMoveSpeed = swimSpeed;

        if (isTouchingWall && ((moveInput > 0 && transform.localScale.x > 0) || (moveInput < 0 && transform.localScale.x < 0)))
        {
            currentMoveSpeed = 0;
        }

        // Rigidbody2D의 velocity를 직접 설정합니다. (기존 방식 유지)
        rb.linearVelocity = new Vector2(moveInput * currentMoveSpeed, rb.linearVelocity.y);
    }
}