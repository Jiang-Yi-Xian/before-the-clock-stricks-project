using UnityEngine;

public class InteractionPoint : MonoBehaviour
{
    [Tooltip("互動點名稱")]
    public string pointName;

    [Tooltip("該互動點朝向(可選)")]
    public Transform facingDirection;

    public Vector3 Position => transform.position;
    public Vector3 Forward => facingDirection ? facingDirection.forward : transform.forward;

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.1f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * 0.5f);
        UnityEditor.Handles.Label(transform.position + Vector3.up * 0.3f, pointName);
    }
#endif
}
