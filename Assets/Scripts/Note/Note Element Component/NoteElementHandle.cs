using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NoteElementHandle : MonoBehaviour, INote
{
    bool selected = false;

    public abstract void SetTime(float time);
    public abstract float GetTime();
    public abstract void OnChangeSelection(bool selected);

    public void SetSelect(bool selection)
    {
        selected = selection;
        OnChangeSelection(selected);
    }
}
