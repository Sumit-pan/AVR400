using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    void Update()
    {
        if (GameManager.Instance != null && moneyText != null)
        {
            moneyText.text = "Money: $" + GameManager.Instance.money.ToString("F2");
        }
    }
}