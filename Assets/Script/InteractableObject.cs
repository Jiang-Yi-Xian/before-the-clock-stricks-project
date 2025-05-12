using NUnit.Framework.Interfaces;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    public ItemData itemData;

    public void Interact() 
    {
        switch (itemData.interactionType) 
        {
            case InteractionType.Pick:
                Pickup();
                break;
        }
    }

    void Pickup() 
    {
        InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
        inventory.AddItem(itemData);
        Destroy(gameObject);
    }
}
