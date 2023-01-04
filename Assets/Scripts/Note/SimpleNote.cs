using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SimpleNote : Note
{
    const string AirNoteAnimationName = "in_air";
    const string RoadNoteAnimationName = "in_road";

    [SerializeField]
    Effect interactEffect = null;

    protected override void OnValidate()
    {
        base.OnValidate();

        placeableDesc.Air = true;
        placeableDesc.Center = false;
        placeableDesc.Road = true;
    }

    public override void Init()
    {
        base.Init();

        string animationName = "";
        switch (data.position)
        {
            case NotePosition.Air:
            animationName = AirNoteAnimationName;
            break;

            case NotePosition.Road:
            animationName = RoadNoteAnimationName;
            break;
        }

        airSpine.AnimationName = animationName;
        roadSpine.AnimationName = animationName;
    }

    protected override void UpdateLocalPosition()
    {
        base.UpdateLocalPosition();

        float y = GetHitLocalY();
        transform.localPosition = new Vector2(ratio * mediator.gameSettings.lengthPerSeconds, y);
    }

    protected virtual void OnInteract() { }

    void SpawnInteractEffect()
    {
        float worldY = 0;
        switch (data.position)
        {
            case NotePosition.Air:
            worldY = mediator.hitPoint.airPos.y;
            break;

            case NotePosition.Center:
            worldY = mediator.hitPoint.Pos.y;
            break;

            case NotePosition.Road:
            worldY = mediator.hitPoint.roadPos.y;
            break;
        }

        Effect effect = mediator.effectPool.SpawnEffect(interactEffect);

        float worldX = mediator.character.transform.position.x;
        Vector2 worldPosition = new Vector2(worldX, worldY);
        effect.transform.position = new Vector2(worldX, worldY);
    }

    public bool IsOnInteractibleRange()
    {
        float characterTime = mediator.character.timeOfCharacterPos;
        float diffTime = characterTime - data.time;
        return diffTime >= 0 && diffTime < mediator.gameSettings.simpleNoteInteractibleRangeTime;

    }

    void TryInteract()
    {
        if (!mediator.gameSettings.isEditor)
        {
            if (mediator.character.characterPosition != data.position)
            {
                return;
            }

            if (!IsOnSimplteNoteInteractibleRange())
            {
                return;
            }

            isDead = true;
            SpawnInteractEffect();
            mediator.gameState.AddScore(defaultScore);
            mediator.gameState.AddAccuracy(1.0f);
            OnInteract();
        }
    }

    protected override void NoteUpdate()
    {
        base.NoteUpdate();

        TryInteract();
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

    public override int GetMaxCombo()
    {
        return 0;
    }
}
