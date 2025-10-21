using UnityEngine;

public class PlayerSpawnHandler : MonoBehaviour
{
    void Start()
    {
        if (LoopMemoryManager.Instance != null)
        {
            //transform.position = LoopMemoryManager.Instance.spawnPosition;
            //transform.rotation = Quaternion.Euler(LoopMemoryManager.Instance.spawnRotation);

            CameraManager.Instance.SwitchTo(LoopMemoryManager.Instance.activeCameraName);
        }
    }
}
