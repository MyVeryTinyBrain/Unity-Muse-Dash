using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class BeamNote : Note
{
    [System.Serializable]
    public struct AnimationDesc
    {
        public string air;
        public string road;
        public float hitRatio;
        public float attackRatio;
    }

    [SerializeField]
    List<AnimationDesc> defaultAnimations;

    [SerializeField]
    List<AnimationDesc> brokenAnimations;

    [SerializeField]
    GameObject airPivot = null;

    [SerializeField]
    GameObject roadPivot = null;

    Spine.Bone airXBone = null;
    Spine.Bone roadXBone = null;
    Spine.Animation airDefaultAnimation = null;
    Spine.Animation roadDefaultAnimation = null;
    float defaultHitRatio = 0.0f;
    float defaultAttackRatio = 0.0f;
    Spine.Animation airBrokenAnimation = null;
    Spine.Animation roadBrokenAnimation = null;
    float brokenHitRatio = 0.0f;
    float brokenAttackRatio = 0.0f;

    bool airAliveFlag = true;
    bool roadAliveFlag = true;

    private bool airAlive
    {
        get => airAliveFlag;
        set
        {
            airAliveFlag = value;
            airSpine.gameObject.SetActive(value);
        }
    }

    private bool roadAlive
    {
        get => roadAliveFlag;
        set
        {
            roadAliveFlag = value;
            roadSpine.gameObject.SetActive(value);
        }
    }

    public override NoteType GetNoteType()
    {
        return NoteType.Beam;
    }

    public override NoteVisualizeMethod GetNoteVisualizeMethod()
    {
        return NoteVisualizeMethod.Spine;
    }

    public override SkeletonDataAsset GetNoteVisualizeSpineDataAtTime(float playingTime)
    {
        MapType mapType = mediator.music.GetMapTypeAtTime(playingTime);
        return roadSpineDatas[(int)mapType];
    }

    public override Sprite GetNoteVisualizeSpriteDataAtTime(float playingTime)
    {
        return null;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        placeableDesc.Air = false;
        placeableDesc.Center = true;
        placeableDesc.Road = false;
    }

    public override void Init()
    {
        base.Init();

        airXBone = airSpine.Skeleton.FindBone("X");
        roadXBone = roadSpine.Skeleton.FindBone("X");

        airDefaultAnimation = airSpine.skeleton.Data.FindAnimation(defaultAnimations[(int)mapType].air);
        roadDefaultAnimation = roadSpine.skeleton.Data.FindAnimation(defaultAnimations[(int)mapType].road);
        defaultHitRatio = defaultAnimations[(int)mapType].hitRatio;
        defaultAttackRatio = defaultAnimations[(int)mapType].attackRatio;
        airBrokenAnimation = airSpine.skeleton.Data.FindAnimation(brokenAnimations[(int)mapType].air);
        roadBrokenAnimation = roadSpine.skeleton.Data.FindAnimation(brokenAnimations[(int)mapType].road);
        brokenHitRatio = brokenAnimations[(int)mapType].hitRatio;
        brokenAttackRatio = brokenAnimations[(int)mapType].attackRatio;

        ClearState();
        SetDefaultAnimationByTime(0);
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();

        Vector2 localAirPivotPos = airPivot.transform.localPosition;
        Vector2 localRoadPivotPos = roadPivot.transform.localPosition;
        float localAirHeight = airPivot.transform.InverseTransformVector(0, mediator.hitPoint.airWorldHeight, 0).y;
        float localRoadHeight = roadPivot.transform.InverseTransformVector(0, mediator.hitPoint.roadWorldHeight, 0).y;
        localAirPivotPos.y = localAirHeight;
        localRoadPivotPos.y = -localRoadHeight;
        airPivot.transform.localPosition = localAirPivotPos;
        roadPivot.transform.localPosition = localRoadPivotPos;

        float y = GetHitLocalY();
        transform.localPosition = new Vector2(ratio * mediator.gameSettings.lengthPerSeconds, y);
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        float characterRatio = mediator.character.ratioOfCharacterPos;
        float invCharacterRatio = 1 - characterRatio;
        float invRatio = 1 - ratio;

        // 1. invRatio가 1일때 정규화된 애니메이션 시간은 AnimationNormalizedHitRatio 입니다.
        // 2. invRatio가 invCharacterRatio일때 정규화된 애니메이션 시간은 AnimationNormalizedBeamRatio 입니다.
        // ax + b = y : a * invRatio + b = animationRatio 라고 할 때
        // 1. a * 1 + b = AnimationNormalizedHitRatio
        // 2. a * invCharacterRatio + b = AnimationNormalizedBeamRatio
        // a = (AnimationNormalizedBeamRatio - AnimationNormalizedHitRatio) / (invCharacterRatio - 1)
        // b = AnimationNormalizedHitRatio - (AnimationNormalizedBeamRatio - AnimationNormalizedHitRatio) / (invCharacterRatio - 1)
        //
        // (AnimationNormalizedBeamRatio - AnimationNormalizedHitRatio) / (invCharacterRatio - 1) * invRatio +
        // AnimationNormalizedHitRatio - (AnimationNormalizedBeamRatio - AnimationNormalizedHitRatio) / (invCharacterRatio - 1) =
        // animationRatio

        bool defaultAnimation = airAlive && roadAlive;
        float hitRatio = 0.58f;
        float beamRatio = 0.61f;
        if (defaultAnimation)
        {
            hitRatio = defaultHitRatio;
            beamRatio = defaultAttackRatio;
        }
        else
        {
            hitRatio = brokenHitRatio;
            beamRatio = brokenAttackRatio;
        }

        float animationRatio =
            (beamRatio - hitRatio) / (invCharacterRatio - 1) * invRatio +
            hitRatio - (beamRatio - hitRatio) / (invCharacterRatio - 1);

        ClearState();
        if (defaultAnimation)
        {
            SetDefaultAnimationByRatio(animationRatio);
        }
        else
        {
            SetBrokenAnimationByRatio(animationRatio);
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

        if (time < 0)
        {
            float absTime = Mathf.Abs(time);
            airTime = airDefaultAnimation.Duration - Mathf.Repeat(absTime, airDefaultAnimation.Duration);
            roadTime = roadDefaultAnimation.Duration - Mathf.Repeat(absTime, roadDefaultAnimation.Duration);
        }

        ApplyAnimation(airSpine, airDefaultAnimation, airTime, true);
        ApplyAnimation(roadSpine, roadDefaultAnimation, roadTime, true);
    }

    private void SetDefaultAnimationByRatio(float ratio)
    {
        // 기본 애니메이션은 루프되도록 구현합니다.
        ratio = Mathf.Repeat(ratio, 1.0f);

        ApplyAnimation(airSpine, airDefaultAnimation, ratio * airDefaultAnimation.Duration, false);
        ApplyAnimation(roadSpine, roadDefaultAnimation, ratio * roadDefaultAnimation.Duration, false);
    }

    private void SetBrokenAnimationByRatio(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0.0f, 1.0f);

        ApplyAnimation(airSpine, airBrokenAnimation, ratio * airBrokenAnimation.Duration, false);
        ApplyAnimation(roadSpine, roadBrokenAnimation, ratio * roadBrokenAnimation.Duration, false);
    }

    protected override NoteResult OnHit(NotePosition notePosition)
    {
        if (isDead)
        {
            return NoteResult.None();
        }

        float diff = Mathf.Abs(mediator.music.adjustedTime - data.time);
        if (diff > mediator.gameSettings.greatDiffTime)
        {
            return NoteResult.None();
        }

        switch (notePosition)
        {
            case NotePosition.Air:
            if (airAlive)
            {
                airAlive = false;
            }
            else
            {
                return NoteResult.None();
            }
            break;

            case NotePosition.Road:
            if (roadAlive)
            {
                roadAlive = false;
            }
            else
            {
                return NoteResult.None();
            }
            break;

            default:
            return NoteResult.None();
        }

        NoteResult result = NoteResult.Hit(notePosition);
        result.precision = (diff <= mediator.gameSettings.perfectDiffTime) ? ComboPrecision.Perfect : ComboPrecision.Great;
        SpawnDyingEffect(notePosition, result.precision);

        if (!airAlive && !roadAlive)
        {
            isDead = true;
        }

        return result;
    }

    protected override void CustomSpawnDyingEffectPosition(NotePosition notePosition, ref Vector2 worldPosition)
    {
        switch (notePosition)
        {
            case NotePosition.Air:
            worldPosition = airPivot.transform.position;
            break;

            case NotePosition.Road:
            worldPosition = roadPivot.transform.position;
            break;
        }
    }

    protected override bool IsDamagableCharacterPosition(NotePosition characterPosition)
    {
        if (airAlive && roadAlive)
        {
            return true;
        }

        switch (characterPosition)
        {
            case NotePosition.Air:
            return airAlive;

            case NotePosition.Road:
            return roadAlive;

            default:
            return false;
        }
    }

    public override int GetAccuracyScore()
    {
        return 2;
    }

    public override int GetMissCount()
    {
        return (airAlive ? 1 : 0) + (roadAlive ? 1 : 0);
    }

    public override int GetMaxCombo()
    {
        return 2;
    }
}
