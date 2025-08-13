using NUnit.Framework.Interfaces;
using UnityEngine;

public class InteractableObject : MonoBehaviour, IInteractable
{
    // 此互動物件的 ItemData
    [SerializeField] private ItemData itemData;

    // 互動時可觸發的對話節點
    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    // 玩家互動時需要靠近的互動點
    [SerializeField] private Transform interactionPoint;

    // 物件被互動時的主邏輯 (根據 ItemData 設定的互動類型，分派對應行為)
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

    // 處理拾取物品的邏輯
    private void Pickup() 
    {
        InventorySystem inventory = InventorySystem.Instance; // 取得物品欄系統

        if (inventory != null)
        {
            // 將物品加入物品欄
            inventory.AddItem(itemData);

            // 如果拾取的物品是鑰匙，且設定了對話節點，則觸發對話
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

    // 處理觸碰物件的邏輯
    private void Touch() 
    {
        // 將物件從場景刪除
        Destroy(this.gameObject);
    }

    // 回傳玩家應該靠近的互動點位置
    public Vector3 GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint.position : transform.position;
    }
}
