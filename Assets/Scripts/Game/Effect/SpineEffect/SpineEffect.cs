using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

[RequireComponent(typeof(SkeletonAnimation))]
public class SpineEffect : MonoBehaviour
{
    public enum CompleteResult { Completed, Aborted };
    public delegate void OnCompleteDelegate(SpineEffect effect, CompleteResult result);
    public event OnCompleteDelegate OnComplete;

    SkeletonAnimation spine = null;
    new MeshRenderer renderer = null;

    // 애니메이션이 존재하면 고정 업데이트로 애니메이션을 재생합니다.
    Spine.Animation fixedUpdateAnimation = null;
    float fixedUpdateStartTime = 0;
    bool fixedUpdateLoop = false;
    Func<float> fixedUpdateTimeFunction = null;

    public string sortingLayer
    {
        get => renderer.sortingLayerName;
        set => renderer.sortingLayerName = value;
    }

    public int sortingOrder
    {
        get => renderer.sortingOrder;
        set => renderer.sortingOrder = value;
    }

    public bool visibility
    {
        get => renderer.enabled;
        set => renderer.enabled = value;
    }

    public void ResetEffect()
    {
        ResetEffect_Internal(CompleteResult.Aborted);
    }

    protected void ResetEffect_Internal(CompleteResult result)
    {
        Spine.TrackEntry trackEntry = spine.state.GetCurrent(0);
        if (trackEntry != null || fixedUpdateAnimation != null)
        {
            OnComplete?.Invoke(this, result);
        }

        visibility = false;

        sortingLayer = SortingLayers.Default;
        sortingOrder = 0;
        spine.loop = false;
        spine.ClearState();

        fixedUpdateAnimation = null;
        fixedUpdateStartTime = 0;
        fixedUpdateLoop = false;
        fixedUpdateTimeFunction = null;
    }

    [System.Serializable]
    public struct SetupDesc
    {
        public string skin;
        public string sortingLayer;
        public int sortingOrder;
        public bool loop;

        public static SetupDesc Default()
        {
            return new SetupDesc()
            {
                skin = "default",
                sortingLayer = SortingLayers.Default,
                sortingOrder = 0,
                loop = false,
            };
        }
    }

    public void SetSpine(SkeletonDataAsset asset, string animationName, SetupDesc desc)
    {
        SetSpine_Internal(asset, animationName, desc, false);
    }

    public void SetSpineToFixedUpdate(SkeletonDataAsset asset, string animationName, SetupDesc desc, float fixedUpdateStartTime, Func<float> fixedUpdateTimeFunction = null)
    {
        SetSpine_Internal(asset, animationName, desc, true, fixedUpdateStartTime, fixedUpdateTimeFunction);
    }

    public void SetSpine_Internal(SkeletonDataAsset asset, string animationName, SetupDesc desc, bool fixedUpdate = false, float fixedUpdateStartTime = 0, Func<float> fixedUpdateTimeFunction = null)
    {
        spine.Init(asset, desc.skin);

        if (fixedUpdate)
        {
            this.fixedUpdateAnimation = spine.skeleton.Data.FindAnimation(animationName);
            this.fixedUpdateStartTime = fixedUpdateStartTime;
            this.fixedUpdateLoop = desc.loop;
            this.fixedUpdateTimeFunction = fixedUpdateTimeFunction;

            FixedUpdateAnimate();
        }
        else
        {
            spine.AnimationName = animationName;
            spine.loop = desc.loop;
        }
        sortingLayer = desc.sortingLayer;
        sortingOrder = desc.sortingOrder;

        visibility = true; 
    }

    void FixedUpdateAnimate()
    {
        float currentTime = fixedUpdateTimeFunction != null ? fixedUpdateTimeFunction() : Time.time;
        float animationTime = currentTime - fixedUpdateStartTime;

        spine.ApplyAnimation(fixedUpdateAnimation, animationTime, fixedUpdateLoop);

        if (!fixedUpdateLoop && animationTime >= fixedUpdateAnimation.Duration)
        {
            ResetEffect_Internal(CompleteResult.Completed);
        }
    }

    private void Awake()
    {
        spine = GetComponent<SkeletonAnimation>();
        spine.UpdateComplete += OnUpdateComplete;

        renderer = GetComponent<MeshRenderer>();
        visibility = false;
    }

    private void Update()
    {
        if (fixedUpdateAnimation != null)
        {
            FixedUpdateAnimate();
        }
    }

    private void OnUpdateComplete(ISkeletonAnimation animated)
    {
        if (fixedUpdateAnimation != null)
        {
            return;
        }

        if (spine.loop)
        {
            return;
        }

        Spine.TrackEntry trackEntry = spine.state.GetCurrent(0);
        if(trackEntry == null)
        {
            return;
        }

        if(trackEntry.AnimationTime >= trackEntry.Animation.Duration)
        {
            ResetEffect_Internal(CompleteResult.Completed);
        }
    }
}
