using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class HammerNote : Note
{
    [System.Serializable]
    public struct AnimationDesc
    {
        public string air;
        public float airHitRatio;
        public string road;
        public float roadHitRatio;
    }

    [SerializeField]
    List<AnimationDesc> animations;

    Spine.Animation airAnimation = null;
    float airHitRatio = 0.0f;
    Spine.Animation roadAnimation = null;
    float roadHitRatio = 0.0f;

    public override NoteType GetNoteType()
    {
        return NoteType.Hammer;
    }

    public override NoteVisualizeMethod GetNoteVisualizeMethod()
    {
        return NoteVisualizeMethod.Spine;
    }

    public override SkeletonDataAsset GetNoteVisualizeSpineDataAtTime(float playingTime)
    {
        MapType mapType = mediator.music.GetMapTypeAtTime(playingTime);
        return airSpineDatas[(int)mapType];
    }

    public override Sprite GetNoteVisualizeSpriteDataAtTime(float playingTime)
    {
        return null;
    }

    public override void Init() 
    {
        base.Init();

        airAnimation = airSpine.skeleton.Data.FindAnimation(animations[(int)mapType].air);
        roadAnimation = roadSpine.skeleton.Data.FindAnimation(animations[(int)mapType].road);
        airHitRatio = animations[(int)mapType].airHitRatio;
        roadHitRatio = animations[(int)mapType].roadHitRatio;

        ClearState();
        SetAnimationByRatio(0);
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();

        float y = GetHitLocalY();
        transform.localPosition = new Vector2(0, y);
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        ClearState();
        SetAnimationByRatio(ratio);
    }

    private void SetAnimationByRatio(float ratio)
    {
        SetAirAnimationByRatio(ratio);
        SetRoadAnimationByRatio(ratio);
    }

    // 1. invRatio가 0일때 정규화된 애니메이션 시간은 0입니다.
    // 2. invRatio가 1(타격점)일때 정규화된 애니메이션 시간은 animationNormalizedHitRatio(노트가 타격점에 위치하게 되는 정규화된 애니메이션 시간)입니다.
    // ax + b = y : a * invRatio + b = animationRatio 라고 할 때
    // 1. a * 0 + b = 0
    // 2. a * 1 + b = animationNormalizedHitRatio
    // b = 0
    // a = animationNormalizedHitRatio
    // animationNormalizedHitRatio * invRatio = animationRatio

    private void SetAirAnimationByRatio(float ratio)
    {
        float invRatio = 1.0f - ratio;
        float a = airHitRatio;
        float animationRatio = a * invRatio;
        animationRatio = Mathf.Clamp(animationRatio, 0.0f, 1.0f);

        ApplyAnimation(airSpine, airAnimation, animationRatio * airAnimation.Duration, false);
    }

    private void SetRoadAnimationByRatio(float ratio)
    {
        float invRatio = 1.0f - ratio;
        float a = roadHitRatio;
        float animationRatio = a * invRatio;
        animationRatio = Mathf.Clamp(animationRatio, 0.0f, 1.0f);

        ApplyAnimation(roadSpine, roadAnimation, animationRatio * roadAnimation.Duration, false);
    }

    protected override void CustomSpawnDyingEffectPosition(NotePosition notePosition, ref Vector2 worldPosition)
    {
        switch (notePosition)
        {
            case NotePosition.Air:
            worldPosition = airSpine.transform.position;
            break;

            case NotePosition.Road:
            worldPosition = roadSpine.transform.position;
            break;
        }
    }
}
