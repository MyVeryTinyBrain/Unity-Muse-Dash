using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class ScoreNote : SimpleNote
{
    public override NoteType GetNoteType()
    {
        return NoteType.Score;
    }

    public override NoteVisualizeMethod GetNoteVisualizeMethod()
    {
        return NoteVisualizeMethod.Spine;
    }

    public override SkeletonDataAsset GetNoteVisualizeSpineDataAtTime(float playingTime)
    {
        MapType mapType = mediator.music.GetMapTypeAtTime(playingTime);
        return airSpineDatas[(int)mapType];
    }

    public override Sprite GetNoteVisualizeSpriteDataAtTime(float playingTime)
    {
        return null;
    }

    protected override void OnInteract()
    {
        mediator.gameState.AddScore(100);
    }
}
