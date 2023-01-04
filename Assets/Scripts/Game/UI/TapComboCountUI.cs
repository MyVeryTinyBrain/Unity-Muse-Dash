using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TapComboCountUI : MonoBehaviour, IGameReset
{
    [SerializeField]
    Animation tapAnimation = null;

    [SerializeField]
    GameObject body = null;

    [SerializeField]
    TextMeshPro text = null;

    int visibleStack = 0;

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
        if(int.TryParse(text.text, out value))
        {
            return value;
        }
        return 0;
    }

    void SetCombo(int combo, bool playAnimation)
    {
        if (playAnimation)
        {
            tapAnimation.Stop();
            tapAnimation.Play();
        }

        text.text = combo.ToString();
    }

    public void AddVisibleStack()
    {
        visibleStack++;
        Show_Internal();
    }

    public void RemoveVisibleStack()
    {
        if(visibleStack == 0)
        {
            return;
        }

        visibleStack = Mathf.Clamp(visibleStack - 1, 0, int.MaxValue);

        if(visibleStack == 0)
        {
            Hide_Internal();
        }
    }

    public void Clear()
    {
        visibleStack = 0;
        Hide_Internal();
    }

    void Show_Internal()
    {
        body.gameObject.SetActive(true);
    }

    void Hide_Internal()
    {
        text.text = "1";
        body.gameObject.SetActive(false);
    }

    private void Awake()
    {
        Hide_Internal();
    }

    public void GameReset()
    {
        visibleStack = 0;
        Hide_Internal();
    }
}
