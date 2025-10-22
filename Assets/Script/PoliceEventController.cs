using System.Collections;
using UnityEngine;
using UnityEngine.Playables;

public class PoliceEventController : MonoBehaviour
{
    public static PoliceEventController Instance;

    private string dialogueKnotName = "PoliceArrivedAndKnockDoor/policeknockdoor";

    public PlayableDirector timeline;

    [SerializeField]
    private Animator Animator;

    void Awake()
    {
        Instance = this;
    }

    public void OnPoliceArrived() 
    {
        GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);

        if (timeline != null) timeline.Stop();
    }

    public void OnTriggerAnim(string key) 
    {
        StartCoroutine(WaitForAnim(key));
    }

    private IEnumerator WaitForAnim(string key) 
    {
        Animator.SetTrigger(key);

        yield return null;

        AnimatorStateInfo stateInfo = Animator.GetCurrentAnimatorStateInfo(0);

        while (Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f &&
           Animator.GetCurrentAnimatorStateInfo(0).IsName(stateInfo.shortNameHash.ToString()) == false)
        {
            yield return null;
        }
    }
}
