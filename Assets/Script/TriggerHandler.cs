using UnityEngine;
using System.Collections;

public class TriggerHandler : MonoBehaviour
{
    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    [SerializeField] private bool intervalTrigger = false;
    [SerializeField] private float intervalTime = 1.0f;

    [Header("是否只能觸發一次?")]
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered = false;

    public ClockTimelineController timelineController;

    bool played;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (triggerOnce && hasTriggered) return;

        if (!string.IsNullOrEmpty(dialogueKnotName))
        {
            hasTriggered = true;

            if (!intervalTrigger)
            {
                GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
            }
            else
            {
                StartCoroutine(DelayedTrigger());
            }
        }

        if (gameObject.tag == "EnterDoor") 
        {
            hasTriggered = true;

            if (played) return;
            played = true;
            timelineController.PlaySequence();
        }
    }
    private IEnumerator DelayedTrigger()
    {
        yield return new WaitForSeconds(intervalTime);
        GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
    }
}
