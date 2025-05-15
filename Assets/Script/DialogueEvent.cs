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
}
