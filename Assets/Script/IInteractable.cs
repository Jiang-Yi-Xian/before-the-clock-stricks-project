using UnityEngine;
public interface IInteractable
{
    Vector3 GetInteractionPoint();

    bool GetInteractionForward(out Vector3 forward);
    void Interact();
}
