using NUnit.Framework.Interfaces;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    [SerializeField] private ItemData itemData;

    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    [SerializeField] private Transform interactionPoint;

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
            case InteractionType.Touch:
                Touch();
                break;
        }
    }

    private void Pickup() 
    {
        InventorySystem inventory = InventorySystem.Instance;
        if (inventory != null)
        {
            inventory.AddItem(itemData);
            if (itemData.itemName == "key") 
            {
                if (!dialogueKnotName.Equals(""))
                {
                    GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
                }
            }
            Destroy(gameObject);
        }
        else 
        {
            Debug.Log("InventorySystem No Find");
        }
    }
    private void Touch() 
    {
        Destroy(this.gameObject);
    }

    public Vector3 GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint.position : transform.position;
    }
}
