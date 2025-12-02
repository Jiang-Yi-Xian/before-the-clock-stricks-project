using UnityEngine;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

[System.Serializable]
public class InkStoryEntry
{
    public string storyName;
    public TextAsset inkJson;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Ink Stories")]
    [SerializeField] private InkStoryEntry[] inkStories;

    [SerializeField] private PlayerController playerController;

    private Dictionary<string, Story> storyMap = new Dictionary<string, Story>();
    private Story story;
    private int currentChoiceIndex = -1;
    public bool dialoguePlaying = false;

    private InkExternalFunction inkExternalFunctions;

    [SerializeField] private float timePerCharacter = 0.2f;
    [SerializeField] private float sentenceEndDelay = 0.5f;

    private Coroutine autoContinue;

    private float nextLineExtraDelay = 0f; // 由 #delay= 標籤設定的額外延遲（單位：秒）

    public bool interrupted = false;

    private void Awake()
    {
        Instance = this;

        foreach (var entry in inkStories)
        {
            if (!storyMap.ContainsKey(entry.storyName) && entry.inkJson != null)
            {
                storyMap.Add(entry.storyName, new Story(entry.inkJson.text));
            }
        }

        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
            if (playerController == null)
            {
                Debug.Log("PlayerController not found.");
            }
        }

        inkExternalFunctions = new InkExternalFunction();
        inkExternalFunctions.BindAll(storyMap);
    }

    private void OnEnable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue += EnterDialogue;
        GameEventsManager.Instance.dialogueEvents.OnSubmitPress += SubmitPressed;
        GameEventsManager.Instance.dialogueEvents.OnUpdateChoiceIndex += UpdateChoiceIndex;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.dialogueEvents.OnEnterDialogue -= EnterDialogue;
        GameEventsManager.Instance.dialogueEvents.OnSubmitPress -= SubmitPressed;
        GameEventsManager.Instance.dialogueEvents.OnUpdateChoiceIndex -= UpdateChoiceIndex;
    }

    private void UpdateChoiceIndex(int choiceIndex)
    {
        currentChoiceIndex = choiceIndex;
    }

    private void SubmitPressed()
    {
        if (!dialoguePlaying) return;

        if (autoContinue != null)
        {
            StopCoroutine(autoContinue);
            autoContinue = null;
        }

        if (story.currentChoices.Count > 0 && currentChoiceIndex != -1) 
        {
            story.ChooseChoiceIndex(currentChoiceIndex);
            currentChoiceIndex = -1;

            ContinueOrExitStory();
        }
    }

    private void EnterDialogue(string knotNameWithStory)
    {
        interrupted = false;
        if (dialoguePlaying) return;

        string[] parts = knotNameWithStory.Split('/');
        string storyName = parts[0];
        string knotName = parts.Length > 1 ? parts[1] : "";

        if (!storyMap.TryGetValue(storyName, out Story selectedStory))
        {
            Debug.LogError($"找不到對應的 Ink 劇本：{storyName}");
            return;
        }

        story = selectedStory;
        dialoguePlaying = true;

        GameEventsManager.Instance.dialogueEvents.DialogueStarted();

        //playerController?.StopMovementHard();
        //playerController.isMove = false;

        if (!string.IsNullOrEmpty(knotName))
        {
            story.ChoosePathString(knotName);
        }

        ContinueOrExitStory();
    }

    private void ContinueOrExitStory()
    {
        if (interrupted) return;

        if (story.currentChoices.Count > 0)
        {
            HandleTags(story.currentTags);
            GameEventsManager.Instance.dialogueEvents.DisplayDialogue("", story.currentChoices);
            return;
        }

        if (story.canContinue)
        {
            string line = story.Continue();

            HandleTags(story.currentTags);

            bool isBlank = string.IsNullOrWhiteSpace(line);
            bool hasDelay = nextLineExtraDelay > 0f;

            if (!isBlank)
            {
                GameEventsManager.Instance.dialogueEvents.DisplayDialogue(line, null);
                StartAutoContinue(line, nextLineExtraDelay);
                return;
            }

            if (hasDelay)
            {
                StartAutoContinue("", nextLineExtraDelay);
                return;
            }

            ContinueOrExitStory();
            return;
        }

        StartCoroutine(ExitDialogue());
    }

    private IEnumerator ExitDialogue()
    {
        yield return null;

        //playerController.isMove = true;

        dialoguePlaying = false;
        GameEventsManager.Instance.dialogueEvents.DialogueFinished();

        story.ResetState();
    }

    private bool IsLineBlank(string dialogueLine)
    {
        return dialogueLine.Trim() == "" || dialogueLine.Trim() == "\n";
    }

    private void StartAutoContinue(string line, float extraDelay)
    {
        if (autoContinue != null)
            StopCoroutine(autoContinue);

        autoContinue = StartCoroutine(AutoContinueNextLine(line, extraDelay));
    }

    private IEnumerator AutoContinueNextLine(string line, float extraDelay)
    {
        bool isEventLine = string.IsNullOrEmpty(line);

        var voice = AudioManager.Instance.defaultVoiceSource;
        var subtitleData = AudioManager.Instance.CurrentSubtitleData;

        if (isEventLine)
        {
            yield return new WaitForSeconds(extraDelay);
            nextLineExtraDelay = 0f;
            ContinueOrExitStory();
            yield break;
        }

        bool hasVoice =
            !string.IsNullOrEmpty(line) &&      // 本行是文字
            voice != null &&
            voice.clip != null &&
            voice.isPlaying &&                  // 語音真的在播
            subtitleData != null &&
            subtitleData.lines.Count > 0;

        if (hasVoice)
        {
            int index = 0;

            while (voice.isPlaying && index < subtitleData.lines.Count)
            {
                float targetTime = subtitleData.lines[index].time;
                while (voice.time < targetTime)
                    yield return null;
                index++;
            }

            float lastTime = subtitleData.lines[^1].time;
            float endTime = voice.clip.length;
            float remain = Mathf.Clamp(endTime - lastTime, 0.2f, 10f);

            yield return new WaitForSeconds(remain + extraDelay);

            nextLineExtraDelay = 0f;
            ContinueOrExitStory();
            yield break;
        }

        float timePerChar = subtitleData != null ? subtitleData.timePerCharacter : timePerCharacter;
        float endDelay = subtitleData != null ? subtitleData.sentenceEndDelay : sentenceEndDelay;

        float waitTime = (line.Length * timePerChar) + endDelay + extraDelay;

        yield return new WaitForSeconds(waitTime);

        nextLineExtraDelay = 0f;
        ContinueOrExitStory();
    }
    private void HandleTags(List<string> tags) 
    {
        if (tags == null || tags.Count == 0) return;

        foreach (string tag in tags) 
        {
            if (tag == "block_move" && playerController != null)
            {
                playerController.isMove = false;
            }
            else if (tag == "allow_move" && playerController != null)
            {
                playerController.isMove = true;
            }
            // 解析 delay
            else if (tag.StartsWith("delay="))
            {
                var val = tag.Substring("delay=".Length);
                if (float.TryParse(val, out var sec) && sec >= 0f)
                    nextLineExtraDelay = sec;
            }
        }
    }

    public void InterruptDialogue() 
    {
        if (autoContinue != null)
        {
            StopCoroutine(autoContinue);
            autoContinue = null;
        }

        ExitDialogueImmediate();

        interrupted = true;
    }

    private void ExitDialogueImmediate()
    {
        //playerController.isMove = true;

        dialoguePlaying = false;
        GameEventsManager.Instance.dialogueEvents.DialogueFinished();

        if (story != null)
            story.ResetState();

        interrupted = false;
    }
}
