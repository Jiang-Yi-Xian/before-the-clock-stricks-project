using System.Collections.Generic;
using UnityEngine;

public class LoopMemoryManager : MonoBehaviour
{
    public static LoopMemoryManager Instance;

    public int loopCount = 0;
    private HashSet<string> memories = new HashSet<string>();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMemory(string key) => memories.Add(key);
    public bool HashMemory(string key) => memories.Contains(key);
    public void ClearMemories() => memories.Clear();
    public void IncrementLoop() => loopCount++;
}
