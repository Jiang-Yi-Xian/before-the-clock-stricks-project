using UnityEngine;

public class InventoryAnim : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private RectTransform inventoryPanel;
    [SerializeField] private RectTransform inventoryButton;

    [Header("Animation Positions")]
    [SerializeField] private Vector2 panelVisiblePosition = new Vector2(0, -50);
    [SerializeField] private Vector2 panelHiddenPosition = new Vector2(0, 40); // Off-screen position
    [SerializeField] private Vector2 buttonVisiblePosition = new Vector2(0, -90);
    [SerializeField] private Vector2 buttonHiddenPosition = new Vector2(0, -10);

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;
    [SerializeField] private LeanTweenType easeType = LeanTweenType.easeOutBack;
    [SerializeField] private bool startVisible = false;

    // Track panel state
    private bool isPanelVisible = false;
    // Track animation IDs for cancellation
    private int panelTweenId = -1;
    private int buttonTweenId = -1;

    private void Start()
    {
        // Initialize panel in hidden position
        isPanelVisible = startVisible;

        if (inventoryPanel != null)
        {
            inventoryPanel.anchoredPosition = isPanelVisible ? panelVisiblePosition : panelHiddenPosition;
        }

        if (inventoryButton != null)
        {
            inventoryButton.anchoredPosition = isPanelVisible ? buttonVisiblePosition : buttonHiddenPosition;
        }
    }

    /// <summary>
    /// Toggle the inventory panel visibility with animation
    /// </summary>
    public void ToggleInventory()
    {
        isPanelVisible = !isPanelVisible;

        AnimatePanel();
        AnimateButton();
    }

    /// <summary>
    /// Animate the inventory panel
    /// </summary>
    private void AnimatePanel()
    {
        if (inventoryPanel == null) return;

        LeanTween.cancel(inventoryPanel.gameObject);

        // Get target position based on visibility state
        Vector2 targetPosition = isPanelVisible ? panelVisiblePosition : panelHiddenPosition;

        // Use anchoredPosition for RectTransforms
        LeanTween.value(inventoryPanel.gameObject, inventoryPanel.anchoredPosition, targetPosition, animationDuration)
            .setEase(easeType)
            .setOnUpdate((Vector2 pos) => {
                inventoryPanel.anchoredPosition = pos;
            })
            .setOnComplete(() => {
                // Ensure final position is exact
                inventoryPanel.anchoredPosition = targetPosition;
                Debug.Log($"Panel animation complete - Now at {inventoryPanel.anchoredPosition}");
            });
    }

    /// <summary>
    /// Animate the inventory button
    /// </summary>
    private void AnimateButton()
    {
        if (inventoryButton == null) return;

        // Cancel any ongoing animations
        LeanTween.cancel(inventoryButton.gameObject);

        // Get target position based on visibility state
        Vector2 targetPosition = isPanelVisible ? buttonVisiblePosition : buttonHiddenPosition;

        // Use anchoredPosition for RectTransforms
        LeanTween.value(inventoryButton.gameObject, inventoryButton.anchoredPosition, targetPosition, animationDuration)
            .setEase(easeType)
            .setOnUpdate((Vector2 pos) => {
                inventoryButton.anchoredPosition = pos;
            })
            .setOnComplete(() => {
                // Ensure final position is exact
                inventoryButton.anchoredPosition = targetPosition;
                Debug.Log($"Button animation complete - Now at {inventoryButton.anchoredPosition}");
            });
    }

    /// <summary>
    /// Force show the inventory panel
    /// </summary>
    public void ShowInventory()
    {
        if (!isPanelVisible)
        {
            ToggleInventory();
        }
    }

    /// <summary>
    /// Force hide the inventory panel
    /// </summary>
    public void HideInventory()
    {
        if (isPanelVisible)
        {
            ToggleInventory();
        }
    }
}
