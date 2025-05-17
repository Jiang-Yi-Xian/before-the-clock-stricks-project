using UnityEngine;
using System;

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

    public event Action<string> OnDisplayDialogue;
    public void DisplayDialogue(String dialogueLine) 
    {
        if (OnDisplayDialogue != null) 
        {
            OnDisplayDialogue(dialogueLine);
        } 
    }
}
