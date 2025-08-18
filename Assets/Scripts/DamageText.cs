using UnityEngine;
using TMPro;

public class DamageText : MonoBehaviour
{
    public float floatSpeed = 1f; // 텍스트가 떠오르는 속도 (월드 단위/초)
    public float lifetime = 1f;   // 텍스트가 유지되는 시간 (초)
    public TextMeshProUGUI damageText; // TextMeshProUGUI 컴포넌트

    private float elapsedTime = 0f; // 경과 시간 추적

    private void Start()
    {
        Destroy(gameObject, lifetime); // lifetime 초 후 오브젝트 파괴
    }

    private void Update()
    {
        // 텍스트를 월드 공간에서 위로 이동
        transform.Translate(Vector3.up * floatSpeed * Time.deltaTime, Space.World);

        // 경과 시간 업데이트
        elapsedTime += Time.deltaTime;

        // 알파값을 점진적으로 감소 (1 -> 0)
        if (damageText != null)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / lifetime);
            Color textColor = damageText.color;
            textColor.a = alpha;
            damageText.color = textColor;
        }
    }

    public void SetDamage(int damage)
    {
        if (damageText != null)
            damageText.text = damage.ToString();
    }
}