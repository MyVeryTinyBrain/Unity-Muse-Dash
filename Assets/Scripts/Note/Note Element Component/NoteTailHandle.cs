using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteTailHandle : NoteElementHandle
{
    ITailedNote note = null;

    private void Awake()
    {
        note = GetComponentInParent<ITailedNote>();
    }

    public override float GetTime()
    {
        return note.tailTime;
    }

    public override void SetTime(float time)
    {
        note.tailTime = time;
    }

    public override void OnChangeSelection(bool selection)
    {
        note.OnChangeTailSelection(selection);
    }
}
