using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using UnityEngine.Rendering;
using System;
using System.Linq;

public class Boss : MonoBehaviour, IGameReset
{
    protected Mediator mediator => Mediator.i;

    class BossAnimationInfo
    {
        public Spine.Animation animation;
        public BossAnimationSubData subData;
    }

    [SerializeField]
    BossDescs descsAsset = null;

    [SerializeField]
    SkeletonAnimation spine = null;

    BossDesc currentDesc = null;

    Dictionary<MapType, BossDesc> bossDescs = new Dictionary<MapType, BossDesc>();

    Dictionary<BossAnimationType, BossAnimationInfo> animationsInfos = new Dictionary<BossAnimationType, BossAnimationInfo>();

    MapType mapType = MapType._01;

    // 해당 타입이 None이 아니면 수동 애니메이션을 적용합니다.
    BossAnimationType manualAnimationType = BossAnimationType.None;
    bool manualAnimationLoop = false;
    bool manualFixedUpdate = false;
    float manualFixedUpdateStartTime = 0;
    Func<float> manualFixedUpdateTimeFunc = null;

    Spine.Bone weapon1NoteEffectSpawnBone = null;
    Spine.Bone weapon2NoteEffectSpawnBone = null;
    Effect weapon1NoteEffect = null;
    Effect weapon2NoteEffect = null;

    BossAnimationData lastBossAnimationData = null;

    public void SetManualAnimation(BossAnimationType type, bool loop)
    {
        SetManualAnimation_Internal(type, loop, false);
    }

    public void SetManualAnimationToFixedUpdate(BossAnimationType type, bool loop, float fixedUpateStartTime, Func<float> fixedUpdateTimeFunc = null)
    {
        SetManualAnimation_Internal(type, loop, true, fixedUpateStartTime, fixedUpdateTimeFunc);
    }

    public void SetManualAnimation_Internal(BossAnimationType type, bool loop, bool fixedUpdate = false, float fixedUpateStartTime = 0, Func<float> fixedUpdateTimeFunc = null)
    {
        this.manualAnimationType = type;
        this.manualAnimationLoop = loop;
        this.manualFixedUpdate = fixedUpdate;
        this.manualFixedUpdateStartTime = fixedUpateStartTime;
        this.manualFixedUpdateTimeFunc = fixedUpdateTimeFunc;

        if (manualAnimationType != BossAnimationType.None && !fixedUpdate)
        {
            spine.loop = manualAnimationLoop;
            BossAnimationInfo manualAnimationInfo = animationsInfos[manualAnimationType];
            spine.AnimationName = manualAnimationInfo.animation.Name;

            Spine.TrackEntry trackEntry = spine.state.GetCurrent(0);
            if (trackEntry != null)
            {
                trackEntry.TrackTime = 0;
            }
        }
    }

    public void Init()
    {
        bossDescs.Clear();
        foreach(BossDesc desc in descsAsset.descs)
        {
            bossDescs.Add(desc.mapType, desc);
        }

        if(!bossDescs.TryGetValue(mapType, out currentDesc))
        {
            Debug.LogWarning($"There's no BossDesc of MapType({mapType})", this);
            currentDesc = bossDescs.First().Value;
        }

        spine.Init(currentDesc.spineAsset, currentDesc.skin);
        GenerateAnimationInfos();

        weapon1NoteEffectSpawnBone = spine.skeleton.FindBone(currentDesc.weapon1EffectSpawnBone);
        weapon2NoteEffectSpawnBone = spine.skeleton.FindBone(currentDesc.weapon2EffectSpawnBone);
        weapon1NoteEffect = currentDesc.weapon1Effect;
        weapon2NoteEffect = currentDesc.weapon2Effect;

        GameReset();

        SetManualAnimation_Internal(manualAnimationType, manualAnimationLoop);
    }

    public float GetAnimationDuration(BossAnimationType type, MapType mapType)
    {
        if (bossDescs.TryGetValue(mapType, out BossDesc bossDesc))
        {
            BossAnimationDesc desc = bossDesc.animationDescs.Find(x => (x.animationType == type));
            Spine.Animation animation = bossDesc.spineAsset.GetSkeletonData(false).FindAnimation(desc.animationName);
            return animation.Duration;
        }

        return 0;
    }

    public float GetAnimationEndTime(BossAnimationType type, MapType mapType, float startTime, float speed, bool useUnifiedTime, float unifiedTime)
    {
        if (bossDescs.TryGetValue(mapType, out BossDesc bossDesc))
        {
            BossAnimationDesc desc = bossDesc.animationDescs.Find(x => (x.animationType == type));
            Spine.Animation animation = bossDesc.spineAsset.GetSkeletonData(false).FindAnimation(desc.animationName);

            //float duration = desc.subData.useUnifiedTime ? desc.subData.unifiedTime : animation.Duration;
            float duration = useUnifiedTime ? unifiedTime : animation.Duration;
            return startTime + duration * speed;
        }

        return 0;
    }

    public float GetAnimationEndTime(MapType mapType, BossAnimationData data)
    {
        return GetAnimationEndTime(data.type, mapType, data.time, data.speed, data.useUnifiedDuration, data.unifiedDuration);
    }

    public BossAnimationType GetStateAnimationType(BossState state)
    {
        switch (state)
        {
            case BossState.In:
            return BossAnimationType.Standby;

            case BossState.Out:
            return BossAnimationType.Outside;

            case BossState.Weapon1:
            return BossAnimationType.Attack1_Standby;

            case BossState.Weapon2:
            return BossAnimationType.Attack2_Standby;
        }

        return BossAnimationType.Outside;
    }

    // 상태가 전환될 때 재생해야 하는 애니메이션이 없으면 None을 반환합니다.
    public BossAnimationType GetStateChangeAnimationType(BossState from, BossState to, out float minNormalizedDuration)
    {
        minNormalizedDuration = 0;
        switch (from)
        {
            case BossState.In:
            switch (to)
            {
                case BossState.Weapon1:
                return BossAnimationType.Attack1_Start;

                case BossState.Weapon2:
                return BossAnimationType.Attack2_Start;
            }
            break;

            case BossState.Out:
            return BossAnimationType.None;

            case BossState.Weapon1:
            switch (to)
            {
                case BossState.In:
                case BossState.Out:
                return BossAnimationType.Attack1_End;

                case BossState.Weapon2:
                minNormalizedDuration = 0.6f;
                return BossAnimationType.Attack1_To_Attack2;
            }
            break;

            case BossState.Weapon2:
            switch (to)
            {
                case BossState.In:
                case BossState.Out:
                return BossAnimationType.Attack2_End;

                case BossState.Weapon1:
                minNormalizedDuration = 0.6f;
                return BossAnimationType.Attack2_To_Attack1;
            }
            break;
        }

        return BossAnimationType.None;
    }

    void GenerateAnimationInfos()
    {
        animationsInfos.Clear();
        foreach (BossAnimationDesc desc in currentDesc.animationDescs)
        {
            Spine.Animation animation = spine.skeleton.Data.FindAnimation(desc.animationName);
            if (animation != null)
            {
                BossAnimationInfo info = new BossAnimationInfo();
                info.animation = animation;
                info.subData = desc.subData;
                animationsInfos.Add(desc.animationType, info);
            }
            else
            {
                Debug.LogError($"Can't find animation on skeleton data(Animation Name: {desc.animationName})", this);
            }
        }
    }

    void UpdateMapType()
    {
        MapType newMapType = mediator.music.GetMapTypeAtTime(mediator.music.adjustedTime);
        if (newMapType != mapType)
        {
            mapType = newMapType;
            Init();
            OnChangeMapType(mapType);
        }
    }

    void OnChangeMapType(MapType value)
    {

    }

    bool ManualAnimationUpdate()
    {
        if (manualAnimationType != BossAnimationType.None)
        {
            if (manualFixedUpdate)
            {
                spine.ClearState();

                BossAnimationInfo manualAnimation = animationsInfos[manualAnimationType];
                float currentTime = manualFixedUpdateTimeFunc != null ? manualFixedUpdateTimeFunc() : Time.time;
                float animationTime = currentTime - manualFixedUpdateStartTime;

                spine.ApplyAnimation(manualAnimation.animation, animationTime, manualAnimationLoop);

                if (!manualAnimationLoop && animationTime > manualAnimation.animation.Duration)
                {
                    manualAnimationType = BossAnimationType.None;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                Spine.TrackEntry trackEntry = spine.state.GetCurrent(0);
                if (manualAnimationLoop)
                {
                    return true;
                }
                else if (trackEntry != null && trackEntry.AnimationTime < trackEntry.Animation.Duration)
                {
                    return true;
                }
            }
        }

        return false;
    }

    void ApplyDefaultAnimation()
    {
        BossAnimationInfo defaultInfo = animationsInfos[BossAnimationType.Outside];

        spine.ClearState();
        spine.ApplyAnimation(defaultInfo.animation, 0, false);
    }

    private void Awake()
    {
        Init();

        MeshRenderer bossRenderer = spine.GetComponent<MeshRenderer>();
        bossRenderer.sortingLayerName = SortingLayers.Boss;
    }

    private void Update()
    {
        // 보스 스킨을 맵에 알맞게 설정합니다.
        UpdateMapType();

        // 수동 애니메이션 업데이트로, 보스 애니메이션 노트보다 우선입니다.
        // 수동 애니메이션이 재생되는 중에는 보스 애니메이션 노트의 영향을 받지 않습니다.
        // 사용처 예시) 보스가 공격 애니메이션 중 타격을 입는 경우에 사용됩니다.
        bool manualUpdated = ManualAnimationUpdate();
        if (manualUpdated)
        {
            return;
        }

        // 재생해야 하는 보스 애니메이션 데이터를 이진탐색합니다.
        List<BossAnimationData> bossAnimationDatas = mediator.music.bossAnimations;
        int closetIndex = mediator.music.GetBossAnimationIndexAtTime(mediator.music.adjustedTime);

        // 애니메이션 데이터가 있으면 그 애니메이션을 재생합니다.
        if (Mathf.Clamp(closetIndex, 0, bossAnimationDatas.Count - 1) == closetIndex)
        {
            BossAnimationData closetAnimationData = bossAnimationDatas[closetIndex];
            BossAnimationInfo closetAnimationInfo = animationsInfos[closetAnimationData.type];
            float deltaTime = mediator.music.adjustedTime - closetAnimationData.time;
            float animationTime = deltaTime * closetAnimationData.speed; // 애니메이션 재생시간
            float closetAnimationEndTime = GetAnimationEndTime(mapType, closetAnimationData); // 이 애니메이션이 끝나는 시간

            if (!mediator.gameSettings.isEditor && closetAnimationData != lastBossAnimationData)
            {
                lastBossAnimationData = closetAnimationData;

                // 애니메이션이 공격 타입이면, 공격 타입에 따른 이펙트를 설정합니다.
                Spine.Bone effectSpawnBone = null;
                Effect fireEffect = null;
                switch (lastBossAnimationData.type)
                {
                    case BossAnimationType.Attack1_Air:
                    case BossAnimationType.Attack1_Road:
                    effectSpawnBone = weapon1NoteEffectSpawnBone;
                    fireEffect = weapon1NoteEffect;
                    break;

                    case BossAnimationType.Attack2:
                    effectSpawnBone = weapon2NoteEffectSpawnBone;
                    fireEffect = weapon2NoteEffect;
                    break;
                }

                // 생성해야 할 이펙트가 있으면 그 이펙트를 생성합니다.
                if(effectSpawnBone != null && fireEffect != null)
                {
                    Effect effect = mediator.effectPool.SpawnEffect(fireEffect);
                    effect.transform.position = effectSpawnBone.GetWorldPosition(spine.transform);
                }
            }

            // 플래그가 활성화 상태면 기존 애니메이션 재생시간 대신 레벨 에디터에서 지정한 재생시간을 사용합니다.
            if (closetAnimationData.useUnifiedDuration)
            {
                // duration * x = unifiedTime
                // x = unifiedTime / duration
                float divider = closetAnimationData.unifiedDuration / closetAnimationInfo.animation.Duration;
                animationTime /= divider;
            }

            // 다음 애니메이션이 존재하는 경우 그 애니메이션을 가져옵니다.
            BossAnimationData nextAnimationData = closetIndex + 1 < bossAnimationDatas.Count ? bossAnimationDatas[closetIndex + 1] : null;

            // 다음 애니메이션으로의 애니메이션 전환효과가 있다면 stateChangeAnimationType에 None이 아닌 값이 저장됩니다.
            BossAnimationType stateChangeAnimationType = BossAnimationType.None;
            float stateChangeMinNormalizedDuration = 0;
            if (nextAnimationData != null)
            {
                stateChangeAnimationType = GetStateChangeAnimationType(
                        animationsInfos[closetAnimationData.type].subData.state,
                        animationsInfos[nextAnimationData.type].subData.state,
                        out stateChangeMinNormalizedDuration);

                // 같은 애니메이션으로의 자동 전환을 방지합니다.
                if (stateChangeAnimationType == nextAnimationData.type)
                {
                    stateChangeAnimationType = BossAnimationType.None;
                }
            }

            // 애니메이션 전환효과가 필요하다면 다음 애니메이션 전까지 그 애니메이션을 재생하기 위해 변수를 준비합니다.
            BossAnimationInfo stateChangeAnimationInfo = null;
            float stateChangeAnimationEndTime = float.MaxValue; // 상태전환 애니메이션 종료시간
            float stateChangeAnimationStartTime = float.MaxValue; // 상태전환 애니메이션 시작시간
            if (stateChangeAnimationType != BossAnimationType.None)
            {
                stateChangeAnimationInfo = animationsInfos[stateChangeAnimationType];
                // 다음 애니메이션이 시작되기 전까지 상태전환 애니메이션을 재생합니다.
                stateChangeAnimationEndTime = nextAnimationData.time;
                // 다음 애니메이션의 시작시간에서 상태전환 애니메이션 시간을 뺀 시간부터 재생합니다.
                stateChangeAnimationStartTime = nextAnimationData.time - stateChangeAnimationInfo.animation.Duration;

                // 현재 재생중인 애니메이션이 루프 애니메이션이 아니면
                if (!closetAnimationInfo.subData.loop)
                {
                    float stateChangeMinDuration = stateChangeMinNormalizedDuration * stateChangeAnimationInfo.animation.Duration;
                    // 상태전환 애니메이션의 시작시간을 범위 내에 고정합니다.
                    // 현재 애니메이션이 끝나는 시간과 상태전환 애니메이션이 끝나는 시간 사이.
                    stateChangeAnimationStartTime = Mathf.Clamp(stateChangeAnimationStartTime, closetAnimationEndTime, stateChangeAnimationEndTime);
                    // 재설정된 상태 애니메이션의 재생시간이 최소 재생시간보다 짧은 경우 최소 재생시간을 보장하도록
                    // 상태전환 애니메이션의 시작시간을 앞당깁니다.
                    if (stateChangeAnimationEndTime - stateChangeAnimationStartTime < stateChangeMinDuration)
                    {
                        stateChangeAnimationStartTime = stateChangeAnimationEndTime - stateChangeMinDuration;
                    }
                }
                else
                {
                    stateChangeAnimationStartTime = Mathf.Clamp(stateChangeAnimationStartTime, closetAnimationData.time, stateChangeAnimationEndTime);
                }
            }

            if (mediator.music.adjustedTime >= closetAnimationData.time)
            {
                if (stateChangeAnimationInfo != null && mediator.music.adjustedTime > stateChangeAnimationStartTime)
                {
                    // 다음 애니메이션에서 상태가 변환된다면
                    // 다음 애니메이션이 시작되기 직전에 상태 변환 애니메이션을 적용합니다.
                    float normalizedStateChangeAnimationTime =
                        MathUtility.SmoothStep(stateChangeAnimationStartTime, stateChangeAnimationEndTime, mediator.music.adjustedTime);
                    float stateChangeAnimationTime = normalizedStateChangeAnimationTime * stateChangeAnimationInfo.animation.Duration;

                    spine.ClearState();
                    spine.ApplyAnimation(stateChangeAnimationInfo.animation, stateChangeAnimationTime, stateChangeAnimationInfo.subData.loop);
                }
                else if (animationTime <= closetAnimationInfo.animation.Duration)
                {
                    // 해당 애니메이션이 완료되지 않을 재생시간이고, 플래그가 꺼져 있을 때 해당 애니메이션을 적용합니다.
                    spine.ClearState();
                    spine.ApplyAnimation(closetAnimationInfo.animation, animationTime, closetAnimationInfo.subData.loop);
                }
                else
                {
                    // 애니메이션의 끝 이후 상태 애니메이션을 적용합니다.
                    BossAnimationType stateAnimationType = GetStateAnimationType(closetAnimationInfo.subData.state);
                    BossAnimationInfo stateAnimationInfo = animationsInfos[stateAnimationType];

                    // 애니메이션이 끝난 시점부터 음악 재생시간까지의 시간입니다.
                    float stateAnimationTime = mediator.music.adjustedTime - closetAnimationEndTime;

                    spine.ClearState();
                    spine.ApplyAnimation(stateAnimationInfo.animation, stateAnimationTime, stateAnimationInfo.subData.loop);
                }
            }
            else
            {
                // 재생할 애니메이션이 없다면 기본 애니메이션(Outside)을 재생합니다.
                ApplyDefaultAnimation();
            }
        }
        else
        {
            // 재생할 애니메이션이 없다면 기본 애니메이션(Outside)을 재생합니다.
            ApplyDefaultAnimation();
        }
    }

    public void GameReset()
    {
        manualAnimationType = BossAnimationType.None;
    }
}
