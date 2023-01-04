using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Spine.Unity;

public class BossAnimationNote : BossNote
{
    [SerializeField]
    GameObject guide = null;

    [SerializeField]
    SpriteRenderer guideSprite = null;

    [SerializeField]
    TMPro.TextMeshPro text = null;

    public override NoteType GetNoteType()
    {
        return NoteType.BossAnimation;
    }

    public override NoteVisualizeMethod GetNoteVisualizeMethod()
    {
        return NoteVisualizeMethod.Sprite;
    }

    public override SkeletonDataAsset GetNoteVisualizeSpineDataAtTime(float playingTime)
    {
        return null;
    }

    public override Sprite GetNoteVisualizeSpriteDataAtTime(float playingTime)
    {
        return guideSprite.sprite;
    }

    public override Color GetNoteVisualizeSpriteColor()
    {
        return guideSprite.color;
    }

    protected override void OnValidate()
    {
        base.OnValidate();

        placeableDesc.Air = false;
        placeableDesc.Center = true;
        placeableDesc.Road = false;
    }

    public override void Init()
    {
        base.Init();
    }

    public override void Init_CreateInGame()
    {
        base.Init_CreateInGame();

        guide.gameObject.SetActive(false);
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();

        float y = GetHitLocalY();
        transform.localPosition = new Vector2(ratio * mediator.gameSettings.lengthPerSeconds, y);
    }

    protected override void UpdateBossAnimationData()
    {
        base.UpdateBossAnimationData();

        bossAnimationData.time = data.time;
    }

    protected override void UpdateAnimation()
    {
        base.UpdateAnimation(); 

        if (text)
        {
            text.text = bossAnimationData.type.ToString();
        }
    }

    protected override NoteResult OnHit(NotePosition notePosition)
    {
        return NoteResult.None();
    }

    protected override bool CustomDamagableState()
    {
        return false;
    }

    protected override bool CheckEndHitable()
    {
        return false;
    }

    public override int GetAccuracyScore()
    {
        return 0;
    }

    public override int GetMaxCombo()
    {
        return 0;
    }
}
