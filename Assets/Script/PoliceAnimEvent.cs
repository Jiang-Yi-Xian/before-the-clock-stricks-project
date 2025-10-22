using UnityEngine;

public class PoliceAnimEvent : MonoBehaviour
{
    public void Onkcock() 
    {
        DoorFeedBackController.Instance.OnKnock();
    }
    public void Onkick() 
    {
        DoorFeedBackController.Instance.OnKick();
    }
}
