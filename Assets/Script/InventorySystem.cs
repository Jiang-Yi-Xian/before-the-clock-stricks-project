using NUnit.Framework;
using NUnit.Framework.Interfaces;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class InventorySystem : MonoBehaviour
{
    public static InventorySystem Instance { get; private set; }

    [SerializeField] private Transform inventoryUIContainer;
    [SerializeField] private GameObject inventorySlotPrefab;

    private List<ItemData> items = new List<ItemData>();

    private void Awake()
    {
        if (Instance != null && Instance != this) 
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    public void AddItem(ItemData item) 
    {
        if(item == null) return;

        items.Add(item);
        RefreshInventoryUI();
    }
    public void RemoveItem(ItemData item)
    {
        if (item == null) return;

        items.Remove(item);
        RefreshInventoryUI();
    }
    public void RefreshInventoryUI()
    {
        if (inventoryUIContainer == null || inventorySlotPrefab == null) return;

        foreach (Transform child in inventoryUIContainer) 
        {
            Destroy(child.gameObject);
        }

        foreach (ItemData item in items)
        {
            GameObject slot = Instantiate(inventorySlotPrefab, inventoryUIContainer, false);

            if (slot.GetComponent<CanvasGroup>() == null)
            {
                slot.AddComponent<CanvasGroup>();
            }

            Image slotImage = slot.GetComponent<Image>();
            if (slotImage != null) 
            {
                slotImage.sprite = item.icon;
            }

            DraggableItem draggable = slot.AddComponent<DraggableItem>();
            draggable.itemData = item;
            draggable.enabled = true;
        }
    }
}
