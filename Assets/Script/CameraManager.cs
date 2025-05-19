using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private GameObject mainCam;
    [SerializeField] private GameObject PassRoadCam;
    private bool filp = false;

    private void Awake()
    {
        mainCam = GameObject.Find("MainRoomCamera");
        PassRoadCam = GameObject.Find("Camera");

        mainCam.SetActive(false);
    }
    private void SwithchCamera() 
    {
        filp = !filp;
        mainCam.SetActive(filp);
        PassRoadCam.SetActive(!filp);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) 
        {
            SwithchCamera();
        }
    }
}
