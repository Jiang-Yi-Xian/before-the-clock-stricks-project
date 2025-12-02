using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class PoliceController : MonoBehaviour
{
    [Header("Ref")]
    public static PoliceController Instance;
    public NavMeshAgent agent;
    public Animator animator;
    public ParticleSystem muzzleFlash;
    public AudioSource shootSound;
    public Transform shootPoint;
    public float shootDamage = 100f;

    [Header("Gun")]
    public GameObject gun;
    public MeshRenderer gunRenderer;
    public Vector3 gunLocalPosOffset;
    public Vector3 gunLocalRotOffset;

    [Header("Player")]
    public Transform playerTransform;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        BindGunToHand();
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

    void BindGunToHand()
    {
        Transform rightHand = animator.GetBoneTransform(HumanBodyBones.RightHand);
        gun.transform.SetParent(rightHand);
        gun.transform.localPosition = gunLocalPosOffset;
        gun.transform.localRotation = Quaternion.Euler(gunLocalRotOffset);
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
        //if (eventName == "PoliceEnter") 
        //{
        //    StartCoroutine(DelayEnterDoor());
        //}
        switch (eventName) 
        {
            case "PoliceEnter":
                StartCoroutine(DelayEnterDoor());
                break;
            case "PoliceDrawGun":
                DrawGun();
                break;
            case "PoliceShoot":
                Shoot();
                break;
            case "PoliceHolsterGun":
                HolsterGun();
                break;

        }
    }

    private IEnumerator DelayEnterDoor() 
    {
        yield return new WaitForSeconds(2f);

        StartCoroutine(MoveToInteractionPoint("PoliceEnterDoorNormal"));

        yield return new WaitUntil(() => !DialogueManager.Instance.dialoguePlaying);
        yield return new WaitForSeconds(0.5f);

        GameEventsManager.Instance.dialogueEvents.EnterDialogue("PoliceEnter/PoliceEnter");
    }

    void DrawGun() 
    {
        animator.SetTrigger("drawGun");
    }

    void Shoot() 
    {
        muzzleFlash.Play();
        shootSound.Play();


        if (Physics.Raycast(shootPoint.position, shootPoint.forward, out RaycastHit hit, 50f))
        {
            if (hit.collider.CompareTag("Player"))
            {
                hit.collider.GetComponent<PlayerController>().DieLoop();
            }
        }
    }

    void HolsterGun() 
    {
        animator.SetTrigger("holsterGun");
    }

    public void ShowGun() 
    {
        gunRenderer.enabled = true;
    }
    public void HideGun()
    {
        gunRenderer.enabled = false;
    }
}
