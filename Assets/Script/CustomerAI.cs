using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    public enum CustomerState
    {
        GoingToShelf,
        GoingToCheckout,
        Leaving
    }

    public ShelfSlot targetShelf;
    public Transform checkoutPoint;
    public Transform exitPoint;
    public CheckoutCounter checkoutCounter;

    private NavMeshAgent agent;
    private CustomerState currentState;
    private float itemPrice;
    private bool actionDone;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent == null)
        {
            Debug.LogError("CustomerAI requires a NavMeshAgent.");
            return;
        }

        if (targetShelf != null && targetShelf.HasItem())
        {
            currentState = CustomerState.GoingToShelf;
            actionDone = false;
            agent.SetDestination(targetShelf.transform.position);
        }
        else
        {
            GoToExit();
        }
    }

    void Update()
    {
        if (agent == null || agent.pathPending) return;

        if (agent.remainingDistance <= agent.stoppingDistance && !actionDone)
        {
            actionDone = true;

            switch (currentState)
            {
                case CustomerState.GoingToShelf:
                    BuyFromShelf();
                    break;

                case CustomerState.GoingToCheckout:
                    PayAtCheckout();
                    break;

                case CustomerState.Leaving:
                    Destroy(gameObject, 1f);
                    break;
            }
        }
    }

    void BuyFromShelf()
    {
        if (targetShelf == null || !targetShelf.HasItem())
        {
            GoToExit();
            return;
        }

        bool success = targetShelf.TryBuyOne(out itemPrice);

        if (!success)
        {
            GoToExit();
            return;
        }

        currentState = CustomerState.GoingToCheckout;
        actionDone = false;

        if (checkoutPoint != null)
            agent.SetDestination(checkoutPoint.position);
        else
            GoToExit();
    }

    void PayAtCheckout()
    {
        if (checkoutCounter != null)
        {
            checkoutCounter.ProcessPayment(itemPrice);
        }

        GoToExit();
    }

    void GoToExit()
    {
        currentState = CustomerState.Leaving;
        actionDone = false;

        if (exitPoint != null)
            agent.SetDestination(exitPoint.position);
        else
            Destroy(gameObject);
    }
}