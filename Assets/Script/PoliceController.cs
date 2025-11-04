using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PoliceController : MonoBehaviour
{
    public static PoliceController Instance;
    public NavMeshAgent agent;
    public Animator animator;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        UpdateAnimator();
    }

    private void OnEnable()
    {
        StoryManager.Instance.OnStoryEventTrigger += HandleStoryEvent;
    }
    private void OnDisable()
    {
        StoryManager.Instance.OnStoryEventTrigger -= HandleStoryEvent;
    }

    private void UpdateAnimator()
    {
        if(!DialogueManager.Instance.interrupted) return;
        if (!PoliceEventController.Instance.canInterrupt) return;

        if (animator == null || agent == null)
            return;

        if (!agent.enabled || agent.isStopped)
        {
            animator.SetBool("iswalk", false);
            return;
        }

        bool hasDestination = agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete;
        bool isMoving = hasDestination &&
                        agent.remainingDistance > agent.stoppingDistance + 0.05f &&
                        agent.velocity.sqrMagnitude > 0.0001f;

        animator.SetBool("iswalk", isMoving);
    }

    public IEnumerator MoveToInteractionPoint(string pointName) 
    {
        var point = InteractionPointManager.Instance.GetPoint(pointName);
        if (point == null) yield break;

        agent.isStopped = false;
        agent.SetDestination(point.Position);

        while (Vector3.Distance(transform.position, point.Position) > agent.stoppingDistance + 0.1f)
            yield return null;

        Vector3 faceDir = point.Forward;
        faceDir.y = 0;
        if (faceDir.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(faceDir);

        animator.CrossFadeInFixedTime("Idle", 0.2f);
    }


    private void HandleStoryEvent(string eventName)
    {
        if (eventName == "PoliceEnter")
            DelayEnterDoor();
    }

    private IEnumerator DelayEnterDoor() 
    {
        yield return new WaitForSeconds(3f);

        StartCoroutine(MoveToInteractionPoint("PoliceEnterDoorNormal"));
    }
}
