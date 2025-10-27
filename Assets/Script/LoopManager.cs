using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoopManager : MonoBehaviour
{
    public static LoopManager Instance;
    public GameObject player;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerLoop() 
    {
        LoopMemoryManager.Instance.IncrementLoop();
        LoopMemoryManager.Instance.AddMemory("Loop_Proof");
        LoopMemoryManager.Instance.AddMemory("SaveWife");
        LoopTimer.Instance.ResetTimer();

        LoopMemoryManager.Instance.forceRespawnThisLoop = true;
        LoopMemoryManager.Instance.activeCameraName = "MainRoomCamera";

        Debug.Log("[LoopManager] 開始淡出並重載場景...");

        Debug.Log("TimerLoop 觸發");

        StartCoroutine(TimeLoopAnim.Instance.PlayTransition(() =>
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);

            CameraManager.Instance.SwitchTo(LoopMemoryManager.Instance.activeCameraName);
        }));

        void OnSceneLoaded(Scene scene, LoadSceneMode mode) 
        {
            Debug.Log("[LoopManager] 場景載入完成，準備重生 Player...");

            // 找新的 Player 物件（場景重載後）
            GameObject player = GameObject.FindWithTag("Player");

            Collider playerCollider = player.GetComponent<Collider>();

            if (player != null)
            {
                var agent = player.GetComponent<UnityEngine.AI.NavMeshAgent>();
                var rb = player.GetComponent<Rigidbody>();
                var cols = player.GetComponentsInChildren<Collider>(true);

                if (agent && agent.enabled) agent.enabled = false;
                foreach (var c in cols) c.enabled = false;
                bool restoreRB = false;
                if (rb && !rb.isKinematic) { rb.isKinematic = true; restoreRB = true; }

                if (agent)
                { 
                    agent.enabled = true; 
                    agent.Warp(LoopMemoryManager.Instance.spawnPosition);
                } 
                foreach (var c in cols) c.enabled = true;
                if (rb && restoreRB) rb.isKinematic = false;

                player.transform.position = LoopMemoryManager.Instance.spawnPosition;
                player.transform.rotation = Quaternion.Euler(LoopMemoryManager.Instance.spawnRotation);
                Debug.Log($"[LoopManager] Player 已重生於 {player.transform.position}");
            }
            else
            {
                Debug.LogWarning("[LoopManager] 找不到 Player 物件！");
            }

            if (LoopManager.Instance != null) 
            {
                LoopTimer.Instance.StartTimer();
            }

            if (TimeLineTrigger.Instance != null) 
            {
                TimeLineTrigger.Instance.OnPlayerEnterDoor();
            }

            if (OpenDoorController.Instance.IsDooropened == false) 
            {
                OpenDoorController.Instance.IsDooropened = true;
            }

            // 解除事件註冊（避免重複觸發）
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }
}
