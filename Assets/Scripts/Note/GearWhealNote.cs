using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class GearWhealNote : Note
{
    const string DefaultAnimationName = "in_nor_33";

    Spine.Bone airXBone = null;
    Spine.Bone roadXBone = null;
    Spine.Animation airDefaultAnimation = null;
    Spine.Animation roadDefaultAnimation = null;

    public override NoteType GetNoteType()
    {
        return NoteType.GearWheal;
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

        float animationTime = mediator.music.adjustedTime - data.time;

        ClearState();
        SetDefaultAnimationByTime(animationTime);
    }

    protected override void NoteUpdate()
    {
        base.NoteUpdate();

        if (!mediator.gameSettings.isEditor && isHitable && IsOnGearWehalNotePassRange())
        {
            mediator.gameState.Pass(data.position, defaultScore);
            SetToNonHitable();
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
        if (time < 0)
        {
            float absTime = Mathf.Abs(time);
            airTime = airDefaultAnimation.Duration - Mathf.Repeat(absTime, airDefaultAnimation.Duration);
            roadTime = roadDefaultAnimation.Duration - Mathf.Repeat(absTime, roadDefaultAnimation.Duration);
        }

        ApplyAnimation(airSpine, airDefaultAnimation, airTime, true);
        ApplyAnimation(roadSpine, roadDefaultAnimation, roadTime, true);
    }

    protected override NoteResult OnHit(NotePosition notePosition)
    {
        return NoteResult.None();
    }

    protected override bool CheckEndHitable()
    {
        return false;
    }

    public override int GetMaxCombo()
    {
        return 0;
    }
}
