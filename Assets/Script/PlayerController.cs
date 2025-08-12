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

    // 控制角色面向移動方向
    private void HandleRotation()
    {
        
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
        {
            isRotating = true;

            // 計算目標方向
            Vector3 direction = agent.steeringTarget - transform.position;
            direction.y = 0;

            // 平滑旋轉至目標方向
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (isRotating)
        {   
            // 停止旋轉
            isRotating = false;

            // 若接近目標點且幾乎停止，重置路徑
            if (agent.velocity.sqrMagnitude < 0.01f && agent.remainingDistance < stoppingDistance)
            {
                agent.ResetPath();
            }
        }
    }

    // 滑鼠點擊事件
    private void OnMove(InputAction.CallbackContext context)
    {
        if (clickBlockedByUI || !isMove)
        {
            return;
        }

        // 取得互動與可行走區域 Layer
        int interactableMask = LayerMask.GetMask("Interactable");
        int walkableMask = LayerMask.GetMask("walkable");

        // 從滑鼠位置發射 Ray
        Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 檢測是否點擊到可互動物件
        if (Physics.Raycast(ray, out RaycastHit interactHit, Mathf.Infinity, interactableMask))
        {
            if (interactHit.collider.TryGetComponent<IInteractable>(out var interactable))
            {
                // 與一般互動物件互動
                Vector3 interactionPoint = interactable.GetInteractionPoint();
                StartCoroutine(MoveAndInteract(interactionPoint, interactable));
            }
            else if (interactHit.collider.GetComponentInParent<WifeInteractable>() is WifeInteractable wifeTarget)
            {
                // 與妻子互動
                Vector3 interactionPoint = wifeTarget.GetInteractionPoint();
                StartCoroutine(MoveAndInteract(interactionPoint, wifeTarget));
            }
        }
        // 檢測是否點擊到地面
        else if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, walkableMask))
        {
            agent.SetDestination(groundHit.point);
            targetPositon = groundHit.point;

            // 生成點擊特效 (生成位置往上 0.1f 避免與地面重疊)
            SpawnClickEffect(groundHit.point + new Vector3(0, 0.1f, 0));
        }
    }

    // 更新動畫狀態
    private void UpdateAnimator()
    {
        if (animator == null) return;

        // 計算水平移動速度
        float speed = new Vector3(agent.velocity.x, 0f, agent.velocity.z).magnitude;

        // 是否進入行走動畫
        bool isWalking = speed > 0.1f;
        animator.SetBool(animIDIsWalk, isWalking);
    }

    // 生成滑鼠點擊特效
    private void SpawnClickEffect(Vector3 position)
    {
        if (clickEffect != null)
        {
            ParticleSystem effectInstance = Instantiate(clickEffect, position, Quaternion.identity);
            float duration = effectInstance.main.duration + effectInstance.main.startLifetime.constant;
            Destroy(effectInstance.gameObject, duration);
        }
    }

    // 移動到指定位置並與物件互動
    private IEnumerator MoveAndInteract(Vector3 point, IInteractable interactable)
    {
        agent.SetDestination(point);

        // 等待到達目標點 (加 0.1f 容錯距離)
        while (Vector3.Distance(transform.position, point) > agent.stoppingDistance + 0.1f)
        {
            yield return null;
        }

        // 面向互動物件
        transform.LookAt(point);

        // 執行互動邏輯
        interactable.Interact();
    }
}