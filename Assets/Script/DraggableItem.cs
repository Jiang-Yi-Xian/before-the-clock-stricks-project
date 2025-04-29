using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public ItemData itemData;
    private Image image;
    private Transform originalParent;

    void Awake() 
    {
        image = GetComponent<Image>();
    }

    public void OnBeginDrag(PointerEventData eventData) 
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root);
        image.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData) 
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        transform.SetParent(originalParent);
        transform.localPosition = Vector3.zero;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("DrapZone"))
            {
                Instantiate(itemData.prefab, hit.point, Quaternion.identity);

                InventorySystem inventory = FindObjectOfType<InventorySystem>();
                inventory.RemoveItem(itemData);
            }
        }
    }
}
