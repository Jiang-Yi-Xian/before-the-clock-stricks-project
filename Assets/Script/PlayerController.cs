using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using NUnit.Framework.Interfaces;

public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; set; }

    [Header("MouseInputAction")]
    [SerializeField] private InputAction mouseClickInput;

    [Header("MouseClickEffect")]
    [SerializeField] private ParticleSystem clickEffect;

    [Header("MoveClickablelayer")]
    [SerializeField] private LayerMask clickableLayer;

    [Header("MainCamByRaycast")]
    [SerializeField] private Camera maincam;

    [Header("NavMeshAgent")]
    [SerializeField] private NavMeshAgent agent;

    [Header("PlayerAnimator")]
    [SerializeField] private Animator animator;
    private int animIDIsWalk;

    private Vector3 targetPositon;
    private float rotationSpeed = 7.0f;
    private bool clickBlockedByUI = false;
    public bool isMove { get; set; }
    private bool isRotating = false;

    private float stoppingDistance = 0.5f;

    private void Awake()
    {
        Instance = this;

        isMove = true;

        if (agent == null) 
        {
            agent = GetComponent<NavMeshAgent>();
        }

        stoppingDistance = agent.stoppingDistance;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
        animIDIsWalk = Animator.StringToHash("iswalk");
    }

    void Start()
    {
        // 關閉 agent 自動旋轉
        agent.updateRotation = false;
    }
    private void Update()
    {
        // 檢查滑鼠是否點擊在 UI 上
        clickBlockedByUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        HandleRotation(); // 玩家轉向

        UpdateAnimator(); // 更新動畫狀態
    }

    // 滑鼠事件監聽
    private void OnEnable()
    {
        mouseClickInput.Enable();
        mouseClickInput.performed += OnMove;
    }

    private void OnDisable()
    {
        mouseClickInput.performed -= OnMove;
        mouseClickInput.Disable();
    }

    // 需要切換相機時更新為新的相機
    public void UpdateCamera(Camera newCam)
    {
        maincam = newCam;
    }

    private void HandleRotation()
    {
        
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
        {
            isRotating = true;

            
            Vector3 direction = agent.steeringTarget - transform.position;
            direction.y = 0;

            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (isRotating)
        {     
            isRotating = false;

            if (agent.velocity.sqrMagnitude < 0.01f && agent.remainingDistance < stoppingDistance)
            {
                agent.ResetPath();
            }
        }
    }
    private void OnMove(InputAction.CallbackContext context)
    {
        if (clickBlockedByUI || !isMove)
        {
            return;
        }

        int interactableMask = LayerMask.GetMask("Interactable");
        int walkableMask = LayerMask.GetMask("walkable");

        Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit interactHit, Mathf.Infinity, interactableMask))
        {
            if (interactHit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                Vector3 interactionPoint = interactable.GetInteractionPoint();
                StartCoroutine(MoveAndInteract(interactionPoint, interactable));
            }
            else if (interactHit.collider.GetComponentInParent<WifeInteractable>() is WifeInteractable wifeTarget)
            {
                Vector3 interactionPoint = wifeTarget.GetInteractionPoint();
                StartCoroutine(MoveAndInteract(interactionPoint, wifeTarget));
            }
        }
        else if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, walkableMask))
        {
            agent.SetDestination(groundHit.point);
            targetPositon = groundHit.point;

            SpawnClickEffect(groundHit.point + new Vector3(0, 0.1f, 0));
        }
    }
    private void UpdateAnimator()
    {
        if (animator == null) return;

        float speed = new Vector3(agent.velocity.x, 0f, agent.velocity.z).magnitude;

        bool isWalking = speed > 0.1f;

        animator.SetBool(animIDIsWalk, isWalking);
    }
    private void SpawnClickEffect(Vector3 position)
    {
        if (clickEffect != null)
        {
            ParticleSystem effectInstance = Instantiate(clickEffect, position, Quaternion.identity);
            float duration = effectInstance.main.duration + effectInstance.main.startLifetime.constant;
            Destroy(effectInstance.gameObject, duration);
        }
    }

    private IEnumerator MoveAndInteract(Vector3 point, IInteractable interactable)
    {
        agent.SetDestination(point);
        while (Vector3.Distance(transform.position, point) > agent.stoppingDistance + 0.1f)
        {
            yield return null;
        }

        transform.LookAt(point);

        interactable.Interact();
    }
}