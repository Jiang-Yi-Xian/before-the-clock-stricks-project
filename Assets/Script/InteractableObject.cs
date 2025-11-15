using NUnit.Framework.Interfaces;
using System.Collections;
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

    private bool isInteracted = false;

    public enum PickupStyle
    {
        Standing,
        Crouching
    }

    [Header("Pickup Settings")]
    [SerializeField] private PickupStyle pickupStyle = PickupStyle.Standing;

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
            case InteractionType.OpenDoor:
                OpenDoor();
                break;
        }
    }

    // 處理拾取物品的邏輯
    private void Pickup() 
    {
        if (isInteracted || this == null) return;
        isInteracted = true;

        var inventory = InventorySystem.Instance;
        if (inventory == null)
        {
            Debug.Log("InventorySystem No Find");
            return;
        }
        if (PlayerController.Instance.TryGetComponent<Animator>(out var anim)) 
        {
            PlayerController.Instance.StopMovementHard();
            PlayerController.Instance.isMove = false;

            switch (pickupStyle)
            {
                case PickupStyle.Crouching:
                    anim.SetTrigger("pickup");
                    break;
                case PickupStyle.Standing:
                default:
                    anim.SetTrigger("pickupstand");
                    break;
            }

            PlayerController.Instance.LockAnimator(true);

            PlayerController.Instance.StartCoroutine(WaitForPickupAnim(anim, inventory));
        }
    }

    // 處理觸碰物件的邏輯
    private void Touch() 
    {
        if (isInteracted || this == null) return;
        isInteracted = true;

        onInteract?.Invoke();
        Destroy(gameObject);
    }

    // 回傳玩家應該靠近的互動點位置
    public Vector3 GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint.position : transform.position;
    }
    public bool GetInteractionForward(out Vector3 forward)
    {
        // 若有指定方向點 → 回傳它的 forward
        if (interactionPoint != null)
        {
            forward = interactionPoint.forward;
            return true;
        }

        // 否則就直接用物件本身的 forward（例如門、人物）
        forward = transform.forward;
        return true;
    }

    private IEnumerator WaitForPickupAnim(Animator anim, InventorySystem inventory) 
    {
        yield return new WaitForSeconds(1.333f);

        inventory.AddItem(itemData);

        if (itemData.itemName == "key" && !string.IsNullOrEmpty(dialogueKnotName)) 
        {
            GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
        }

        PlayerController.Instance.LockAnimator(false);
        PlayerController.Instance.isMove = true;

        onInteract?.Invoke();

        Destroy(gameObject);
    }

    private void OpenDoor() 
    {
        if (OpenDoorController.Instance.IsDooropened == false) return;

        if (CompareTag("BathroomDoor") || CompareTag("BedroomDoor"))
        {
            PlayerController.Instance.TriggerPlayerAnim("openingDoor");

            var door = GetComponent<SimpleDoorController>();
            if (door != null)
            {
                if (door.isDoorOpen)
                {
                    door.CloseDoor();
                }
                else
                {
                    door.OpenDoor();
                }
            }
            //Debug.Log("BathroomDoor or BedroomDoor interacted");
        }
        else 
        {
            PlayerController.Instance.TriggerPlayerAnim("openingDoor");

            var door = GetComponent<SimpleDoorController>();
            if (door != null)
            {
                if (door.isDoorOpen)
                {
                    door.CloseDoor();
                }
                else
                {
                    door.OpenDoor();
                }
            }


            if (PoliceEventController.Instance.canInterrupt)
            {
                DialogueManager.Instance.InterruptDialogue();

                StoryManager.Instance.TriggerEvent("PoliceEnter");

                PoliceEventController.Instance.canInterrupt = false;
            }
        }
    }
}
