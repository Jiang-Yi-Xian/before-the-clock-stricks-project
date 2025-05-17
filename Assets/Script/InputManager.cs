using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class InputManager : MonoBehaviour
{
    public void OnSubmitPressed(InputAction.CallbackContext context)
    {
        if (context.started) 
        {
            GameEventsManager.Instance.dialogueEvents.SubmitPress();
        }
    }
}
