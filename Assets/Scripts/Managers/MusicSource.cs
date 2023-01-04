using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicSource : MonoBehaviour
{
    IMusicSourcePlayMethod playMethod = new MusicSourceDefault();
    public event IMusicSourcePlayMethod.OnStartMusicDelegate OnStartMusic;
    public event IMusicSourcePlayMethod.OnEndPlayDelegate OnEndPlay;
    AudioSource audioSource = null;

    public IMusicSourcePlayMethod musicPlayMethod
    {
        get => playMethod;
        set
        {
            playMethod.OnStartMusic += this.OnStartMusic;
            playMethod.OnEndPlay += this.OnEndPlay;
            playMethod.Stop(audioSource);

            value.Init(audioSource);
            value.OnStartMusic += this.OnStartMusic;
            value.OnEndPlay += this.OnEndPlay;
            playMethod = value;
        }
    }

    public AudioClip clip
    {
        get => audioSource.clip;
        set => audioSource.clip = value;
    }

    public bool isPlaying
    {
        get => playMethod.IsPlaying(audioSource);
    }

    public bool isPaused
    {
        get => playMethod.IsPaused(audioSource);
    }

    public float playingTime
    {
        get => playMethod.GetPlayingTime(audioSource);
        set => playMethod.SetPlayingTime(audioSource, value);
    }

    public int playingTimeSample
    {
        get => playMethod.GetPlayingTimeSample(audioSource);
        set => playMethod.SetPlayingTimeSample(audioSource, value);
    }

    public float normalizedPlayingTime
    {
        get => playMethod.GetNormalizedPlayingTime(audioSource);
        set => playMethod.SetNormalizedPlayingTime(audioSource, value);
    }

    public float volume
    {
        get => audioSource.volume;
        set => audioSource.volume = value;
    }

    public float pitch
    {
        get => playMethod.GetPitch(audioSource);
        set => playMethod.SetPitch(audioSource, value);
    }

    public int frequency
    {
        get => audioSource.clip ? audioSource.clip.frequency : 0;
    }

    public int timeSamples
    {
        get => audioSource.timeSamples;
    }

    public float length
    {
        get => audioSource.clip ? MusicUtility.TimeSamplesToTime(audioSource.clip.samples, audioSource.clip.frequency) : 0;
    }

    public void Play()
    {
        playMethod.Play(audioSource);
    }

    public void PlayAt(float time)
    {
        playMethod.PlayAt(audioSource, time);
    }

    public void Stop()
    {
        playMethod.Stop(audioSource);
    }

    public void Pause()
    {
        playMethod.Pause(audioSource);
    }

    public void Resume()
    {
        playMethod.Resume(audioSource);
    }

    public void TogglePause()
    {
        playMethod.TogglePause(audioSource);
    }

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        playMethod.Update(audioSource);
    }
}

public interface IMusicSourcePlayMethod
{
    public delegate void OnStartMusicDelegate();
    public delegate void OnEndPlayDelegate();
    public event OnStartMusicDelegate OnStartMusic;
    public event OnEndPlayDelegate OnEndPlay;
    public void Init(AudioSource audioSource);
    public void Update(AudioSource audioSource);
    public float GetPlayingTime(AudioSource audioSource);
    public void SetPlayingTime(AudioSource audioSource, float time);
    public int GetPlayingTimeSample(AudioSource audioSource);
    public void SetPlayingTimeSample(AudioSource audioSource, int timeSample);
    public float GetNormalizedPlayingTime(AudioSource audioSource);
    public void SetNormalizedPlayingTime(AudioSource audioSource, float normalizedPlayingTime);
    public float GetPitch(AudioSource audioSource);
    public void SetPitch(AudioSource audioSource, float pitch);
    public void Play(AudioSource audioSource);
    public void PlayAt(AudioSource audioSource, float time);
    public void Stop(AudioSource audioSource);
    public void Pause(AudioSource audioSource);
    public void Resume(AudioSource audioSource);
    public void TogglePause(AudioSource audioSource);
    public bool IsPlaying(AudioSource audioSource);
    public bool IsPaused(AudioSource audioSource);
}

public class MusicSourceDefault : IMusicSourcePlayMethod
{
    public event IMusicSourcePlayMethod.OnStartMusicDelegate OnStartMusic;
    public event IMusicSourcePlayMethod.OnEndPlayDelegate OnEndPlay;

    float pitchMultiplier = 1.0f;
    bool isPlaying = false;
    bool isPaused = false;

    public void Init(AudioSource audioSource)
    {
    }

    public void Update(AudioSource audioSource)
    {
        audioSource.pitch = pitchMultiplier * (isPaused ? 0f : 1f) * Time.timeScale;

        if (isPlaying && !audioSource.isPlaying && Application.isFocused)
        {
            OnEndPlay?.Invoke();
            Stop(audioSource);
        }
    }

    public float GetPlayingTime(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return 0;
        }
        return audioSource.clip ? MusicUtility.TimeSamplesToTime(audioSource.timeSamples, audioSource.clip.frequency) : 0;
    }

    public void SetPlayingTime(AudioSource audioSource, float time)
    {
        if (audioSource.clip)
        {
            Stop(audioSource);
            int timeSamples = MusicUtility.TimeToTimeSamples(time, audioSource.clip.frequency);
            SetPlayingTimeSample(audioSource, timeSamples);
        }
        else
        {
            SetPlayingTimeSample(audioSource, 0);
        }
    }

    public int GetPlayingTimeSample(AudioSource audioSource)
    {
        return audioSource.timeSamples;
    }

    public void SetPlayingTimeSample(AudioSource audioSource, int timeSample)
    {
        Stop(audioSource);
        audioSource.timeSamples = Mathf.Clamp(timeSample, 0, int.MaxValue);
    }

    public float GetNormalizedPlayingTime(AudioSource audioSource)
    {
        if (!audioSource.clip)
        {
            return 0.0f;
        }
        return audioSource.timeSamples / (float)audioSource.clip.samples;
    }

    public void SetNormalizedPlayingTime(AudioSource audioSource, float normalizedPlayingTime)
    {
        if (audioSource.clip)
        {
            Stop(audioSource);
            int timeSample = (int)(normalizedPlayingTime * audioSource.clip.samples);
            SetPlayingTimeSample(audioSource, timeSample);
        }
    }

    public float GetPitch(AudioSource audioSource)
    {
        return pitchMultiplier;
    }

    public void SetPitch(AudioSource audioSource, float pitch)
    {
        pitchMultiplier = pitch;
    }

    public void Play(AudioSource audioSource)
    {
        audioSource.Play();
        audioSource.timeSamples = 0;
        isPaused = false;
        isPlaying = true;

        OnStartMusic?.Invoke();
    }

    public void PlayAt(AudioSource audioSource, float time)
    {
        audioSource.Play();
        audioSource.time = time;
        isPaused = false;
        isPlaying = true;

        OnStartMusic?.Invoke();
    }

    public void Stop(AudioSource audioSource)
    {
        audioSource.timeSamples = 0;
        audioSource.Stop();
        isPaused = false;
        isPlaying = false;
    }

    public void Pause(AudioSource audioSource)
    {
        isPaused = true;
    }

    public void Resume(AudioSource audioSource)
    {
        isPaused = false;
    }

    public void TogglePause(AudioSource audioSource)
    {
        if (!audioSource.isPlaying)
        {
            PlayAt(audioSource, GetPlayingTime(audioSource));
        }
        else
        {
            isPaused = !isPaused;
        }
    }

    public bool IsPlaying(AudioSource audioSource)
    {
        return isPlaying;
    }

    public bool IsPaused(AudioSource audioSource)
    {
        return isPaused;
    }
}

public class MusicSourcePrecision : IMusicSourcePlayMethod
{
    public event IMusicSourcePlayMethod.OnStartMusicDelegate OnStartMusic;
    public event IMusicSourcePlayMethod.OnEndPlayDelegate OnEndPlay;

    const float MinPlayScheduledDelay = 0.5f;
    float scheduleDelay = MinPlayScheduledDelay;
    float scheduleStartTime = 0.0f;

    public float startDelay = 0.0f;
    public float endDelay = 0.0f;

    bool isPlaying = false;
    bool isCallableOnStartPlay = false;

    public float playScheduledDelay
    {
        get => scheduleDelay;
        set => scheduleDelay = Mathf.Clamp(value, MinPlayScheduledDelay, float.MaxValue);
    }

    public void Init(AudioSource audioSource)
    {
        audioSource.pitch = 1.0f;
    }

    public void Update(AudioSource audioSource)
    {
        if (isPlaying && isCallableOnStartPlay && GetPlayingTime(audioSource) > 0.0f)
        {
            isCallableOnStartPlay = false;
            OnStartMusic?.Invoke();
        }

        if (isPlaying && GetPlayingTime(audioSource) > audioSource.clip.length + endDelay)
        {
            OnEndPlay?.Invoke();
            Stop(audioSource);
        }
    }

    public float GetPlayingTime(AudioSource audioSource)
    {
        if (audioSource == null)
        {
            return 0;
        }

        return (float)AudioSettings.dspTime - scheduleStartTime;
    }

    public void SetPlayingTime(AudioSource audioSource, float time)
    {
        Debug.LogError("Can't change playing time or time sample using MusicSourcePrecision");
    }

    public int GetPlayingTimeSample(AudioSource audioSource)
    {
        float playingTime = GetPlayingTime(audioSource);
        int playingTimeSample = MusicUtility.TimeToTimeSamples(playingTime, audioSource.clip.frequency);
        return playingTimeSample;
    }

    public void SetPlayingTimeSample(AudioSource audioSource, int timeSample)
    {
        Debug.LogError("Can't change playing time or time sample using MusicSourcePrecision");
    }

    public float GetNormalizedPlayingTime(AudioSource audioSource)
    {
        if (!audioSource.clip)
        {
            return 0.0f;
        }
        return GetPlayingTime(audioSource) / (float)audioSource.clip.length;
    }

    public void SetNormalizedPlayingTime(AudioSource audioSource, float normalizedPlayingTime)
    {
        Debug.LogError("Can't change playing time or time sample using MusicSourcePrecision");
    }

    public float GetPitch(AudioSource audioSource)
    {
        return audioSource.pitch;
    }

    public void SetPitch(AudioSource audioSource, float pitch)
    {
        Debug.LogError("Can't change pitch using MusicSourcePrecision");
    }

    public void Play(AudioSource audioSource)
    {
        scheduleStartTime = (float)AudioSettings.dspTime + scheduleDelay + startDelay;
        audioSource.PlayScheduled(scheduleStartTime);
        isPlaying = true;
        isCallableOnStartPlay = true;
    }

    public void PlayAt(AudioSource audioSource, float time)
    {
    }

    public void Stop(AudioSource audioSource)
    {
        isPlaying = false;
        isCallableOnStartPlay = false;
        audioSource.Stop();
    }

    public void Pause(AudioSource audioSource)
    {
        if (AudioListener.pause)
        {
            return;
        }

        AudioListener.pause = true;
    }

    public void Resume(AudioSource audioSource)
    {
        if (!AudioListener.pause)
        {
            return;
        }

        AudioListener.pause = false;
    }

    public void TogglePause(AudioSource audioSource)
    {
        if (!audioSource.isPlaying)
        {
            Play(audioSource);
            AudioListener.pause = false;
        }
        else
        {
            AudioListener.pause = !AudioListener.pause;
        }
    }

    public bool IsPlaying(AudioSource audioSource)
    {
        return isPlaying;
    }

    public bool IsPaused(AudioSource audioSource)
    {
        return AudioListener.pause;
    }
}
