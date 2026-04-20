using System.Collections.Generic;
using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    public GameObject customerPrefab;
    public Transform spawnPoint;
    public ShelfSlot[] shelfSlots;
    public Transform checkoutPoint;
    public Transform exitPoint;
    public CheckoutCounter checkoutCounter;
    public float minSpawnInterval = 4f;
    public float maxSpawnInterval = 10f;

    private float timer;
    private float nextSpawnInterval;

    void Start()
    {
        ResetSpawnTimer();
    }

    void Update()
    {
        if (GameManager.Instance != null && !GameManager.Instance.IsStoreOpen)
        {
            return;
        }

        timer += Time.deltaTime;

        if (timer >= nextSpawnInterval)
        {
            Debug.Log("Trying to spawn customer...");
            SpawnCustomer();
            ResetSpawnTimer();
        }
    }

    void ResetSpawnTimer()
    {
        timer = 0f;
        nextSpawnInterval = Random.Range(minSpawnInterval, maxSpawnInterval);
    }

    void SpawnCustomer()
    {
        if (customerPrefab == null)
        {
            Debug.LogWarning("CustomerSpawner: customerPrefab is missing.");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("CustomerSpawner: spawnPoint is missing.");
            return;
        }

        if (checkoutPoint == null)
        {
            Debug.LogWarning("CustomerSpawner: checkoutPoint is missing.");
            return;
        }

        if (exitPoint == null)
        {
            Debug.LogWarning("CustomerSpawner: exitPoint is missing.");
            return;
        }

        if (checkoutCounter == null)
        {
            Debug.LogWarning("CustomerSpawner: checkoutCounter is missing.");
            return;
        }

        ShelfSlot randomShelf = FindRandomAvailableShelf();

        if (randomShelf == null)
        {
            Debug.LogWarning("CustomerSpawner: no available shelf with stock found.");
            return;
        }

        GameObject customer = Instantiate(customerPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Customer spawned successfully.");

        CustomerAI ai = customer.GetComponent<CustomerAI>();

        if (ai == null)
        {
            Debug.LogWarning("CustomerSpawner: customer prefab is missing CustomerAI.");
            Destroy(customer);
            return;
        }

        ai.targetShelf = randomShelf;
        ai.checkoutPoint = checkoutPoint;
        ai.exitPoint = exitPoint;
        ai.checkoutCounter = checkoutCounter;
    }

    ShelfSlot FindRandomAvailableShelf()
    {
        List<ShelfSlot> availableShelves = new List<ShelfSlot>();

        Debug.Log("Checking shelves... Total shelves: " + shelfSlots.Length);

        foreach (ShelfSlot shelf in shelfSlots)
        {
            if (shelf == null)
            {
                Debug.LogWarning("A shelf slot entry is null.");
                continue;
            }

            Debug.Log("Shelf checked: " + shelf.name + " | HasItem = " + shelf.HasItem());

            if (shelf.HasItem())
            {
                availableShelves.Add(shelf);
            }
        }

        Debug.Log("Available shelves found: " + availableShelves.Count);

        if (availableShelves.Count == 0)
            return null;

        int randomIndex = Random.Range(0, availableShelves.Count);
        return availableShelves[randomIndex];
    }
}
