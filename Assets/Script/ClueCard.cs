using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class ClueCard : MonoBehaviour, IPointerClickHandler
{
    [Header("UI 元件")]
    public Image iconImage;
    public TextMeshProUGUI nameText;

    [Header("資料")]
    public ClueData clueData;

    [Header("定位點")]
    public RectTransform leftAnchor;
    public RectTransform rightAnchor;

    public void Setup(ClueData data)
    {
        clueData = data;
        if (iconImage != null) iconImage.sprite = data.icon;
        if (nameText != null) nameText.text = data.name;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            Debug.Log($"右鍵點擊線索：{clueData.name}");
            ClueConnectManager.Instance.OnRightClickClue(this);
        }
    }
}
