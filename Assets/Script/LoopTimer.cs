using UnityEngine;

public class LoopTimer : MonoBehaviour
{
    public static LoopTimer Instance;

    public float loopDuration = 10f; // ¤Q¤­¤ÀÄÁ
    private float elapsedTime = 0f;

    private bool isRunning = false;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        if (!isRunning) return;

        elapsedTime += Time.deltaTime;
        if (elapsedTime >= loopDuration) 
        {
            LoopManager.Instance.TriggerLoop();
            isRunning = false;
        }
    }

    public void ResetTimer() => elapsedTime = 0f;

    public void StartTimer() 
    {
        elapsedTime = 0f;
        isRunning = true;
    }

    public void StopTimer() 
    {
        isRunning = false;
    }
}
