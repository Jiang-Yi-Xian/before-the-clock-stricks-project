using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TimeLoopAnim : MonoBehaviour
{
    public static TimeLoopAnim Instance;

    [Header("Transition UI")]
    public Image fadeOverlay;
    public float fadeDuration = 1.4f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bellSound;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 每次載入場景後重新尋找 fadeOverlay
        if (fadeOverlay == null)
        {
            GameObject found = GameObject.Find("TimeLoopUI");
            if (found != null)
            {
                fadeOverlay = found.GetComponent<Image>();
            }
        }
    }

    public IEnumerator PlayTransition(Action onMidpoint)
    {
        yield return FadeToBlack();

        audioSource?.PlayOneShot(bellSound);
        onMidpoint?.Invoke();

        // 等待場景載入完成後執行 FadeFromBlack
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(() => fadeOverlay != null);
        yield return FadeFromBlack();
    }

    private IEnumerator FadeToBlack()
    {
        if (fadeOverlay == null) yield break;

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
        if (fadeOverlay == null) yield break;

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
