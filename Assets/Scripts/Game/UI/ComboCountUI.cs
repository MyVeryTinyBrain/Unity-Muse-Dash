using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Spine.Unity;

public class ComboCountUI : MonoBehaviour
{
    public enum State { Hide, Level0, Level1, Level2 };

    [SerializeField]
    TextMeshProUGUI text = null;

    [SerializeField]
    SkeletonGraphic spine = null;

    [SerializeField]
    Animation comboAnimation = null;

    [SerializeField]
    SkeletonDataAsset comboAsset0 = null;

    [SerializeField]
    SkeletonDataAsset comboAsset1 = null;

    [SerializeField]
    SkeletonDataAsset comboAsset2 = null;

    [SerializeField]
    Material comboFontMaterial0 = null;

    [SerializeField]
    Material comboFontMaterial1 = null;

    [SerializeField]
    Material comboFontMaterial2 = null;

    State currentState = State.Hide;

    public State state
    {
        get => currentState;
        set => SetState(value, false);
    }

    public int combo
    {
        get => GetCombo();
        set => SetCombo(value, true);
    }

    public void SetComboWithoutAnimation(int combo)
    {
        SetCombo(combo, false);
    }

    int GetCombo()
    {
        int value;
        if (int.TryParse(text.text, out value))
        {
            return value;
        }
        return 0;
    }

    void SetCombo(int combo, bool playAnimation)
    {
        if (playAnimation)
        {
            comboAnimation.Stop();
            comboAnimation.Play();
        }

        text.text = combo.ToString();
    }

    void SetState(State comboState, bool forceExecution)
    {
        if(!forceExecution && comboState == currentState)
        {
            return;
        }

        switch (comboState)
        {
            case State.Level0:
            spine.Init(comboAsset0);
            text.fontSharedMaterial = comboFontMaterial0;
            break;

            case State.Level1:
            spine.Init(comboAsset1);
            text.fontSharedMaterial = comboFontMaterial1;
            break;

            case State.Level2:
            spine.Init(comboAsset2);
            text.fontSharedMaterial = comboFontMaterial2;
            break;
        }

        spine.gameObject.SetActive(true);

        switch (comboState)
        {
            case State.Hide:
            text.gameObject.SetActive(false);
            spine.AnimationState.ClearTracks();
            spine.AnimationState.AddAnimation(0, "end", false, 0);
            break;

            case State.Level0:
            case State.Level1:
            case State.Level2:
            text.gameObject.SetActive(true);
            spine.AnimationState.AddAnimation(0, "start", false, 0);
            spine.AnimationState.AddAnimation(0, "stand", true, 0);
            break;
        }

        currentState = comboState;
    }

    public void Clear()
    {
        text.text = "1";
        spine.gameObject.SetActive(false);
        text.gameObject.SetActive(false);
        state = State.Hide;
    }

    private void Awake()
    {
        spine.Init(comboAsset0);
        text.text = "1";
        Clear();
    }
}
