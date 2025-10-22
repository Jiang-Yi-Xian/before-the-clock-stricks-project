using UnityEngine;
using System.Collections;

public class DoorFeedBackController : MonoBehaviour
{
    public static DoorFeedBackController Instance { get; private set; }
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public enum HingeAxis { X, Y, Z }

    [Header("目標門 (必填)")]
    public Transform door;                 // 指到門的 Transform（樞紐在鉸鏈上）

    [Header("鉸鏈軸設定")]
    public HingeAxis hingeAxis = HingeAxis.Z; // 你的門繞 Z 軸轉，預設 Z
    public bool useLocalRotation = true;      // 以 localRotation 控制（通常 true）

    [Header("抖動設定")]
    public float knockShakeAngle = 2.5f;   // 敲門時左右微擺角度（度）
    public float kickShakeAngle = 6.0f;   // 踹門時較大的擺幅（度）
    public float shakeDuration = 0.10f;  // 單次抖動時間（秒）

    [Header("踹開設定")]
    public int kicksToBreak = 3;         // 踹幾次會開門
    public float openAngle = 90f;       // 開門相對「關門角度」的角度（度），正/負決定方向
    public float openDuration = 0.20f;     // 旋轉到開門角度花費時間（秒）

    [Header("初始角度（自動記錄）")]
    public float closedYaw;                // 只作觀察用途（保留欄位）
    public bool captureOnStart = true;    // 勾選就會在 Start 紀錄關門旋轉

    private bool isShaking = false;
    private int kickCount = 0;
    private Quaternion closedRot;         // 關門時的旋轉
    private Coroutine shakeCo;

    void Start()
    {
        if (door == null)
        {
            Debug.LogError("[DoorFeedBackController] 請指定 door Transform");
            enabled = false;
            return;
        }

        if (captureOnStart)
        {
            closedRot = useLocalRotation ? door.localRotation : door.rotation;
            // 觀察值：以 Y 顯示方便閱讀（實際旋轉不限定 Y）
            closedYaw = useLocalRotation ? door.localEulerAngles.y : door.eulerAngles.y;
        }
    }

    /// <summary> 敲門：小幅抖動 </summary>
    public void OnKnock()
    {
        if (door == null || isShaking) return;
        shakeCo = StartCoroutine(ShakeAxis(knockShakeAngle, shakeDuration));
    }

    /// <summary> 踹門：大幅抖動；累計到達 kicksToBreak 則開門 </summary>
    public void OnKick()
    {
        if (door == null) return;

        if (!isShaking)
            shakeCo = StartCoroutine(ShakeAxis(kickShakeAngle, shakeDuration));

        kickCount++;
        if (kickCount >= kicksToBreak)
        {
            StopShakeIfAny();
            StartCoroutine(OpenDoor());
            kickCount = 0; // 重置，避免後續再觸發
        }
    }

    public void ResetDoorToClosedImmediate()
    {
        StopShakeIfAny();
        if (useLocalRotation) door.localRotation = closedRot;
        else door.rotation = closedRot;
        kickCount = 0;
    }

    // === Internals ===

    private IEnumerator ShakeAxis(float angleDeg, float duration)
    {
        isShaking = true;
        float half = duration * 0.5f;

        Quaternion baseRot = useLocalRotation ? door.localRotation : door.rotation;

        // 往負方向偏
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(0f, angleDeg, t / half);
            ApplyAxisOffset(baseRot, -a);
            yield return null;
        }

        // 回正方向
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float a = Mathf.SmoothStep(angleDeg, 0f, t / half);
            ApplyAxisOffset(baseRot, +a);
            yield return null;
        }

        // 回到初始
        if (useLocalRotation) door.localRotation = baseRot;
        else door.rotation = baseRot;

        isShaking = false;
    }

    private void ApplyAxisOffset(Quaternion baseRot, float deltaDeg)
    {
        Vector3 axisLocal = AxisVectorLocal();                         // X / Y / Z（local）
        if (useLocalRotation)
        {
            Quaternion offset = Quaternion.AngleAxis(deltaDeg, axisLocal);
            door.localRotation = baseRot * offset;
        }
        else
        {
            Vector3 axisWorld = door.TransformDirection(axisLocal);    // 轉到世界軸
            Quaternion offset = Quaternion.AngleAxis(deltaDeg, axisWorld);
            door.rotation = baseRot * offset;
        }
    }

    private IEnumerator OpenDoor()
    {
        Quaternion from = useLocalRotation ? door.localRotation : door.rotation;

        Vector3 axisLocal = AxisVectorLocal();
        Quaternion to;
        if (useLocalRotation)
        {
            to = closedRot * Quaternion.AngleAxis(openAngle, axisLocal);
        }
        else
        {
            Vector3 axisWorld = door.TransformDirection(axisLocal);
            to = closedRot * Quaternion.AngleAxis(openAngle, axisWorld);
        }

        float t = 0f;
        while (t < openDuration)
        {
            t += Time.deltaTime;
            Quaternion q = Quaternion.Slerp(from, to, t / openDuration);
            if (useLocalRotation) door.localRotation = q; else door.rotation = q;
            yield return null;
        }
        if (useLocalRotation) door.localRotation = to; else door.rotation = to;
    }

    private Vector3 AxisVectorLocal()
    {
        switch (hingeAxis)
        {
            case HingeAxis.X: return Vector3.right;
            case HingeAxis.Y: return Vector3.up;
            default: return Vector3.forward; // Z
        }
    }

    private void StopShakeIfAny()
    {
        if (shakeCo != null) StopCoroutine(shakeCo);
        isShaking = false;
        shakeCo = null;
    }
}
