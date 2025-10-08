using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
public class OpenDoorController : MonoBehaviour
{
    [Header("Refs")]
    public Transform interactionPoint; // 門邊互動點 
    public Transform doorHinge; // 門的旋轉樞紐（local Y 旋轉） 
    public GameObject player; // 玩家物件 
    public NavMeshAgent agent; // 玩家 NavMeshAgent 
    public Animator playerAnimator; // 玩家 Animator 
    public ClockTimelineController timelineController;
    public Transform intoRoomDir;

    [Header("Player Control Scripts (會暫停)")]
    public MonoBehaviour[] playerControlScripts; // 例如點地面移動腳本 

    [Header("Animation Clips / State Names")]
    public string openDoorState = "opening"; // 主角的「推/開門」動畫狀態名 
    public string walkState = "walk"; // 走路動畫狀態名 
    public string idleState = "Idle"; // 站立動畫狀態名 

    [Header("Timings")]
    public float stopDistance = 0.25f; // 判定抵達互動點距離 
    public float faceSpeed = 10f; // 旋轉看向門的速度 
    public float doorOpenAngle = -90f; // 門打開角度（正負依鉸鏈方向） 
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
        PlayerController.Instance.StopMovementHard();
        PlayerController.Instance.isMove = false;

        // 2) 導航到互動點（沿用你既有的協程也可以，見下方「替代：使用 MoveAndInteract」） 
        yield return StartCoroutine(MoveToPointCo(interactionPoint.position)); // 面向門 
        yield return StartCoroutine(FaceTargetCo(transform.position, faceSpeed, 0.25f));

        // 3) 交接控制權：Timeline/程式將直接改 Transform，避免跟 NavMesh 搶位 
        if (!disableClickOnly)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.updatePosition = false;
            agent.updateRotation = false;
            PlayerController.Instance.LockAnimator(true);
        }

        // 4) 播主角「開門」動畫，同步讓門旋轉到打開 
        yield return StartCoroutine(PlayOpenDoorWithRotateCo());

        // 用 intoRoomDir.forward 算理想終點
        Vector3 dir = intoRoomDir ? intoRoomDir.forward : player.transform.forward;
        dir.y = 0f;
        dir = dir.sqrMagnitude > 1e-4f ? dir.normalized : player.transform.forward;
        Vector3 desiredEnd = player.transform.position + dir * walkIntoRoomDistance;

        // 先把理想終點貼回 NavMesh（找不到就不要推太深）
        if (!TryMakeSafeInsidePoint(desiredEnd, 1.0f, out var safeEnd))
        {
            // 找不到安全點：保守處理（小推半步並再次取樣）
            safeEnd = player.transform.position + dir * 0.5f; // 小推半步（可選）
            if (NavMesh.SamplePosition(safeEnd, out var snap, 0.6f, NavMesh.AllAreas))
                safeEnd = snap.position;
        }

        // 5) 播主角走路動畫，並把玩家往前移動（Root Motion 關閉，程式位移） 
        yield return StartCoroutine(MovePlayerToPointCo(safeEnd, walkIntoRoomDuration));

        // 6) 關門動畫 
        yield return StartCoroutine(RotateDoorCo(doorOpenAngle, 0f, doorCloseDuration));

        // 7) 收尾：停在 Idle 
        if (!string.IsNullOrEmpty(idleState))
        {
            playerAnimator.CrossFadeInFixedTime(idleState, 0.1f);
        }
        playerAnimator.SetBool("iswalk", false);

        // 9) 播放輪迴 Timeline
        yield return StartCoroutine(timelineController.PlaySequenceAndWait());

        // 8) 還權給 NavMeshAgent（避免回彈：warp 到玩家現位置） 
        if (!disableClickOnly)
        {
            if (NavMesh.SamplePosition(safeEnd, out var snap, 0.6f, NavMesh.AllAreas))
                agent.Warp(snap.position);
            else
                agent.Warp(player.transform.position); // 次選

            agent.nextPosition = agent.transform.position;
            agent.updatePosition = true;
            agent.updateRotation = false; // 一律保持由 PlayerController 控制轉向
            agent.isStopped = false;
            agent.ResetPath();
        }

        PlayerController.Instance?.LockAnimator(false);
        PlayerController.Instance.isMove = true;

        SetPlayerControls(true);
        alreadyOpenedOnce = true;

        // 若不想限制可移除此行
        busy = false;
    }

    // ---------- 小協程們 ---------- 
    IEnumerator MoveToPointCo(Vector3 targetPos)
    {
        agent.isStopped = false;
        agent.updatePosition = true;
        agent.updateRotation = false;
        agent.SetDestination(targetPos); // 等抵達 

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

    IEnumerator MovePlayerToPointCo(Vector3 end, float duration)
    {
        if (!string.IsNullOrEmpty(walkState))
            playerAnimator.CrossFadeInFixedTime(walkState, 0.1f);
        playerAnimator.SetBool("iswalk", true);

        Vector3 start = player.transform.position;
        Vector3 dir = end - start; dir.y = 0f;
        if (dir.sqrMagnitude > 1e-4f)
            player.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);

        float t = 0f, inv = 1f / Mathf.Max(0.0001f, duration);
        while (t < 1f)
        {
            t += Time.deltaTime * inv;
            player.transform.position = Vector3.Lerp(start, end, t);

            // ★ 關鍵：手動位移時讓 Agent 跟上 Transform（避免結束時被拉回）
            if (!agent.updatePosition) agent.nextPosition = player.transform.position;

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

    // 以 desired 為目標，先貼回 NavMesh；不行就沿著反方向逐步回縮找最近可站點
    bool TryMakeSafeInsidePoint(Vector3 desired, float maxSnap, out Vector3 safe)
    {
        if (NavMesh.SamplePosition(desired, out var hit, maxSnap, NavMesh.AllAreas))
        { safe = hit.position; return true; }

        Vector3 back = (player.transform.position - desired); back.y = 0;
        for (int i = 1; i <= 3; i++)
        {
            var p = desired + back.normalized * (0.3f * i); // 每次回縮 0.3m
            if (NavMesh.SamplePosition(p, out hit, 0.5f, NavMesh.AllAreas))
            { safe = hit.position; return true; }
        }
        safe = Vector3.zero;
        return false;
    }

    void SafeWarpTo(Vector3 worldPoint)
    {
        if (NavMesh.SamplePosition(worldPoint, out var hit, 0.6f, NavMesh.AllAreas))
            agent.Warp(hit.position);
    }
}