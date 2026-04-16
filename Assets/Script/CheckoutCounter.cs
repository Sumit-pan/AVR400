using UnityEngine;

public class CheckoutCounter : MonoBehaviour
{
    public void ProcessPayment(float amount)
    {
        GameManager.Instance.AddMoney(amount);
        Debug.Log("Customer paid $" + amount);
    }
}