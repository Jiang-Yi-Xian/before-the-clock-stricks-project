using UnityEngine;

public class TriggerHandler : MonoBehaviour
{
    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (!dialogueKnotName.Equals(""))
            {
                GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
            }
        }
    }
}
