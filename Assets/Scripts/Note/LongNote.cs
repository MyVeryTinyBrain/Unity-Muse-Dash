using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class LongNote : Note, ITailedNote
{
    [Header("Long Note")]

    [SerializeField]
    GameObject headPivot = null;

    [SerializeField]
    GameObject tailPivot = null;

    [SerializeField]
    SpriteRenderer headSprite = null;

    [SerializeField]
    SpriteRenderer tailSprite = null;

    [SerializeField]
    LineRenderer line = null;

    [SerializeField]
    List<Sprite> airSpriteDatas;

    [SerializeField]
    List<Sprite> roadSpriteDatas;

    [SerializeField]
    List<Material> airLineMaterialDatas;

    [SerializeField]
    List<Material> roadLineMaterialDatas;

    NoteTailHandle tailHandle = null;

    bool interactible = true;
    bool interacting = false;
    Effect interactingEffect = null;

    Queue<float> longScoreTimes = new Queue<float>();

    public float tailTime
    {
        get
        {
            return data.time + data.longNoteData.duration; 
        }
        set
        {
            float tailTime = value - data.time;
            tailTime = Mathf.Clamp(tailTime, 0, float.MaxValue);
            data.longNoteData.duration = tailTime;
        }
    }

    public void OnChangeTailSelection(bool selection)
    {
        Color color = selection ? Color.red : Color.white;
        tailSprite.color = color;
    }

    public override NoteType GetNoteType()
    {
        return NoteType.Long;
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
        MapType mapType = mediator.music.GetMapTypeAtTime(playingTime);
        return airSpriteDatas[(int)mapType];
    }

    protected override NoteVisibleState GetNoteVisibleStateAtCurrentTime()
    {
        float headLocalX = ratio * mediator.gameSettings.lengthPerSeconds;
        float tailLocalX = GetTailLocalXAtCurrentTime();

        if (tailLocalX < mediator.gameSettings.localLeftVisibleX)
        {
            return NoteVisibleState.OutsideLeft;
        }
        else if (headLocalX > mediator.gameSettings.localRightVisibleX)
        {
            return NoteVisibleState.OutsideRight;
        }
        else
        {
            return NoteVisibleState.In;
        }
    }

    public override void Init_CreateInEditor(bool isLoaded)
    {
        base.Init_CreateInEditor( isLoaded);

        if(!isLoaded && data.longNoteData.duration < 0)
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

    void ApplySprites()
    {
        Sprite sprite = null;

        switch (data.position)
        {
            case NotePosition.Air:
            sprite = airSpriteDatas[(int)mapType];
            break;

            case NotePosition.Road:
            sprite = roadSpriteDatas[(int)mapType];
            break;
        }

        headSprite.sprite = sprite;
        tailSprite.sprite = sprite;
    }

    void ApplyLineMaterials()
    {
        Material material = null;

        switch (data.position)
        {
            case NotePosition.Air:
            material = airLineMaterialDatas[(int)mapType];
            break;

            case NotePosition.Road:
            material = roadLineMaterialDatas[(int)mapType];
            break;
        }

        line.sharedMaterial = material;
    }

    void ApplyVisual()
    {
        ApplySprites();
        ApplyLineMaterials();
    }

    public float GetTotalScore()
    {
        float headAndTailScore = defaultScore * 2;
        int numLongScore = (int)(data.longNoteData.duration / 0.1f + 1);
        float longScore = numLongScore * subScore;
        return headAndTailScore + longScore;
    }

    public override void Init()
    {
        base.Init();

        line.useWorldSpace = false;

        ApplyVisual();
    }

    public override void Init_CreateInGame()
    {
        base.Init_CreateInGame();

        // Fill long scores
        longScoreTimes.Clear();
        for(float t = data.time; t < data.time + data.longNoteData.duration; t += 0.1f)
        {
            longScoreTimes.Enqueue(t);
        }
    }

    protected override void OnChangeMapType(MapType value)
    {
        base.OnChangeMapType(value);

        ApplyVisual();
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
        tailPivot.transform.position = new Vector2(worldX, transform.position.y);
    }

    void UpdateHead()
    {
        if (!isDead && interacting)
        {
            Vector2 headWorldPosition = headPivot.transform.position;
            headWorldPosition.x = mediator.hitPoint.Pos.x;

            Vector2 tailLocalPosition = tailPivot.transform.localPosition;
            Vector2 headLocalPosition = transform.InverseTransformPoint(headWorldPosition);
            // 상호작용 중 헤드부분이 판정선을 넘어가지 않도록 합니다.
            headLocalPosition.x = Mathf.Clamp(headLocalPosition.x, 0, tailLocalPosition.x);
            headPivot.transform.localPosition = headLocalPosition;
        }
    }

    void UpdateLine()
    {
        line.positionCount = 2;
        line.SetPosition(0, tailPivot.transform.localPosition);
        line.SetPosition(1, headPivot.transform.localPosition);
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();

        float y = GetHitLocalY();
        transform.localPosition = new Vector2(ratio * mediator.gameSettings.lengthPerSeconds, y);

        UpdateTail();
        UpdateHead();
        UpdateLine();
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        const float anglePerSec = 360 * 1;
        float animationTime = mediator.music.adjustedTime - data.time;
        float headAngle = anglePerSec * animationTime;
         
        headSprite.transform.localEulerAngles = new Vector3(0, 0, headAngle);
        tailSprite.transform.localEulerAngles = new Vector3(0, 0, headAngle);
    }

    protected override void NoteUpdate()
    {
        base.NoteUpdate();

        if (!isDead && interacting)
        {
            while (longScoreTimes.Count > 0)
            {
                float frontLongScoreTime = longScoreTimes.Peek();
                frontLongScoreTime = mediator.music.ApplyOffset(frontLongScoreTime);
                if(mediator.music.playingTime < frontLongScoreTime)
                {
                    break;
                }

                longScoreTimes.Dequeue();
                mediator.gameState.AddScore(subScore);
            }

            if (mediator.music.adjustedTime >= data.time + data.longNoteData.duration)
            {
                NoteResult noteResult = NoteResult.Hit(data.position);
                noteResult.precision = ComboPrecision.Perfect;
                OnEndInteractCallback?.Invoke(this, noteResult);

                isDead = true;

                interacting = false;

                if (interactingEffect != null)
                {
                    interactingEffect.PlayStopEffect();
                    interactingEffect = null;
                }

                return;
            }
        }
    }

    protected override NoteResult OnHit(NotePosition notePosition)
    {
        return NoteResult.None();
    }

    protected override NoteResult OnInteract(NotePosition notePosition)
    {
        if (isDead)
        {
            return NoteResult.None();
        }

        if (notePosition != data.position)
        {
            return NoteResult.None();
        }

        float diff = Mathf.Abs(mediator.music.adjustedTime - data.time);
        if (diff > mediator.gameSettings.greatDiffTime)
        {
            return NoteResult.None();
        }

        if (interactible)
        {
            interactible = false;
            interacting = true;

            NoteResult result = NoteResult.Hit(notePosition);
            result.precision = (diff <= mediator.gameSettings.perfectDiffTime) ? ComboPrecision.Perfect : ComboPrecision.Great;
            result.startInteract = true;
            return result;
        }

        return NoteResult.None();
    }

    protected override void OnReleaseHit(NotePosition notePosition)
    {
        if (isDead)
        {
            return;
        }

        if (interacting && data.position == notePosition)
        {
            if(mediator.music.adjustedTime < data.time + data.longNoteData.duration - mediator.gameSettings.greatDiffTime)
            {
                OnEndInteractCallback?.Invoke(this, NoteResult.Miss());
            }
            else if (mediator.music.adjustedTime < data.time + data.longNoteData.duration - mediator.gameSettings.perfectDiffTime)
            {
                NoteResult result = NoteResult.Hit(data.position);
                result.precision = ComboPrecision.Great;
                OnEndInteractCallback?.Invoke(this, result);

                isDead = true;
            }
            else
            {
                NoteResult result = NoteResult.Hit(data.position);
                result.precision = ComboPrecision.Perfect;
                OnEndInteractCallback?.Invoke(this, result);

                isDead = true;
            }

            interacting = false;

            if (interactingEffect != null)
            {
                interactingEffect.PlayStopEffect();
                interactingEffect = null;
            }
        }
    }

    protected override int GetInteratWeight()
    {
        return 2;
    }

    protected override void OnSpawnHitEffect(NotePosition notePosition, Effect effect)
    {
        interactingEffect = effect;
    }

    protected override bool CustomDamagableState()
    {
        return false;
    }

    protected override bool CheckEndHitable()
    {
        if (!interactible)
        {
            return false;
        }
        return base.CheckEndHitable();
    }

    public override int GetAccuracyScore()
    {
        return 2;
    }

    public override int GetMissCount()
    {
        if (interactible)
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    public override int GetMaxCombo()
    {
        return 2;
    }
}
