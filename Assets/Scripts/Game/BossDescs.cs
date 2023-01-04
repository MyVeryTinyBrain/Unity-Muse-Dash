using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[System.Serializable]
public struct BossAnimationSubData
{
    public BossState state;
    public bool loop;
}

[System.Serializable]
public struct BossAnimationDesc
{
    public BossAnimationType animationType;
    public BossAnimationSubData subData;
    public string animationName;
}

[System.Serializable]
public class BossDesc
{
    public MapType mapType = MapType._01;
    public SkeletonDataAsset spineAsset;
    public string skin = "default";
    public Effect weapon1Effect;
    public string weapon1EffectSpawnBone;
    public Effect weapon2Effect;
    public string weapon2EffectSpawnBone;
    public List<BossAnimationDesc> animationDescs;
}

[CreateAssetMenu(fileName = "Boss Descriptions", menuName = "Scriptable Object/Boss Descriptions", order = int.MaxValue)]
public class BossDescs : ScriptableObject
{
    public List<BossDesc> descs;
}
