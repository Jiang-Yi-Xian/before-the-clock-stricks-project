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

    private Vector3 targetPositon;
    private float rotationSpeed = 7.0f;
    public bool isMove { get; private set; }

    private void Awake()
    {
        if (maincam == null) maincam = Camera.main;
        if (agent == null) agent = GetComponent<NavMeshAgent>();
    }

    void Start()
    {
        isMove = true;
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
    private void OnMove(InputAction.CallbackContext context) 
    {
        if (EventSystem.current.IsPointerOverGameObject()) 
        {
            return;
        }

        // Camera ray 偵測滑鼠位置
        Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
        {
            Debug.Log("hitSomething");
            // 導航系統 & 角色移動
            agent.SetDestination(hit.point);
            targetPositon = hit.point;

            // 角色轉向
            Vector3 direction = targetPositon - transform.position;
            if (direction.magnitude > 0.1f) 
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }
            // 點擊特效
            SpawnClickEffect(hit.point + new Vector3(0, 0.1f, 0));

            // 判斷射線打到之物件是否為可互動物件
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable)) 
            {
                interactable.Interact(); // 執行互動物件之對應程式
                Debug.Log("nohit");
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
