using UnityEngine;

public class WifeStateManager : MonoBehaviour
{
    [Header("Collider ±±¨îª«¥ó")]
    [SerializeField] private GameObject standingColliderObj;
    [SerializeField] private GameObject lyingColliderObj;

    void Start()
    {
        SetLyingState();
    }
    public void SetStandingState() 
    {
        if (standingColliderObj != null) standingColliderObj.SetActive(true);
        if (lyingColliderObj != null) lyingColliderObj.SetActive(false);
    }
    public void SetLyingState() 
    {
        if (standingColliderObj != null) standingColliderObj.SetActive(false);
        if (lyingColliderObj != null) lyingColliderObj.SetActive(true);
    }
}
