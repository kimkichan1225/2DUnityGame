using UnityEngine;

/// <summary>
/// 영구적으로 스탯을 증가시키는 아이템 데이터
/// </summary>
[CreateAssetMenu(fileName = "New StatBoostItem", menuName = "Inventory/Stat Boost Item")]
public class StatBoostItemData : ItemData
{
    [Header("스탯 증가 타입")]
    public StatBoostType boostType;

    [Header("증가량")]
    public int attackPowerBoost = 0;
    public int defenseBoost = 0;
    public int maxHealthBoost = 0;
    public float moveSpeedBoost = 0f;

    [Header("사용 효과")]
    public AudioClip useSound;
    public GameObject useEffectPrefab; // 사용 시 이펙트 (선택사항)

    public override void Use()
    {
        base.Use();

        // PlayerStats와 PlayerHealth 찾기
        PlayerStats playerStats = FindFirstObjectByType<PlayerStats>();
        PlayerHealth playerHealth = FindFirstObjectByType<PlayerHealth>();
        PlayerController playerController = FindFirstObjectByType<PlayerController>();

        if (playerStats == null || playerHealth == null)
        {
            Debug.LogError("PlayerStats 또는 PlayerHealth를 찾을 수 없습니다!");
            return;
        }

        // 스탯 타입에 따라 처리
        switch (boostType)
        {
            case StatBoostType.AttackPower:
                playerStats.bonusAttackPower += attackPowerBoost;
                Debug.Log($"공격력이 영구적으로 {attackPowerBoost} 증가했습니다! (현재: {playerStats.bonusAttackPower})");
                break;

            case StatBoostType.Defense:
                playerHealth.defense += defenseBoost;
                Debug.Log($"방어력이 영구적으로 {defenseBoost} 증가했습니다! (현재: {playerHealth.defense})");
                break;

            case StatBoostType.MaxHealth:
                playerHealth.AddPermanentHealth(maxHealthBoost);
                Debug.Log($"최대 체력이 영구적으로 {maxHealthBoost} 증가했습니다!");
                break;

            case StatBoostType.MoveSpeed:
                playerStats.bonusMoveSpeed += moveSpeedBoost;
                Debug.Log($"이동 속도가 영구적으로 {moveSpeedBoost} 증가했습니다! (현재: {playerStats.bonusMoveSpeed})");
                break;

            case StatBoostType.AllStats:
                // 모든 스탯 조금씩 증가
                playerStats.bonusAttackPower += attackPowerBoost;
                playerHealth.defense += defenseBoost;
                playerHealth.AddPermanentHealth(maxHealthBoost);
                playerStats.bonusMoveSpeed += moveSpeedBoost;
                Debug.Log("모든 스탯이 영구적으로 증가했습니다!");
                break;
        }

        // 스탯 재계산 및 UI 업데이트
        if (playerController != null)
        {
            playerController.RecalculateStats();
            playerController.UpdateAllStatsUI();
        }

        // 사운드 재생
        if (useSound != null)
        {
            AudioSource audioSource = playerStats.GetComponent<AudioSource>();
            if (audioSource != null)
            {
                audioSource.PlayOneShot(useSound);
            }
        }

        // 이펙트 생성 (선택사항)
        if (useEffectPrefab != null)
        {
            GameObject effect = Instantiate(useEffectPrefab, playerStats.transform.position, Quaternion.identity);
            Destroy(effect, 2f);
        }

        // 인벤토리에서 아이템 제거
        Inventory.instance.Remove(this);
    }
}

/// <summary>
/// 스탯 부스트 타입 열거형
/// </summary>
public enum StatBoostType
{
    AttackPower,  // 공격력
    Defense,      // 방어력
    MaxHealth,    // 최대 체력
    MoveSpeed,    // 이동 속도
    AllStats      // 모든 스탯
}
