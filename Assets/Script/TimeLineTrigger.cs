using UnityEngine;
using UnityEngine.Playables;

public class TimeLineTrigger : MonoBehaviour
{
    [Header("Timeline 控制")]
    public PlayableDirector timeline;

    [Header("是否使用計時器")]
    public bool useTimer = false;

    [Tooltip("若使用計時器，幾秒後觸發 Timeline")]
    public float delayInSeconds = 5f;

    private bool hasTriggered = false;

    void Start()
    {
        if (useTimer && timeline != null)
        {
            Invoke(nameof(PlayTimeline), delayInSeconds);
        }
    }

    public void PlayTimeline()
    {
        if (timeline == null || hasTriggered) return;

        timeline.time = 0;
        timeline.Play();
        hasTriggered = true;
    }

    public void ResetTimeline()
    {
        hasTriggered = false;
        if (timeline != null)
        {
            timeline.Stop();
            timeline.time = 0;
        }
    }
}
