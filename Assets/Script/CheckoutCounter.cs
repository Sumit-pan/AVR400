using UnityEngine;

public class CheckoutCounter : MonoBehaviour
{
    [Header("Money Drop")]
    public Transform dropPoint;
    public Vector3 dropOffset = new Vector3(0.75f, 0.15f, 0f);
    public Vector3 dropScale = new Vector3(0.16f, 0.06f, 0.16f);
    public Color moneyColor = new Color(0.22f, 0.7f, 0.28f, 1f);
    public float tossForce = 0f;

    public void ProcessPayment(float amount)
    {
        SpawnMoneyPickup(amount);
        Debug.Log("Customer left $" + amount.ToString("F2") + " at checkout.");
    }

    void SpawnMoneyPickup(float amount)
    {
        if (amount <= 0f)
        {
            return;
        }

        GameObject moneyObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        moneyObject.name = "MoneyPickup";

        Transform spawnTransform = dropPoint != null ? dropPoint : transform;
        moneyObject.transform.position = spawnTransform.position + dropOffset;
        moneyObject.transform.rotation = Quaternion.identity;
        moneyObject.transform.localScale = dropScale;
        moneyObject.layer = LayerMask.NameToLayer("Interactable");

        Rigidbody rb = moneyObject.AddComponent<Rigidbody>();
        rb.mass = 0.2f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.drag = 6f;
        rb.angularDrag = 10f;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        if (tossForce > 0f)
        {
            rb.AddForce((transform.up + transform.forward * 0.2f) * tossForce, ForceMode.Impulse);
        }

        MoneyPickup pickup = moneyObject.AddComponent<MoneyPickup>();
        pickup.Initialize(amount);

        Renderer moneyRenderer = moneyObject.GetComponent<Renderer>();
        if (moneyRenderer != null)
        {
            moneyRenderer.material.color = moneyColor;
        }
    }
}

public class MoneyPickup : MonoBehaviour, IInteractable
{
    [SerializeField] private float amount = 0f;

    private Rigidbody rb;
    private bool isSettled;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(float value)
    {
        amount = Mathf.Max(0f, value);
        name = "MoneyPickup_$" + amount.ToString("F2");
    }

    void OnCollisionEnter(Collision collision)
    {
        if (isSettled || rb == null || collision.collider.isTrigger)
        {
            return;
        }

        SettleInPlace();
    }

    void SettleInPlace()
    {
        isSettled = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeAll;
    }

    public void Interact(PlayerInteraction player)
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("MoneyPickup: GameManager instance is missing.");
            return;
        }

        GameManager.Instance.AddMoney(amount);
        Debug.Log("Picked up $" + amount.ToString("F2"));
        Destroy(gameObject);
    }

    public string GetInteractionText()
    {
        return "Press E to collect $" + amount.ToString("F2");
    }
}
