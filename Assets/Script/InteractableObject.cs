using NUnit.Framework.Interfaces;
using UnityEngine;
using UnityEngine.Events;

public class InteractableObject : MonoBehaviour, IInteractable
{
    // 此互動物件的 ItemData
    [SerializeField] private ItemData itemData;

    // 互動時可觸發的對話節點
    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    // 玩家互動時需要靠近的互動點
    [SerializeField] private Transform interactionPoint;

    [Header("Click/Block Settings")]
    [Tooltip("在此半徑內，玩家點擊地面將被阻擋（不移動），避免小物件誤點。")]
    [SerializeField] private float blockRadius = 0.6f;
    [Tooltip("是否自動加一個較大的 Trigger 球，提升點擊命中的容錯。")]
    [SerializeField] private bool addClickProxy = true;
    [Tooltip("點擊代理 Trigger 球半徑")]
    [SerializeField] private float clickProxyRadius = 0.3f;

    [Header("Events (optional)")]
    public UnityEvent onInteract; // 如需用 Inspector 綁定額外行為

    // 讓 PlayerController 能讀到自訂半徑
    public float BlockRadius => blockRadius;

    private void Reset()
    {
        // 建議把本物件 Layer 設為 Interactable（請先在專案建立此 Layer）
        gameObject.layer = LayerMask.NameToLayer("Interactable");

        // 主碰撞器（通常保留實體，不一定要 Trigger）
        var mainCol = GetComponent<Collider>();
        if (mainCol != null) mainCol.isTrigger = false;

        // 可選：增加一個較大的 Trigger 作為「點擊代理」，比較不容易 miss 點
        if (addClickProxy)
        {
            var proxyGo = new GameObject("ClickProxy");
            proxyGo.transform.SetParent(transform, false);
            proxyGo.layer = LayerMask.NameToLayer("Interactable");
            var sc = proxyGo.AddComponent<SphereCollider>();
            sc.isTrigger = true;
            sc.radius = clickProxyRadius;
        }
    }

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
        var inventory = InventorySystem.Instance;
        if (inventory != null)
        {
            inventory.AddItem(itemData);

            // 例如鑰匙：可選擇觸發對話
            if (itemData.itemName == "key" && !string.IsNullOrEmpty(dialogueKnotName))
            {
                PlayerController.Instance?.StopMovementHard();
                PlayerController.Instance.isMove = false;

                if (PlayerController.Instance.TryGetComponent<Animator>(out var anim))
                    anim.applyRootMotion = false;

                GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);

                PlayerController.Instance.isMove = true;
            }

            onInteract?.Invoke();
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
        onInteract?.Invoke();
        Destroy(gameObject);
    }

    // 回傳玩家應該靠近的互動點位置
    public Vector3 GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint.position : transform.position;
    }
}
