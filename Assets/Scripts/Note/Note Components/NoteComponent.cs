using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INoteComponent
{
    public void Init(Note note);
}

//[DisallowMultipleComponent]
public abstract class NoteComponent : MonoBehaviour, INoteComponent
{
    public Note note { get; private set; } = null;

    public void Init(Note note)
    {
        this.note = note;
        OnInit(note);
    }

    protected abstract void OnInit(Note note);
    protected abstract void OnPostInit();

    protected virtual void Start()
    {
        OnPostInit();
    }
}
