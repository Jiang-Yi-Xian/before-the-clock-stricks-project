using UnityEngine;
using System.Collections;

public class SimpleDoorController : MonoBehaviour
{
    [Header("Refs")]
    public Transform doorHinge;
    public float doorOpenAngle = -90f;
    public float doorOpenDuration = 0.6f;

    public bool isDoorOpen = false;


    public void OpenDoor() 
    {
        PlayerController.Instance.StopMovementHard();
        PlayerController.Instance.isMove = false;

        StartCoroutine(PlayOpenDoorWithRotateCo());

        PlayerController.Instance?.LockAnimator(false);
        PlayerController.Instance.isMove = true;
    }

    public void CloseDoor() 
    {
        PlayerController.Instance.StopMovementHard();
        PlayerController.Instance.isMove = false;

        StartCoroutine(PlayCloseDoorWithRotateCo());

        PlayerController.Instance?.LockAnimator(false);
        PlayerController.Instance.isMove = true;
    }

    IEnumerator PlayOpenDoorWithRotateCo()
    {
        // 門旋轉（0 -> openAngle） 
        yield return StartCoroutine(RotateDoorCo(0f, doorOpenAngle, doorOpenDuration));

        isDoorOpen = true;
    }
    IEnumerator PlayCloseDoorWithRotateCo() 
    {
        // 門旋轉（openAngle -> 0） 
        yield return StartCoroutine(RotateDoorCo(doorOpenAngle, 0f, doorOpenDuration));

        isDoorOpen = false;
    }

    IEnumerator RotateDoorCo(float fromAngle, float toAngle, float duration)
    {
        if (doorHinge == null) yield break;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            float z = Mathf.Lerp(fromAngle, toAngle, t);
            Vector3 e = doorHinge.localEulerAngles;
            e.z = z;
            doorHinge.localEulerAngles = e;
            yield return null;
        }
    }
}
