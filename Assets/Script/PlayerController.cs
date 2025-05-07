using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputAction mouseClickInput;
    [SerializeField] ParticleSystem clickEffect;
    [SerializeField] LayerMask clickableLayer;

    public Camera maincam;
    public NavMeshAgent agent;
    private Vector3 targetPositon;
    private float rotationSpeed = 7.0f;

    public bool isMove;

    void Start()
    {
        isMove = true;
    }
    private void OnEnable()
    {
        mouseClickInput.Enable(); // 啟用滑鼠輸入
        mouseClickInput.performed += Move; // 綁定滑鼠點擊事件
    }

    private void OnDisable()
    {
        mouseClickInput.performed -= Move; // 取消綁定事件
        mouseClickInput.Disable(); // 停用滑鼠輸入
    }
    private void Move(InputAction.CallbackContext context) 
    {
        if (isMove)
        {
            // Camera ray 偵測滑鼠位置
            Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, clickableLayer))
            {
                // 導航系統 & 角色移動
                agent.SetDestination(hit.point);
                targetPositon += hit.point;

                // 角色轉向
                Vector3 direction = targetPositon - transform.position;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), rotationSpeed * Time.deltaTime);
                // 點擊特效
                if (clickEffect != null)
                {
                    ParticleSystem effectInstance = Instantiate(clickEffect, hit.point += new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                    Destroy(effectInstance.gameObject, effectInstance.main.duration + effectInstance.main.startLifetime.constant);
                }
            }
        }
    }
}
