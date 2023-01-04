using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public class SpineEffectPool : MonoBehaviour, IGameReset
{
    List<SpineEffect> effects = new List<SpineEffect>();
    Stack<SpineEffect> usableEeffects = new Stack<SpineEffect>();

    private SpineEffect AddEffect()
    {
        GameObject effectObject = new GameObject("effect");
        effectObject.transform.SetParent(this.transform);
        
        SpineEffect effect = effectObject.gameObject.AddComponent<SpineEffect>();
        effect.OnComplete += OnCompleteEffect;
        effects.Add(effect);
        return effect;
    }

    public SpineEffect SpawnEffect(SkeletonDataAsset asset, string animationName, SpineEffect.SetupDesc desc)
    {
        return SpawnEffect_Internal(asset, animationName, desc, false);
    }

    public SpineEffect SpawnEffectToFixedUpdate(SkeletonDataAsset asset, string animationName, SpineEffect.SetupDesc desc, float fixedUpdateStartTime, Func<float> fixedUpdateTimeFunction = null)
    {
        return SpawnEffect_Internal(asset, animationName, desc, true, fixedUpdateStartTime, fixedUpdateTimeFunction);
    }

    public SpineEffect SpawnEffect_Internal(SkeletonDataAsset asset, string animationName, SpineEffect.SetupDesc desc, bool fixedUpdate = false, float fixedUpdateStartTime = 0, Func<float> fixedUpdateTimeFunction = null)
    {
        SpineEffect effect = null;
        if (usableEeffects.Count == 0)
        {
            effect = AddEffect();
        }
        else
        {
            effect = usableEeffects.Pop();
            effect.gameObject.SetActive(true);
        }

        if (fixedUpdate)
        {
            effect.SetSpineToFixedUpdate(asset, animationName, desc, fixedUpdateStartTime, fixedUpdateTimeFunction);
        }
        else
        {
            effect.SetSpine(asset, animationName, desc);
        }

        return effect;
    }

    public void HideAll()
    {
        foreach(SpineEffect effect in effects)
        {
            effect.ResetEffect();
        }
    }

    private void OnCompleteEffect(SpineEffect effect, SpineEffect.CompleteResult result)
    {
        effect.gameObject.SetActive(false);
        usableEeffects.Push(effect);
    }

    public void GameReset()
    {
        HideAll();
    }
}
