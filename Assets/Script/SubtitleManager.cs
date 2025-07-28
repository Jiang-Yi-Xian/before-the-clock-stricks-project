using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SubtitleManager : MonoBehaviour
{
    public static SubtitleManager Instance;

    private TextMeshProUGUI dialogueText;

    private void Awake()
    {
        Instance = this;
        dialogueText = FindObjectOfType<DialoguePanelUI>().GetDialogueText();
    }

    public void PlaySubtitles(AudioSource source, SubtitleData data)
    {
        StartCoroutine(SubtitleRoutine(source, data));
    }

    private IEnumerator SubtitleRoutine(AudioSource source, SubtitleData data)
    {
        if (data == null || data.lines == null || data.lines.Count == 0)
        {
            Debug.LogWarning("字幕資料為空！");
            yield break;
        }

        int index = 0;
        dialogueText.text = "";

        while (source.isPlaying && index < data.lines.Count)
        {
            if (source.time >= data.lines[index].time)
            {
                dialogueText.text = data.lines[index].text;
                index++;
            }
            yield return null;
        }

        dialogueText.text = "";
    }
}