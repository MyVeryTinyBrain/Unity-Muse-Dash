using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour, IGameReset
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    Image hpBar = null;

    [SerializeField]
    TextMeshProUGUI hpText = null;

    [SerializeField]
    Image feverBar = null;
    float prevFeverRatio = 0.0f;
    float targetFeverRatio = 0.0f;
    float feverSettedTime = float.MinValue;
    float feverFillDelay = 0.1f;

    [SerializeField]
    Image feverEffectImg = null;
    RectTransform feverEffectImgRect = null;
    RectTransform feverEffectImgParent = null;

    [SerializeField]
    TextMeshProUGUI scoreText = null;

    [SerializeField]
    new Animation animation = null;

    [SerializeField]
    AnimationClip initClip = null;

    [SerializeField]
    AnimationClip showClip = null;

    [SerializeField]
    AnimationClip hideClip = null;

    [SerializeField]
    Animation readygo = null;

    [SerializeField]
    Animation fullcombo = null;

    string currentAnimation = "";
    float animationTime = 0.0f;

    Coroutine coroutine_Show = null;

    public void SetHP(float maxHP, float hp)
    {
        float ratio = hp / maxHP;
        hpBar.fillAmount = Mathf.Clamp(ratio, 0, 1);
        hpText.text = $"{((int)hp).ToString()} / {((int)maxHP).ToString()}";
    }

    public void ResetFever()
    {
        SetFeverImmediate(0);
    }

    public void SetFeverImmediate(float ratio)
    {
        ratio = Mathf.Clamp(ratio, 0, 1);
        prevFeverRatio = ratio;
        targetFeverRatio = ratio;
        feverBar.fillAmount = ratio;
        feverSettedTime = float.MinValue;
    }

    public void SetFever(float prevRatio, float ratio)
    {
        prevFeverRatio = Mathf.Clamp(prevRatio, 0, 1);
        targetFeverRatio = Mathf.Clamp(ratio, 0, 1);
        feverSettedTime = Time.time;

        if (feverEffectImgRect == null)
        {
            feverEffectImgRect = feverEffectImg.transform as RectTransform;
            feverEffectImgParent = feverEffectImgRect.parent as RectTransform;
        }

        float worldMinX = feverEffectImgParent.TransformPoint(feverEffectImgParent.rect.xMin, 0, 0).x;
        float worldMaxX = feverEffectImgParent.TransformPoint(feverEffectImgParent.rect.xMax, 0, 0).x;
        float worldX = Mathf.Lerp(worldMinX, worldMaxX, prevFeverRatio);

        Vector2 worldPosition = feverEffectImgRect.position;
        worldPosition.x = worldX;
        feverEffectImgRect.position = worldPosition;
    }

    void UpdateFeverGauge()
    {
        float currentTime = Time.time;
        float diffTime = currentTime - feverSettedTime;
        float lerpRatio = Mathf.Clamp01(diffTime / feverFillDelay);

        float feverRatio = Mathf.Lerp(prevFeverRatio, targetFeverRatio, lerpRatio);
        feverBar.fillAmount = feverRatio;

        float effectAlpha = 1 - lerpRatio;
        Color color = feverEffectImg.color;
        color.a = effectAlpha;
        feverEffectImg.color = color;
    }

    public void SetScore(int score)
    {
        scoreText.text = score.ToString();
    }

    public void Show_ReadyGo()
    {
        readygo.gameObject.SetActive(true);
        readygo.Stop();
        readygo.Play();

        mediator.fxAudio.Play(FXA.V_ReadyGO);
    }

    public void Show_FullCombo()
    {
        fullcombo.gameObject.SetActive(true);
        fullcombo.Stop();
        fullcombo.Play();

        mediator.fxAudio.Play(FXA.V_FullCombo);
    }

    public void Show(float delay = 0.0f)
    {
        if (coroutine_Show != null)
        {
            StopCoroutine(coroutine_Show);
        }
        coroutine_Show = StartCoroutine(Coroutine_Show(delay));
    }

    IEnumerator Coroutine_Show(float delay)
    {
        yield return new WaitForSeconds(delay);

        PlayAnimation(showClip);
        coroutine_Show = null;
    }

    public void HideImmediate()
    {
        if (coroutine_Show != null)
        {
            StopCoroutine(coroutine_Show);
            coroutine_Show = null;
        }

        PlayAnimation(initClip);

        readygo.gameObject.SetActive(false);
        fullcombo.gameObject.SetActive(false);
    }

    public void Hide()
    {
        if (coroutine_Show != null)
        {
            StopCoroutine(coroutine_Show);
            coroutine_Show = null;
        }

        PlayAnimation(hideClip);

        readygo.gameObject.SetActive(false);
        fullcombo.gameObject.SetActive(false);
    }

    void PlayAnimation(AnimationClip clip)
    {
        animation.Stop();
        animation.Play(clip.name);
        animation[clip.name].speed = 0.0f;

        currentAnimation = clip.name;
        animationTime = 0.0f;
    }

    void UpdateAnimation()
    {
        if (animation.isPlaying)
        {
            animationTime += Time.unscaledDeltaTime;
            animation[currentAnimation].time = animationTime;
        }
    }

    private void Awake()
    {
        if (!animation.isPlaying)
        {
            PlayAnimation(initClip);
        }

        //SetHP(100, 100);

        SetScore(0);

        // Hide effect image
        Color color = feverEffectImg.color;
        color.a = 0;
        feverEffectImg.color = color;
    }

    private void Update()
    {
        UpdateFeverGauge();
        UpdateAnimation();
    }

    public void GameReset()
    {
        HideImmediate();
    }
}
