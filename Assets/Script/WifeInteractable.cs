using UnityEngine;

public class WifeInteractable : MonoBehaviour, IInteractable
{
    [SerializeField] private Transform interactionPoint;
    [SerializeField] private Animator wifeAnimator;
    [SerializeField] private WifeStateManager wifeState;


    [Header("Dialogue(optional)")]
    [SerializeField] private string dialogueKnotName;

    private bool hasBeenSaved = false;

    public Vector3 GetInteractionPoint()
    {
        return interactionPoint != null ? interactionPoint.position : transform.position;
    }

    public void Interact()
    {
        Debug.Log("Wife 被互動了，但沒有指定物品。");
    }

    public void HandleInteractionWith(ItemData item)
    {
        if (item == null) return;

        switch (item.itemName)
        {
            case "aidkit":
                if (!hasBeenSaved)
                {
                    wifeAnimator?.CrossFade("idle", 0.05f);
                    wifeState?.SetStandingState();
                    hasBeenSaved = true;

                    GameEventsManager.Instance.dialogueEvents.EnterDialogue(dialogueKnotName);
                }
                break;

            case "water":
                Debug.Log("給她水喝的反應");
                break;

            case "letter":
                Debug.Log("她讀了信之後哭了");
                break;
        }
    }
}
