using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonFadeIn : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Fade")]
    public CanvasGroup canvasGroup;            // 拖你的按鈕上 CanvasGroup
    public float fadeDuration = 2f;            // 淡入時間（秒）
    public float fadeDelay = 2f;               // 淡入前等待時間（秒）

    [Header("Text Hover")]
    public TextMeshProUGUI buttonText;         // 拖按鈕上的 TextMeshProUGUI
    public float sizeIncrease = 5f;            // 移入時增加多少字體
    public float sizeAnimDuration = 0.12f;     // 字體漲縮動畫時間（秒）

    private float originalFontSize;            // 改成 float（不要用 int）
    private FontStyles originalFontStyle;
    private Coroutine sizeCoroutine;

    private void Start()
    {
        if (canvasGroup == null) Debug.LogWarning("CanvasGroup 未綁定");
        if (buttonText == null) Debug.LogWarning("buttonText 未綁定");

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            StartCoroutine(FadeIn());
        }

        if (buttonText != null)
        {
            // 記錄原始字體大小 & 樣式（fontSize 是 float）
            originalFontSize = buttonText.fontSize;
            originalFontStyle = buttonText.fontStyle;
        }
    }

    private System.Collections.IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(fadeDelay);

        float time = 0f;
        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, time / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    // 滑鼠移到上面
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (buttonText == null) return;

        // 設為粗體（立刻）
        buttonText.fontStyle = FontStyles.Bold;

        // 字體漸變到 target
        float target = originalFontSize + sizeIncrease;
        if (sizeCoroutine != null) StopCoroutine(sizeCoroutine);
        sizeCoroutine = StartCoroutine(AnimateFontSize(buttonText.fontSize, target, sizeAnimDuration));
    }

    // 滑鼠移開
    public void OnPointerExit(PointerEventData eventData)
    {
        if (buttonText == null) return;

        // 還原字體樣式（立刻或延後都可）
        buttonText.fontStyle = originalFontStyle;

        // 字體漸變回原始大小
        if (sizeCoroutine != null) StopCoroutine(sizeCoroutine);
        sizeCoroutine = StartCoroutine(AnimateFontSize(buttonText.fontSize, originalFontSize, sizeAnimDuration));
    }

    private System.Collections.IEnumerator AnimateFontSize(float from, float to, float duration)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float v = Mathf.Lerp(from, to, Mathf.Clamp01(t / duration));
            buttonText.fontSize = v;
            yield return null;
        }
        buttonText.fontSize = to;
        sizeCoroutine = null;
    }
}

