using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Ink.Runtime;

public class DialogueEvent
{
    public event Action<string> OnEnterDialogue;
    public void EnterDialogue(string knotName)
    {
        if (OnEnterDialogue != null)
        {
            OnEnterDialogue(knotName);
        }
    }

    public event Action OnSubmitPress;
    public void SubmitPress()
    {
        if (OnSubmitPress != null)
        {
            OnSubmitPress();
        }
    }

    public event Action OnDialogueStarted;
    public void DialogueStarted()
    {
        if (OnDialogueStarted != null)
        {
            OnDialogueStarted();
        }
    }

    public event Action OnDialogueFinished;
    public void DialogueFinished()
    {
        if (OnDialogueFinished != null)
        {
            OnDialogueFinished();
        }
    }

    public event Action<string, List<Choice>> OnDisplayDialogue;
    public void DisplayDialogue(string dialogueLine, List<Choice> dialogueChoices)
    {
        if (OnDisplayDialogue != null)
        {
            OnDisplayDialogue(dialogueLine, dialogueChoices);
        }
    }
    public event Action<int> OnUpdateChoiceIndex;
    public void UpdateChoiceIndex(int choiceIndex)
    {
        if (OnUpdateChoiceIndex != null) 
        {
            OnUpdateChoiceIndex(choiceIndex);
        }
    }

}
