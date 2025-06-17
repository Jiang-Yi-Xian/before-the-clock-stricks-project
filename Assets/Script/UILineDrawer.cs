using UnityEngine;

public class UILineDrawer : MonoBehaviour
{
    public RectTransform linePrefab; // 是一張細長的 Image
    public RectTransform canvasRect; // 你的 Canvas，用來轉換座標

    public void DrawUILine(RectTransform from, RectTransform to)
    {
        Vector2 fromPos;
        Vector2 toPos;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, from.position), null, out fromPos);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, RectTransformUtility.WorldToScreenPoint(null, to.position), null, out toPos);

        Vector2 direction = toPos - fromPos;
        float distance = direction.magnitude;

        RectTransform newLine = Instantiate(linePrefab, canvasRect);
        newLine.sizeDelta = new Vector2(distance, 4); // 4 是粗細
        newLine.anchoredPosition = fromPos + direction / 2;
        newLine.rotation = Quaternion.FromToRotation(Vector3.right, direction);
    }
}
