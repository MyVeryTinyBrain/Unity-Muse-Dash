using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class GhostNote : Note
{
    const string DefaultAnimationName = "standby";

    Spine.Bone airXBone = null;
    Spine.Bone roadXBone = null;
    Spine.Animation airDefaultAnimation = null;
    Spine.Animation roadDefaultAnimation = null;

    public override NoteType GetNoteType()
    {
        return NoteType.Ghost;
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

        airDefaultAnimation = airSpine.skeleton.Data.FindAnimation(DefaultAnimationName);
        roadDefaultAnimation = roadSpine.skeleton.Data.FindAnimation(DefaultAnimationName);

        ClearState();
        SetDefaultAnimationByTime(0);
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

        // 이와 같은 일차식으로 가정합니다.
        // a * x + b = y
        // y = alpha
        // beginHideRatio일때 투명도는 1입니다.
        // a * begin + b = 1
        // beginHideRatio로부터 duration만큼 진행되었을때 투명도는 0입니다.
        // a * (begin + duration) + b = 0
        // a * begin + a * duration + b = 0
        // 
        // a * (begin - begin - duration) + b - b = 1
        // -a * duration = 1
        // -a = 1 / duration
        // a = -1 / duration
        // 
        // b = 1 - a * begin

        float invRatio = 1.0f - ratio;
        float a = -1.0f / data.ghostNoteData.hideDurationRatio;
        float b = 1.0f - a * data.ghostNoteData.beginHideRatio;
        float alpha = a * invRatio + b;
        alpha = Mathf.Clamp(alpha, 0, 1);

        if (!isSelected)
        {
            airSpine.skeleton.SetColor(new Color(1, 1, 1, alpha));
            roadSpine.skeleton.SetColor(new Color(1, 1, 1, alpha));
        }

        float animationTime = mediator.music.adjustedTime - data.time;

        ClearState();
        SetDefaultAnimationByTime(animationTime);
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
        if (time < 0)
        {
            float absTime = Mathf.Abs(time);
            airTime = airDefaultAnimation.Duration - Mathf.Repeat(absTime, airDefaultAnimation.Duration);
            roadTime = roadDefaultAnimation.Duration - Mathf.Repeat(absTime, roadDefaultAnimation.Duration);
        }

        ApplyAnimation(airSpine, airDefaultAnimation, airTime, true);
        ApplyAnimation(roadSpine, roadDefaultAnimation, roadTime, true);
    }
}
