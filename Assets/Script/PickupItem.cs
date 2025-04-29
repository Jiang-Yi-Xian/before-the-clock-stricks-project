using UnityEngine;

public class PickupItem : MonoBehaviour
{
    public ItemData itemData;

    void OnMouseDown()
    {
        InventorySystem inventory = FindObjectOfType<InventorySystem>();
        inventory.AddItem(itemData);
        Destroy(gameObject);
    }
}
