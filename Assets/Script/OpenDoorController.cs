using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;

public class OpenDoorController : MonoBehaviour
{
    [Header("Refs")]
    public Transform interactionPoint;        // 門邊互動點
    public Transform doorHinge;               // 門的旋轉樞紐（local Y 旋轉）
    public GameObject player;                 // 玩家物件
    public NavMeshAgent agent;                // 玩家 NavMeshAgent
    public Animator playerAnimator;           // 玩家 Animator
    public ClockTimelineController timelineController;
    public Transform intoRoomDir;

    [Header("Player Control Scripts (會暫停)")]
    public MonoBehaviour[] playerControlScripts; // 例如點地面移動腳本

    [Header("Animation Clips / State Names")]
    public string openDoorState = "opening"; // 主角的「推/開門」動畫狀態名
    public string walkState = "walk";         // 走路動畫狀態名
    public string idleState = "Idle";         // 站立動畫狀態名

    [Header("Timings")]
    public float stopDistance = 0.25f;  // 判定抵達互動點距離
    public float faceSpeed = 10f;       // 旋轉看向門的速度
    public float doorOpenAngle = -90f;   // 門打開角度（正負依鉸鏈方向）
    public float doorOpenDuration = 0.6f;
    public float walkIntoRoomDistance = 3f; // 進門向前走的距離（公尺）
    public float walkIntoRoomDuration = 0.7f; // 走這段需要的時間（秒）
    public float doorCloseDuration = 0.6f;

    [Header("Options")]
    public bool disableClickOnly = false; // 若你只想禁止「點地面」，但仍建議交接 Agent 寫入權（預設 false）
    public bool alreadyOpenedOnce = false; // 若僅允許一次

    bool busy;

    public void BeginSequence()
    {
        if (busy || alreadyOpenedOnce) return;
        StartCoroutine(SequenceCo());
    }

    IEnumerator SequenceCo()
    {
        busy = true;

        // 1) 暫停玩家控制（UI 點擊、移動腳本）
        SetPlayerControls(false);

        // 2) 導航到互動點（沿用你既有的協程也可以，見下方「替代：使用 MoveAndInteract」）
        yield return StartCoroutine(MoveToPointCo(interactionPoint.position));

        // 面向門
        yield return StartCoroutine(FaceTargetCo(transform.position, faceSpeed, 0.25f));

        PlayerController.Instance?.LockAnimator(true);

        // 3) 交接控制權：Timeline/程式將直接改 Transform，避免跟 NavMesh 搶位
        if (!disableClickOnly)
        {
            agent.isStopped = true;
            agent.updatePosition = false;
            agent.updateRotation = false;
            agent.ResetPath();
        }

        // 4) 播主角「開門」動畫，同步讓門旋轉到打開
        yield return StartCoroutine(PlayOpenDoorWithRotateCo());

        // 5) 播主角走路動畫，並把玩家往前移動（Root Motion 關閉，程式位移）
        yield return StartCoroutine(MovePlayerForwardCo(walkIntoRoomDistance, walkIntoRoomDuration));

        // 6) 關門動畫
        yield return StartCoroutine(RotateDoorCo(doorOpenAngle, 0f, doorCloseDuration));

        // 7) 收尾：停在 Idle
        if (!string.IsNullOrEmpty(idleState))
            playerAnimator.CrossFadeInFixedTime(idleState, 0.1f);
            playerAnimator.SetBool("iswalk", false);

        // 8) 還權給 NavMeshAgent（避免回彈：warp 到玩家現位置）
        if (!disableClickOnly)
        {
            agent.Warp(player.transform.position);
            agent.updatePosition = true;
            agent.updateRotation = true;
            agent.isStopped = false;
        }

        PlayerController.Instance?.LockAnimator(false);

        // 9) 播放輪迴 Timeline（你已經有了）
        timelineController.PlaySequence();

        SetPlayerControls(true);
        alreadyOpenedOnce = true; // 若不想限制可移除此行
        busy = false;
    }

    // ---------- 小協程們 ----------

    IEnumerator MoveToPointCo(Vector3 targetPos)
    {
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.SetDestination(targetPos);

        // 等抵達
        while (Vector3.Distance(player.transform.position, targetPos) > stopDistance)
            yield return null;

        agent.isStopped = true;
    }

    IEnumerator FaceTargetCo(Vector3 lookAtPos, float slerpSpeed, float minDuration)
    {
        Vector3 dir = (lookAtPos - player.transform.position);
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) yield break;

        Quaternion start = player.transform.rotation;
        Quaternion target = Quaternion.LookRotation(dir.normalized, Vector3.up);

        float t = 0f;
        while (t < 1f || minDuration > 0f)
        {
            t += Time.deltaTime * slerpSpeed;
            player.transform.rotation = Quaternion.Slerp(start, target, Mathf.Clamp01(t));
            minDuration -= Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator PlayOpenDoorWithRotateCo()
    {
        // 播主角開門動畫
        if (!string.IsNullOrEmpty(openDoorState))
            playerAnimator.CrossFadeInFixedTime(openDoorState, 0.1f);

        // 門旋轉（0 -> openAngle）
        yield return StartCoroutine(RotateDoorCo(0f, doorOpenAngle, doorOpenDuration));
    }

    IEnumerator MovePlayerForwardCo(float distance, float duration)
    {
        if (!string.IsNullOrEmpty(walkState))
            playerAnimator.CrossFadeInFixedTime(walkState, 0.1f);
            playerAnimator.SetBool("iswalk", true);

        // 用 intoRoomDir 的 forward，投影到水平面以避免上下誤差
        Vector3 dir = intoRoomDir ? intoRoomDir.forward : player.transform.forward;
        dir.y = 0f;
        dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : player.transform.forward;

        // 先把玩家朝這個方向，避免一邊轉一邊走造成斜行
        Quaternion targetRot = Quaternion.LookRotation(dir, Vector3.up);
        player.transform.rotation = targetRot;

        Vector3 start = player.transform.position;
        Vector3 end = start + dir * distance;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, duration);
            player.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
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

    void SetPlayerControls(bool enable)
    {
        foreach (var c in playerControlScripts)
            if (c) c.enabled = enable;
    }
}
