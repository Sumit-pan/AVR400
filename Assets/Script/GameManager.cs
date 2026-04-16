using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float money = 0f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddMoney(float amount)
    {
        money += amount;
        Debug.Log("Money: $" + money.ToString("F2"));
    }
}