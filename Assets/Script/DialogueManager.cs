using UnityEngine;
using Ink.Runtime;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;

    [SerializeField] private PlayerController playerController;

    private Story story;
    private bool dialoguePlaying = false;

    private void Awake()
    {
        story = new Story(inkJson.text);

        if (playerController == null) 
        {
            playerController = FindObjectOfType<PlayerController>();

            if (playerController != null) 
            {
                Debug.Log("PlayerController not find.");
            }
        }
    }
    private void OnEnable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue += EnterDialogue;
        GameEventsManager.Instance.dialogueEvents.OnSubmitPress += SubmitPressed;
    }
    private void OnDisable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue -= EnterDialogue;
        GameEventsManager.Instance.dialogueEvents.OnSubmitPress -= SubmitPressed;
    }

    private void SubmitPressed() 
    {
        if (!dialoguePlaying) 
        {
            return;
        }

        ContinueOrExitStory();
    }
    private void EnterDialogue(string knotName) 
    {
        if (dialoguePlaying) 
        {
            return;
        }

        dialoguePlaying = true;

        GameEventsManager.Instance.dialogueEvents.DialogueStarted();

        // Lock Player movement when entering the dialogue
        if (playerController != null) 
        {
            playerController.isMove = false;
        }

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
            GameEventsManager.Instance.dialogueEvents.DisplayDialogue(dialogueLine);
        }
        else 
        {
            StartCoroutine(ExitDialogue());
        }
    }
    private IEnumerator ExitDialogue() 
    {
        // prevent dialogue looping
        yield return null;

        // UnLock Player movement when exiting the dialogue
        if (playerController != null)
        {
            playerController.isMove = true;
        }
        else 
        {
            Debug.Log("PlayerController not find.");
        }

        dialoguePlaying = false;

        GameEventsManager.Instance.dialogueEvents.DialogueFinished();

        story.ResetState();
    }
}
