using TMPro;
using UnityEngine;

public class MoneyUI : MonoBehaviour
{
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI gameCompleteText;
    public TextMeshProUGUI objectiveText;

    [Header("Pointer")]
    public TextMeshProUGUI pointerText;
    public string pointerSymbol = "+";
    public Vector2 pointerSize = new Vector2(32f, 32f);
    public Color pointerColor = new Color(1f, 1f, 1f, 0.92f);
    public float pointerFontSize = 30f;

    void Awake()
    {
        EnsurePointer();
        EnsureGameCompleteText();
        EnsureObjectiveText();
    }

    void Update()
    {
        if (GameManager.Instance != null && moneyText != null)
        {
            moneyText.text = "Money: $" + GameManager.Instance.money.ToString("F2");
        }

        if (gameCompleteText != null)
        {
            bool showComplete = GameManager.Instance != null && GameManager.Instance.IsGameCompleted;
            gameCompleteText.gameObject.SetActive(showComplete);

            if (showComplete)
            {
                gameCompleteText.text = "GAME COMPLETED\nSales Reached $200";
            }
        }

        if (objectiveText != null)
        {
            bool showObjective = GameManager.Instance != null && !GameManager.Instance.IsGameCompleted;
            objectiveText.gameObject.SetActive(showObjective);

            if (showObjective)
            {
                objectiveText.text = "Objective: Earn $200 in sales";
            }
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

    void EnsureGameCompleteText()
    {
        if (gameCompleteText != null || moneyText == null)
        {
            return;
        }

        Canvas canvas = moneyText.canvas;
        if (canvas == null)
        {
            return;
        }

        GameObject completeObject = new GameObject("Game Complete Text");
        completeObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = completeObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = new Vector2(0f, 80f);
        rectTransform.sizeDelta = new Vector2(700f, 180f);

        gameCompleteText = completeObject.AddComponent<TextMeshProUGUI>();
        gameCompleteText.font = moneyText.font;
        gameCompleteText.fontSize = 42f;
        gameCompleteText.color = new Color(1f, 0.95f, 0.45f, 1f);
        gameCompleteText.alignment = TextAlignmentOptions.Center;
        gameCompleteText.text = "GAME COMPLETED\nSales Reached $200";
        gameCompleteText.raycastTarget = false;
        gameCompleteText.gameObject.SetActive(false);
    }

    void EnsureObjectiveText()
    {
        if (objectiveText != null || moneyText == null)
        {
            return;
        }

        Canvas canvas = moneyText.canvas;
        if (canvas == null)
        {
            return;
        }

        GameObject objectiveObject = new GameObject("Objective Text");
        objectiveObject.transform.SetParent(canvas.transform, false);

        RectTransform rectTransform = objectiveObject.AddComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 1f);
        rectTransform.anchorMax = new Vector2(0.5f, 1f);
        rectTransform.pivot = new Vector2(0.5f, 1f);
        rectTransform.anchoredPosition = new Vector2(0f, -20f);
        rectTransform.sizeDelta = new Vector2(500f, 60f);

        objectiveText = objectiveObject.AddComponent<TextMeshProUGUI>();
        objectiveText.font = moneyText.font;
        objectiveText.fontSize = 28f;
        objectiveText.color = new Color(1f, 1f, 1f, 0.95f);
        objectiveText.alignment = TextAlignmentOptions.Center;
        objectiveText.text = "Objective: Earn $200 in sales";
        objectiveText.raycastTarget = false;
    }
}
