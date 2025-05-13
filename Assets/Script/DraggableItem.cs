using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    // Making itemData public again to allow InventorySystem to set it
    public ItemData itemData { get; set; }

    private Image image;
    private Transform originalParent;
    private Vector3 originalPosition;
    private Canvas rootCanvas;
    private CanvasGroup canvasGroup;

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        rootCanvas = GetComponentInParent<Canvas>()?.rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.localPosition;

        // Move to root canvas for proper drag across UI elements
        if (rootCanvas != null)
        {
            transform.SetParent(rootCanvas.transform);
        }
        else
        {
            transform.SetParent(transform.root);
        }

        canvasGroup.alpha = 0.7f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool placed = false;

        if (!placed)
        {
            // Return to original position
            transform.SetParent(originalParent);
            transform.localPosition = originalPosition;
        }

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("DropZone"))
            {
                placed = true;
                PlaceItemInWorld(hit.point);
            }
        }
    }

    private void PlaceItemInWorld(Vector3 position)
    {
        if (itemData?.prefab != null)
        {
            Instantiate(itemData.prefab, position, Quaternion.identity);
            InventorySystem.Instance?.RemoveItem(itemData);
        }
    }
}