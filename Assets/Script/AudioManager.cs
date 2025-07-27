using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    [Header("角色語音")]
    public AudioSource defaultVoiceSource;
    public AudioSource mainRoleVoiceSource;
    public AudioSource wifeVoiceSource;
    public AudioSource policeVoiceSource;

    [Header("混音器")]
    public AudioMixerGroup defaultMixerGroup;

    private Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    // 不需分角色的語音
    public void PlayVoiceLine(string audioId) 
    {
        PlayVoiceLine("Default", audioId);
    }

    // 需分角色的語音
    public void PlayVoiceLine(string character, string audioId) 
    {
        string path = $"Voice/{character}/{audioId}";
        if (!clipCache.TryGetValue(path, out AudioClip clip)) 
        {
            clip = Resources.Load<AudioClip>(path);
            if (clip == null)
            {
                Debug.LogWarning($"語音檔未找到：{path}");
                return;
            }
            clipCache[path] = clip;
        }

        AudioSource source = GetAudioSource(character);
        if (source == null) source = defaultVoiceSource;

        source.clip = clip;
        source.outputAudioMixerGroup = defaultMixerGroup;
        source.Play();
    }

    private AudioSource GetAudioSource(string character)
    {
        switch (character.ToLower())
        {
            case "wife": return wifeVoiceSource;
            case "police": return policeVoiceSource;
            case "default":
            case "main":
            default: return defaultVoiceSource;
        }
    }

    public bool IsAnyVoicePlaying()
    {
        return defaultVoiceSource.isPlaying || wifeVoiceSource.isPlaying || policeVoiceSource.isPlaying;
    }

    public bool IsCharacterVoicePlaying(string character)
    {
        return GetAudioSource(character)?.isPlaying ?? false;
    }
}
