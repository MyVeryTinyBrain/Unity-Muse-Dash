using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class BossRegularNote : BossNote
{
    [System.Serializable]
    public struct AnimationDesc
    {
        public string air;
        public float airHitRatio;
        public float airSpeedScale;
        public string road;
        public float roadHitRatio;
        public float roadSpeedScale;
    }

    [SerializeField]
    List<AnimationDesc> animations;

    [SerializeField]
    BossProjectileWeapon bossProjectileWeapon = BossProjectileWeapon.Weapon1;

    Spine.Animation airAnimation = null;
    public float airHitRatio = 0.0f;
    public float airSpeedScale = 0.0f;
    Spine.Animation roadAnimation = null;
    public float roadHitRatio = 0.0f;
    public float roadSpeedScale = 0.0f;

    public override NoteType GetNoteType()
    {
        return NoteType.BossRegular;
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
        airHitRatio = animations[(int)mapType].airHitRatio;
        airSpeedScale = animations[(int)mapType].airSpeedScale;
        roadAnimation = roadSpine.skeleton.Data.FindAnimation(animations[(int)mapType].air);
        roadHitRatio = animations[(int)mapType].roadHitRatio;
        roadSpeedScale = animations[(int)mapType].roadSpeedScale;
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

        float animationNormalizedHitRatio = 0.0f;
        switch (data.position)
        {
            case NotePosition.Air:
            animationNormalizedHitRatio = airHitRatio;
            break;

            case NotePosition.Road:
            animationNormalizedHitRatio = roadHitRatio;
            break;
        }

        // 1. invRatio가 0일때 정규화된 애니메이션 시간은 0입니다.
        // 2. invRatio가 1일때 정규화된 애니메이션 시간은 animationNormalizedHitRatio일때 입니다.
        // ax + b = y : a * invRatio + b = animationRatio 라고 할 때
        // 1. a * 0 + b = 0
        // 2. a * 1 + b = animationNormalizedHitRatio
        // a = animationNormalizedHitRatio
        // b = 0
        // animationNormalizedHitRatio * invRatio = animationRatio

        float speedMultiplier = GetSpeedMultiplier();
        float invRatio = 1.0f - ratio * speedMultiplier;
        float animationRatio = animationNormalizedHitRatio * invRatio;
        animationRatio = Mathf.Clamp(animationRatio, 0.0f, 1.0f);

        ClearState();
        SetAnimationByRatio(animationRatio);
    }

    protected override void UpdateBossAnimationData()
    {
        base.UpdateBossAnimationData();

        // 구하려는 것
        // 애니메이션 시간이 0인 '음악 재생시간'

        // invRatio가 0일때 정규화된 애니메이션 시간은 0입니다.
        // invRatio = 0 = 1.0f - ratio * speedMultiplier
        // 0 = 1 - ratio * speedMultiplier
        // ratio * speedMultiplier = 1
        // ratio = 1 / speedMultiplier

        // ratio가 speedMultiplier의 역수일 때 정규화된 애니메이션 시간은 0입니다.
        // ratio 계산식은 'Ratio = (Time - PlayingTime) * FieldLength' 이고
        // 노트의 ratio 계산은 'noteRatio = MusicUtility.TimeToRatio(data.time, data.speed * mediator.GetSpeedScaleAtTime(data.time), playingTime)' 입니다.
        // 따라서 FieldLength = data.speed * mediator.GetSpeedScaleAtTime(data.time) 입니다.
        // 여기서 최종적으로 PlayingTime을 구하면 애니메이션 시간이 0인 음악 재생시간을 구합니다.

        // 1 / speedMultiplier = (data.time - x) * data.speed * mediator.GetSpeedScaleAtTime(data.time)
        // 1 / (speedMultiplier * data.speed * mediator.GetSpeedScaleAtTime(data.time)) = data.time - x
        // -data.time + 1 / (speedMultiplier * data.speed * mediator.GetSpeedScaleAtTime(data.time)) = -x
        // data.time - 1 / (speedMultiplier * data.speed * mediator.GetSpeedScaleAtTime(data.time)) = x

        float speedMultiplier = GetSpeedMultiplier();
        float zeroAnimationTime = data.time - 1 / (speedMultiplier * data.speed * mediator.music.adjustedTimeGlobalSpeedScale);
        bossAnimationData.time = zeroAnimationTime;

        bossAnimationData.speed = data.speed;

        switch (bossProjectileWeapon)
        {
            case BossProjectileWeapon.Weapon1:
            switch (data.position)
            {
                case NotePosition.Air:
                bossAnimationData.type = BossAnimationType.Attack1_Air;
                break;

                case NotePosition.Road:
                bossAnimationData.type = BossAnimationType.Attack1_Road;
                break;
            }
            break;

            case BossProjectileWeapon.Weapon2:
            bossAnimationData.type = BossAnimationType.Attack2;
            break;
        } 

        bossAnimationData.useUnifiedDuration = false;
        bossAnimationData.unifiedDuration = 0;
    }

    private void SetAnimationByRatio(float ratio)
    {
        bool visible = ratio > 0.0f;

        if (airSpine != null)
        {
            airSpine.enabled = visible;
        }
        if (centerSpine != null)
        {
            centerSpine.enabled = visible;
        }
        if (roadSpine != null)
        {
            roadSpine.enabled = visible;
        }

        ApplyAnimation(airSpine, airAnimation, ratio * airAnimation.Duration, false);
        ApplyAnimation(roadSpine, roadAnimation, ratio * roadAnimation.Duration, false);
    }

    private float GetSpeedMultiplier()
    {
        float speedScale = 0.0f;
        switch (data.position)
        {
            case NotePosition.Air:
            speedScale = airSpeedScale;
            break;

            case NotePosition.Road:
            speedScale = roadSpeedScale;
            break;

            default:
            return 0;
        }

        float speedMultiplier = speedScale * (mediator.gameSettings.lengthPerSeconds / 21.0f);
        return speedMultiplier;
    }
}
