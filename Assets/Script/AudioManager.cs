using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("聲音設定")]
    public AudioSource defaultVoiceSource;
    public AudioMixerGroup defaultMixerGroup;

    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();
    private Dictionary<string, AudioSource> characterSources = new Dictionary<string, AudioSource>();

    public AudioSource CurrentVoiceSource { get; private set; }
    public SubtitleData CurrentSubtitleData { get; private set; }

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void PlayVoiceLine(string character, string voiceId)
    {
        string voicePath = $"Voice/{character}/{voiceId}";
        string subtitlePath = $"Subtitles/{character}/{voiceId}";

        if (!clipCache.TryGetValue(voicePath, out AudioClip clip))
        {
            clip = Resources.Load<AudioClip>(voicePath);
            if (clip == null)
            {
                Debug.LogWarning($"找不到語音檔：{voicePath}");
                return;
            }
            clipCache[voicePath] = clip;
        }

        AudioSource source = GetAudioSource(character);
        if (source == null) source = defaultVoiceSource;

        source.clip = clip;
        source.outputAudioMixerGroup = defaultMixerGroup;
        source.volume = 0.6f;
        source.Play();

        CurrentVoiceSource = source;

        TextAsset subtitleJson = Resources.Load<TextAsset>(subtitlePath);
        if (subtitleJson != null)
        {
            CurrentSubtitleData = JsonUtility.FromJson<SubtitleData>(subtitleJson.text);
        }
        else
        {
            CurrentSubtitleData = null;
        }
    }

    private AudioSource GetAudioSource(string character)
    {
        if (!characterSources.TryGetValue(character, out AudioSource source) || source == null)
        {
            GameObject go = new GameObject($"VoiceSource_{character}");
            source = go.AddComponent<AudioSource>();
            go.transform.parent = transform;
            characterSources[character] = source;
        }
        return source;
    }
}
