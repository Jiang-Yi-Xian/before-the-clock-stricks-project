using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputAction mouseClickInput;
    [SerializeField] private ParticleSystem clickEffect;
    [SerializeField] private LayerMask clickableLayer;
    [SerializeField] private Camera maincam;
    [SerializeField] private NavMeshAgent agent;

    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    private Vector3 targetPositon;
    private float rotationSpeed = 7.0f;
    private bool clickBlockedByUI = false;
    public bool isMove { get; set; }
    private bool isRotating = false;

    private float stoppingDistance = 0.5f;

    private void Awake()
    {
        if (maincam == null) maincam = Camera.main;
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        isMove = true;
        agent.updateRotation = false;

        stoppingDistance = agent.stoppingDistance;
    }
    private void Update()
    {
        clickBlockedByUI = EventSystem.current != null && EventSystem.current.IsPointerOverGameObject();

        HandleRotation();
    }
    private void OnEnable()
    {
        mouseClickInput.Enable(); // 啟用滑鼠輸入
        mouseClickInput.performed += OnMove; // 綁定滑鼠點擊事件
    }

    private void OnDisable()
    {
        mouseClickInput.performed -= OnMove; // 取消綁定事件
        mouseClickInput.Disable(); // 停用滑鼠輸入
    }


    private void HandleRotation()
    {
        // Only rotate if the agent has a path AND is actually moving (check velocity)
        if (agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
        {
            isRotating = true;

            // Calculate direction based on steering target for smooth path following
            Vector3 direction = agent.steeringTarget - transform.position;
            direction.y = 0; // Keep rotation flat on the XZ plane

            // Only rotate if we have a meaningful direction
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
        }
        else if (isRotating)
        {
            // We've stopped moving, so stop rotating
            isRotating = false;

            // Optional: Ensure agent doesn't have a path if we've stopped
            if (agent.velocity.sqrMagnitude < 0.01f && agent.remainingDistance < stoppingDistance)
            {
                agent.ResetPath();
            }
        }
    }
    private void OnMove(InputAction.CallbackContext context) 
    {
        if (clickBlockedByUI ||!isMove)
        {
            return;
        }

        // Camera ray 偵測滑鼠位置
        Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
        {
            // 導航系統 & 角色移動
            agent.SetDestination(hit.point);
            targetPositon = hit.point;

            // 點擊特效
            SpawnClickEffect(hit.point + new Vector3(0, 0.1f, 0));

            // 判斷射線打到之物件是否為可互動物件
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable)) 
            {
                interactable.Interact(); // 執行互動物件之對應程式
            }
            if (hit.collider.CompareTag("NPC")) 
            {
                if (!dialogueKnotName.Equals(""))
                {
                    GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
                }
                else 
                {
                    Debug.Log("Start Function");
                }
            }
        }
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
}
