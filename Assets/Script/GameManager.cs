using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public float money = 0f;
    public bool isStoreOpen = false;

    private OrderStation orderStation;
    private StoreSign storeSign;

    public bool IsStoreOpen => isStoreOpen;

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

    private void Start()
    {
        EnsureOrderStation();
        EnsureStoreSign();
    }

    public void AddMoney(float amount)
    {
        money += amount;
        Debug.Log("Money: $" + money.ToString("F2"));
    }

    public bool TrySpendMoney(float amount)
    {
        float clampedAmount = Mathf.Max(0f, amount);

        if (money < clampedAmount)
        {
            Debug.Log("Not enough money. Need $" + clampedAmount.ToString("F2"));
            return false;
        }

        money -= clampedAmount;
        Debug.Log("Spent $" + clampedAmount.ToString("F2") + ". Money: $" + money.ToString("F2"));
        return true;
    }

    public void ToggleStoreOpen()
    {
        isStoreOpen = !isStoreOpen;
        Debug.Log("Store is now " + (isStoreOpen ? "OPEN" : "CLOSED"));

        if (storeSign != null)
        {
            storeSign.RefreshVisuals();
        }
    }

    void EnsureOrderStation()
    {
        orderStation = FindObjectOfType<OrderStation>();
        if (orderStation != null)
        {
            orderStation.BuildCatalogFromScene();
            return;
        }

        CheckoutCounter checkoutCounter = FindObjectOfType<CheckoutCounter>();
        if (checkoutCounter == null)
        {
            Debug.LogWarning("GameManager: couldn't create order station because no CheckoutCounter was found.");
            return;
        }

        GameObject stationObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        stationObject.name = "OrderStation";
        stationObject.transform.position = checkoutCounter.transform.position + new Vector3(2f, -0.25f, 1.2f);
        stationObject.transform.localScale = new Vector3(1.1f, 1f, 1.1f);
        stationObject.layer = LayerMask.NameToLayer("Interactable");

        Renderer stationRenderer = stationObject.GetComponent<Renderer>();
        if (stationRenderer != null)
        {
            stationRenderer.material.color = new Color(0.18f, 0.28f, 0.38f, 1f);
        }

        orderStation = stationObject.AddComponent<OrderStation>();

        GameObject spawnPointObject = new GameObject("OrderSpawnPoint");
        spawnPointObject.transform.SetParent(stationObject.transform, false);
        spawnPointObject.transform.localPosition = new Vector3(1.25f, 0.65f, 0f);

        orderStation.spawnPoint = spawnPointObject.transform;
        orderStation.BuildCatalogFromScene();
    }

    void EnsureStoreSign()
    {
        storeSign = FindObjectOfType<StoreSign>();
        if (storeSign != null)
        {
            storeSign.RefreshVisuals();
            return;
        }

        CheckoutCounter checkoutCounter = FindObjectOfType<CheckoutCounter>();
        if (checkoutCounter == null)
        {
            Debug.LogWarning("GameManager: couldn't create store sign because no CheckoutCounter was found.");
            return;
        }

        GameObject signObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
        signObject.name = "StoreSign";
        signObject.transform.position = checkoutCounter.transform.position + new Vector3(0f, 1.45f, -2.2f);
        signObject.transform.localScale = new Vector3(1.4f, 1f, 0.12f);
        signObject.layer = LayerMask.NameToLayer("Interactable");

        storeSign = signObject.AddComponent<StoreSign>();
        storeSign.RefreshVisuals();
    }
}

public class StoreSign : MonoBehaviour, IInteractable
{
    private Renderer signRenderer;
    private TextMesh signText;

    void Awake()
    {
        signRenderer = GetComponent<Renderer>();
        EnsureText();
    }

    public void Interact(PlayerInteraction player)
    {
        if (GameManager.Instance == null)
        {
            return;
        }

        GameManager.Instance.ToggleStoreOpen();
    }

    public string GetInteractionText()
    {
        if (GameManager.Instance == null)
        {
            return "Store sign unavailable";
        }

        return GameManager.Instance.IsStoreOpen
            ? "Press E to close the store"
            : "Press E to open the store";
    }

    public void RefreshVisuals()
    {
        if (signRenderer == null)
        {
            signRenderer = GetComponent<Renderer>();
        }

        EnsureText();

        if (signRenderer == null || GameManager.Instance == null)
        {
            return;
        }

        signRenderer.material.color = GameManager.Instance.IsStoreOpen
            ? new Color(0.18f, 0.72f, 0.28f, 1f)
            : new Color(0.78f, 0.18f, 0.18f, 1f);

        if (signText != null)
        {
            signText.text = GameManager.Instance.IsStoreOpen ? "OPEN" : "CLOSED";
            signText.color = Color.white;
        }
    }

    void EnsureText()
    {
        if (signText != null)
        {
            return;
        }

        Transform existing = transform.Find("SignText");
        if (existing != null)
        {
            signText = existing.GetComponent<TextMesh>();
            if (signText != null)
            {
                return;
            }
        }

        GameObject textObject = new GameObject("SignText");
        textObject.transform.SetParent(transform, false);
        textObject.transform.localPosition = new Vector3(0f, 0f, -0.65f);
        textObject.transform.localRotation = Quaternion.identity;

        signText = textObject.AddComponent<TextMesh>();
        signText.anchor = TextAnchor.MiddleCenter;
        signText.alignment = TextAlignment.Center;
        signText.characterSize = 0.22f;
        signText.fontSize = 48;
        signText.text = "OPEN";
    }
}

public class OrderStation : MonoBehaviour, IInteractable
{
    [System.Serializable]
    public class OrderableProduct
    {
        public string productName;
        public float productPrice;
        public Transform originalParent;
        public Vector3 originalLocalPosition;
        public Quaternion originalLocalRotation;
        public Vector3 originalLocalScale;
        public GameObject template;
        public PickupItem activeInstance;
    }

    public Transform spawnPoint;
    public float spawnSpacing = 0.4f;
    public float spawnHeightOffset = 0.15f;
    public float orderCostMultiplier = 0.5f;
    public float minimumOrderCost = 0.5f;
    public OrderableProduct[] orderableProducts;

    private int selectedIndex;
    private int spawnCount;

    public void BuildCatalogFromScene()
    {
        if (orderableProducts != null && orderableProducts.Length > 0)
        {
            return;
        }

        PickupItem[] pickups = FindObjectsOfType<PickupItem>();
        List<OrderableProduct> catalog = new List<OrderableProduct>();
        HashSet<string> seenNames = new HashSet<string>();

        foreach (PickupItem pickup in pickups)
        {
            if (pickup == null)
            {
                continue;
            }

            ProductData data = pickup.GetComponent<ProductData>();
            if (data == null)
            {
                data = pickup.GetComponentInChildren<ProductData>();
            }

            if (data == null || string.IsNullOrEmpty(data.productName) || seenNames.Contains(data.productName))
            {
                continue;
            }

            seenNames.Add(data.productName);

            GameObject template = Instantiate(pickup.gameObject);
            template.name = pickup.gameObject.name + "_Template";
            template.transform.SetParent(transform, false);
            template.SetActive(false);

            catalog.Add(new OrderableProduct
            {
                productName = data.productName,
                productPrice = data.price,
                originalParent = pickup.transform.parent,
                originalLocalPosition = pickup.transform.localPosition,
                originalLocalRotation = pickup.transform.localRotation,
                originalLocalScale = pickup.transform.localScale,
                template = template,
                activeInstance = pickup
            });
        }

        orderableProducts = catalog.ToArray();

        if (orderableProducts.Length == 0)
        {
            Debug.LogWarning("OrderStation: no products found in the scene to use as order templates.");
        }
    }

    public void Interact(PlayerInteraction player)
    {
        OrderCurrentProduct();
    }

    public void CycleSelection()
    {
        if (orderableProducts == null || orderableProducts.Length == 0)
        {
            Debug.Log("OrderStation: no products available to cycle.");
            return;
        }

        selectedIndex = (selectedIndex + 1) % orderableProducts.Length;
        Debug.Log("OrderStation selected: " + GetCurrentProductName());
    }

    public string GetInteractionText()
    {
        if (orderableProducts == null || orderableProducts.Length == 0)
        {
            return "No products available to order";
        }

        return "Press E to order " + GetCurrentProductName() + " | Press R to change item";
    }

    void OrderCurrentProduct()
    {
        if (orderableProducts == null || orderableProducts.Length == 0)
        {
            Debug.Log("OrderStation: no products available to order.");
            return;
        }

        OrderableProduct product = orderableProducts[selectedIndex];
        if (product == null || product.template == null)
        {
            Debug.LogWarning("OrderStation: selected product reference is missing.");
            return;
        }

        if (product.activeInstance != null && product.activeInstance.gameObject.activeInHierarchy)
        {
            Debug.Log(product.productName + " is already available on the floor.");
            return;
        }

        float orderCost = GetCurrentOrderCost();
        if (GameManager.Instance == null || !GameManager.Instance.TrySpendMoney(orderCost))
        {
            return;
        }

        GameObject orderedProduct = Instantiate(product.template);
        orderedProduct.name = product.productName;
        orderedProduct.SetActive(true);
        orderedProduct.transform.SetParent(product.originalParent, false);
        orderedProduct.transform.localPosition = product.originalLocalPosition;
        orderedProduct.transform.localRotation = product.originalLocalRotation;
        orderedProduct.transform.localScale = product.originalLocalScale;

        Rigidbody[] rigidbodies = orderedProduct.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = orderedProduct.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            collider.enabled = true;
        }

        product.activeInstance = orderedProduct.GetComponent<PickupItem>();

        spawnCount++;
        Debug.Log("Reordered item: " + product.productName + " for $" + orderCost.ToString("F2"));
    }

    string GetCurrentProductName()
    {
        if (orderableProducts == null || orderableProducts.Length == 0)
        {
            return "Nothing";
        }

        OrderableProduct product = orderableProducts[selectedIndex];
        if (product == null)
        {
            return "Missing Item";
        }

        return string.IsNullOrEmpty(product.productName) ? "Missing Item" : product.productName;
    }

    float GetCurrentOrderCost()
    {
        if (orderableProducts == null || orderableProducts.Length == 0)
        {
            return minimumOrderCost;
        }

        OrderableProduct product = orderableProducts[selectedIndex];
        if (product == null)
        {
            return minimumOrderCost;
        }

        return Mathf.Max(minimumOrderCost, product.productPrice * orderCostMultiplier);
    }
}
