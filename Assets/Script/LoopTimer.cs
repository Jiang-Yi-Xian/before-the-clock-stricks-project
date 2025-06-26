using UnityEngine;

public class LoopTimer : MonoBehaviour
{
    public static LoopTimer Instance;

    public float loopDuration = 10f; // ¤Q¤­¤ÀÄÁ
    private float elapsedTime = 0f;
    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    void Update()
    {
        elapsedTime += Time.deltaTime;
        if (elapsedTime >= loopDuration) 
        {
            LoopManager.Instance.TriggerLoop();
        }
    }

    public void ResetTimer() => elapsedTime = 0f;
}
