using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance;
    public PlayerController playerController;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }

        Instance = this;
    }

    public void TriggerLoop() 
    {
        LoopMemoryManager.Instance.IncrementLoop();
        LoopMemoryManager.Instance.AddMemory("Loop_Proof");
        LoopMemoryManager.Instance.AddMemory("SaveWife");
        LoopTimer.Instance.ResetTimer();

        LoopMemoryManager.Instance.forceRespawnThisLoop = true;
        LoopMemoryManager.Instance.activeCameraName = "MainRoomCamera";

        Debug.Log("TimerLoop Ä²µo");

        StartCoroutine(TimeLoopAnim.Instance.PlayTransition(() =>
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            CameraManager.Instance.SwitchTo(LoopMemoryManager.Instance.activeCameraName);

            playerController.transform.position = LoopMemoryManager.Instance.spawnPosition;
            playerController.transform.rotation = Quaternion.Euler(LoopMemoryManager.Instance.spawnRotation);
        }));
    }
}
