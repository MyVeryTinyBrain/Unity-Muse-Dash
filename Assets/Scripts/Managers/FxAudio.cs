using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class FxAudio : MonoBehaviour
{
    [SerializeField]
    FXAudios fxAudiosAsset = null;

    new AudioSource audio = null;
    Dictionary<FXA, FXAudioDesc> clips = new Dictionary<FXA, FXAudioDesc>();

    void GenerateClipDictionary()
    {
        clips.Clear();
        foreach(FXAudioDesc desc in fxAudiosAsset.audios)
        {
            clips.Add(desc.fxAudioType, desc);
        }
    }

    void TryGenerateClipDictionary()
    {
        if (clips.Count == 0)
        {
            GenerateClipDictionary();
        }
    }

    public void Play(FXA fxAudioType)
    {
        TryGenerateClipDictionary();

        FXAudioDesc desc = null;
        if(clips.TryGetValue(fxAudioType, out desc))
        {
            audio.PlayOneShot(desc.clip, desc.volume);
        }
    }

    private void Awake()
    {
        audio = GetComponent<AudioSource>();

        TryGenerateClipDictionary();
    }
}
