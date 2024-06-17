using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class EffectPool : MonoBehaviour, IGameReset
{
    // 게임오브젝트 이름으로 분류된 사용 가능한 이펙트 스택입니다.
    Dictionary<string, Stack<Effect>> usableEffects = new Dictionary<string, Stack<Effect>>();
    List<Effect> effects = new List<Effect>();

    private Effect AddEffect(Effect prefab)
    {
        Effect effect = Instantiate<Effect>(prefab);
        // 게임오브젝트 이름으로 분류되므로,
        // 복제한 게임오브젝트의 이름은 반드시 프리팹의 이름과 같아야 합니다.
        effect.gameObject.name = prefab.gameObject.name;
        effect.transform.SetParent(this.transform);
        effect.OnComplete += OnComplete;
        effects.Add(effect);
        return effect;
    }

    private void PushEffect(Effect effect)
    {
        Stack<Effect> stack = null;
        if(!usableEffects.TryGetValue(effect.gameObject.name, out stack))
        {
            stack = new Stack<Effect>();
            usableEffects.Add(effect.gameObject.name, stack);
        }
        stack.Push(effect);
    }

    private Effect PopEffect(Effect prefab)
    {
        Stack<Effect> stack = null;
        if (usableEffects.TryGetValue(prefab.gameObject.name, out stack))
        {
            return stack.Pop();
        }
        return null;
    }

    int GetUsableEffectCount(Effect prefab)
    {
        Stack<Effect> stack = null;
        if (usableEffects.TryGetValue(prefab.gameObject.name, out stack))
        {
            return stack.Count;
        }
        return 0;
    }

    public Effect SpawnEffect(Effect prefab)
    {
        Effect effect = null;
        if (GetUsableEffectCount(prefab) == 0)
        {
            // 사용 가능한 이펙트가 없다면
            // 새 이펙트를 생성합니다.
            effect = AddEffect(prefab);
        }
        else
        {
            // 사용 가능한 비활성화 이펙트가 있다면
            // 해당 이펙트를 사용합니다.
            effect = PopEffect(prefab);
            effect.gameObject.SetActive(true);
        }
        effect.PlayEffect();
        return effect;
    }

    public void HideAll()
    {
        foreach (Effect effect in effects)
        {
            effect.ClearState();
        }
    }

    private void OnComplete(Effect effect, Effect.CompleteResult result)
    {
        // 이펙트가 완료되면 비활성화하고, 스택에 담습니다.
        effect.gameObject.SetActive(false);
        PushEffect(effect);
    }

    public void GameReset()
    {
        HideAll();
    }
}
