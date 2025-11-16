using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class PlayVideoOnClick : MonoBehaviour
{
    [Header("Video Settings")]
    public VideoPlayer videoPlayer;              // 指向場景中的 VideoPlayer

    [Header("Objects to Hide During Video")]
    public GameObject objectsToHide;             // 要暫時隱藏的 UI / 場景父物件

    private bool hasSubscribed = false;

    void Start()
    {
        if (videoPlayer != null && !hasSubscribed)
        {
            videoPlayer.loopPointReached += OnVideoEnd;
            hasSubscribed = true;
        }
    }
    public void PlayVideo()
    {
        if (videoPlayer != null)
        {
            // 播放前隱藏指定物件
            if (objectsToHide != null)
            {
                objectsToHide.SetActive(false);
            }

            videoPlayer.Play();
            Debug.Log("Play!");
        }
        else
        {
            Debug.LogWarning("VideoPlayer is not assigned.");
        }
    }
    private void OnVideoEnd(VideoPlayer vp)
    {
        // 播放結束後還原場景物件
        
        if (videoPlayer != null)
        {
            videoPlayer.Stop();                       // 先停止影片
            videoPlayer.gameObject.SetActive(false);  // 再關閉整個 VideoPlayer
        }
        SceneManager.LoadScene(1);
      
    }

    public void QuitGame() 
    {
        Application.Quit();
    }
}
