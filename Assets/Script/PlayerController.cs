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
        mouseClickInput.Enable(); // �ҥηƹ���J
        mouseClickInput.performed += Move; // �j�w�ƹ��I���ƥ�
    }

    private void OnDisable()
    {
        mouseClickInput.performed -= Move; // �����j�w�ƥ�
        mouseClickInput.Disable(); // ���ηƹ���J
    }
    private void Move(InputAction.CallbackContext context) 
    {
        if (isMove)
        {
            // Camera ray �����ƹ���m
            Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, clickableLayer))
            {
                // �ɯ�t�� & ���Ⲿ��
                agent.SetDestination(hit.point);
                targetPositon += hit.point;

                // ������V
                Vector3 direction = targetPositon - transform.position;
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction.normalized), rotationSpeed * Time.deltaTime);
                // �I���S��
                if (clickEffect != null)
                {
                    ParticleSystem effectInstance = Instantiate(clickEffect, hit.point += new Vector3(0, 0.1f, 0), clickEffect.transform.rotation);
                    Destroy(effectInstance.gameObject, effectInstance.main.duration + effectInstance.main.startLifetime.constant);
                }
            }
        }
    }
}
