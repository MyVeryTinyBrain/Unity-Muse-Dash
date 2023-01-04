using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Spine.Unity;

[DisallowMultipleComponent]
public class NoteColor : NoteComponent
{
    List<SkeletonAnimation> spines = null;
    List<SpriteRenderer> sprites = null;
    List<LineRenderer> lines = null;
    List<NoteSubHandle> subHandles = null;

    public void SetColor(Color color)
    {
        spines.ForEach((x) => x.skeleton.SetColor(color));
        sprites.ForEach((x) => x.color = color);
        lines.ForEach((x) => x.startColor = x.endColor = color);
        //subHandles.ForEach((x) => x.subColor = color);
    }

    protected override void OnInit(Note note)
    {
        spines = new List<SkeletonAnimation>();
        spines.AddRange(GetComponentsInChildren<SkeletonAnimation>(true));

        sprites = new List<SpriteRenderer>();
        sprites.AddRange(GetComponentsInChildren<SpriteRenderer>(true));

        lines = new List<LineRenderer>();
        lines.AddRange(GetComponentsInChildren<LineRenderer>(true));

        subHandles = new List<NoteSubHandle>();
        subHandles.AddRange(GetComponentsInChildren<NoteSubHandle>(true));
    }

    protected override void OnPostInit()
    {

    }
}
