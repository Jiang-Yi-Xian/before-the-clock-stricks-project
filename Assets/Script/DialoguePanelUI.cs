using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Ink.Runtime;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private DialogueChoiceButton[] choiceButtons; 

    private void Awake()
    {
        contentParent.SetActive(false);

        ResetPanel();
    }
    private void OnEnable()
    {
        GameEventsManager.Instance.dialogueEvents.OnDialogueStarted += DialogueStarted;
        GameEventsManager.Instance.dialogueEvents.OnDialogueFinished += DialogueFinished;
        GameEventsManager.Instance.dialogueEvents.OnDisplayDialogue += DisplayDialogue;
    }
    private void OnDisable()
    {
        GameEventsManager.Instance.dialogueEvents.OnDialogueStarted -= DialogueStarted;
        GameEventsManager.Instance.dialogueEvents.OnDialogueFinished -= DialogueFinished;
        GameEventsManager.Instance.dialogueEvents.OnDisplayDialogue -= DisplayDialogue;
    }
    private void DialogueStarted() 
    {
        contentParent.SetActive(true);
    }
    private void DialogueFinished() 
    {
        contentParent.SetActive(false);

        // reset anything for next time
        ResetPanel();
    }
    private void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices) 
    {
        dialogueText.text = dialogueLine;

        if (dialogueChoices.Count > choiceButtons.Length) 
        {
            Debug.LogError("More dialogue choice (" 
                + dialogueChoices.Count + ") came thorugh than supported (" 
                + choiceButtons.Length + ").");
        }

        // start with all of the choice buttons hidden
        foreach (DialogueChoiceButton choiceButton in choiceButtons) 
        {
            choiceButton.gameObject.SetActive(false);
        }

        // enable and set info for button depending on ink choice information
        int choiceButtonIndex = dialogueChoices.Count - 1;
        for (int inkChoiceIndex = 0; inkChoiceIndex < dialogueChoices.Count; inkChoiceIndex++) 
        {
            Choice dialogueChoice = dialogueChoices[inkChoiceIndex];
            DialogueChoiceButton choiceButton = choiceButtons[choiceButtonIndex]; 

            choiceButton.gameObject.SetActive(true);
            choiceButton.SetChoiceText(dialogueChoice.text);
            choiceButton.SetChoiceIndex(inkChoiceIndex);

            if (inkChoiceIndex == 0) 
            {
                choiceButton.SelectButton();
                GameEventsManager.Instance.dialogueEvents.UpdateChoiceIndex(0);
            }

            choiceButtonIndex--;
        }
    }
    private void ResetPanel() 
    {
        dialogueText.text = "";
    }
}
