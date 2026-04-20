using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Interaction Settings")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("Hold Settings")]
    public Transform holdPoint;
    public GameObject heldItem;

    private IInteractable currentInteractable;

    void Update()
    {
        DetectInteractable();

        if (Input.GetKeyDown(KeyCode.E) && currentInteractable != null)
        {
            currentInteractable.Interact(this);
        }

        if (Input.GetKeyDown(KeyCode.R) && currentInteractable is OrderStation orderStation)
        {
            orderStation.CycleSelection();
        }
    }

    void DetectInteractable()
    {
        currentInteractable = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * interactDistance, Color.green);

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            currentInteractable = hit.collider.GetComponent<IInteractable>();

            if (currentInteractable == null)
            {
                currentInteractable = hit.collider.GetComponentInParent<IInteractable>();
            }

            if (currentInteractable != null)
            {
                Debug.Log(currentInteractable.GetInteractionText());
            }
        }
    }

    public bool IsHoldingItem()
    {
        return heldItem != null;
    }

    public void PickUpItem(GameObject item)
    {
        if (heldItem != null) return;

        heldItem = item;

        Rigidbody[] rigidbodies = item.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider[] colliders = item.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

        item.transform.SetParent(holdPoint);
        item.transform.localPosition = Vector3.zero;
        item.transform.localRotation = Quaternion.identity;
    }

    public GameObject DropHeldItem()
    {
        if (heldItem == null) return null;

        GameObject item = heldItem;
        heldItem = null;

        item.transform.SetParent(null);

        Rigidbody[] rigidbodies = item.GetComponentsInChildren<Rigidbody>(true);
        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }

        Collider[] colliders = item.GetComponentsInChildren<Collider>(true);
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        return item;
    }
}
