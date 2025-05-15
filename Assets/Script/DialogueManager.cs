using UnityEngine;
using Ink.Runtime;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;
    private Story story;

    private bool dialoguePlaying = false;

    private void Awake()
    {
        story = new Story(inkJson.text);
    }
    private void OnEnable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue += EnterDialogue;
    }
    private void OnDisable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue -= EnterDialogue;
    }
    private void EnterDialogue(string knotName) 
    {
        if (dialoguePlaying) 
        {
            return;
        }

        dialoguePlaying = true;

        if (!knotName.Equals(""))
        {
            story.ChoosePathString(knotName);
        }
        else 
        {
            Debug.Log("Knot name was the empty string when entering dialogue");
        }

        ContinueOrExitStory();
    }
    private void ContinueOrExitStory() 
    {
        if (story.canContinue)
        {
            string dialogueLine = story.Continue();

            Debug.Log(dialogueLine);
        }
        else 
        {
            ExitDialogue();
        }
    }
    private void ExitDialogue() 
    {
        Debug.Log("Exiting Dialogue");

        dialoguePlaying = false;

        story.ResetState();
    }
}
