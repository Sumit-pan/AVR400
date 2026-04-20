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
    public string idleAnimationState = "Idle_Breathing";
    public string walkAnimationState = "Walk_Forward";
    public float visualBobAmount = 0.14f;
    public float visualBobSpeed = 9f;
    public float visualLeanAmount = 10f;
    public float visualAnimationSmooth = 8f;

    private NavMeshAgent agent;
    private Animator animator;
    private CustomerState currentState;
    private float itemPrice;
    private bool actionDone;
    private string currentAnimationState = "";
    private Transform visualRoot;
    private Vector3 visualBaseLocalPosition;
    private Quaternion visualBaseLocalRotation;
    private float visualAnimationTime;
    private bool useAnimator;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        visualRoot = transform.childCount > 0 ? transform.GetChild(0) : null;

        if (visualRoot != null)
        {
            visualBaseLocalPosition = visualRoot.localPosition;
            visualBaseLocalRotation = visualRoot.localRotation;
        }

        if (agent == null)
        {
            Debug.LogError("CustomerAI requires a NavMeshAgent.");
            return;
        }

        useAnimator = animator != null && animator.avatar != null && animator.avatar.isValid && animator.avatar.isHuman;

        if (animator != null && !useAnimator)
        {
            animator.enabled = false;
        }

        if (useAnimator)
        {
            PlayAnimation(idleAnimationState);
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
        UpdateAnimation();

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

    void UpdateAnimation()
    {
        if (agent == null)
        {
            return;
        }

        bool isMoving = agent.velocity.sqrMagnitude > 0.01f
            && (!agent.isStopped && agent.remainingDistance > agent.stoppingDistance);

        if (useAnimator)
        {
            PlayAnimation(isMoving ? walkAnimationState : idleAnimationState);
        }
        else
        {
            UpdateProceduralVisuals(isMoving);
        }
    }

    void PlayAnimation(string stateName)
    {
        if (animator == null || string.IsNullOrEmpty(stateName) || currentAnimationState == stateName)
        {
            return;
        }

        animator.CrossFade(stateName, 0.15f);
        currentAnimationState = stateName;
    }

    void UpdateProceduralVisuals(bool isMoving)
    {
        if (visualRoot == null)
        {
            return;
        }

        visualAnimationTime += Time.deltaTime * (isMoving ? visualBobSpeed : 2f);

        Vector3 localVelocity = transform.InverseTransformDirection(agent.velocity);
        float bobOffset = isMoving ? Mathf.Abs(Mathf.Sin(visualAnimationTime)) * visualBobAmount : 0f;
        float lean = isMoving ? -Mathf.Clamp(localVelocity.x, -1f, 1f) * visualLeanAmount : 0f;
        float bouncePitch = isMoving ? Mathf.Sin(visualAnimationTime * 2f) * 4f : 0f;

        Vector3 targetLocalPosition = visualBaseLocalPosition + new Vector3(0f, bobOffset, 0f);
        Quaternion targetLocalRotation = visualBaseLocalRotation * Quaternion.Euler(bouncePitch, 0f, lean);

        visualRoot.localPosition = Vector3.Lerp(visualRoot.localPosition, targetLocalPosition, Time.deltaTime * visualAnimationSmooth);
        visualRoot.localRotation = Quaternion.Slerp(visualRoot.localRotation, targetLocalRotation, Time.deltaTime * visualAnimationSmooth);
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
