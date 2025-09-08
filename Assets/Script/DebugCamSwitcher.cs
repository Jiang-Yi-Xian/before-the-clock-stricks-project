using UnityEngine;

public class DebugCamSwitcher : MonoBehaviour
{
    public void SwitchToMain() 
    {
        CameraManager.Instance.SwitchTo("MainRoomCamera");
    }

    public void SwitchToHallway()
    {
        CameraManager.Instance.SwitchTo("HallwayCam");
    }
}
