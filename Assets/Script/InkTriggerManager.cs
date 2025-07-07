using UnityEngine;

public class InkTriggerManager : MonoBehaviour
{
    public void TriggerPoloceFirstKnockDialogue() 
    {
        GameEventsManager.Instance.dialogueEvents.EnterDialogue("FirstAsk/FirstAsk");
    }
    public void TriggerPoloceSecondKnockDialogue()
    {
        GameEventsManager.Instance.dialogueEvents.EnterDialogue("SecondAsk/SecondAsk");
    }
    public void TriggerPoloceThirdKnockDialogue()
    {
        GameEventsManager.Instance.dialogueEvents.EnterDialogue("ThirdAsk/ThirdAsk");
    }
}
