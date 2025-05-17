using UnityEngine;
using Ink.Runtime;
using System.Collections;

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Story")]
    [SerializeField] private TextAsset inkJson;

    [SerializeField] private PlayerController playerController;

    private Story story;
    private int currentChoiceIndex = -1;
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
        GameEventsManager.Instance.dialogueEvents.OnUpdateChoiceIndex += UpdateChoiceIndex;
    }
    private void OnDisable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue -= EnterDialogue;
        GameEventsManager.Instance.dialogueEvents.OnSubmitPress -= SubmitPressed;
        GameEventsManager.Instance.dialogueEvents.OnUpdateChoiceIndex -= UpdateChoiceIndex;
    }

    private void UpdateChoiceIndex(int choiceIndex) 
    {
        this.currentChoiceIndex = choiceIndex;
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
        // make a choice, if applicable

        if (story.currentChoices.Count > 0 && currentChoiceIndex != -1) 
        {
            story.ChooseChoiceIndex(currentChoiceIndex);
            // reset choice index for next time
            currentChoiceIndex = -1;
        }
        if (story.canContinue)
        {
            string dialogueLine = story.Continue();

            // handle the case where there's an empty Line of dialogue
            // by continuing until we get a line with aontext
            while (IsLineBlank(dialogueLine) && story.canContinue) 
            {
                dialogueLine = story.Continue();
            }
            // handle the case where the Last Line of dialogue is blank
            // (empty choicew, external function, etc...)
            if (IsLineBlank(dialogueLine) && !story.canContinue)
            {
                StartCoroutine(ExitDialogue());
            }
            else 
            {
                GameEventsManager.Instance.dialogueEvents.DisplayDialogue(dialogueLine, story.currentChoices);
            }
        }
        else if(story.currentChoices.Count == 0)
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
    private bool IsLineBlank(string dialogueLine) 
    {
        return dialogueLine.Trim().Equals("") || dialogueLine.Trim().Equals("\n");
    }
}
