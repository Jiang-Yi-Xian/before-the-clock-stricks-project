using UnityEngine;
using UnityEngine.UI;

public class ClueBoardUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject clueBoardPanel;
    [SerializeField] private Button toggleButton;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        // 初始化狀態（關閉面板）
        clueBoardPanel.SetActive(false);

        toggleButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(false);

        // 綁定按鈕事件
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleClueBoard);
        }
        if (closeButton != null) 
        {
            closeButton.onClick.AddListener(CloseClueBoard);
        }
    }

    public void ToggleClueBoard()
    {
        clueBoardPanel.SetActive(true);

        toggleButton.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(true);
    }

    public void CloseClueBoard()
    {
        clueBoardPanel.SetActive(false);

        toggleButton.gameObject.SetActive(true);
        closeButton.gameObject.SetActive(false);
    }
}
