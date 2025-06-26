using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class TimeLoopAnim : MonoBehaviour
{
    public static TimeLoopAnim Instance;

    [Header("Transition UI")]
    public Image fadeOverlay;
    public float fadeDuration = 1f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bellSound;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public IEnumerator PlayTransition(Action onMidpoint)
    {
        yield return FadeToBlack();
        audioSource.PlayOneShot(bellSound);
        onMidpoint?.Invoke();
        yield return new WaitForSeconds(0.5f);
        yield return FadeFromBlack();
    }

    private IEnumerator FadeToBlack()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeFromBlack()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, t / fadeDuration);
            fadeOverlay.color = new Color(0, 0, 0, alpha);
            yield return null;
        }
    }
}
