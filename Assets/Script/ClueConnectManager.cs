using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ClueConnectManager : MonoBehaviour
{
    public static ClueConnectManager Instance;

    [Header("UI Canvas 元件")]
    public RectTransform canvasRect;
    public RectTransform linePrefab;         // 細長 Image 線條
    public RectTransform lineContainer;      // 裝線條的 UI Panel
    public RectTransform clueBoardPanel;

    [Header("提示圖示")]
    public GameObject checkIconPrefab;
    public GameObject crossIconPrefab;

    private ClueCard firstClue;
    private RectTransform tempLine;
    private bool isConnecting = false;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (isConnecting && tempLine != null && firstClue != null)
        {
            bool isMouseRight = Input.mousePosition.x > RectTransformUtility.WorldToScreenPoint(null, firstClue.transform.position).x;
            RectTransform anchor = isMouseRight ? firstClue.rightAnchor : firstClue.leftAnchor;

            Vector2 fromPos = GetAnchorPosition(anchor);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, Input.mousePosition, null, out Vector2 toPos);

            UpdateUILine(tempLine, fromPos, toPos);
        }
    }

    public void OnRightClickClue(ClueCard clicked)
    {
        if (!isConnecting)
        {
            firstClue = clicked;
            isConnecting = true;

            tempLine = Instantiate(linePrefab, lineContainer);
            tempLine.name = "TempUILine";
        }
        else
        {
            if (clicked == firstClue)
            {
                CancelTempLine();
                return;
            }

            // 判斷滑鼠相對於第二張卡的位置
            bool isMouseRight = Input.mousePosition.x > RectTransformUtility.WorldToScreenPoint(null, clicked.transform.position).x;

            RectTransform startAnchor = isMouseRight ? firstClue.leftAnchor : firstClue.rightAnchor;
            RectTransform endAnchor = isMouseRight ? clicked.rightAnchor : clicked.leftAnchor;

            Vector2 fromPos = GetAnchorPosition(startAnchor);
            Vector2 toPos = GetAnchorPosition(endAnchor);

            UpdateUILine(tempLine, fromPos, toPos);

            bool result = InferenceManager.Instance.TryGenerateInference(firstClue, clicked);

            if (result)
            {
                tempLine.name = "FinalUILine";
                ShowIconBetween(fromPos, toPos, checkIconPrefab);
            }
            else
            {
                ShowIconBetween(fromPos, toPos, crossIconPrefab, true);
                Destroy(tempLine.gameObject, 0.5f);
            }

            // 重置狀態
            isConnecting = false;
            firstClue = null;
            tempLine = null;
        }
    }

    private void CancelTempLine()
    {
        isConnecting = false;
        firstClue = null;

        if (tempLine != null)
        {
            Destroy(tempLine.gameObject);
            tempLine = null;
        }
    }

    private Vector2 GetAnchorPosition(RectTransform anchor)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            RectTransformUtility.WorldToScreenPoint(null, anchor.position),
            null,
            out Vector2 localPos
        );
        return localPos;
    }

    private void UpdateUILine(RectTransform line, Vector2 start, Vector2 end)
    {
        Vector2 direction = end - start;
        float distance = direction.magnitude;

        line.sizeDelta = new Vector2(distance, 4f);
        line.anchoredPosition = start + direction / 2f;
        line.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }

    private void ShowIconBetween(Vector2 from, Vector2 to, GameObject iconPrefab, bool autoDestroy = false)
    {
        Vector2 mid = (from + to) / 2f;
        GameObject icon = Instantiate(iconPrefab, canvasRect);
        icon.GetComponent<RectTransform>().anchoredPosition = mid;

        if (autoDestroy)
            Destroy(icon, 2f);
    }
}
