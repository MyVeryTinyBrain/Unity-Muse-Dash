using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboResultUI : MonoBehaviour
{
    protected Mediator mediator => Mediator.i;

    public Effect perfectPrefab = null;
    public Effect perfectFeverPrefab = null;
    public Effect greatPrefab = null;
    public Effect greatFeverPrefab = null;
    public Effect passPrefab = null;

    public Effect SpawnPassResult(NotePosition notePosition)
    {
        Vector2 worldPosition;
        switch (notePosition)
        {
            case NotePosition.Air:
            worldPosition = mediator.hitPoint.airPos;
            break;

            case NotePosition.Center:
            worldPosition = mediator.hitPoint.Pos;
            break;

            default:
            case NotePosition.Road:
            worldPosition = mediator.hitPoint.roadPos;
            break;
        }

        Effect effect = mediator.effectPool.SpawnEffect(passPrefab);
        effect.gameObject.transform.position = worldPosition;
        return effect;
    }

    public Effect SpawnResult(NotePosition notePosition, ComboPrecision type)
    {
        Vector2 worldPosition;
        switch (notePosition)
        {
            case NotePosition.Air:
            worldPosition = mediator.hitPoint.airPos;
            break;

            case NotePosition.Center:
            worldPosition = mediator.hitPoint.Pos;
            break;

            default:
            case NotePosition.Road:
            worldPosition = mediator.hitPoint.roadPos;
            break;
        }

        Effect prefab = null;
        switch (type)
        {
            case ComboPrecision.Great:
            if (mediator.gameState.isFever)
            {
                prefab = greatFeverPrefab;
            }
            else
            {
                prefab = greatPrefab;
            }
            break;

            default:
            case ComboPrecision.Perfect:
            if (mediator.gameState.isFever)
            {
                prefab = perfectFeverPrefab;
            }
            else
            {
                prefab = perfectPrefab;
            }
            break;
        }

        Effect effect = mediator.effectPool.SpawnEffect(prefab);
        effect.gameObject.transform.position = worldPosition;
        return effect;
    }
}
