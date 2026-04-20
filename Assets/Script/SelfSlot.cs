using UnityEngine;

public class ShelfSlot : MonoBehaviour, IInteractable
{
    public Transform placePoint;

    [Header("Starting Stock")]
    public GameObject startingProductPrefab;
    public int startingStock = 5;

    [Header("Shelf Stock")]
    public int currentStock = 0;
    public int maxStock = 10;
    public int stockAddedPerItem = 5;

    [SerializeField] private string storedProductName = "";
    [SerializeField] private float storedProductPrice = 0f;
    [SerializeField] private GameObject displayItemInstance;

    void Start()
    {
        if (startingProductPrefab != null)
        {
            InitializeShelf(startingProductPrefab, startingStock);
        }
    }

    public void InitializeShelf(GameObject productPrefab, int stockAmount)
    {
        if (productPrefab == null)
        {
            Debug.LogWarning(name + ": InitializeShelf called with null prefab.");
            return;
        }

        ProductData data = productPrefab.GetComponent<ProductData>();

        if (data == null)
        {
            data = productPrefab.GetComponentInChildren<ProductData>();
        }

        if (data == null)
        {
            Debug.LogWarning(name + ": Product prefab has no ProductData.");
            return;
        }

        storedProductName = data.productName;
        storedProductPrice = data.price;
        currentStock = Mathf.Clamp(stockAmount, 0, maxStock);

        if (displayItemInstance == null)
        {
            CreateDisplayItem(productPrefab);
        }

        Debug.Log(name + " initialized with " + storedProductName + " x" + currentStock);
    }

    public bool HasItem()
    {
        return currentStock > 0 && !string.IsNullOrEmpty(storedProductName);
    }

    public string GetProductName()
    {
        return storedProductName;
    }

    public float GetProductPrice()
    {
        return storedProductPrice;
    }

    public bool TryBuyOne(out float pricePaid)
    {
        pricePaid = 0f;

        if (!HasItem())
            return false;

        currentStock--;
        pricePaid = storedProductPrice;

        Debug.Log(storedProductName + " sold. Stock left: " + currentStock);

        if (currentStock <= 0)
        {
            currentStock = 0;

            if (displayItemInstance != null)
            {
                Destroy(displayItemInstance);
                displayItemInstance = null;
            }

            storedProductName = "";
            storedProductPrice = 0f;
        }

        return true;
    }

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHoldingItem())
        {
            Debug.Log("You are not holding anything.");
            return;
        }

        GameObject held = player.heldItem;
        ProductData heldData = held.GetComponent<ProductData>();

        if (heldData == null)
        {
            heldData = held.GetComponentInChildren<ProductData>();
        }

        if (heldData == null)
        {
            Debug.Log("Held object has no ProductData.");
            return;
        }

        if (string.IsNullOrEmpty(storedProductName))
        {
            storedProductName = heldData.productName;
            storedProductPrice = heldData.price;

            AddStockFromPlayer(player, held);

            if (displayItemInstance == null)
            {
                CreateDisplayItem(held);
            }

            return;
        }

        if (storedProductName != heldData.productName)
        {
            Debug.Log("This shelf already stores " + storedProductName);
            return;
        }

        if (currentStock >= maxStock)
        {
            Debug.Log("Shelf is full.");
            return;
        }

        AddStockFromPlayer(player, held);
    }

    void AddStockFromPlayer(PlayerInteraction player, GameObject held)
    {
        PickupItem pickup = held.GetComponent<PickupItem>();

        player.DropHeldItem();

        if (pickup != null)
        {
            pickup.ConsumeAfterRestock();
        }
        else
        {
            Destroy(held);
        }

        int stockToAdd = Mathf.Max(1, stockAddedPerItem);
        currentStock = Mathf.Clamp(currentStock + stockToAdd, 0, maxStock);
        Debug.Log(name + " restocked. Current stock: " + currentStock);
    }

    void CreateDisplayItem(GameObject sourcePrefab)
    {
        if (placePoint == null)
        {
            Debug.LogWarning(name + ": PlacePoint is not assigned.");
            return;
        }

        displayItemInstance = Instantiate(sourcePrefab, placePoint.position, placePoint.rotation, placePoint);

        Rigidbody rb = displayItemInstance.GetComponent<Rigidbody>();
        if (rb != null)
        {
            Destroy(rb);
        }

        Collider[] cols = displayItemInstance.GetComponentsInChildren<Collider>();
        foreach (Collider c in cols)
        {
            Destroy(c);
        }
    }

    public string GetInteractionText()
    {
        if (string.IsNullOrEmpty(storedProductName))
            return "Press E to stock shelf";

        return "Press E to restock " + storedProductName + " (" + currentStock + "/" + maxStock + ")";
    }
}
