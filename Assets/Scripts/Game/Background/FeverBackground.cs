using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FeverBackground : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    new Animation animation = null;

    [SerializeField]
    GameObject backgroundBody = null;

    [SerializeField]
    GameObject elementBody = null;

    [SerializeField]
    Effect feverEffect = null;

    [SerializeField]
    SpriteBounds feverEffectArea = null;
    
    List<SpriteRenderer> backgrounds = new List<SpriteRenderer>();
    List<SpriteRenderer> elements = new List<SpriteRenderer>();

    bool showing = false;
    bool showAnimating = false;
    float showStartTime = float.MinValue;
    float showDuration = 0.4f;
    bool hiding = false;
    float hideStartTime = float.MinValue;
    float hideDuration = 0.3f;
    bool isShow = false;

    [HideInInspector]
    public bool spawnFx = false;
    float prevFxSpawnTime = float.MinValue;
    float fxSpawnDelay = 0.3f;

    private void CollectRenderers()
    {
        backgrounds.Clear();
        backgrounds.AddRange(backgroundBody.GetComponentsInChildren<SpriteRenderer>(true));

        elements.Clear();
        elements.AddRange(elementBody.GetComponentsInChildren<SpriteRenderer>(true));
    }

    private void SetBackgroundAlphas(float alpha)
    {
        SetAlphas(backgrounds, alpha);
    }

    private void SetElementAlphas(float alpha)
    {
        SetAlphas(elements, alpha);
    }

    private void SetAlphas(List<SpriteRenderer> renderers, float alpha)
    {
        foreach (SpriteRenderer renderer in renderers)
        {
            Color color = renderer.color;
            color.a = alpha;
            renderer.color = color;
        }
    }

    public void Show()
    {
        if (showing)
        {
            return;
        }

        if (elements.Count == 0)
        {
            CollectRenderers();
        }

        gameObject.SetActive(true);
        animation.Stop();
        animation.Play();
        SetBackgroundAlphas(1);
        SetElementAlphas(0);

        showing = true;
        showAnimating = true;
        hiding = false;
        isShow = false;
    }

    public void Hide()
    {
        if (hiding)
        {
            return;
        }

        hiding = true;
        hideStartTime = Time.time;
    }

    private void ClearShowAndHideState()
    {
        showing = false;
        showAnimating = false;
        hiding = false;
        isShow = false;
    }

    public void ShowImmediate()
    {
        ClearShowAndHideState();
        SetElementAlphas(1);
        SetBackgroundAlphas(1);
        isShow = true;
    }

    public void HideImmediate()
    {
        ClearShowAndHideState();
        SetElementAlphas(0);
        SetBackgroundAlphas(0);
        gameObject.SetActive(false);
        isShow = false;
    }

    private void Showing_Internal()
    {
        if (!showing)
        {
            return;
        }

        if (showAnimating)
        {
            if (animation.isPlaying)
            {
                showStartTime = Time.time;
            }
            else
            {
                showAnimating = false;
            }
        }

        float showRatio = Mathf.Clamp01((Time.time - showStartTime) / showDuration);
        if (showRatio < 1)
        {
            SetElementAlphas(showRatio);
        }
        else
        {
            ShowImmediate();
        }
    }

    private void Hiding_Internal()
    {
        if (!hiding)
        {
            return;
        }

        float hideRatio = Mathf.Clamp01((Time.time - hideStartTime) / hideDuration);
        if (hideRatio < 1)
        {
            SetElementAlphas(1 - hideRatio);
            SetBackgroundAlphas(1 - hideRatio);
        }
        else
        {
            HideImmediate();
        }
    }

    void Spawn_LineFx()
    {
        Effect effect = mediator.effectPool.SpawnEffect(feverEffect);
        float x = UnityEngine.Random.Range(feverEffectArea.worldMinX, feverEffectArea.worldMaxX);
        float y = UnityEngine.Random.Range(feverEffectArea.worldMinY, feverEffectArea.worldMaxY);
        effect.gameObject.transform.position = new Vector2(x, y);
    }

    private void Awake()
    {
        CollectRenderers();
    }

    private void Update()
    {
        Showing_Internal();
        Hiding_Internal();

        if(spawnFx && isShow && prevFxSpawnTime + fxSpawnDelay < Time.time)
        {
            prevFxSpawnTime = Time.time;
            Spawn_LineFx();
        }
    }
}
