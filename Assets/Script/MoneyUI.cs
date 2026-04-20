using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;

    [Header("Pointer")]
    public TextMeshProUGUI pointerText;
    public string pointerSymbol = "+";
    public Vector2 pointerSize = new Vector2(32f, 32f);
    public Color pointerColor = new Color(1f, 1f, 1f, 0.92f);
    public float pointerFontSize = 30f;

    void Awake()
    {
        EnsurePointer();
    }

    void Update()
    {
        if (GameManager.Instance != null && moneyText != null)
        {
            moneyText.text = "Money: $" + GameManager.Instance.money.ToString("F2");
        }
    }

    void EnsurePointer()
    {
        if (pointerText != null || moneyText == null)
        {
            return;
        }

        Canvas canvas = moneyText.canvas;
        if (canvas == null)
        {
            return;
        }

        GameObject pointerObject = new GameObject("Center Pointer");
        pointerObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = pointerObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = pointerSize;

        pointerText = pointerObject.AddComponent<TextMeshProUGUI>();
        pointerText.text = pointerSymbol;
        pointerText.font = moneyText.font;
        pointerText.fontSize = pointerFontSize;
        pointerText.color = pointerColor;
        pointerText.alignment = TextAlignmentOptions.Center;
        pointerText.raycastTarget = false;
    }
}
