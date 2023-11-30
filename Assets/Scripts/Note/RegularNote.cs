using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class RegularNote : Note
{
    [System.Serializable]
    public struct AnimationDesc
    {
        public string airDefault;
        public string airFromUp;
        public string airFromDown;
        public float airFromUpHeightRatio;
        public float airFromDownHeightRatio;
        public string roadDefault;
        public string roadFromUp;
        public string roadFromDown;
        public float roadFromUpHeightRatio;
        public float roadFromDownHeightRatio;
    }

    [SerializeField]
    List<AnimationDesc> animations;

    Spine.Bone airXBone = null;
    Spine.Bone roadXBone = null;
    Spine.Animation airDefaultAnimation = null;
    Spine.Animation airFromUpAnimation = null;
    Spine.Animation airFromDownAnimation = null;
    float airFromUpHeightRatio = 0.0f;
    float airFromDownHeightRatio = 0.0f;
    Spine.Animation roadDefaultAnimation = null;
    Spine.Animation roadFromUpAnimation = null;
    Spine.Animation roadFromDownAnimation = null;
    float roadFromUpHeightRatio = 0.0f;
    float roadFromDownHeightRatio = 0.0f;

    Spine.Animation airFromAnimation = null;
    Spine.Animation roadFromAnimation = null;
    float airFromHeightRatio = 0.0f;
    float roadFromHeightRatio = 0.0f;

    public override NoteType GetNoteType()
    {
        return NoteType.Regular;
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

        airXBone = airSpine.Skeleton.FindBone("X");
        roadXBone = roadSpine.Skeleton.FindBone("X");

        airDefaultAnimation = airSpine.skeleton.Data.FindAnimation(animations[(int)mapType].airDefault);
        airFromUpAnimation = airSpine.skeleton.Data.FindAnimation(animations[(int)mapType].airFromUp);
        airFromDownAnimation = airSpine.skeleton.Data.FindAnimation(animations[(int)mapType].airFromDown);
        airFromUpHeightRatio = animations[(int)mapType].airFromUpHeightRatio;
        airFromDownHeightRatio = animations[(int)mapType].airFromDownHeightRatio;
        roadDefaultAnimation = roadSpine.skeleton.Data.FindAnimation(animations[(int)mapType].roadDefault);
        roadFromUpAnimation = roadSpine.skeleton.Data.FindAnimation(animations[(int)mapType].roadFromUp);
        roadFromDownAnimation = roadSpine.skeleton.Data.FindAnimation(animations[(int)mapType].roadFromDown);
        roadFromUpHeightRatio = animations[(int)mapType].roadFromUpHeightRatio;
        roadFromDownHeightRatio = animations[(int)mapType].roadFromDownHeightRatio;

        ClearState();
        switch (data.regularNoteData.noteInType)
        {
            case RegularNoteInType.Default:
            SetDefaultAnimationByTime(0);
            break;

            case RegularNoteInType.FromUp:
            airFromAnimation = airFromUpAnimation;
            airFromHeightRatio = airFromUpHeightRatio;
            roadFromAnimation = roadFromUpAnimation;
            roadFromHeightRatio = roadFromUpHeightRatio;
            SetFromAnimationByRatio(0);
            break;

            case RegularNoteInType.FromDown:
            airFromAnimation = airFromDownAnimation;
            airFromHeightRatio = airFromDownHeightRatio;
            roadFromAnimation = roadFromDownAnimation;
            roadFromHeightRatio = roadFromDownHeightRatio;
            SetFromAnimationByRatio(0);
            break;
        }
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();
        float y = GetHitLocalY();
        transform.localPosition = new Vector2(ratio * mediator.gameSettings.lengthPerSeconds, y);
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();
        switch (data.regularNoteData.noteInType)
        {
            case RegularNoteInType.Default:
            {
                float animationTime = mediator.music.adjustedTime - data.time;
                ClearState();
                SetDefaultAnimationByTime(animationTime);
            }
            break;
            case RegularNoteInType.FromUp:
            case RegularNoteInType.FromDown:
            {
                ClearState();
                SetFromAnimationByRatio(ratio);
            }
            break;
        }
    }

    protected override void Spine_Update()
    {
        airXBone?.SetLocalPosition(Vector2.zero);
        roadXBone?.SetLocalPosition(Vector2.zero);
    }

    private void SetDefaultAnimationByTime(float time)
    {
        float airTime = time;
        float roadTime = time;

        // 음수 루프 처리
        if(time < 0)
        {
            float absTime = Mathf.Abs(time);
            airTime = airDefaultAnimation.Duration - Mathf.Repeat(absTime, airDefaultAnimation.Duration);
            roadTime = roadDefaultAnimation.Duration - Mathf.Repeat(absTime, roadDefaultAnimation.Duration);
        }

        ApplyAnimation(airSpine, airDefaultAnimation, airTime, true);
        ApplyAnimation(roadSpine, roadDefaultAnimation, roadTime, true);
    }

    private void SetFromAnimationByRatio(float ratio)
    {
        SetAirFromAnimationByRatio(ratio);
        SetRoadFromAnimationByRatio(ratio);
    }

    // 1. invRatio가 0일때 정규화된 애니메이션 시간은 0입니다.
    // 2. invRatio가 endInRatio일때 정규화된 애니메이션 시간은 animationNormalizedHitRatio 입니다.
    // ax + b = y : a * invRatio + b = animationRatio 라고 할 때
    // 1. a * 0 + b = 0
    // 2. a * endInRatio + b = animationNormalizedHitRatio
    // a = animationNormalizedHitRatio / endInRatio
    // b = 0
    // (animationNormalizedHitRatio / endInRatio) * invRatio = animationRatio

    private void SetAirFromAnimationByRatio(float ratio)
    {
        float invRatio = 1.0f - ratio;
        float animationRatio = (airFromHeightRatio / data.regularNoteData.endInRatio) * invRatio;
        animationRatio = Mathf.Clamp(animationRatio, 0.0f, 1.0f);

        ApplyAnimation(airSpine, airFromAnimation, animationRatio * airFromAnimation.Duration, false);
    }

    private void SetRoadFromAnimationByRatio(float ratio)
    {
        float invRatio = 1.0f - ratio;
        float animationRatio = (roadFromHeightRatio / data.regularNoteData.endInRatio) * invRatio;
        animationRatio = Mathf.Clamp(animationRatio, 0.0f, 1.0f);

        ApplyAnimation(roadSpine, roadFromAnimation, animationRatio * roadFromAnimation.Duration, false);
    }
}
