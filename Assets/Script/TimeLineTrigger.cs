using UnityEngine;
using UnityEngine.Playables;

public class TimeLineTrigger : MonoBehaviour
{
    public static TimeLineTrigger Instance { get; set; }

    [Header("Timeline 控制")]
    public PlayableDirector timeline;

    [Header("是否使用計時器")]
    public bool useTimer = false;

    [Tooltip("若使用計時器，幾秒後觸發 Timeline")]
    public float delayInSeconds = 5f;

    [Header("要出現的角色物件")]
    public GameObject policeCharacter;

    [Header("主角進門後才開始倒數？")]
    public bool requireEnterDoor = false;

    private bool hasTriggered = false;
    private bool doorEntered = false;

    void Start()
    {

        if (policeCharacter != null)
            policeCharacter.SetActive(false);

        if (useTimer && !requireEnterDoor && timeline != null)
        {
            Invoke(nameof(PlayTimeline), delayInSeconds);
        }
    }
    void Awake()
    {
        Instance = this;
    }

    public void OnPlayerEnterDoor()
    {
        doorEntered = true;

        // 若此時計時功能開啟，就啟動倒數
        if (useTimer && timeline != null && !hasTriggered)
        {
            Invoke(nameof(PlayTimeline), delayInSeconds);
        }
    }

    public void PlayTimeline()
    {
        if (hasTriggered) return;

        if (policeCharacter != null)
            policeCharacter.SetActive(true);

        if (timeline != null) 
        {
            timeline.time = 0;
            timeline.Play();
        }
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
