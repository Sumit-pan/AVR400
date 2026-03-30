using UnityEngine;

public class ShelfSlot : MonoBehaviour, IInteractable
{
    public Transform placePoint;
    private GameObject placedItem;

    public void Interact(PlayerInteraction player)
    {
        if (placedItem == null && player.IsHoldingItem())
        {
            GameObject item = player.DropHeldItem();
            placedItem = item;

            Rigidbody rb = item.GetComponent<Rigidbody>();
            Collider col = item.GetComponent<Collider>();

            item.transform.position = placePoint.position;
            item.transform.rotation = placePoint.rotation;

            if (rb != null)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }

            if (col != null)
            {
                col.enabled = true;
            }
        }
        else if (placedItem != null && !player.IsHoldingItem())
        {
            player.PickUpItem(placedItem);
            placedItem = null;
        }
        else if (placedItem != null && player.IsHoldingItem())
        {
            Debug.Log("Shelf already has an item.");
        }
        else
        {
            Debug.Log("You are not holding anything.");
        }
    }

    public string GetInteractionText()
    {
        if (placedItem == null)
            return "Press E to place item";

        return "Press E to pick up shelf item";
    }
}