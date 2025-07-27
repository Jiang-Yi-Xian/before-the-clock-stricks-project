using UnityEngine;
using Ink.Runtime;
using System.Collections.Generic;

public class InkExternalFunction
{
    public void BindAll(Dictionary<string, Story> storyMap)
    {
        foreach (var kvp in storyMap)
        {
            Bind(kvp.Value);
        }
    }

    public void UnbindAll(Dictionary<string, Story> storyMap)
    {
        foreach (var kvp in storyMap)
        {
            Unbind(kvp.Value);
        }
    }

    public void Bind(Story story)
    {
        story.BindExternalFunction("HasMemory", (string key) => LoopMemoryManager.Instance.HashMemory(key));

        story.BindExternalFunction("Audio", (string character, string audioId) => {
            AudioManager.Instance.PlayVoiceLine(character, audioId);
        });
    }

    public void Unbind(Story story)
    {
        story.UnbindExternalFunction("Audio");
        story.UnbindExternalFunction("HasMemory");
    }
}
