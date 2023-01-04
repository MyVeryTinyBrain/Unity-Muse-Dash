using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BackgroundPiston : MonoBehaviour, IBackgroundObject
{
    public List<GameObject> moveTargets;
    public Vector2 localTarget = Vector2.zero;
    public float loopDuration = 10.0f;

    public void SetTime(float time)
    {
        float sin = Mathf.Sin(time * Mathf.PI / loopDuration);
        float t = sin * sin;

        foreach(GameObject target in moveTargets)
        {
            target.transform.localPosition = Vector2.Lerp(Vector2.zero, localTarget, t);
        }
    }

    private void OnEnable()
    {
        Background background = null;
        if (background = GetComponentInParent<Background>())
        {
            background.RegistBackgroundObject(this);
        }
    }

    private void OnDisable()
    {
        Background background = null;
        if (background = GetComponentInParent<Background>())
        {
            background.UnregistBackgroundObject(this);
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BackgroundPiston))]
[CanEditMultipleObjects]
public class BackgroundPistonEditor : Editor
{
    BackgroundPiston targetBackgroundPistion = null;
    bool buttonPressing = false;

    private void OnEnable()
    {
        targetBackgroundPistion = (BackgroundPiston)target;
    }

    public virtual void OnSceneGUI()
    {
        const float buttonSize = 0.25f;
        Vector2 buttonExtents = Vector2.one * buttonSize;
        Handles.color = Color.green;

        Vector2 worldTarget = targetBackgroundPistion.transform.TransformPoint(targetBackgroundPistion.localTarget);
        Rect worldRect = new Rect(worldTarget - buttonExtents * 0.5f, buttonExtents);

        Event e = Event.current;
        switch (e.type)
        {
            case EventType.MouseDown:
            {
                Vector2 guiMouse = e.mousePosition;
                Vector2 worldMouse = HandleUtility.GUIPointToWorldRay(guiMouse).origin;
                if (worldRect.Contains(worldMouse))
                {
                    buttonPressing = true;
                    Undo.RegisterCompleteObjectUndo(targetBackgroundPistion, "Edit Local Target");
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

        Vector2 newWorldTarget = Handles.FreeMoveHandle(worldTarget, Quaternion.identity, buttonSize, Vector3.one, Handles.RectangleHandleCap);
        Vector2 newLocalTarget = targetBackgroundPistion.transform.InverseTransformPoint(newWorldTarget);
        if (buttonPressing)
        {
            targetBackgroundPistion.localTarget = newLocalTarget;
        }

        Handles.DrawLine(targetBackgroundPistion.transform.position, newWorldTarget);
    }
}
#endif