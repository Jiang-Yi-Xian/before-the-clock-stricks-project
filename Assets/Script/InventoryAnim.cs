using System.Collections;
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

    [Header("Arrow Rotation Setting")]
    [SerializeField] private float arrowPointDownRotation = 0f;
    [SerializeField] private float arrowPointUpRotation = 180f;

    // Track panel state
    private bool isPanelVisible = false;
    // Track animation IDs for cancellation
    private int panelTweenId = -1;
    private int buttonPositionTweenId = -1;
    private int buttonRotationTweenId = -1;

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

            // Initialize button icon rotation
            Vector3 rotation = inventoryButton.localEulerAngles;
            rotation.z = isPanelVisible ? arrowPointUpRotation : arrowPointDownRotation;
            inventoryButton.localEulerAngles = rotation;
        }
    }

    /// Toggle the inventory panel visibility with animation
    public void ToggleInventory()
    {
        isPanelVisible = !isPanelVisible;

        AnimatePanel();
        AnimateButtonPosition();
        AnimateButtonRotation();
    }

    /// Animate the inventory panel
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
            });
    }

    /// Animate the inventory button
    private void AnimateButtonPosition()
    {
        if (inventoryButton == null) return;

        // Cancel any ongoing position animations
        LeanTween.cancel(buttonPositionTweenId);

        // Get target position based on visibility state
        Vector2 targetPosition = isPanelVisible ? buttonVisiblePosition : buttonHiddenPosition;

        // Use anchoredPosition for RectTransforms
        buttonPositionTweenId = LeanTween.value(inventoryButton.gameObject, inventoryButton.anchoredPosition, targetPosition, animationDuration)
            .setEase(easeType)
            .setOnUpdate((Vector2 pos) => {
                inventoryButton.anchoredPosition = pos;
            })
            .setOnComplete(() => {
                // Ensure final position is exact
                inventoryButton.anchoredPosition = targetPosition;
            })
            .id;
    }

    /// Animate the button rotation to reflect arrow direction
    private void AnimateButtonRotation()
    {
        if (inventoryButton == null) return;

        // Cancel any ongoing rotation animations
        LeanTween.cancel(buttonRotationTweenId);

        // Get the current Z rotation
        float currentRotation = inventoryButton.localEulerAngles.z;

        // Normalize the rotation (handle cases like 360¢X -> 0¢X)
        if (currentRotation > 180f) currentRotation -= 360f;

        // Get target rotation based on visibility state
        float targetRotation = isPanelVisible ? arrowPointUpRotation : arrowPointDownRotation;

        // Animate rotation on Z axis (this rotates the button which contains the arrow image)
        buttonRotationTweenId = LeanTween.value(inventoryButton.gameObject, currentRotation, targetRotation, animationDuration)
            .setEase(easeType)
            .setOnUpdate((float rot) => {
                Vector3 eulerAngles = inventoryButton.localEulerAngles;
                eulerAngles.z = rot;
                inventoryButton.localEulerAngles = eulerAngles;
            })
            .setOnComplete(() => {
                // Ensure final rotation is exact
                Vector3 eulerAngles = inventoryButton.localEulerAngles;
                eulerAngles.z = targetRotation;
                inventoryButton.localEulerAngles = eulerAngles;
            })
            .id;
    }

    /// Force show the inventory panel
    public void ShowInventory()
    {
        if (!isPanelVisible)
        {
            ToggleInventory();
        }
    }

    /// Force hide the inventory panel
    public void HideInventory()
    {
        if (isPanelVisible)
        {
            ToggleInventory();
        }
    }
}
