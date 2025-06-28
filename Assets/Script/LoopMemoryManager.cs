using System.Collections.Generic;
using UnityEngine;

public class LoopMemoryManager : MonoBehaviour
{
    public static LoopMemoryManager Instance;

    public int loopCount = 0;
    private HashSet<string> memories = new HashSet<string>();

    public Vector3 spawnPosition = new Vector3(8.76f, 0f, -2.9f);
    public Vector3 spawnRotation = new Vector3(0f, -90f, 0f);
    public string activeCameraName = "HallwayCam";
    public bool forceRespawnThisLoop = false;

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

    public void AddMemory(string key) => memories.Add(key);
    public bool HashMemory(string key) => memories.Contains(key);
    public void ClearMemories() => memories.Clear();
    public void IncrementLoop() => loopCount++;
}
