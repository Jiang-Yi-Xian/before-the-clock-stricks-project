using UnityEngine;

public class StoryManager : MonoBehaviour
{
    public static StoryManager Instance { get; private set; }

    private void Awake() 
    {
        Instance = this;
    }

    public void TriggerEvent(string eventName) 
    {
        OnStoryEventTrigger?.Invoke(eventName);
    }

    public event System.Action<string> OnStoryEventTrigger;
}
