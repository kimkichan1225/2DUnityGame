using UnityEngine;

public class LanceAttack : MonoBehaviour
{
    public BoxCollider2D lanceHitbox; // 랜스스의 충돌 감지 영역 (인스펙터에서 할당)
    private PlayerController playerController; // 플레이어 상태 및 무기 스탯 참조
    private Animator animator;                // 애니메이션 상태 확인을 위한 Animator
    private Lance lance;                       // Lance 컴포넌트 참조

    // 초기화
    private void Start()
    {
        // 필요한 컴포넌트 가져오기
        playerController = GetComponent<PlayerController>();
        lance = GetComponent<Lance>();
        animator = GetComponent<Animator>(); // Animator 컴포넌트 가져오기
        if (lanceHitbox != null)
            lanceHitbox.enabled = false; // 초기에는 히트박스 비활성화
    }

    // 공격 애니메이션 이벤트: 히트박스 활성화
    public void EnableLanceHitbox()
    {
        if (lanceHitbox != null)
            lanceHitbox.enabled = true; // 히트박스 활성화
    }

    // 공격 애니메이션 이벤트: 히트박스 비활성화
    public void DisableLanceHitbox()
    {
        if (lanceHitbox != null)
            lanceHitbox.enabled = false; // 히트박스 비활성화
    }

    // 히트박스가 적과 충돌했을 때 호출
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 히트박스가 활성화된 상태이고, 충돌한 오브젝트가 적("Enemy" 태그)인 경우
        if (lanceHitbox != null && lanceHitbox.enabled && other.CompareTag("Enemy"))
        {
            // 적의 체력 컴포넌트 가져오기
            var enemyHealth = other.GetComponent<MonsterHealth>();
            if (enemyHealth != null && playerController != null && playerController.currentWeaponStats != null)
            {
                // 기본 공격력 가져오기
                int attackPower = playerController.currentWeaponStats.attackPower;

                // 공격 타입 결정
                int attackType;
                if (playerController.IsDashing())
                {
                    // 대시 중일 때는 대시 데미지로 처리 (attackType 1과 유사)
                    attackType = 1;
                    // 대시 데미지는 공격력의 100%로 설정
                    attackPower = Mathf.RoundToInt(attackPower * 1f);
                }
                else
                {
                    // 일반 공격은 애니메이션 상태에 따라 결정
                    attackType = IsPlayingAttack2Animation() ? 2 : 1;
                }

                // 적에게 데미지 적용 (attackType 전달)
                enemyHealth.TakeDamageLance(attackPower, attackType);
            }
        }
    }

    // Attack2_Lance 애니메이션이 재생 중인지 확인
    private bool IsPlayingAttack2Animation()
    {
        if (animator != null)
        {
            // 현재 애니메이션 상태 가져오기
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            // 애니메이션 이름이 "Attack2_Lance"인지 확인
            return stateInfo.IsName("Attack2_Lance");
        }
        return false;
    }
}