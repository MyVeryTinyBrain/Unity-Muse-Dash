using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class BossRushNote : BossNote, ITailedNote
{
    [SerializeField]
    GameObject guide = null;

    [SerializeField]
    SpriteRenderer guideSprite = null;

    [SerializeField]
    SpriteRenderer tailSprite = null;

    [SerializeField]
    LineRenderer editorLine = null;

    [SerializeField]
    List<float> bossAnimationNormalizedHitRatios;

    [SerializeField]
    BossAnimationType bossAnimationType = BossAnimationType.CloseAttack1_AfterReturn;

    NoteTailHandle tailHandle = null;

    bool interactible = true;
    bool interacting = false;
    int tapCount = 0;
    uint zoomHandle = MainCamera.NullHandle;

    public float tailTime
    {
        get
        {
            return data.time + data.bossRushNoteData.duration;
        }
        set
        {
            float tailTime = value - data.time;
            tailTime = Mathf.Clamp(tailTime, 0, float.MaxValue);
            data.bossRushNoteData.duration = tailTime;
        }
    }

    public void OnChangeTailSelection(bool selection)
    {
        Color color = selection ? Color.red : Color.white;
        tailSprite.color = color;
    }

    public override NoteType GetNoteType()
    {
        return NoteType.BossRush;
    }

    public override NoteVisualizeMethod GetNoteVisualizeMethod()
    {
        return NoteVisualizeMethod.Sprite;
    }

    public override SkeletonDataAsset GetNoteVisualizeSpineDataAtTime(float playingTime)
    {
        return null;
    }

    public override Sprite GetNoteVisualizeSpriteDataAtTime(float playingTime)
    {
        return guideSprite.sprite;
    }

    public override Color GetNoteVisualizeSpriteColor()
    {
        return guideSprite.color;
    }

    protected override NoteVisibleState GetNoteVisibleStateAtCurrentTime()
    {
        float headLocalX = ratio * mediator.gameSettings.lengthPerSeconds;
        float tailLocalX = GetTailLocalXAtCurrentTime();

        if (tailLocalX < mediator.gameSettings.localLeftVisibleX)
        {
            return NoteVisibleState.OutOfLeft;
        }
        else if (headLocalX > mediator.gameSettings.localRightVisibleX)
        {
            return NoteVisibleState.OutOfRight;
        }
        else
        {
            return NoteVisibleState.In;
        }
    }

    public override void Init_CreateInEditor(bool isLoaded)
    {
        base.Init_CreateInEditor( isLoaded);
        
        if (!isLoaded && data.bossRushNoteData.duration < 0)
        {
            List<float> beats = mediator.music.GetBeats(mediator.noteViewport.beatType);
            int closetBeatIndex = mediator.music.GetClosetBeatIndexAtTime(data.time, mediator.noteViewport.beatType);
            int leftBeatIndex = Mathf.Clamp(closetBeatIndex - 1, 0, beats.Count - 1);
            int rightBeatIndex = Mathf.Clamp(closetBeatIndex + 1, 0, beats.Count - 1);

            if (rightBeatIndex > closetBeatIndex)
            {
                tailTime = beats[rightBeatIndex];
            }
            else if (closetBeatIndex > leftBeatIndex)
            {
                tailTime = beats[leftBeatIndex];
            }
        }

        tailHandle = tailSprite.gameObject.AddComponent<NoteTailHandle>();
    }

    public override void Init_CreateInGame()
    {
        base.Init_CreateInGame();

        guide.gameObject.SetActive(false);
    }

    public override void Init()
    {
        base.Init();
    }

    float GetTailLocalXAtCurrentTime()
    {
        float hitPointSpaceX = mediator.music.TimeToLocalXAtAdjustedTime(tailTime, data.speed);
        return hitPointSpaceX;
    }

    void UpdateTail()
    {
        float hitPointSpaceX = GetTailLocalXAtCurrentTime();
        float worldX = mediator.hitPoint.transform.TransformPoint(hitPointSpaceX, 0, 0).x;
        tailSprite.transform.position = new Vector2(worldX, transform.position.y);
    }

    void UpdateLine()
    {
        editorLine.positionCount = 2;
        editorLine.SetPosition(0, Vector3.zero);
        editorLine.SetPosition(1, tailSprite.transform.localPosition);
    }

    void ReserveZoom()
    {
        mediator.mainCamera.ReserveSizeRatio(mediator.gameSettings.tapNoteCamSizeRatio, mediator.gameSettings.tapNoteReserveCamSizeRatioDuration);
    }

    void StartZoom()
    {
        zoomHandle = mediator.mainCamera.RequireSizeRatio(mediator.gameSettings.tapNoteCamSizeRatio);
    }

    void StopZoom()
    {
        mediator.mainCamera.RemoveSizeRatio(zoomHandle);
    }

    void AddShake()
    {
        mediator.mainCamera.AddShake(mediator.gameSettings.tapNoteCamShakeDistance, mediator.gameSettings.tapNoteCamShakeRestoreDuration);
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();

        float y = GetHitLocalY();
        transform.localPosition = new Vector2(ratio * mediator.gameSettings.lengthPerSeconds, y);

        UpdateTail();
        UpdateLine();
    }

    protected override void UpdateBossAnimationData()
    {
        base.UpdateBossAnimationData();

        // 구하려는 것
        // 애니메이션 시간이 data.time - bossAttackAnimationNormalizedHit * animation.Duration 인 '음악 재생시간'
        // 애니메이션 시간이 노트의 타격 시점에서 보스 애니메이션의 타격 시간만큼 뺀 시점
        // 보스 애니메이션이 배치되어야 할 시간
        // 보스가 노트의 타격 시간에 bossAttackAnimationNormalizedHit * animation.Duration 시간의 애니메이션을 재생하도록 배치해야 할 시간

        float bossAnimationNormalizedHit = bossAnimationNormalizedHitRatios[(int)mapType];
        float duration = 
            data.bossRushNoteData.useUnifiedDuration ? 
            data.bossRushNoteData.unifiedDuration : 
            mediator.boss.GetAnimationDuration(bossAnimationType, mapType);
        bossAnimationData.time = data.time - bossAnimationNormalizedHit * duration;

        bossAnimationData.type = bossAnimationType;

        bossAnimationData.speed = 1;
        bossAnimationData.useUnifiedDuration = data.bossRushNoteData.useUnifiedDuration;
        bossAnimationData.unifiedDuration = data.bossRushNoteData.unifiedDuration;
    }

    protected override void NoteUpdate()
    {
        base.NoteUpdate();

        bool interactingTimeOver = mediator.music.adjustedTime >= data.time + data.bossRushNoteData.duration;
        if (interacting && interactingTimeOver)
        {
            bool lessTapCount = tapCount < data.bossRushNoteData.tapCount / 2;
            if (lessTapCount)
            {
                BossAnimationType failAnimationType = BossAnimationType.None;
                switch (data.bossRushNoteData.noteType)
                {
                    case BossRushNoteType.Multi_AfterReturn:
                    failAnimationType = BossAnimationType.Multiattack_BlockFail_AfterReturn;
                    break;

                    case BossRushNoteType.Multi_AfterOut:
                    failAnimationType = BossAnimationType.MultiAttack_BlockFail_AfterOut;
                    break;
                }
                if (failAnimationType != BossAnimationType.None)
                {
                    mediator.boss.SetManualAnimationToFixedUpdate(failAnimationType, false, data.time + data.bossRushNoteData.duration, () => mediator.music.adjustedTime);
                }

                NoteResult result = NoteResult.None();
                result.damage = true;
                result.miss = true;
                StopZoom();
                OnEndInteractCallback?.Invoke(this, result);
                isDead = true;

                mediator.tapComboCountUI.RemoveVisibleStack();
            }
            else
            {
                BossAnimationType killAnimationType = BossAnimationType.None;
                switch (data.bossRushNoteData.noteType)
                {
                    case BossRushNoteType.Close_Short:
                    case BossRushNoteType.Close_Long:
                    killAnimationType = BossAnimationType.Hurt_AfterReturn;
                    break;

                    case BossRushNoteType.Multi_AfterReturn:
                    killAnimationType = BossAnimationType.Hurt_AfterReturn;
                    break;

                    case BossRushNoteType.Multi_AfterOut:
                    killAnimationType = BossAnimationType.Hurt_AfterOut;
                    break;
                }
                mediator.boss.SetManualAnimationToFixedUpdate(killAnimationType, false, mediator.music.adjustedTime, () => mediator.music.adjustedTime);

                NoteResult result = NoteResult.Hit(data.position);
                result.precision = ComboPrecision.Great;
                StopZoom();
                OnEndInteractCallback?.Invoke(this, result);
                isDead = true;

                mediator.tapComboCountUI.RemoveVisibleStack();
            }
        }
    }

    protected override NoteResult OnHit(NotePosition notePosition)
    {
        if (data.bossRushNoteData.tapCount > 1)
        {
            return NoteResult.None();
        }

        if (isDead)
        {
            return NoteResult.None();
        }

        float diff = Mathf.Abs(mediator.music.adjustedTime - data.time);
        if (!interacting && diff > mediator.gameSettings.greatDiffTime)
        {
            return NoteResult.None();
        }

        BossAnimationType killAnimationType = BossAnimationType.None;
        switch (data.bossRushNoteData.noteType)
        {
            case BossRushNoteType.Close_Short:
            case BossRushNoteType.Close_Long:
            killAnimationType = BossAnimationType.Hurt_AfterReturn;
            break;

            case BossRushNoteType.Multi_AfterReturn:
            killAnimationType = BossAnimationType.Hurt_AfterReturn;
            break;

            case BossRushNoteType.Multi_AfterOut:
            killAnimationType = BossAnimationType.Hurt_AfterOut;
            break;
        }
        mediator.boss.SetManualAnimationToFixedUpdate(killAnimationType, false, mediator.music.adjustedTime, () => mediator.music.adjustedTime);

        AddShake();
        ReserveZoom();
        isDead = true;

        NoteResult result = NoteResult.Hit(data.position);
        result.precision = (diff <= mediator.gameSettings.perfectDiffTime) ? ComboPrecision.Perfect : ComboPrecision.Great;
        return result;
    }

    protected override NoteResult OnInteract(NotePosition notePosition)
    {
        if (data.bossRushNoteData.tapCount <= 1)
        {
            return NoteResult.None();
        }

        if (isDead)
        {
            return NoteResult.None();
        }

        float diff = Mathf.Abs(mediator.music.adjustedTime - data.time);
        if (!interacting && diff > mediator.gameSettings.greatDiffTime)
        {
            return NoteResult.None();
        }

        AddShake();

        NoteResult result = NoteResult.Hit(data.position);
        result.noCombo = true;
        result.score = ComboScore.Sub;
        if (interactible)
        {
            StartZoom();
            mediator.tapComboCountUI.AddVisibleStack();

            result.startInteract = true;
            interactible = false;
            interacting = true;
        }

        BossAnimationType hitAnimationType = BossAnimationType.None;
        switch (data.bossRushNoteData.noteType)
        {
            case BossRushNoteType.Multi_AfterReturn:
            case BossRushNoteType.Multi_AfterOut:
            hitAnimationType = BossAnimationType.MultiAttack_Hurt;
            break;
        }
        mediator.boss.SetManualAnimationToFixedUpdate(hitAnimationType, true, mediator.music.adjustedTime, () => mediator.music.adjustedTime);

        if (++tapCount >= data.bossRushNoteData.tapCount)
        {
            BossAnimationType killAnimationType = BossAnimationType.None;
            switch (data.bossRushNoteData.noteType)
            {
                case BossRushNoteType.Close_Short:
                case BossRushNoteType.Close_Long:
                killAnimationType = BossAnimationType.Hurt_AfterReturn; 
                break;

                case BossRushNoteType.Multi_AfterReturn:
                killAnimationType = BossAnimationType.Hurt_AfterReturn;
                break;
                
                case BossRushNoteType.Multi_AfterOut:
                killAnimationType = BossAnimationType.Hurt_AfterOut;
                break;
            }
            mediator.boss.SetManualAnimationToFixedUpdate(killAnimationType, false, mediator.music.adjustedTime, () => mediator.music.adjustedTime);

            StopZoom();
            OnEndInteractCallback?.Invoke(this, NoteResult.Hit(data.position));
            isDead = true;

            mediator.tapComboCountUI.RemoveVisibleStack();
        }

        mediator.tapComboCountUI.combo = tapCount;

        return result;
    }

    protected override int GetInteratWeight()
    {
        return 1;
    }

    protected override void CustomSpawnHitEffectPosition(ref NotePosition notePosition)
    {
        notePosition = NotePosition.Center;
    }

    protected override bool CustomDamagableState()
    {
        return interactible;
    }

    protected override bool IsDamagableCharacterPosition(NotePosition characterPosition)
    {
        return true;
    }

    protected override bool CheckEndHitable()
    {
        if (!interactible)
        {
            return false;
        }
        return base.CheckEndHitable();
    }
}
