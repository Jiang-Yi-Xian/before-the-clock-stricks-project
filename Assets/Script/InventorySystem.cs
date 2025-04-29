using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public Transform inventoryUIContainer;
    public GameObject inventorySlotPrefab;

    public List<ItemData> items = new List<ItemData>();

    public void AddItem(ItemData item) 
    {
        items.Add(item);
        RefreshInventoryUI();
    }
    public void RemoveItem(ItemData item)
    {
        items.Remove(item);
        checkItemList();
        RefreshInventoryUI();
    }
    public void RefreshInventoryUI()
    {
        foreach (Transform child in inventoryUIContainer)
            Destroy(child.gameObject);

        foreach (ItemData item in items)
        {
            GameObject slot = Instantiate(inventorySlotPrefab);
            slot.transform.SetParent(inventoryUIContainer, false);

            slot.GetComponent<Image>().sprite = item.icon;

            DraggableItem draggable = slot.AddComponent<DraggableItem>();
            draggable.itemData = item;
        }
        Debug.Log("slot--");
    }
    public void checkItemList() 
    {
        foreach (var i in items) 
        {
            Debug.Log("目前還有物品：" + i.itemName);
        }
        Debug.Log("目前沒有物品");
    }
}
