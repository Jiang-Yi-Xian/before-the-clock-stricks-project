using UnityEngine;

public class InventoryAnim : MonoBehaviour
{
    public RectTransform inventoryPanel;
    public RectTransform inventoryButton;
    private Vector2 panshownPos = new Vector2(0, -50);
    private Vector2 panhiddenPos = new Vector2(0, 50);
    private Vector2 bntshownPos = new Vector2(0, -90);
    private Vector2 bnthiddenPos = new Vector2(0, -10);
    private bool isShown = true;
    
    public void ToggleInventory() 
    {
        isShown = !isShown;
        inventoryPanel.anchoredPosition = isShown ? panshownPos : panhiddenPos;
        inventoryButton.anchoredPosition = isShown ? bntshownPos : bnthiddenPos;
    }
}
