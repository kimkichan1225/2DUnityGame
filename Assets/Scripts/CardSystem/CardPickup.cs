using UnityEngine;

/// <summary>
/// 카드 아이템 픽업 (공격 또는 방어 카드)
/// </summary>
public class CardPickup : MonoBehaviour
{
    [Header("카드 타입")]
    public DiceType cardType = DiceType.Attack; // Attack 또는 Defense

    [Header("일러스트 (옵션)")]
    public Sprite[] normalArtworks; // [0]: Attack, [1]: Defense
    public Sprite[] highCostArtworks; // [0]: Attack 4코스트, [1]: Defense 4코스트

    [Header("UI 설정")]
    public GameObject pickupEffect; // 획득 이펙트 (옵션)
    public AudioClip pickupSound; // 획득 사운드 (옵션)

    [Header("물리 설정")]
    [SerializeField] private LayerMask groundLayer; // 바닥 레이어
    [SerializeField] private float groundCheckDistance = 0.5f; // 바닥 체크 거리
    [SerializeField] private float gravityForce = 9.8f; // 중력 힘
    [SerializeField] private float bounceForce = 3f; // 바운스 힘
    [SerializeField] private float drag = 2f; // 공기 저항

    private bool hasBeenPickedUp = false;
    private Rigidbody2D rb;
    private Vector2 velocity;
    private bool isGrounded = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Rigidbody2D가 있으면 초기 속도 설정
        if (rb != null)
        {
            velocity = rb.linearVelocity;
            // Continuous Collision Detection 설정으로 빠른 통과 방지
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private void FixedUpdate()
    {
        if (rb == null || isGrounded) return;

        // 레이캐스트로 바닥 체크
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, groundCheckDistance, groundLayer);

        if (hit.collider != null)
        {
            // 바닥에 닿았을 때
            float distanceToGround = hit.distance;

            if (distanceToGround < 0.1f && Mathf.Abs(velocity.y) < 0.5f)
            {
                // 완전히 멈춤
                isGrounded = true;
                velocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero;

                // Kinematic으로 변경하여 더 이상 물리 영향을 받지 않도록
                rb.bodyType = RigidbodyType2D.Kinematic;

                // 바닥 위치로 정렬
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
            else if (velocity.y < 0)
            {
                // 바운스 효과
                velocity.y = Mathf.Abs(velocity.y) * 0.3f; // 30% 반발
            }
        }
        else
        {
            // 공중에 있을 때 중력 적용
            velocity.y -= gravityForce * Time.fixedDeltaTime;
        }

        // 수평 속도 감소 (공기 저항)
        velocity.x = Mathf.Lerp(velocity.x, 0, drag * Time.fixedDeltaTime);

        // Rigidbody에 속도 적용
        rb.linearVelocity = velocity;
        velocity = rb.linearVelocity;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasBeenPickedUp) return;

        if (collision.CompareTag("Player"))
        {
            PickupCard(collision.gameObject);
        }
    }

    private void PickupCard(GameObject player)
    {
        hasBeenPickedUp = true;

        // 랜덤 카드 생성
        CombatPage newCard;
        Sprite artwork = null;

        if (cardType == DiceType.Attack)
        {
            newCard = CardGenerator.GenerateAttackCard();
            artwork = CardGenerator.GetArtworkForCard(DiceType.Attack, newCard.lightCost, normalArtworks, highCostArtworks);
        }
        else
        {
            newCard = CardGenerator.GenerateDefenseCard();
            artwork = CardGenerator.GetArtworkForCard(DiceType.Defense, newCard.lightCost, normalArtworks, highCostArtworks);
        }

        newCard.artwork = artwork;

        // 플레이어에게 카드 추가
        CharacterStats playerStats = player.GetComponent<CharacterStats>();
        if (playerStats != null)
        {
            playerStats.AddCardToCollection(newCard);
            Debug.Log($"[CardPickup] 카드 획득: {newCard.pageName} (코스트: {newCard.lightCost}, 주사위: {newCard.diceList.Count}개)");

            // 카드 획득 UI 표시
            ShowCardAcquiredUI(newCard);
        }

        // 이펙트 및 사운드
        if (pickupEffect != null)
        {
            Instantiate(pickupEffect, transform.position, Quaternion.identity);
        }

        if (pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }

        // 아이템 제거
        Destroy(gameObject);
    }

    /// <summary>
    /// 카드 획득 UI 표시 (추후 구현)
    /// </summary>
    private void ShowCardAcquiredUI(CombatPage card)
    {
        // TODO: UI 팝업 표시
        // CardAcquiredUI.Instance?.Show(card);
    }

    // Gizmo로 카드 타입 및 바닥 체크 표시
    private void OnDrawGizmosSelected()
    {
        // 카드 타입 표시
        Gizmos.color = cardType == DiceType.Attack ? Color.red : Color.blue;
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // 바닥 체크 레이 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
}
