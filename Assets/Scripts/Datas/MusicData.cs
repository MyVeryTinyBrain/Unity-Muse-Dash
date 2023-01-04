using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

public enum BeatType
{
    B4 = 1,
    B8 = 4,
    B16 = 8,
    B32 = 16,
}

public enum NoteType
{
    Regular,
    Long,
    Big,
    Jump,
    Ghost,
    Hammer,
    GearWheal,
    BossRegular,
    BossGearWheal,
    SandBag,
    Beam,
    BossRush,
    Score,
    Heart,
    BossAnimation,
}

public enum NotePosition : int
{
    Air = +1,
    Center = 0,
    Road = -1,
}

public enum RegularNoteInType
{
    Default,
    FromUp,
    FromDown,
}

public enum SandBagNoteInType
{
    Default,
    FromUp,
}

public enum RegularNoteType
{
    A, B, C
}

public enum BigNoteType
{
    A, B
}

public enum BossRegularNoteType
{
    Weapon1A, Weapon2A, Weapon2B
}

public enum BossGearWhealNoteType
{
    Weapon1, Weapon2
}

public enum BossRushNoteType
{
    Close_Long, Close_Short, Multi_AfterReturn, Multi_AfterOut
}

public enum BossAnimationType
{
    None = -1,
    In,
    Out,
    Outside,
    Standby,
    Attack1_Start,
    Attack2_Start,
    Attack1_End,
    Attack2_End,
    Attack1_Standby, 
    Attack2_Standby,
    Attack1_To_Attack2,
    Attack2_To_Attack1,
    Attack1_Air,
    Attack1_Road,
    Attack2,
    CloseAttack1_AfterReturn,
    CloseAttack2_AfterReturn,
    MultiAttack_AfterReturn,
    MultiAttack_AfterOut,
    MultiAttack_Hurt,
    Hurt_AfterReturn,
    Hurt_AfterOut,
    MultiAttack_BlockFail_AfterOut,
    Multiattack_BlockFail_AfterReturn,
}

public enum BossProjectileWeapon
{
    None,
    Weapon1,
    Weapon2,
}

public enum BossState
{
    In,
    Out,
    Weapon1,
    Weapon2,
}

public enum TransitionType
{
    Linear,
    Constant,
}

public enum MapType
{
    _01 = 0,
    _02,
    _03,
    _04,
    _05,
}

public enum NoteVisualizeMethod
{
    Spine,
    Sprite,
}

public enum NoteVisibleState
{
    In,
    OutOfLeft,
    OutOfRight,
}

public enum HDirection : int
{ 
    Left = -1, 
    Right = +1,
};

public enum ComboPrecision
{
    None,
    Great,
    Perfect,
}

public enum ComboScore
{
    None,
    Default,
    Sub,
}

public enum Grade
{
    S_Gold, S_Silver, S, A, B, C, D,
}

public struct NoteResult
{
    public bool             hit;
    public bool             noCombo;
    public bool             startInteract;
    public bool             abort;
    public bool             damage;
    public bool             noFever;
    public bool             miss;
    public ComboPrecision   precision;
    public ComboScore       score;
    public NotePosition     hitPosition;

    public static NoteResult None()
    {
        return new NoteResult()
        {
            hit = false,
            noCombo = false,
            startInteract = false,
            abort = false,
            damage = false,
            noFever = false,
            miss = false,
            precision = ComboPrecision.None,
            score = ComboScore.None,
            hitPosition = NotePosition.Road,
        };
    }

    public static NoteResult Hit(NotePosition noteHitPosition)
    {
        return new NoteResult()
        {
            hit = true,
            noCombo = false,
            startInteract = false,
            abort = true,
            damage = false,
            noFever = false,
            miss = false,
            precision = ComboPrecision.Perfect,
            score = ComboScore.Default,
            hitPosition = noteHitPosition
        };
    }

    public static NoteResult Miss()
    {
        return new NoteResult()
        {
            hit = false,
            noCombo = false,
            startInteract = false,
            abort = false,
            damage = false,
            noFever = false,
            miss = true,
            precision = ComboPrecision.None,
            score = ComboScore.None,
            hitPosition = NotePosition.Road,
        };
    }
}

[System.Serializable]
public struct LongNoteData
{
    [FieldAccess(true)]
    public float duration;

    public static LongNoteData Default()
    {
        return new LongNoteData()
        {
            duration = -1,
        };
    }
}

[System.Serializable]
public struct RegularNoteData
{
    [ReadOnlyInRuntime]
    public RegularNoteInType noteInType;

    // 0 ~ 1: Length per Seconds ~ Hit point
    // 이 비율에서 등장 애니메이션이 끝나게 됩니다.
    // 0 이하의 값을 사용해서는 안됩니다.
    [FieldAccess(typeof(RegularNoteInType), nameof(noteInType), RegularNoteInType.FromUp)]
    [FieldAccess(typeof(RegularNoteInType), nameof(noteInType), RegularNoteInType.FromDown)]
    public float endInRatio;

    [ReadOnlyInRuntime]
    public RegularNoteType noteType;

    public static RegularNoteData Default()
    {
        return new RegularNoteData()
        {
            noteInType = RegularNoteInType.Default,
            endInRatio = 0.6f,
            noteType = RegularNoteType.A,
        };
    }
}

[System.Serializable]
public struct JumpNoteData
{
    // 0 ~ 1: Length per Seconds ~ Hit point
    // 이 비율에서 등장 애니메이션이 시작됩니다.
    // 1 이상의 값을 사용해서는 안됩니다.
    [FieldAccess(true)]
    public float beginRatio;

    public static JumpNoteData Default()
    {
        return new JumpNoteData()
        {
            beginRatio = 0.4f,
        };
    }
}

[System.Serializable]
public struct BigNoteData
{
    [ReadOnlyInRuntime]
    public BigNoteType noteType;

    public static BigNoteData Default()
    {
        return new BigNoteData()
        {
            noteType = BigNoteType.A,
        };
    }
}

[System.Serializable]
public struct BossRegularNoteData
{
    [ReadOnlyInRuntime]
    public BossRegularNoteType noteType;

    public static BossRegularNoteData Default()
    {
        return new BossRegularNoteData()
        {
            noteType = BossRegularNoteType.Weapon1A,
        };
    }
}

[System.Serializable]
public struct BossGearWhealData
{
    public BossGearWhealNoteType noteType;

    public static BossGearWhealData Default()
    {
        return new BossGearWhealData()
        {
            noteType = BossGearWhealNoteType.Weapon1,
        };
    }
}

[System.Serializable]
public struct GhostNoteData
{
    // 0 ~ 1: Length per Seconds ~ Hit point
    // 이 비율에서 알파값 감소가 시작됩니다.
    [FieldAccess(true)]
    public float beginHideRatio;

    // 0 ~ 1: Length per Seconds ~ Hit point
    // 이 비율만큼 알파감 감소가 진행되는 지속시간을 나타내는 비율입니다.
    [FieldAccess(true)]
    public float hideDurationRatio;

    public static GhostNoteData Default()
    {
        return new GhostNoteData()
        {
            beginHideRatio = 0.1f,
            hideDurationRatio = 0.6f,
        };
    }
}

[System.Serializable]
public struct SandBagNoteData
{
    [ReadOnlyInRuntime]
    public SandBagNoteInType noteInType;

    // 0 ~ 1: Length per Seconds ~ Hit point
    // 이 비율에서 등장 애니메이션이 끝나게 됩니다.
    // 0 이하의 값을 사용해서는 안됩니다.
    [FieldAccess(typeof(SandBagNoteInType), nameof(noteInType), SandBagNoteInType.FromUp)]
    public float endInRatio;

    [FieldAccess(true)]
    public float duration;

    [FieldAccess(true)]
    public int tapCount;

    public static SandBagNoteData Default()
    {
        return new SandBagNoteData()
        {
            noteInType = SandBagNoteInType.Default,
            endInRatio = 0.5f,
            duration = -1,
            tapCount = 1,
        };
    }
}

[System.Serializable]
public struct BossRushNoteData
{
    public BossRushNoteType noteType;

    [FieldAccess(true)]
    public float duration;

    // Mulit Attack 타입만 여러번 연타 가능합니다.
    // 그 외의 타입은 단타 노트로 취급합니다.
    [FieldAccess(typeof(BossRushNoteType), nameof(noteType), BossRushNoteType.Multi_AfterReturn)]
    [FieldAccess(typeof(BossRushNoteType), nameof(noteType), BossRushNoteType.Multi_AfterOut)]
    public int tapCount;

    [FieldAccess(true)]
    public bool useUnifiedDuration;

    [FieldAccess(true)]
    public float unifiedDuration;

    public static BossRushNoteData Default()
    {
        return new BossRushNoteData()
        {
            noteType = BossRushNoteType.Close_Long,
            duration = -1,
            tapCount = 1,
            useUnifiedDuration = false,
            unifiedDuration = 0,
        };
    }
}

[System.Serializable]
public struct NoteData
{
    [FieldAccess(true)]
    public float                time;

    [FieldAccess(true)]
    public float                speed;

    [FieldAccess(false)]
    public NotePosition         position;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.Regular)]
    public RegularNoteData      regularNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.Long)]
    public LongNoteData         longNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.Jump)]
    public JumpNoteData         jumpNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.Big)]
    public BigNoteData          bigNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.Ghost)]
    public GhostNoteData        ghostNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.BossRegular)]
    public BossRegularNoteData  bossRegularNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.GearWheal)]
    public BossGearWhealData    bossGearWhealData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.SandBag)]
    public SandBagNoteData      sandBagNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.BossRush)]
    public BossRushNoteData     bossRushNoteData;

    [FieldAccess(typeof(NoteType), nameof(type), NoteType.BossAnimation)]
    public BossAnimationData    bossAnimationData;

    [ReadOnlyInRuntime]
    public NoteType             type;

    [ReadOnlyInRuntime]
    public ulong hash;

    public NoteData Copy()
    {
        NoteData data = new NoteData();

        data.time = this.time;
        data.speed = this.speed;
        data.position = this.position;
        data.regularNoteData = this.regularNoteData;
        data.longNoteData = this.longNoteData;
        data.jumpNoteData = this.jumpNoteData;
        data.bigNoteData = this.bigNoteData;
        data.ghostNoteData = this.ghostNoteData;
        data.bossRegularNoteData = this.bossRegularNoteData;
        data.bossGearWhealData = this.bossGearWhealData;
        data.sandBagNoteData = this.sandBagNoteData;
        data.bossRushNoteData = this.bossRushNoteData;
        data.bossAnimationData = this.bossAnimationData.Copy();
        data.type = this.type;
        data.hash = this.hash;

        return data;
    }

    public static NoteData Default()
    {
        return new NoteData()
        {
            time = 0.0f,
            regularNoteData = RegularNoteData.Default(),
            longNoteData = LongNoteData.Default(),
            jumpNoteData = JumpNoteData.Default(),
            bigNoteData = BigNoteData.Default(),
            ghostNoteData = GhostNoteData.Default(),
            bossRegularNoteData = BossRegularNoteData.Default(),
            bossGearWhealData = BossGearWhealData.Default(),
            sandBagNoteData = SandBagNoteData.Default(),
            bossRushNoteData = BossRushNoteData.Default(),
            bossAnimationData = null,
            speed = 1.0f,
            position = NotePosition.Air,
            type = NoteType.Regular,
        };
    }

    public static bool CompareType(NoteData a, NoteData b)
    {
        if(a.type != b.type)
        {
            return false;
        }

        switch (a.type)
        {
            case NoteType.Regular:
            if (a.regularNoteData.noteType != b.regularNoteData.noteType)
            {
                return false;
            }
            break;

            case NoteType.Big:
            if(a.bigNoteData.noteType != b.bigNoteData.noteType)
            {
                return false;
            }
            break;

            case NoteType.BossRegular:
            if(a.bossRegularNoteData.noteType != b.bossRegularNoteData.noteType)
            {
                return false;
            }
            break;

            case NoteType.BossGearWheal:
            if(a.bossGearWhealData.noteType != b.bossGearWhealData.noteType)
            {
                return false;
            }
            break;

            case NoteType.SandBag:
            if(a.sandBagNoteData.noteInType != b.sandBagNoteData.noteInType)
            {
                return false;
            }
            break;

            case NoteType.BossRush:
            if(a.bossRushNoteData.noteType != b.bossRushNoteData.noteType)
            {
                return false;
            }
            break;
        }

        return true;
    }
}

[System.Serializable]
public class BossAnimationData
{
    public float                time;

    [FieldAccess(true)]
    public float                speed;

    [FieldAccess(true)]
    public BossAnimationType    type;

    [FieldAccess(true)]
    public bool                 useUnifiedDuration;

    [FieldAccess(true)]
    public float                unifiedDuration;

    public float                noteTime;

    public BossAnimationData Copy()
    {
        BossAnimationData data = new BossAnimationData();

        data.time = this.time;
        data.speed = this.speed;
        data.type = this.type;
        data.useUnifiedDuration = this.useUnifiedDuration;
        data.unifiedDuration = this.unifiedDuration;
        data.noteTime = this.noteTime;

        return data;
    }

    public static BossAnimationData Default()
    {
        return new BossAnimationData()
        {
            time = 0.0f,
            speed = 1.0f,
            type = BossAnimationType.Outside,
            useUnifiedDuration = false,
            unifiedDuration = 0,
            noteTime = 0.0f,
        };
    }
}

[System.Serializable]
public struct BPMData
{
    [FieldAccess(true)]
    public float            time;

    [FieldAccess(true)]
    public float            bpm;

    [FieldAccess(true)]
    public TransitionType   transition;

    [FieldAccess(true)]
    public float            offset;

    public static BPMData Default()
    {
        return new BPMData()
        {
            time = 0.0f,
            bpm = 100.0f,
            transition = TransitionType.Linear,
            offset = 0,
        };
    }
}

[System.Serializable]
public struct SpeedScaleData
{
    [FieldAccess(true)]
    public float            time;

    [FieldAccess(true)]
    public float            speedScale;

    [FieldAccess(true)]
    public TransitionType   transition;

    public static SpeedScaleData Default()
    {
        return new SpeedScaleData()
        {
            time = 0.0f,
            speedScale = 1.0f,
            transition = TransitionType.Linear,
        };
    }
}

[System.Serializable]
public struct MapTypeData
{
    [FieldAccess(true)]
    public float    time;

    [FieldAccess(true)]
    public MapType  type;

    public static MapTypeData Default()
    {
        return new MapTypeData()
        {
            time = 0.0f,
            type = MapType._01,
        };
    }
}

[System.Serializable]
public struct MusicConfigData
{
    [FieldAccess(true)]
    public PathString   musicFileName;

    [FieldAccess(true)]
    public float        startDelay;

    [FieldAccess(true)]
    public float        endDelay;

    [FieldAccess(true)]
    public int          difficulty;

    public static MusicConfigData Default()
    {
        return new MusicConfigData()
        {
            musicFileName = "",
            startDelay = 5,
            endDelay = 4,
            difficulty = 1,
        };
    }
}

