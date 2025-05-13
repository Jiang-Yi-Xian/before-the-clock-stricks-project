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
        mouseClickInput.Enable(); // �ҥηƹ���J
        mouseClickInput.performed += OnMove; // �j�w�ƹ��I���ƥ�
    }

    private void OnDisable()
    {
        mouseClickInput.performed -= OnMove; // �����j�w�ƥ�
        mouseClickInput.Disable(); // ���ηƹ���J
    }
    private void OnMove(InputAction.CallbackContext context) 
    {
        if (EventSystem.current.IsPointerOverGameObject()) 
        {
            return;
        }

        // Camera ray �����ƹ���m
        Ray ray = maincam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, clickableLayer))
        {
            Debug.Log("hitSomething");
            // �ɯ�t�� & ���Ⲿ��
            agent.SetDestination(hit.point);
            targetPositon = hit.point;

            // ������V
            Vector3 direction = targetPositon - transform.position;
            if (direction.magnitude > 0.1f) 
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }
            // �I���S��
            SpawnClickEffect(hit.point + new Vector3(0, 0.1f, 0));

            // �P�_�g�u���줧����O�_���i���ʪ���
            if (hit.collider.TryGetComponent<IInteractable>(out var interactable)) 
            {
                interactable.Interact(); // ���椬�ʪ��󤧹����{��
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
