using UnityEngine;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class InkStoryEntry
{
    public string storyName;
    public TextAsset inkJson;
}

public class DialogueManager : MonoBehaviour
{
    [Header("Ink Stories")]
    [SerializeField] private InkStoryEntry[] inkStories;

    [SerializeField] private PlayerController playerController;

    private Dictionary<string, Story> storyMap = new Dictionary<string, Story>();
    private Story story;
    private int currentChoiceIndex = -1;
    private bool dialoguePlaying = false;

    private InkExternalFunction inkExternalFunctions;

    [SerializeField] private float timePerCharacter = 0.2f;
    [SerializeField] private float sentenceEndDelay = 0.5f;

    private Coroutine autoContinue;

    private void Awake()
    {
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
        if (playerController != null) playerController.isMove = false;

        if (!string.IsNullOrEmpty(knotName))
        {
            story.ChoosePathString(knotName);
        }

        ContinueOrExitStory();
    }

    private void ContinueOrExitStory()
    {
        if (story.canContinue)
        {
            string line = story.Continue();
            while (IsLineBlank(line) && story.canContinue)
            {
                line = story.Continue();
            }

            HandleTags(story.currentTags);

            GameEventsManager.Instance.dialogueEvents.DisplayDialogue(line, null);
            if (autoContinue != null)
                StopCoroutine(autoContinue);

            autoContinue = StartCoroutine(AutoContinueNextLine(line));
        }
        else if (story.currentChoices.Count > 0)
        {
            HandleTags(story.currentTags);
            GameEventsManager.Instance.dialogueEvents.DisplayDialogue("", story.currentChoices);
        }
        else
        {
            StartCoroutine(ExitDialogue());
        }
    }

    private IEnumerator ExitDialogue()
    {
        yield return null;

        if (playerController != null)
        {
            playerController.isMove = true;
        }
        else
        {
            Debug.Log("PlayerController not found.");
        }

        dialoguePlaying = false;
        GameEventsManager.Instance.dialogueEvents.DialogueFinished();

        story.ResetState();
    }

    private bool IsLineBlank(string dialogueLine)
    {
        return dialogueLine.Trim() == "" || dialogueLine.Trim() == "\n";
    }

    private IEnumerator AutoContinueNextLine(string line)
    {
        float waitTime = (line.Length * timePerCharacter) + sentenceEndDelay;
        yield return new WaitForSeconds(waitTime);

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
        }
    }
}
