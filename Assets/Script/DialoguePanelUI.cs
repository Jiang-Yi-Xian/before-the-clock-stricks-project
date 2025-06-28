using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Ink.Runtime;

public class DialoguePanelUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject contentParent;
    [SerializeField] private GameObject backingPanel;
    [SerializeField] private TextMeshProUGUI dialogueText;
    [SerializeField] private GameObject choicesGroup;
    [SerializeField] private DialogueChoiceButton[] choiceButtons;

    private void Awake()
    {
        contentParent.SetActive(false);

        ResetPanel();
    }
    private void OnEnable()
    {
        var ev = GameEventsManager.Instance.dialogueEvents;
        ev.OnDialogueStarted += DialogueStarted;
        ev.OnDialogueFinished += DialogueFinished;
        ev.OnDisplayDialogue += DisplayDialogue;
    }
    private void OnDisable()
    {
        var ev = GameEventsManager.Instance.dialogueEvents;
        ev.OnDialogueStarted -= DialogueStarted;
        ev.OnDialogueFinished -= DialogueFinished;
        ev.OnDisplayDialogue -= DisplayDialogue;
    }
    private void DialogueStarted() 
    {
        contentParent.SetActive(true);
    }
    private void DialogueFinished() 
    {
        contentParent.SetActive(false);

        ResetPanel();
    }
    private void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices)
    {
        ResetPanel();

        bool hasChoices = dialogueChoices != null && dialogueChoices.Count > 0;
        bool hasDialogue = !string.IsNullOrWhiteSpace(dialogueLine);

        if (hasChoices && !hasDialogue)
        {
            choicesGroup.SetActive(true);
            backingPanel.SetActive(false);
            dialogueText.gameObject.SetActive(false);

            for (int i = 0; i < dialogueChoices.Count && i < choiceButtons.Length; i++)
            {
                var choice = dialogueChoices[i];
                var button = choiceButtons[i];

                button.gameObject.SetActive(true);
                button.SetChoiceText(choice.text);
                button.SetChoiceIndex(i);

                if (i == 0)
                {
                    button.SelectButton();
                    GameEventsManager.Instance.dialogueEvents.UpdateChoiceIndex(0);
                }
            }
        }
        else if (hasDialogue)
        {
            choicesGroup.SetActive(false);
            backingPanel.SetActive(true);
            dialogueText.gameObject.SetActive(true);
            dialogueText.text = dialogueLine;
        }
    }
    private void ResetPanel() 
    {
        dialogueText.text = "";
        dialogueText.gameObject.SetActive(false);
        backingPanel.SetActive(false);
        choicesGroup.SetActive(false);

        foreach (var button in choiceButtons)
        {
            button.gameObject.SetActive(false);
        }
    }
}
