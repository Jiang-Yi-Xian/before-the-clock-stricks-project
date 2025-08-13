using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData itemData { get; set; }

    private Image image;               // 物品欄顯示圖示
    private Transform originalParent;  // 拖曳前所在的父元件
    private Vector3 originalPosition;  // 拖曳前的本地位置
    private Canvas rootCanvas;         // 根 Canvas (確保拖曳時在最上層顯示)
    private CanvasGroup canvasGroup;   // 控制透明度與是否可被 Raycast 點擊

    private void Awake()
    {
        // 抓取必須元件
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    // 拖曳開始時觸發
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 紀錄原本的父物件與位置
        originalParent = transform.parent;
        originalPosition = transform.localPosition;

        // 把物件移動到 Canvas 最上層，確保拖曳不被 UI 遮擋
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform);
        }
        else
        {
            transform.SetParent(transform.root);
        }

        // 拖曳時半透明，並禁用 Raycast (避免干擾其他 UI 偵測)
        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    // 拖曳進行中 (跟隨滑鼠位置)
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // 拖曳結束時觸發
    public void OnEndDrag(PointerEventData eventData)
    {
        // 還原透明度與 Raycast 偵測
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // 射線檢測滑鼠放開位置
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool placed = false; // 判斷是否成功放置

        // 如果沒有放置到有效位置，就還原到原本的位置
        if (!placed)
        {
            transform.SetParent(originalParent);
            transform.localPosition = originalPosition;
        }

        // 如果射線打到物件
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // 放到 DropZone (一般放置區域)
            if (hit.collider.CompareTag("DropZone"))
            {
                placed = true;
                PlaceItemInWorld(hit.point); // 在遊戲生成物件
            }

            // 拖曳鑰匙到前門 (開門)
            if (itemData.itemName == "key" && hit.collider.CompareTag("FrontDoor")) 
            {
                placed = true;

                // 撥放開門動畫
                Animator doorAnim = hit.collider.GetComponent<Animator>();
                if (doorAnim != null) 
                {
                    doorAnim.SetTrigger("door");
                }

                // 從物品欄中移除鑰匙
                InventorySystem.Instance.RemoveItem(itemData);
            }

            // 拖曳急救包到妻子 (觸發互動)
            WifeInteractable wifeTarget = hit.collider.GetComponentInParent<WifeInteractable>();
            if (itemData.itemName == "aidkit" && wifeTarget != null)
            {
                placed = true;
                Vector3 interactionPoint = wifeTarget.GetInteractionPoint();
                StartCoroutine(MoveAndGiveItem(interactionPoint, wifeTarget, itemData));
            }
        }
    }

    // 在遊戲中生成 Prefab
    private void PlaceItemInWorld(Vector3 position)
    {
        if (itemData?.prefab != null)
        {
            Instantiate(itemData.prefab, position, Quaternion.identity);
            InventorySystem.Instance?.RemoveItem(itemData); // 從物品欄移除
        }
    }

    // 控制玩家走到互動點
    private IEnumerator MoveAndGiveItem(Vector3 point, WifeInteractable target, ItemData item)
    {
        var player = PlayerController.Instance;
        var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();

        // 導航到互動點
        agent.isStopped = false;
        agent.SetDestination(point);

        // 等待玩家走到指定距離內
        while (Vector3.Distance(player.transform.position, point) > agent.stoppingDistance + 0.1f)
        {
            yield return null;
        }

        // 面向目標
        Vector3 look = point; look.y = player.transform.position.y;
        player.transform.LookAt(point);

        player.StopMovementHard();

        // 觸發妻子的互動邏輯
        target.HandleInteractionWith(item);

        // 從物品欄移除該物件
        InventorySystem.Instance.RemoveItem(item);
    }
}