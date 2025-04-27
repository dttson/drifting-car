using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum SoundEffect { Countdown, Go, CarEngine, CarDrift, CarHitObject, Crash, Overtake, FinishLine }

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [Tooltip("Assign a looping AudioSource for background music (BGM)")]
    public AudioSource bgmSource;            // Assign in inspector
    [Tooltip("Assign multiple AudioSources for SFX pooling (order matters)")]
    public AudioSource[] sfxSources;         // Assign in inspector

    private int sfxIndex = 0;
    private int[] sfxSourceKey;              // Maps each SFX source to a SoundEffect enum or -1

    [Header("Volume Settings")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Predefined SFX Clips")]
    public List<SoundEffectClip> sfxClips;
    private Dictionary<SoundEffect, AudioClip> sfxClipDict;

    [System.Serializable]
    public struct SoundEffectClip { public SoundEffect key; public AudioClip clip; }

    void Awake()
    {
        // Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Build clip dictionary
        sfxClipDict = new Dictionary<SoundEffect, AudioClip>();
        foreach (var item in sfxClips)
            if (item.clip != null)
                sfxClipDict[item.key] = item.clip;

        // Initialize source key map
        if (sfxSources != null && sfxSources.Length > 0)
        {
            sfxSourceKey = new int[sfxSources.Length];
            for (int i = 0; i < sfxSourceKey.Length; i++)
                sfxSourceKey[i] = -1;
        }
        else
        {
            Debug.LogWarning("SoundManager: SFX Sources pool is not assigned in the inspector.");
        }

        // Validate BGM source
        if (bgmSource == null)
            Debug.LogWarning("SoundManager: BGM Source is not assigned in the inspector.");

        UpdateVolumes();
    }

    #region Play SFX
    /// <summary>
    /// Play and optionally loop a predefined SoundEffect (stoppable).
    /// </summary>
    public void PlaySFX(SoundEffect effect, bool isLoop = false)
    {
        if (sfxSources == null || sfxSources.Length == 0) return;
        if (!sfxClipDict.TryGetValue(effect, out var clip) || clip == null) return;

        int idx = sfxIndex;
        var src = sfxSources[idx];
        src.clip = clip;
        src.loop = isLoop;
        src.volume = sfxVolume * masterVolume;
        src.Play();

        sfxSourceKey[idx] = (int)effect;
        sfxIndex = (idx + 1) % sfxSources.Length;
    }

    /// <summary>
    /// Play and optionally loop an AudioClip (not tracked by effect).
    /// </summary>
    public void PlaySFX(AudioClip clip, bool isLoop = false)
    {
        if (clip == null || sfxSources == null || sfxSources.Length == 0) return;

        int idx = sfxIndex;
        var src = sfxSources[idx];
        if (isLoop)
        {
            src.clip = clip;
            src.loop = true;
            src.volume = sfxVolume * masterVolume;
            src.Play();
        }
        else
        {
            src.PlayOneShot(clip, sfxVolume * masterVolume);
        }
        sfxSourceKey[idx] = -1;
        sfxIndex = (idx + 1) % sfxSources.Length;
    }

    /// <summary>
    /// Play a one-shot AudioClip from Resources by name, with optional loop.
    /// </summary>
    public void PlaySFX(string clipName, bool isLoop = false)
    {
        var clip = Resources.Load<AudioClip>(clipName);
        PlaySFX(clip, isLoop);
    }

    /// <summary>
    /// Stop any currently playing SFX instances matching the given effect.
    /// </summary>
    public void StopSFX(SoundEffect effect)
    {
        if (sfxSources == null || sfxSourceKey == null) return;
        for (int i = 0; i < sfxSources.Length; i++)
        {
            if (sfxSourceKey[i] == (int)effect)
            {
                sfxSources[i].Stop();
                sfxSourceKey[i] = -1;
            }
        }
    }
    #endregion

    #region Play BGM
    public void PlayBGM(AudioClip clip, bool isLoop = true)
    {
        if (clip == null || bgmSource == null) return;
        bgmSource.clip = clip;
        bgmSource.loop = isLoop;
        bgmSource.volume = bgmVolume * masterVolume;
        bgmSource.Play();
    }

    public void PlayBGM(string clipName, bool isLoop = true)
    {
        var clip = Resources.Load<AudioClip>(clipName);
        PlayBGM(clip, isLoop);
    }

    public void StopBGM()
    {
        if (bgmSource != null && bgmSource.isPlaying)
            bgmSource.Stop();
    }

    public void FadeBGM(float targetVolume, float duration)
    {
        if (bgmSource == null) return;
        DOTween.To(() => bgmSource.volume, v => bgmSource.volume = v,
            Mathf.Clamp01(targetVolume) * masterVolume, duration);
    }
    #endregion

    #region Volume Control
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateVolumes();
    }

    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * masterVolume;
    }

    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    private void UpdateVolumes()
    {
        if (bgmSource != null)
            bgmSource.volume = bgmVolume * masterVolume;
    }
    #endregion

    // Future Enhancements:
    // - Integrate AudioMixer snapshots for advanced volume control
    // - Save/load user volume settings via PlayerPrefs
    // - Add spatial blend support in sfxSources for 3D audio
}
