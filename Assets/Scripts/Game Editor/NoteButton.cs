using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Spine.Unity;

public class NoteButton : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    [SerializeField]
    Button buttonComp = null;

    [SerializeField]
    SkeletonGraphic spine = null;

    [SerializeField]
    Image image = null;

    [HideInInspector]
    Note noteTemplate = null;

    public Button button => buttonComp;

    public Note instancedNoteTemplate => noteTemplate;

    void UpdateVisualize(Note noteTemplate)
    {
        spine.Clear();
        image.sprite = null;

        spine.gameObject.SetActive(false);
        image.gameObject.SetActive(false);

        this.noteTemplate = noteTemplate;

        NoteVisualizeMethod method = noteTemplate.GetNoteVisualizeMethod();
        switch (method)
        {
            case NoteVisualizeMethod.Spine:
            {
                spine.initialSkinName = noteTemplate.GetSkinAtTime(mediator.music.playingTime);
                spine.skeletonDataAsset = noteTemplate.GetNoteVisualizeSpineDataAtTime(mediator.music.playingTime);
                spine.Initialize(true);
                spine.GraphicUpdateComplete();

                spine.gameObject.SetActive(true);
            }
            break;

            case NoteVisualizeMethod.Sprite:
            {
                image.sprite = noteTemplate.GetNoteVisualizeSpriteDataAtTime(mediator.music.playingTime);
                image.color = noteTemplate.GetNoteVisualizeSpriteColor();

                image.gameObject.SetActive(true);
            }
            break;
        }
    }

    public void Init(Note noteTemplate)
    {
        UpdateVisualize(noteTemplate);
    }

    private void Update()
    {
        if (noteTemplate.UpdateMapType())
        {
            UpdateVisualize(noteTemplate);
        }
    }
}

