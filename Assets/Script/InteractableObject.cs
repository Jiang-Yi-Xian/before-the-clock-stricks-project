using NUnit.Framework.Interfaces;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    public void Interact() 
    {
        if (itemData == null) return;

        switch (itemData.interactionType) 
        {
            case InteractionType.Pick:
                Pickup();
                break;
            case InteractionType.Observe:
                // ...
                break;
            case InteractionType.Switch:
                // ...
                break;
        }
    }

    private void Pickup() 
    {
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory != null)
        {
            inventory.AddItem(itemData);
            Destroy(gameObject);
        }
        else 
        {
            Debug.Log("InventorySystem No Find");
        }
    }
}
