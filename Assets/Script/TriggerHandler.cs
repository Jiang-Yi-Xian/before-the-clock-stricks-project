using UnityEngine;
using System.Collections;

public class TriggerHandler : MonoBehaviour
{
    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    [SerializeField] private bool intervalTrigger = false;
    [SerializeField] private float intervalTime = 1.0f;

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {

        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;

        if (!string.IsNullOrEmpty(dialogueKnotName))
        {
            if (!intervalTrigger)
            {
                GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
            }
            else
            {
                StartCoroutine(DelayedTrigger());
            }
        }
    }
    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(intervalTime);
        GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
    }
}
