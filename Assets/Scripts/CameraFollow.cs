using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player; // 플레이어의 Transform
    public float minX = -5f; // 카메라의 최소 x 값
    public float maxX = 23f; // 카메라의 최대 x 값
    public float defaultMinY = 2f; // 기본 최소 y 값
    public float defaultMaxY = 10f; // 기본 최대 y 값
    public float undergroundMinY = -10f; // 지하 최소 y 값 (필요에 따라 조정)
    public float undergroundMaxY = -2f; // 지하 최대 y 값 (필요에 따라 조정)

    private float currentMinY; // 현재 최소 y 값
    private float currentMaxY; // 현재 최대 y 값

    void Start()
    {
        // 초기 Y 값 설정
        currentMinY = defaultMinY;
        currentMaxY = defaultMaxY;
    }

    void LateUpdate()
    {
        if (player != null)
        {
            // 플레이어의 Y 위치에 따라 카메라 Y 범위 동적 설정
            if (player.position.y < 2f)
            {
                currentMinY = undergroundMinY;
                currentMaxY = undergroundMaxY;
            }
            else
            {
                currentMinY = defaultMinY;
                currentMaxY = defaultMaxY;
            }

            // 플레이어의 현재 위치를 기준으로 카메라 위치 설정
            Vector3 newPosition = transform.position;
            newPosition.x = Mathf.Clamp(player.position.x, minX, maxX);
            newPosition.y = Mathf.Clamp(player.position.y, currentMinY, currentMaxY);
            transform.position = newPosition;
        }
    }
}