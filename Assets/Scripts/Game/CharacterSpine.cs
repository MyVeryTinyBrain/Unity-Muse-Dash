using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;

public enum CharacterHitType
{
    None,
    Air_hit_great,
    Air_hit_perfect,
    Road_hit_great,
    Road_hit_perfect,
    Double_hit,
    Up_hit,
    Down_hit,
    Up_press_hit,
    Down_press_hit,
    Press,
}

public class CharacterSpine : MonoBehaviour
{
    public delegate void OnGroundDelegate();
    public delegate void OnAnimatedDelegate();
    public event OnGroundDelegate OnGroundCallback;
    public event OnAnimatedDelegate OnAnimatedCallback;

    public class AnimationInfo
    {
        public Spine.Animation animation;
        public AnimationDesc desc;

        public float timeMultiplier
        {
            get
            {
                return desc.useUnifiedDuration ? animation.Duration / desc.unifiedDuration : 1;
            }
        }
    }

    class CharacterHit
    {
        List<CharacterAnimationType> animations = new List<CharacterAnimationType>();
        int index = 0;
        
        public CharacterHit(params CharacterAnimationType[] animations)
        {
            this.animations.AddRange(animations);
        }

        public CharacterAnimationType Next()
        {
            return animations[(index++) % animations.Count];
        }
    }

    [SerializeField, ReadOnlyInRuntime]
    CharacterDescs descs = null;

    CharacterDesc character = null;

    [SerializeField]
    Shader shader = null;

    [SerializeField]
    Texture noise = null;

    [SerializeField, ReadOnlyInRuntime, ColorUsage(true, true)]
    Color hologramColor = new Color(177 / 255f * 2, 207 / 255f * 2, 255 / 255f * 2, 255 / 255f);

    [SerializeField, ReadOnlyInRuntime, ColorUsage(true, true)]
    Color hurtColor = Color.red;

    [SerializeField]
    SkeletonAnimation spine = null;

    bool hologram = false;

    Material mainMaterial = null;

    Dictionary<CharacterAnimationType, AnimationInfo> animations = new Dictionary<CharacterAnimationType, AnimationInfo>();
    AnimationInfo currentAnimationInfo = null;
    float currentAnimationTime = 0;
    float fixedUpdateStartTime = 0;
    Dictionary<CharacterHitType, CharacterHit> hits = new Dictionary<CharacterHitType, CharacterHit>();

    float lastHurtTime = float.MinValue;
    float hurtColorDuration = 0.5f;

    public CharacterDesc desc
    {
        get => character;
    }

    public SkeletonAnimation characterSpine
    {
        get => spine;
    }

    public float hurtColorVisibleDuration
    {
        get => hurtColorDuration;
        set => hurtColorDuration = Mathf.Clamp(value, 0, float.MaxValue);
    }

    public float dissolveAmount
    {
        get => mainMaterial.GetFloat("_DissolveAmount");
        set
        {
            mainMaterial.SetFloat("_DissolveAmount", Mathf.Clamp(value, 0, 1));
        }
    }

    public float animationTime
    {
        get => currentAnimationTime;
    }

    public AnimationInfo animationInfo
    {
        get => currentAnimationInfo;
    }

    private void GenerateHitDictionary()
    {
        hits.Clear();

        hits.Add(CharacterHitType.Air_hit_great, new CharacterHit(
            CharacterAnimationType.Air_hit_great_1,
            CharacterAnimationType.Air_hit_great_2,
            CharacterAnimationType.Air_hit_great_3));
        hits.Add(CharacterHitType.Air_hit_perfect, new CharacterHit(
            CharacterAnimationType.Air_hit_perfect_1,
            CharacterAnimationType.Air_hit_perfect_2,
            CharacterAnimationType.Air_hit_perfect_3,
            CharacterAnimationType.Air_hit_perfect_4));
        hits.Add(CharacterHitType.Road_hit_great, new CharacterHit(
            CharacterAnimationType.Road_hit_great_1,
            CharacterAnimationType.Road_hit_great_2,
            CharacterAnimationType.Road_hit_great_3));
        hits.Add(CharacterHitType.Road_hit_perfect, new CharacterHit(
            CharacterAnimationType.Road_hit_perfect_1,
            CharacterAnimationType.Road_hit_perfect_2,
            CharacterAnimationType.Road_hit_perfect_3,
            CharacterAnimationType.Road_hit_perfect_4));
        hits.Add(CharacterHitType.Double_hit, new CharacterHit(
            CharacterAnimationType.Double_hit_1,
            CharacterAnimationType.Double_hit_2));
        hits.Add(CharacterHitType.Up_hit, new CharacterHit(
            CharacterAnimationType.Up_hit));
        hits.Add(CharacterHitType.Down_hit, new CharacterHit(
            CharacterAnimationType.Down_hit));
        hits.Add(CharacterHitType.Up_press_hit, new CharacterHit(
            CharacterAnimationType.Up_press_hit));
        hits.Add(CharacterHitType.Down_press_hit, new CharacterHit(
            CharacterAnimationType.Down_press_hit));
        hits.Add(CharacterHitType.Press, new CharacterHit(
            CharacterAnimationType.Press));
    }

    public void Init(CharacterType characterType, bool hologram)
    {
        GenerateHitDictionary();

        int index = descs.descs.FindIndex((x) => x.type == characterType);
        if(index == -1)
        {
            Debug.LogError($"Can't find matched character type in character descs. (CharacterType: {characterType})");
            return;
        } 
        character = descs.descs[index];
        SkeletonDataAsset asset = character.asset;

        animations.Clear();
        foreach(AnimationDesc animationDesc in character.animationDescs)
        {
            Spine.SkeletonData data = asset.GetSkeletonData(true); 
            Spine.Animation animation = data.FindAnimation(animationDesc.name);
            AnimationInfo info = new AnimationInfo();
            info.animation = animation;
            info.desc = animationDesc;
            animations.Add(animationDesc.type, info);
        }

        spine.Init(asset);

        this.hologram = hologram;

        Material originalMaterial = spine.SkeletonDataAsset.atlasAssets[0].PrimaryMaterial;
        mainMaterial = new Material(shader);
        mainMaterial.CopyPropertiesFromMaterial(originalMaterial);
        mainMaterial.SetTexture("_NoiseTexture", noise);
        mainMaterial.SetTextureScale("_NoiseTexture", new Vector2(0.4f, 0.4f));
        mainMaterial.SetColor("_Tone", hologram ? hologramColor : Color.white);
        mainMaterial.SetFloat("_ToneAmount", hologram ? 1 : 0);

        // 아래의 코드는 동작하지 않음
        //material.SetInt("_StraightAlphaInput", 1);
        //
        // 이 함수에 false를 전달하여 반투명 모드를 활성화합니다.
        // 
        // 스파인 쉐이더의 shader_feature 키워드는 사용되지 않을 경우 게임 빌드에 포함되지 않습니다.
        // 따라서 게임 빌드에 포함시키기 위하여 multi_compile 키워드를 사용합니다.
        // #pragma shader_feature _ _STRAIGHT_ALPHA_INPUT
        // #pragma multi_compile _ _STRAIGHT_ALPHA_INPUT
        Utility.EnablePMAAtMaterial(mainMaterial, false);

        spine.CustomMaterialOverride.Clear();
        spine.CustomMaterialOverride.Add(originalMaterial, mainMaterial);

        if (hologram)
        {
            foreach (BlendModeMaterials.ReplacementMaterial m in spine.skeletonDataAsset.blendModeMaterials.additiveMaterials)
            {
                Material newReplacementMaterial = new Material(m.material);
                newReplacementMaterial.CopyPropertiesFromMaterial(m.material);
                newReplacementMaterial.SetColor("_Color", hologramColor);
                spine.CustomMaterialOverride.Add(m.material, newReplacementMaterial);
            }
            foreach (BlendModeMaterials.ReplacementMaterial m in spine.skeletonDataAsset.blendModeMaterials.multiplyMaterials)
            {
                Material newReplacementMaterial = new Material(m.material);
                newReplacementMaterial.CopyPropertiesFromMaterial(m.material);
                newReplacementMaterial.SetColor("_Color", hologramColor);
                spine.CustomMaterialOverride.Add(m.material, newReplacementMaterial);
            }
            foreach (BlendModeMaterials.ReplacementMaterial m in spine.skeletonDataAsset.blendModeMaterials.screenMaterials)
            {
                Material newReplacementMaterial = new Material(m.material);
                newReplacementMaterial.CopyPropertiesFromMaterial(m.material);
                newReplacementMaterial.SetColor("_Color", hologramColor);
                spine.CustomMaterialOverride.Add(m.material, newReplacementMaterial);
            }

            MeshRenderer meshRenderer = null;
            if (spine.TryGetComponent<MeshRenderer>(out meshRenderer))
            {
                meshRenderer.sortingOrder = -1;
            }
        }
    }

    private bool Play(CharacterAnimationType animationType)
    {
        currentAnimationTime = 0;
        return Play_Internal(animationType, Time.time);
    }

    private bool Play_Internal(CharacterAnimationType animationType, float fixedUpdateStartTime)
    {
        AnimationInfo info = null;
        if (animations.TryGetValue(animationType, out info))
        {
            this.fixedUpdateStartTime = fixedUpdateStartTime;
            currentAnimationInfo = info;

            return true;
        }

        return false;
    }

    private bool Change(CharacterAnimationType animationType, bool keepStartTime)
    {
        AnimationInfo info = null;
        if (animations.TryGetValue(animationType, out info))
        {
            currentAnimationInfo = info;

            if (!keepStartTime)
            {
                this.fixedUpdateStartTime = Time.time;
            }

            return true;
        }

        return false;
    }

    public void CopyAnimationState(CharacterSpine other)
    {
        AnimationInfo info = null;
        if (animations.TryGetValue(other.currentAnimationInfo.desc.type, out info))
        {
            currentAnimationInfo = info;
            currentAnimationTime = other.currentAnimationTime;
            fixedUpdateStartTime = other.fixedUpdateStartTime;
        }
    }

    public void Do_Run()
    {
        Play(CharacterAnimationType.Run);
    }

    public void Do_Hit(CharacterHitType type)
    {
        CharacterHit hit = null;
        if(hits.TryGetValue(type, out hit))
        {
            Play(hit.Next());
        }
    }

    public void Do_Jump()
    {
        Play(CharacterAnimationType.Jump);
    }

    public void Do_JumpToDown()
    {
        AnimationInfo info = null;
        if (!animations.TryGetValue(CharacterAnimationType.Jump_to_down, out info))
        {
            Play(CharacterAnimationType.Down_hit);
            return;
        }

        if(info.animation == null)
        {
            Play(CharacterAnimationType.Down_hit);
            return;
        }

        Play(CharacterAnimationType.Jump_to_down);
    }

    public void Do_MissOnRoad()
    {
        Play(CharacterAnimationType.Road_hit_miss);
    }

    public void Do_Hurt()
    {
        CharacterAnimationType currentType = currentAnimationInfo != null ? currentAnimationInfo.desc.type : CharacterAnimationType.None;
        switch (currentType)
        {
            case CharacterAnimationType.Press:
            case CharacterAnimationType.Up_press_hit:
            case CharacterAnimationType.Down_press_hit:
            // Do Nothing
            break;

            case CharacterAnimationType.Air_hit_great_1:
            case CharacterAnimationType.Air_hit_great_2:
            case CharacterAnimationType.Air_hit_great_3:
            case CharacterAnimationType.Air_hit_perfect_1:
            case CharacterAnimationType.Air_hit_perfect_2:
            case CharacterAnimationType.Air_hit_perfect_3:
            case CharacterAnimationType.Air_hit_perfect_4:
            case CharacterAnimationType.Up_hit:
            Change(CharacterAnimationType.Air_hit_hurt, true);
            break;

            case CharacterAnimationType.Jump:
            Change(CharacterAnimationType.Jump_hurt, true);
            break;

            case CharacterAnimationType.Jump_to_down:
            Change(CharacterAnimationType.Jump_to_down_hurt, true);
            break;

            case CharacterAnimationType.Air_press_end:
            Change(CharacterAnimationType.Air_press_hurt, true);
            break;

            default:
            Play(CharacterAnimationType.Hurt);
            break;
        }

        lastHurtTime = Time.time;
    }

    public void Do_PressToUp()
    {
        Play(CharacterAnimationType.Up_press_hit);
    }

    public void Do_PressToDown()
    {
        Play(CharacterAnimationType.Down_press_hit);
    }

    public void Do_Press()
    {
        Play(CharacterAnimationType.Press);
    }

    public void Do_EndPressInAir()
    {
        Do_Run();
    }

    public void Do_EndPressInRoad()
    {
        Play(CharacterAnimationType.Air_press_end);
    }

    public void Do_In()
    {
        Play(CharacterAnimationType.In);
    }

    public void Do_Die()
    {
        Play(CharacterAnimationType.Die);
    }

    void ApplyHurtTone()
    {
        float hurtedTime = Time.time - lastHurtTime;
        float toneAlpha = Mathf.Clamp01(hurtedTime / hurtColorDuration);
        float toneAmount = 1 - toneAlpha;
        Color tone = Color.Lerp(Color.white, hurtColor, toneAmount);
        mainMaterial.SetColor("_Tone", tone);
        mainMaterial.SetFloat("_ToneAmount", toneAmount);
    }

    private void Awake()
    {
        if (character == null)
        {
            Init(CharacterType.Rin_Rock, false);
        }

        spine.UpdateComplete += delegate { OnAnimatedCallback?.Invoke(); };
    }

    private void Update()
    {
        if (currentAnimationInfo == null)
        {
            return;
        }

        currentAnimationTime = Time.time - fixedUpdateStartTime;
        currentAnimationTime *= currentAnimationInfo.timeMultiplier;
        spine.ApplyAnimation(currentAnimationInfo.animation, currentAnimationTime, currentAnimationInfo.desc.loop);
        //spine.BlendAnimation(currentAnimationInfo.animation, currentAnimationTime, currentAnimationInfo.desc.loop, 1);

        if (hologram)
        {
            float x = currentAnimationTime / currentAnimationInfo.animation.Duration;
            dissolveAmount = Mathf.Pow(x, 5);
            return;
        }

        bool done = !currentAnimationInfo.desc.loop && currentAnimationTime >= currentAnimationInfo.animation.Duration;
        if (done)
        {
            switch (currentAnimationInfo.desc.type)
            {
                default:
                Change(CharacterAnimationType.Run, false);
                OnGroundCallback?.Invoke();
                break;

                case CharacterAnimationType.Up_press_hit:
                case CharacterAnimationType.Down_press_hit:
                Change(CharacterAnimationType.Press, true);
                break;

                case CharacterAnimationType.Die:
                case CharacterAnimationType.Press:
                case CharacterAnimationType.Run:
                break;
            }
        }

        ApplyHurtTone();
    }
}
