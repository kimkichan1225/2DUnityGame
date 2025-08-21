using UnityEngine;

// 이 스크립트는 씬의 카메라 경계 값을 저장하는 데이터 컨테이너 역할을 합니다.
public class CameraBounds : MonoBehaviour
{
    public float minX = -5f;
    public float maxX = 23f;
    public float defaultMinY = 2f;
    public float defaultMaxY = 10f;
    public float undergroundMinY = -10f;
    public float undergroundMaxY = -2f;
}
