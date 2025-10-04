// PlayerSwimming.cs (수정된 버전)

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerSwimming : MonoBehaviour
{
    [Header("헤엄 설정")]
    public float swimForce = 4f; // 좌우 이동 힘
    public float ascendForce = 5f; // K키를 눌렀을 때 위로 솟구치는 힘 (기존 buoyancy 변수를 대체)
    public float waterDrag = 2f; // 물 저항

    private Rigidbody2D rb;
    private float originalGravityScale;
    private Vector2 moveDirection; // Update에서 계산된 이동 방향
    private bool ascendInput; // Update에서 감지된 상승 입력

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // 이 컴포넌트가 활성화될 때 (수영 시작)
    void OnEnable()
    {
        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0.5f; // 중력 약화
        rb.linearDamping = waterDrag; // Rigidbody2D의 선형 저항(drag)을 사용
    }

    // 이 컴포넌트가 비활성화될 때 (수영 종료)
    void OnDisable()
    {
        rb.gravityScale = originalGravityScale;
        rb.linearDamping = 0f; // 기본 저항으로 복원
    }

    // 입력 처리는 매 프레임 실행되는 Update에서 담당합니다.
    void Update()
    {
        // 좌우 이동 입력
        float moveX = Input.GetAxis("Horizontal");
        // 상하 이동 입력 (K키와 별개로, 위아래 방향키로도 움직일 수 있게)
        float moveY = Input.GetAxis("Vertical");
        moveDirection = new Vector2(moveX, moveY).normalized;

        // K 키를 '눌렀을 때' 상승 입력을 감지합니다.
        if (Input.GetKeyDown(KeyCode.K))
        {
            ascendInput = true;
        }
    }

    // 물리 계산은 고정된 시간 간격으로 실행되는 FixedUpdate에서 담당합니다.
    void FixedUpdate()
    {
        // 좌우 및 상하 방향키 이동 적용
        rb.AddForce(moveDirection * swimForce);

        // Update에서 K 키 입력이 감지되었다면, 위로 힘을 가합니다.
        if (ascendInput)
        {
            // 위쪽 방향으로 순간적인 힘(Impulse)을 가해 솟구치는 느낌을 줍니다.
            rb.AddForce(Vector2.up * ascendForce, ForceMode2D.Impulse);

            // 힘을 한번만 적용하기 위해 바로 false로 바꿔줍니다.
            ascendInput = false;
        }
    }
}