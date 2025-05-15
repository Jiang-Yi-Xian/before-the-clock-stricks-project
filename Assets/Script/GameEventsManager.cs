using UnityEngine;

public class GameEventsManager : MonoBehaviour
{
    public static GameEventsManager Instance { get; private set; }

    public DialogueEvent dialogueEvents;

    private void Awake()
    {
        if (Instance != null) 
        {
            Debug.Log("Find more than one GameEventsManager in the scene.");
        }
        Instance = this;

        dialogueEvents = new DialogueEvent();
    }
}
