using UnityEngine;
using UnityEngine.Playables;
using System.Collections;

public class ClockTimelineController : MonoBehaviour
{
    public PlayableDirector timeline;
    public string timelineCameraName;
    public string returnCameraName;
    public Transform playerTeleportPoint;

    public double fadeOutTime = 5.0f; // 在 timeline 的第幾秒觸發 fade out

    private bool hasFadedOut = false;

    private void Start()
    {
        timeline.stopped += OnTimelineFinished;
    }

    public void PlaySequence()
    {
        StartCoroutine(PlayTimelineSequence());
    }

    private IEnumerator PlayTimelineSequence()
    {
        // 畫面淡出
        yield return TimeLoopAnim.Instance.FadeToBlack();

        // 切換相機
        CameraManager.Instance.SwitchTo(timelineCameraName);

        // 播放 timeline
        timeline.Play();

        // 畫面淡入
        yield return TimeLoopAnim.Instance.FadeFromBlack();

        // 檢查時間，在指定時間點 fade out
        while (timeline.state == PlayState.Playing)
        {
            if (!hasFadedOut && timeline.time >= fadeOutTime)
            {
                hasFadedOut = true;

                yield return TimeLoopAnim.Instance.FadeToBlack();

                // 切回原本相機
                CameraManager.Instance.SwitchTo(returnCameraName);

                yield return TimeLoopAnim.Instance.FadeFromBlack();
            }

            yield return null;
        }
    }

    private void OnTimelineFinished(PlayableDirector director)
    {
        hasFadedOut = false;
    }
}
