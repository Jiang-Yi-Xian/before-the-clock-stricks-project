using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;

    void OnMouseDown()
    {
        InventorySystem inventory = FindFirstObjectByType<InventorySystem>();
        inventory.AddItem(itemData);
        Destroy(gameObject);
    }
}
