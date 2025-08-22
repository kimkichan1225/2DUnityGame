using UnityEngine;

// 골드 드롭 기능을 관리하는 클래스
public class ItemDrop : MonoBehaviour
{
    [Header("골드 드롭 설정")]
    [SerializeField] private int baseGoldDrop = 10; // 기본 골드 드롭 양
    [SerializeField] [Range(0, 1)] private float goldDropRange = 0.2f; // 골드 드롭 양의 변동 범위 (+-)

    private PlayerStats playerStats;

    private void Start()
    {
        // 플레이어 스탯을 미리 찾아두어 성능 최적화
        playerStats = FindObjectOfType<PlayerStats>();
    }

    // 골드 드롭 실행
    public void GenerateDrops()
    {
        DropGold();
    }

    private void DropGold()
    {
        if (baseGoldDrop <= 0) return;

        // 골드 드롭 양 계산 (기본값 +- 20%)
        int minGold = Mathf.RoundToInt(baseGoldDrop * (1 - goldDropRange));
        int maxGold = Mathf.RoundToInt(baseGoldDrop * (1 + goldDropRange));
        int amount = Random.Range(minGold, maxGold + 1);

        if (amount > 0 && playerStats != null)
        {
            playerStats.AddMoney(amount);
        }
    }
}
