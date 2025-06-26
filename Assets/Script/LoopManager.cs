using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
    }

    public void TriggerLoop() 
    {
        LoopMemoryManager.Instance.IncrementLoop();
        LoopTimer.Instance.ResetTimer();
        StartCoroutine(TimeLoopAnim.Instance.PlayTransition(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }));
    }
}
