using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Effect : MonoBehaviour
{
    public enum CompleteResult { Completed, Aborted };
    public delegate void OnEffectCompleteDelegate(Effect effect, CompleteResult result);
    public event OnEffectCompleteDelegate OnComplete;

    List<Animation> currentAnimations = new List<Animation>();
    List<ParticleSystem> currentParticles = new List<ParticleSystem>();

    [Header("On Start")]
    public List<Animation> startAnimations = new List<Animation>();
    public List<ParticleSystem> startParicleSystems = new List<ParticleSystem>();

    [Header("On End")]
    public List<Animation> endAnimations = new List<Animation>();
    public List<ParticleSystem> endParicleSystems = new List<ParticleSystem>();

    public bool playOnAwake = true;

    bool playing = false;
    public bool isPlaying => playing;
    public bool isCompleted => IsCompleted();

    SortingGroup sortingGroup = null;
    public int sortingOrder
    {
        get => sortingGroup ? sortingGroup.sortingOrder : 0;
        set
        {
            if (sortingGroup)
            {
                sortingGroup.sortingOrder = value;
            }
        }
    }

    public void ClearState()
    {
        ClearState_Internal(CompleteResult.Aborted);
    }

    protected void ClearState_Internal(CompleteResult result)
    {
        if (playing)
        {
            OnComplete?.Invoke(this, result);
            playing = false;
        }

        startAnimations.ForEach((x) => ClearAnimation(x));
        startParicleSystems.ForEach((x) =>
        {
            x.Stop();
            x.gameObject.SetActive(false);
        });
        endAnimations.ForEach((x) => ClearAnimation(x));
        endParicleSystems.ForEach((x) =>
        {
            x.Stop();
            x.gameObject.SetActive(false);
        });

        currentAnimations.Clear();
        currentParticles.Clear();
    }

    public void PlayEffect()
    {
        playing = true;

        currentAnimations.ForEach((x) => ClearAnimation(x));
        currentAnimations.Clear();
        startAnimations.ForEach((x) =>
        {
            x.gameObject.SetActive(true);
            x.Play();
            currentAnimations.Add(x);
        });

        currentParticles.Clear();
        startParicleSystems.ForEach((x) =>
        {
            x.gameObject.SetActive(true);
            x.Play();
            currentParticles.Add(x);
        });
    }

    public void PlayStopEffect()
    {
        playing = true;

        currentAnimations.ForEach((x) => ClearAnimation(x));
        currentAnimations.Clear();
        endAnimations.ForEach((x) =>
        {
            x.gameObject.SetActive(true);
            x.Play();
            currentAnimations.Add(x);
        });

        currentParticles.ForEach((x) => x.Stop(true, ParticleSystemStopBehavior.StopEmitting));
        currentParticles.Clear();
        endParicleSystems.ForEach((x) =>
        {
            x.gameObject.SetActive(true);
            x.Play(true);
            currentParticles.Add(x);
        });
    }

    void ClearAnimation(Animation animation)
    {
        animation.Stop();
        animation.gameObject.SetActive(false);
    }

    bool IsCompleted()
    {
        foreach (Animation animation in currentAnimations)
        {
            if (animation.isPlaying)
            {
                return false;
            }
        }
        foreach (ParticleSystem particleSystem in currentParticles)
        {
            if (particleSystem.isPlaying)
            {
                return false;
            }
        }
        return true;
    }

    private void Awake()
    {
        ClearState();
        if (playOnAwake)
        {
            PlayEffect();
        }

        sortingGroup = GetComponent<SortingGroup>();
    }

    private void Update()
    {
        if (playing && IsCompleted())
        {
            ClearState_Internal(CompleteResult.Completed);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(Effect))]
[CanEditMultipleObjects]
public class EffectEditor : Editor
{
    Effect targetEffect = null;

    protected virtual void OnEnable()
    {
        targetEffect = (Effect)target;
    }

    public override void OnInspectorGUI()
    {
        bool guiEnabled = GUI.enabled;

        GUILayout.BeginHorizontal();
        GUI.enabled = Application.isPlaying;
        if (GUILayout.Button("Start"))
        {
            targetEffect.gameObject.SetActive(true);
            targetEffect.PlayEffect();
        }
        if (GUILayout.Button("Stop"))
        {
            targetEffect.PlayStopEffect();
        }
        GUILayout.EndHorizontal();

        GUI.enabled = guiEnabled;

        base.OnInspectorGUI();
    }
}
#endif
