using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public enum CharacterType
{
    None,
    Rin_Rock,
    Rin_Rampage,
    Marisa,
    Clear,
}

public enum CharacterAnimationType
{
    None,
    Air_hit_great_1,
    Air_hit_great_2,
    Air_hit_great_3,
    Air_hit_hurt,
    Air_hit_perfect_1,
    Air_hit_perfect_2,
    Air_hit_perfect_3,
    Air_hit_perfect_4,
    Air_press_end,
    Air_press_hurt,
    Die,
    Double_hit_1,
    Double_hit_2,
    Down_hit,
    Down_press_hit,
    Hurt,
    In,
    Jump,
    Jump_hurt,
    Jump_to_down,
    Jump_to_down_hurt,
    Press,
    Road_hit_great_1,
    Road_hit_great_2,
    Road_hit_great_3,
    Road_hit_miss,
    Road_hit_perfect_1,
    Road_hit_perfect_2,
    Road_hit_perfect_3,
    Road_hit_perfect_4,
    Run,
    Up_hit,
    Up_press_hit,
}

[System.Serializable]
public class AnimationDesc
{
    public CharacterAnimationType type;
    public string name; // "" == empty
    public bool loop;
    public bool useUnifiedDuration;
    public float unifiedDuration;
}

[System.Serializable]
public class CharacterDesc
{
    public CharacterType type;
    public SkeletonDataAsset asset;
    public SkeletonDataAsset clearAsset;
    public string clearAssetSkin = "default";
    public SkeletonDataAsset failAsset;
    public string failAssetSkin = "default";
    public bool fly;
    public float hp;
    public List<AnimationDesc> animationDescs;
}

[CreateAssetMenu(fileName = "Character Descriptions", menuName = "Scriptable Object/Character Descriptions", order = int.MaxValue)]
public class CharacterDescs : ScriptableObject
{
    public List<CharacterDesc> descs;
}
