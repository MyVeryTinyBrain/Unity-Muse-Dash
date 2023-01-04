using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpriteBounds : MonoBehaviour
{
    public Vector2 localBoundPosition = Vector2.zero;
    public Vector2 localBoundSize = Vector2.zero;

    public float localMinX => localBoundPosition.x - localBoundSize.x * 0.5f;
    public float localMaxX => localBoundPosition.x + localBoundSize.x * 0.5f;
    public float localMinY => localBoundPosition.y - localBoundSize.y * 0.5f;
    public float localMaxY => localBoundPosition.y + localBoundSize.y * 0.5f;

    public float worldMinX => transform.TransformPoint(localMinX, 0, 0).x;
    public float worldMaxX => transform.TransformPoint(localMaxX, 0, 0).x;
    public float worldMinY => transform.TransformPoint(0, localMinY, 0).y;
    public float worldMaxY => transform.TransformPoint(0, localMaxY, 0).y;

    public float pivotToWorldMinX => worldMinX - transform.position.x;
    public float pivotToWorldMaxX => worldMaxX - transform.position.x;
    public float pivotToWorldMinY => worldMinY - transform.position.y;
    public float pivotToWorldMaxY => worldMaxY - transform.position.y;

    public Vector2 localLeftDown => new Vector2(localMinX, localMinY);
    public Vector2 localLeftUp => new Vector2(localMinX, localMaxY);
    public Vector2 localRightUp => new Vector2(localMaxX, localMaxY);
    public Vector2 localRightDown => new Vector2(localMaxX, localMinY);

    public Vector2 worldLeftDown => transform.TransformPoint(localLeftDown);
    public Vector2 worldLeftUp => transform.TransformPoint(localLeftUp);
    public Vector2 worldRightUp => transform.TransformPoint(localRightUp);
    public Vector2 worldRightDown => transform.TransformPoint(localRightDown);

    public void SnapLeftEdge(Vector2 worldPosition)
    {
        worldPosition.x -= pivotToWorldMinX;
        transform.position = worldPosition;
    }

    public void SnapRightEdge(Vector2 worldPosition)
    {
        worldPosition.x -= pivotToWorldMaxX;
        transform.position = worldPosition;
    }

    public void SnapDownEdge(Vector2 worldPosition)
    {
        worldPosition.y -= pivotToWorldMinY;
        transform.position = worldPosition;
    }

    public void SnapUpEdge(Vector2 worldPosition)
    {
        worldPosition.y -= pivotToWorldMaxY;
        transform.position = worldPosition;
    }

    public void SnapLeftEdge(SpriteBounds baseBounds, float distance = 0)
    {
        SnapLeftEdge(new Vector2(baseBounds.worldMaxX + distance, baseBounds.transform.position.y));
    }

    public void SnapRightEdge(SpriteBounds baseBounds, float distance = 0)
    {
        SnapRightEdge(new Vector2(baseBounds.worldMinX - distance, baseBounds.transform.position.y));
    }

    public void SnapDownEdge(SpriteBounds baseBounds, float distance = 0)
    {
        SnapDownEdge(new Vector2(baseBounds.transform.position.x, baseBounds.worldMaxY + distance));
    }

    public void SnapUpEdge(SpriteBounds baseBounds, float distance = 0)
    {
        SnapUpEdge(new Vector2(baseBounds.transform.position.x, baseBounds.worldMinY - distance));
    }

    public SpriteBounds CopyOnLeft(float distance = 0)
    {
        SpriteBounds newBounds = Instantiate<SpriteBounds>(this, this.transform.parent);
        newBounds.gameObject.name = this.gameObject.name;
        newBounds.SnapRightEdge(this, distance);
        return newBounds;
    }

    public SpriteBounds CopyOnRight(float distance = 0)
    {
        SpriteBounds newBounds = Instantiate<SpriteBounds>(this, this.transform.parent);
        newBounds.gameObject.name = this.gameObject.name;
        newBounds.SnapLeftEdge(this, distance);
        return newBounds;
    }

    public SpriteBounds CopyOnDown(float distance = 0)
    {
        SpriteBounds newBounds = Instantiate<SpriteBounds>(this, this.transform.parent);
        newBounds.gameObject.name = this.gameObject.name;
        newBounds.SnapUpEdge(this, distance);
        return newBounds;
    }

    public SpriteBounds CopyOnUp(float distance = 0)
    {
        SpriteBounds newBounds = Instantiate<SpriteBounds>(this, this.transform.parent);
        newBounds.gameObject.name = this.gameObject.name;
        newBounds.SnapDownEdge(this, distance);
        return newBounds;
    }

    public void SetLocalBound(float newLocalMinX, float newLocalMaxX, float newLocalMinY, float newLocalMaxY)
    {
        Vector2 newLocalMin = new Vector2(newLocalMinX, newLocalMinY);
        Vector2 newLocalMax = new Vector2(newLocalMaxX, newLocalMaxY);
        localBoundPosition = (newLocalMin + newLocalMax) * 0.5f;
        localBoundSize = new Vector2(newLocalMax.x - newLocalMin.x, newLocalMax.y - newLocalMin.y);
    }

    [ContextMenu("Fit To Sprite Bounds")]
    public void FitToSpriteBounds()
    {
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        if (spriteRenderers.Length > 0)
        {
            Vector2 currentLocalMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 currentLocalMax = new Vector2(float.MinValue, float.MinValue);
            foreach (SpriteRenderer spriteRenderer in spriteRenderers)
            {
                Vector2 worldBoundsMin = spriteRenderer.bounds.min;
                Vector2 worldBoundsMax = spriteRenderer.bounds.max;
                Vector2 localBoundsMin = transform.InverseTransformPoint(worldBoundsMin);
                Vector2 localBoundsMax = transform.InverseTransformPoint(worldBoundsMax);
                currentLocalMin.x = Mathf.Min(localBoundsMin.x, currentLocalMin.x);
                currentLocalMin.y = Mathf.Min(localBoundsMin.y, currentLocalMin.y);
                currentLocalMax.x = Mathf.Max(localBoundsMax.x, currentLocalMax.x);
                currentLocalMax.y = Mathf.Max(localBoundsMax.y, currentLocalMax.y);
            }
            localBoundPosition = (currentLocalMin + currentLocalMax) * 0.5f;
            localBoundSize = new Vector2(currentLocalMax.x - currentLocalMin.x, currentLocalMax.y - currentLocalMin.y);
        }
    }

    public virtual void OnChangedRepeatIndex() { }

    protected virtual void Reset()
    {
        FitToSpriteBounds();
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.matrix = transform.localToWorldMatrix;

        Vector2 p0 = localLeftDown;
        Vector2 p1 = localLeftUp;
        Vector2 p2 = localRightUp;
        Vector2 p3 = localRightDown;

        Gizmos.DrawLine(p0, p1);
        Gizmos.DrawLine(p1, p2);
        Gizmos.DrawLine(p2, p3);
        Gizmos.DrawLine(p3, p0);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteBounds))]
[CanEditMultipleObjects]
public class SpriteBoundsEditor : Editor
{
    SpriteBounds targetSpriteBounds = null; 
    bool buttonPressing = false;

    protected virtual void OnEnable()
    {
        targetSpriteBounds = (SpriteBounds)target;
        buttonPressing = false;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Fit To Sprite Bounds"))
        {
            Undo.RegisterCompleteObjectUndo(targetSpriteBounds, "Edit bounds");
            Undo.FlushUndoRecordObjects();

            targetSpriteBounds.FitToSpriteBounds();
        }

        GUILayout.Space(10);

        if (GUILayout.Button("Copy on up"))
        {
            SpriteBounds spriteBounds = targetSpriteBounds.CopyOnUp();
            Selection.activeGameObject = spriteBounds.gameObject;
            Undo.RegisterCreatedObjectUndo(spriteBounds.gameObject, "Copy SpriteBounds");
        }

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Copy on left"))
        {
            SpriteBounds spriteBounds = targetSpriteBounds.CopyOnLeft();
            Selection.activeGameObject = spriteBounds.gameObject;
            Undo.RegisterCreatedObjectUndo(spriteBounds.gameObject, "Copy SpriteBounds");
        }
        if (GUILayout.Button("Copy on right"))
        {
            SpriteBounds spriteBounds = targetSpriteBounds.CopyOnRight();
            Selection.activeGameObject = spriteBounds.gameObject;
            Undo.RegisterCreatedObjectUndo(spriteBounds.gameObject, "Copy SpriteBounds");
        }
        GUILayout.EndHorizontal();

        if (GUILayout.Button("Copy on down"))
        {
            SpriteBounds spriteBounds = targetSpriteBounds.CopyOnDown();
            Selection.activeGameObject = spriteBounds.gameObject;
            Undo.RegisterCreatedObjectUndo(spriteBounds.gameObject, "Copy SpriteBounds");
        }
    }

    public virtual void OnSceneGUI()
    {
        const float buttonSize = 0.25f;
        Vector2 buttonExtents = Vector2.one * buttonSize;
        Handles.color = Color.green;

        Vector2 localLeft = (targetSpriteBounds.localLeftUp + targetSpriteBounds.localLeftDown) * 0.5f;
        Vector2 worldLeft = targetSpriteBounds.transform.TransformPoint(localLeft);
        Rect worldLeftRect = new Rect(worldLeft - buttonExtents * 0.5f, buttonExtents);

        Vector2 localRight = (targetSpriteBounds.localRightUp + targetSpriteBounds.localRightDown) * 0.5f;
        Vector2 worldRight = targetSpriteBounds.transform.TransformPoint(localRight);
        Rect worldRightRect = new Rect(worldRight - buttonExtents * 0.5f, buttonExtents);

        Vector2 localDown = (targetSpriteBounds.localLeftDown + targetSpriteBounds.localRightDown) * 0.5f;
        Vector2 worldDown = targetSpriteBounds.transform.TransformPoint(localDown);
        Rect worldDownRect = new Rect(worldDown - buttonExtents * 0.5f, buttonExtents);

        Vector2 localUp = (targetSpriteBounds.localLeftUp + targetSpriteBounds.localRightUp) * 0.5f;
        Vector2 worldUp = targetSpriteBounds.transform.TransformPoint(localUp);
        Rect worldUpRect = new Rect(worldUp - buttonExtents * 0.5f, buttonExtents);

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
            {
                Vector2 guiMouse = e.mousePosition;
                Vector2 worldMouse = HandleUtility.GUIPointToWorldRay(guiMouse).origin;
                if (worldLeftRect.Contains(worldMouse) ||
                    worldUpRect.Contains(worldMouse) ||
                    worldRightRect.Contains(worldMouse) ||
                    worldDownRect.Contains(worldMouse))
                {
                    buttonPressing = true;
                    Undo.RegisterCompleteObjectUndo(targetSpriteBounds, "Edit bounds");
                    Undo.FlushUndoRecordObjects();
                }
            }
            break;

            case EventType.MouseUp:
            {
                buttonPressing = false;
            }
            break;
        }

        Vector2 newWorldLeft = Handles.FreeMoveHandle(worldLeft, Quaternion.identity, buttonSize, Vector3.one, Handles.RectangleHandleCap);
        Vector2 newLocalLeft = targetSpriteBounds.transform.InverseTransformPoint(newWorldLeft);
        if (buttonPressing)
        {
            targetSpriteBounds.SetLocalBound(newLocalLeft.x, targetSpriteBounds.localMaxX, targetSpriteBounds.localMinY, targetSpriteBounds.localMaxY);
        }

        Vector2 newWorldRight = Handles.FreeMoveHandle(worldRight, Quaternion.identity, buttonSize, Vector3.one, Handles.RectangleHandleCap);
        Vector2 newLocalRight = targetSpriteBounds.transform.InverseTransformPoint(newWorldRight);
        if (buttonPressing)
        {
            targetSpriteBounds.SetLocalBound(targetSpriteBounds.localMinX, newLocalRight.x, targetSpriteBounds.localMinY, targetSpriteBounds.localMaxY);
        }

        Vector2 newWorldDown = Handles.FreeMoveHandle(worldDown, Quaternion.identity, buttonSize, Vector3.one, Handles.RectangleHandleCap);
        Vector2 newLocalDown = targetSpriteBounds.transform.InverseTransformPoint(newWorldDown);
        if (buttonPressing)
        {
            targetSpriteBounds.SetLocalBound(targetSpriteBounds.localMinX, targetSpriteBounds.localMaxX, newLocalDown.y, targetSpriteBounds.localMaxY);
        }

        Vector2 newWorldUp = Handles.FreeMoveHandle(worldUp, Quaternion.identity, buttonSize, Vector3.one, Handles.RectangleHandleCap);
        Vector2 newLocalUp = targetSpriteBounds.transform.InverseTransformPoint(newWorldUp);
        if (buttonPressing)
        {
            targetSpriteBounds.SetLocalBound(targetSpriteBounds.localMinX, targetSpriteBounds.localMaxX, targetSpriteBounds.localMinY, newLocalUp.y);
        }
    }
}
#endif
