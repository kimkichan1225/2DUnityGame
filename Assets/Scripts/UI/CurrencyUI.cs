using UnityEngine;
using UnityEngine.UI;

public class CurrencyUI : MonoBehaviour
{
    public Text moneyText;

    private void OnEnable()
    {
        // PlayerStats의 이벤트에 구독
        PlayerStats.onMoneyChanged += UpdateMoneyText;
    }

    private void OnDisable()
    {
        // 오브젝트가 비활성화될 때 이벤트 구독 해제
        PlayerStats.onMoneyChanged -= UpdateMoneyText;
    }

    private void UpdateMoneyText(int currentMoney)
    {
        if (moneyText != null)
        {
            moneyText.text = "Gold: " + currentMoney.ToString();
        }
    }
}
