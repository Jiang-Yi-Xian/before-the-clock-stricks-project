using UnityEngine;

public class PoliceEventController : MonoBehaviour
{
    private string dialogueKnotName = "PoliceArrivedAndKnockDoor/policeknockdoor";

    public void OnPoliceArrived() 
    {
        GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
    }
}
