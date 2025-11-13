using GLTFast.Schema;
using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class WifeController : MonoBehaviour
{
    public static WifeController Instance;
    public NavMeshAgent agent;
    public Animator animator;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        StoryManager.Instance.OnStoryEventTrigger += HandleStoryEvent;
    }

    private void OnDisable()
    {
        StoryManager.Instance.OnStoryEventTrigger -= HandleStoryEvent;
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

        animator.CrossFadeInFixedTime("idle", 0.2f);
    }

    private void HandleStoryEvent(string eventName)
    {
        // Event
    }
}
