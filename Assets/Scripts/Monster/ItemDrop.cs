using UnityEngine;

// 아이템 드롭 기능을 관리하는 클래스
public class ItemDrop : MonoBehaviour
{
    [Header("드롭 설정")]
    [SerializeField] private GameObject[] possibleDrops; // 드롭 가능한 아이템 프리팹 배열
    [SerializeField] private float dropChance = 0.5f;   // 아이템 드롭 확률 (0~1)
    [SerializeField] private Vector3 dropOffset = new Vector3(0, 0.5f, 0); // 드롭 위치 오프셋

    // 아이템 드롭 실행
    public void DropItem()
    {
        if (possibleDrops == null || possibleDrops.Length == 0) return;

        // 드롭 확률 체크
        if (Random.value <= dropChance)
        {
            // 랜덤한 아이템 선택
            int randomIndex = Random.Range(0, possibleDrops.Length);
            // 아이템 생성
            Instantiate(possibleDrops[randomIndex], transform.position + dropOffset, Quaternion.identity);
            Debug.Log("아이템이 드롭되었습니다.");
        }
    }
}