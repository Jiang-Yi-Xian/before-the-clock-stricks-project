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

    [Header("MainCamByRaycast")]
    [SerializeField] private Camera maincam;

    [Header("NavMeshAgent")]
    [SerializeField] private NavMeshAgent agent;

    [Header("PlayerAnimator")]
    [SerializeField] private Animator animator;
    private int animIDIsWalk;

    [Header("Interact Settings")]
    [SerializeField] private float interactArriveTolerance = 0.15f;

    [Header("Click Routing")]
    [Tooltip("可行走地面用的 Raycast Layer")]
    [SerializeField] private LayerMask groundMask;
    [Tooltip("互動物件用的 Raycast/Overlap Layer（建議獨立成 Interactable）")]
    [SerializeField] private LayerMask interactableMask;
    [Tooltip("若互動物沒自訂，使用此封鎖半徑（m）")]
    [SerializeField] private float defaultBlockRadius = 0.6f;
    [Tooltip("吸附到 NavMesh 的最遠距離；超過就拒絕移動")]
    [SerializeField] private float navSnapMax = 1.0f;
    [Tooltip("只允許完整路徑，避免去不了的點造成怪移動")]
    [SerializeField] private bool requirePathComplete = true;

    private Vector3 targetPositon;
    private float rotationSpeed = 7.0f;
    private bool clickBlockedByUI = false;
    public bool isMove { get; set; }
    private bool isRotating = false;

    private float stoppingDistance = 0.5f;

    public bool animatorLocked = false;

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

        if (!isMove || agent == null || agent.isStopped) return;
        if (!agent.hasPath) return;

        // 1) 優先用期望速度方向
        Vector3 vel = agent.desiredVelocity; vel.y = 0f;

        // 2) 速度很小時，用「朝向最終目的地」的方向（不是 steeringTarget）
        if (vel.sqrMagnitude < 0.0004f)
            vel = (agent.destination - transform.position);

        vel.y = 0f;
        if (vel.sqrMagnitude < 0.0004f) return;

        Quaternion target = Quaternion.LookRotation(vel.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
    }

    // 滑鼠點擊事件
    private void OnMove(InputAction.CallbackContext context)
    {
        if (clickBlockedByUI || !isMove) return;
        if (maincam == null || agent == null) return;

        // 從滑鼠位置發射 Ray
        Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());

        // 1) 互動優先：命中互動物 → 近距離直接互動；遠距離才導航過去
        if (Physics.Raycast(ray, out RaycastHit interactHit, Mathf.Infinity, interactableMask, QueryTriggerInteraction.Collide))
        {
            var interactable = interactHit.collider.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                // 互動點先貼回 NavMesh（避免終點貼邊導致側移）
                Vector3 interactionPoint = interactable.GetInteractionPoint();
                if (!NavMesh.SamplePosition(interactionPoint, out var ip, 0.6f, NavMesh.AllAreas))
                    return;

                // 近距離：不移動，直接面向＋互動
                float needMoveDist = agent.stoppingDistance + interactArriveTolerance;
                float dist = Vector3.Distance(transform.position, ip.position);
                if (dist <= needMoveDist)
                {
                    StopMovementHard();

                    Quaternion targetRot = Quaternion.identity;
                    if (interactable.GetInteractionForward(out Vector3 fwd))
                    {
                        fwd.y = 0;
                        targetRot = Quaternion.LookRotation(fwd.normalized);
                    }
                    else
                    {
                        Vector3 look = ip.position; look.y = transform.position.y;
                        targetRot = Quaternion.LookRotation((look - transform.position).normalized);
                    }

                    StartCoroutine(SmoothRotateTo(targetRot, 0.25f));

                    interactable.Interact();
                    return;
                }

                // 遠距離：要求完整路徑再走，避免末端貼邊造成橫移
                var path = new NavMeshPath();
                if (!agent.CalculatePath(ip.position, path) || path.status != NavMeshPathStatus.PathComplete)
                    return;

                agent.isStopped = false;
                agent.updatePosition = true;
                agent.updateRotation = false; // 旋轉交由 HandleRotation 控制
                agent.SetDestination(ip.position);

                // 抵達後由協程處理「最後貼點 + 清路徑 + 互動」
                StartCoroutine(MoveAndInteract(ip.position, interactable));
                return; // 命中互動物後結束，避免再落到地面分支
            }
        }

        // 2) 沒打到互動物 → 嘗試地面移動（但先做互動封鎖圈檢查）
        if (!Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundMask, QueryTriggerInteraction.Ignore))
            return;

        // 2a) 若點在任何互動物封鎖半徑內 → 不移動（避免小物件誤點造成怪平移）
        if (IsInsideAnyInteractBlock(groundHit.point))
            return;

        // 2b) 吸附到 NavMesh（超過 navSnapMax 就拒絕）
        if (!NavMesh.SamplePosition(groundHit.point, out var sp, navSnapMax, NavMesh.AllAreas))
            return;

        // 2c) 可選：要求完整路徑（避免不可達目標造成奇異路徑/側移）
        if (requirePathComplete)
        {
            var path = new NavMeshPath();
            if (!agent.CalculatePath(sp.position, path) || path.status != NavMeshPathStatus.PathComplete)
                return;
        }

        // 3) 一切 OK → 移動
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = false; // 旋轉仍由 HandleRotation 控制
        agent.SetDestination(sp.position);
        targetPositon = sp.position;

        // 點擊特效（略抬一點避免貼地 Z-fighting）
        SpawnClickEffect(sp.position + new Vector3(0, 0.1f, 0));
    }

    private bool IsInsideAnyInteractBlock(Vector3 p)
    {
        // 先用預設半徑掃一圈找附近的互動 Collider（含 Trigger）
        var cols = Physics.OverlapSphere(p, defaultBlockRadius, interactableMask, QueryTriggerInteraction.Collide);
        if (cols == null || cols.Length == 0) return false;

        foreach (var c in cols)
        {
            if (c == null) continue;

            // 如果該互動物有自訂半徑就用較大者
            float r = defaultBlockRadius;
            var io = c.GetComponentInParent<InteractableObject>();
            if (io != null) r = Mathf.Max(r, io.BlockRadius);

            // 用 bounds 中心做近似（足夠穩定）
            Vector3 center = c.bounds.center; center.y = p.y;
            if ((center - p).sqrMagnitude <= r * r)
                return true;
        }
        return false;
    }

    // 更新動畫狀態
    private void UpdateAnimator()
    {
        if (animator == null || agent == null) return;
        if (animatorLocked) return;

        bool hasFarTarget = agent.remainingDistance > agent.stoppingDistance + 0.15f;
        bool hasDesiredMove = agent.desiredVelocity.sqrMagnitude > 0.0001f;
        bool hasRealMove = agent.velocity.sqrMagnitude > 0.0001f;
        bool isWalking = !agent.isStopped && hasFarTarget && (hasDesiredMove || hasRealMove);

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
        agent.isStopped = false;
        agent.SetDestination(point);

        while (Vector3.Distance(transform.position, point) > agent.stoppingDistance + 0.1f)
            yield return null;

        // 關鍵：確實停車 + 清路徑，避免殘留旋轉
        StopMovementHard(); // 這個方法裡有 isStopped=true + ResetPath() + 關走路動畫

        if (NavMesh.SamplePosition(point, out var snap, 0.3f, NavMesh.AllAreas))
            agent.Warp(snap.position);

        Quaternion targetRot = Quaternion.identity;
        if (interactable.GetInteractionForward(out Vector3 fwd))
        {
            fwd.y = 0;
            targetRot = Quaternion.LookRotation(fwd.normalized);
        }
        else
        {
            Vector3 look = point; look.y = transform.position.y;
            targetRot = Quaternion.LookRotation((look - transform.position).normalized);
        }
        yield return StartCoroutine(SmoothRotateTo(targetRot, 0.25f));

        // 執行互動
        interactable.Interact();
    }

    // 停止移動的方法
    public void StopMovementHard() 
    {
        if (agent == null) return;

        agent.isStopped = true;
        agent.ResetPath();

        if (animator != null) 
        {
            animator.SetBool(animIDIsWalk, false);
        }
    }

    public void LockAnimator(bool locked)
    {
        animatorLocked = locked;
    }
    public void TriggerPlayerAnim(string anim) 
    {
        animator.SetTrigger(anim);
    }

    private IEnumerator SmoothRotateTo(Quaternion targetRot, float duration)
    {
        Quaternion startRot = transform.rotation;
        float t = 0f;
        while (t < duration)
        {
            transform.rotation = Quaternion.Slerp(startRot, targetRot, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.rotation = targetRot;
    }
}