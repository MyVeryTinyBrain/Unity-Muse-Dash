using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class SandBagNote : Note, ITailedNote
{
    [System.Serializable]
    public struct AnimationDesc
    {
        public string defaultName;
        public string fromUp;
        public float fromUpHeightRatio;
        public string outName;
        public List<string> hurts;
    }

    [SerializeField]
    List<AnimationDesc> animations;

    [SerializeField]
    GameObject guide = null;

    [SerializeField]
    SpriteRenderer tailSprite = null;

    [SerializeField]
    LineRenderer editorLine = null;

    Spine.Bone xBone = null;
    Spine.Animation defaultAnimation = null;
    Spine.Animation fromAnimation = null;
    float fromUpHeightRatio = 0.0f;
    string outAnimationName = "";
    List<Spine.Animation> hurtAnimations = null;
    Vector2 originCenterSpineLocalPosition = Vector2.zero;

    NoteTailHandle tailHandle = null;

    bool interactible = true;
    bool interacting = false;
    int tapCount = 0;
    uint zoomHandle = MainCamera.NullHandle;

    public float tailTime
    {
        get
        {
            return data.time + data.sandBagNoteData.duration;
        }
        set
        {
            float tailTime = value - data.time;
            tailTime = Mathf.Clamp(tailTime, 0, float.MaxValue);
            data.sandBagNoteData.duration = tailTime;
        }
    }

    public void OnChangeTailSelection(bool selection)
    {
        Color color = selection ? Color.red : Color.white;
        tailSprite.color = color;
    }

    public override NoteType GetNoteType()
    {
        return NoteType.SandBag;
    }

    public override NoteVisualizeMethod GetNoteVisualizeMethod()
    {
        return NoteVisualizeMethod.Spine;
    }

    public override SkeletonDataAsset GetNoteVisualizeSpineDataAtTime(float playingTime)
    {
        MapType mapType = mediator.music.GetMapTypeAtTime(playingTime);
        return centerSpineDatas[(int)mapType];
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

    protected override void Awake()
    {
        base.Awake();

        originCenterSpineLocalPosition = centerSpine.transform.localPosition;
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

        if (!isLoaded && data.sandBagNoteData.duration < 0)
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

        xBone = centerSpine.Skeleton.FindBone("X");

        defaultAnimation = centerSpine.skeleton.Data.FindAnimation(animations[(int)mapType].defaultName);
        fromAnimation = centerSpine.skeleton.Data.FindAnimation(animations[(int)mapType].fromUp);
        fromUpHeightRatio = animations[(int)mapType].fromUpHeightRatio;
        outAnimationName = animations[(int)mapType].outName;
        hurtAnimations = new List<Spine.Animation>();
        foreach(string hurtName in animations[(int)mapType].hurts)
        {
            hurtAnimations.Add(centerSpine.skeleton.Data.FindAnimation(hurtName));
        }

        switch (data.sandBagNoteData.noteInType)
        {
            case SandBagNoteInType.Default:
            ClearState();
            SetDefaultAnimationByTime(0);
            break;

            case SandBagNoteInType.FromUp:
            ClearState();
            SetFromAnimationByRatio(0);
            break;
        }
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

        // 상호작용 중 노트가 판정선 이상 넘어가지 않도록 합니다.
        float newRatio = ratio;
        if (interacting)
        {
            newRatio = Mathf.Clamp(ratio, 0, float.MaxValue);
        }

        float localX = newRatio * mediator.gameSettings.lengthPerSeconds;
        float y = GetHitLocalY();
        transform.localPosition = new Vector2(localX, y);

        UpdateTail();
        UpdateLine();
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation();

        if (interacting)
        {
            return;
        }

        switch (data.sandBagNoteData.noteInType)
        {
            case SandBagNoteInType.Default:
            {
                float animationTime = mediator.music.adjustedTime - data.time;

                ClearState();
                SetDefaultAnimationByTime(animationTime);
            }
            break;
            case SandBagNoteInType.FromUp:
            {
                float animationNormalizedHitRatio = fromUpHeightRatio;

                // 1. invRatio가 0일때 정규화된 애니메이션 시간은 0입니다.
                // 2. invRatio가 endInRatio일때 정규화된 애니메이션 시간은 animationNormalizedHitRatio 입니다.
                // ax + b = y : a * invRatio + b = animationRatio 라고 할 때
                // 1. a * 0 + b = 0
                // 2. a * endInRatio + b = animationNormalizedHitRatio
                // a = animationNormalizedHitRatio / endInRatio
                // b = 0
                // (animationNormalizedHitRatio / endInRatio) * invRatio = animationRatio

                float invRatio = 1.0f - ratio;
                float animationRatio = (animationNormalizedHitRatio / data.sandBagNoteData.endInRatio) * invRatio;
                animationRatio = Mathf.Clamp(animationRatio, 0.0f, 1.0f);

                ClearState();
                SetFromAnimationByRatio(animationRatio);
            }
            break;
        }
    }

    protected override void NoteUpdate()
    {
        base.NoteUpdate();

        bool interactingTimeOver = mediator.music.adjustedTime >= data.time + data.sandBagNoteData.duration;
        if (interacting && interactingTimeOver)
        {
            bool lessTapCount = tapCount < data.sandBagNoteData.tapCount / 2;
            if (lessTapCount)
            {
                SpineEffect.SetupDesc desc = SpineEffect.SetupDesc.Default();
                desc.skin = centerSpine.initialSkinName;

                MeshRenderer meshRenderer = null;
                if (!centerSpine.TryGetComponent<MeshRenderer>(out meshRenderer))
                {
                    return;
                }
                desc.sortingLayer = meshRenderer.sortingLayerName;
                desc.sortingOrder = meshRenderer.sortingOrder;
                SpineEffect effect = mediator.spineEffectPool.SpawnEffectToFixedUpdate(centerSpine.skeletonDataAsset, outAnimationName, desc, data.time + data.sandBagNoteData.duration, () => mediator.music.adjustedTime);
                effect.transform.position = transform.position + new Vector3(8, -4);

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
                SpineEffect.SetupDesc desc = SpineEffect.SetupDesc.Default();
                desc.skin = centerSpine.initialSkinName;

                MeshRenderer meshRenderer = null;
                if (!centerSpine.TryGetComponent<MeshRenderer>(out meshRenderer))
                {
                    return;
                }
                desc.sortingLayer = meshRenderer.sortingLayerName;
                desc.sortingOrder = meshRenderer.sortingOrder;
                SpineEffect effect = mediator.spineEffectPool.SpawnEffectToFixedUpdate(centerSpine.skeletonDataAsset, "out_g", desc, mediator.music.adjustedTime, () => mediator.music.adjustedTime);
                effect.transform.position = body.transform.position;

                NoteResult result = NoteResult.Hit(data.position);
                result.precision = ComboPrecision.Great;
                StopZoom();
                OnEndInteractCallback?.Invoke(this, result);
                isDead = true;

                mediator.tapComboCountUI.RemoveVisibleStack();
            }
        }
    }

    protected override void Spine_Update()
    {
        xBone?.SetLocalPosition(Vector2.zero);
    }

    private void SetFromAnimationByRatio(float ratio)
    {
        ApplyAnimation(centerSpine, fromAnimation, ratio * fromAnimation.Duration, false);
    }

    private void SetDefaultAnimationByTime(float time)
    {
        float centerTime = time;
        float newDuration = Mathf.Clamp(defaultAnimation.Duration, 0, 2.0f);

        if (time < 0)
        {
            // 음수 루프 처리
            float absTime = Mathf.Abs(time);
            centerTime = newDuration - Mathf.Repeat(absTime, newDuration);
        }
        else
        {
            centerTime = Mathf.Repeat(centerTime, newDuration);
        }

        ApplyAnimation(centerSpine, defaultAnimation, centerTime, true);
    }

    protected override NoteResult OnHit(NotePosition notePosition)
    {
        if (data.sandBagNoteData.tapCount > 1)
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

        SpineEffect.SetupDesc desc = SpineEffect.SetupDesc.Default();
        desc.skin = centerSpine.initialSkinName;

        MeshRenderer meshRenderer = null;
        if (!centerSpine.TryGetComponent<MeshRenderer>(out meshRenderer))
        {
            return NoteResult.None();
        }
        desc.sortingLayer = meshRenderer.sortingLayerName;
        desc.sortingOrder = meshRenderer.sortingOrder;
        SpineEffect effect = mediator.spineEffectPool.SpawnEffectToFixedUpdate(centerSpine.skeletonDataAsset, "out_p", desc, mediator.music.adjustedTime, () => mediator.music.adjustedTime);
        effect.transform.position = body.transform.position;

        AddShake();
        ReserveZoom();
        isDead = true;

        NoteResult result = NoteResult.Hit(data.position);
        result.precision = (diff <= mediator.gameSettings.perfectDiffTime) ? ComboPrecision.Perfect : ComboPrecision.Great;
        return result;
    }

    protected override NoteResult OnInteract(NotePosition notePosition)
    {
        if (data.sandBagNoteData.tapCount <= 1)
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

        centerSpine.loop = true;
        centerSpine.AnimationName = hurtAnimations[tapCount % hurtAnimations.Count].Name;

        centerSpine.transform.localPosition = Vector2.zero;

        if (++tapCount >= data.sandBagNoteData.tapCount)
        {
            SpineEffect.SetupDesc desc = SpineEffect.SetupDesc.Default();
            desc.skin = centerSpine.initialSkinName;

            MeshRenderer meshRenderer = null;
            if (!centerSpine.TryGetComponent<MeshRenderer>(out meshRenderer))
            {
                return NoteResult.None();
            }
            desc.sortingLayer = meshRenderer.sortingLayerName;
            desc.sortingOrder = meshRenderer.sortingOrder;
            SpineEffect effect = mediator.spineEffectPool.SpawnEffectToFixedUpdate(centerSpine.skeletonDataAsset, "out_p", desc, mediator.music.adjustedTime, () => mediator.music.adjustedTime);
            effect.transform.position = body.transform.position;

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
