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

        LoopMemoryManager.Instance.spawnPosition = new Vector3(1.4f, 0.0f, -2.9f);
        LoopMemoryManager.Instance.spawnRotation = new Vector3(0f, -90f, 0f);
        LoopMemoryManager.Instance.forceRespawnThisLoop = true;
        LoopMemoryManager.Instance.activeCameraName = "MainRoomCamera";

        Debug.Log("TimerLoop Ä²µo");

        StartCoroutine(TimeLoopAnim.Instance.PlayTransition(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            CameraManager.Instance.SwitchTo(LoopMemoryManager.Instance.activeCameraName);
        }));
    }
}
