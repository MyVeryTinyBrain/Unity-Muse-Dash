using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FXA
{
    None = 0,

    V_Fever = 1,
    V_FullCombo = 2,
    V_321 = 3,
    V_ReadyGO = 4,
}

[System.Serializable]
public class FXAudioDesc
{
    public FXA fxAudioType = FXA.None;
    public AudioClip clip = null;
    public float volume = 1.0f;
}

[CreateAssetMenu(fileName = "FXAudios", menuName = "Scriptable Object/FXAudios", order = int.MaxValue)]
public class FXAudios : ScriptableObject
{
    public List<FXAudioDesc> audios;
}
