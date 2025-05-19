using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private Camera mainCam;
    [SerializeField] private Camera PassRoadCam;
    private bool filp = false;

    private void Awake()
    {
        mainCam.enabled = false;
    }
    private void SwithchCamera() 
    {
        filp = !filp;
        mainCam.enabled = filp;
        PassRoadCam.enabled = !filp;

        Camera activCam = filp ? mainCam : PassRoadCam;

        PlayerController.Instance.UpdateCamera(activCam);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            SwithchCamera();
        }
    }
}
