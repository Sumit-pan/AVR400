using UnityEngine;

public class PickupItem : MonoBehaviour, IInteractable
{
    public string itemName = "Product";

    public void Interact(PlayerInteraction player)
    {
        if (!player.IsHoldingItem())
        {
            player.PickUpItem(gameObject);
        }
        else
        {
            Debug.Log("Your hands are full.");
        }
    }

    public string GetInteractionText()
    {
        return "Press E to pick up " + itemName;
    }
}