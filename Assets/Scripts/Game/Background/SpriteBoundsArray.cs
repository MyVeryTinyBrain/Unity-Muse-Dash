using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpriteBoundsArray : SpriteBounds, IBackgroundObject
{
    [System.Serializable]
    public class ChildSpriteBounds
    {
        [ReadOnly]
        public SpriteBounds spriteBounds;

        // 여기서의 로컬은 이 컴포넌트에 대한 로컬 위치입니다.
        // spriteBounds world position -> sprite bounds array's local position 

        [ReadOnly]
        public Vector2 originLocal;

        [ReadOnly]
        public Vector2 originLocalMin;

        [ReadOnly]
        public Vector2 originLocalMax;

        [ReadOnly]
        public Vector2 prevUnRepeatedLocal;
    }

    public enum Direction { ToLeft, ToRight, ToUp, ToDown };

    [SerializeField]
    Direction direction = Direction.ToLeft;

    [System.NonSerialized]
    public float time = 0;

    [SerializeField]
    public float speed = 1;

    [SerializeField]
    List<ChildSpriteBounds> childSpriteBounds = new List<ChildSpriteBounds>();

    public void SetTime(float time)
    {
        this.time = time;
    }

    [ContextMenu("Fit To Children Sprite Bounds")]
    public void FitToChildrenSpriteBounds()
    {
        childSpriteBounds.Clear();

        SpriteBounds[] spriteBoundsArray = GetComponentsInChildren<SpriteBounds>(true);
        List<SpriteBoundsArray> spriteBoundsArrayList = new List<SpriteBoundsArray>();
        foreach(SpriteBounds x in spriteBoundsArray)
        {
            if (x != this)
            {
                if(x is SpriteBoundsArray)
                {
                    spriteBoundsArrayList.Add((SpriteBoundsArray)x);
                    continue;
                }

                childSpriteBounds.Add(new ChildSpriteBounds()
                {
                    spriteBounds = x,
                    originLocal = transform.InverseTransformPoint(x.transform.position),
                    originLocalMin = transform.InverseTransformPoint(x.worldLeftDown),
                    originLocalMax = transform.InverseTransformPoint(x.worldRightUp),
                    prevUnRepeatedLocal = transform.InverseTransformPoint(x.transform.position),
                });
            }
        }

        // 이미 다른 배열에 속한 경계 컴포넌트를 제외합니다.
        foreach(SpriteBoundsArray boundsArray in spriteBoundsArrayList)
        {
            foreach(ChildSpriteBounds childBounds in boundsArray.childSpriteBounds)
            {
                int index = childSpriteBounds.FindIndex((x) => childBounds.spriteBounds == x.spriteBounds);
                if(index >= 0)
                {
                    childSpriteBounds.RemoveAt(index);
                }
            }
        }

        if (childSpriteBounds.Count > 0)
        {
            Vector2 currentLocalMin = new Vector2(float.MaxValue, float.MaxValue);
            Vector2 currentLocalMax = new Vector2(float.MinValue, float.MinValue);
            foreach (ChildSpriteBounds bounds in childSpriteBounds)
            {
                Vector2 worldBoundsMin = bounds.spriteBounds.worldLeftDown;
                Vector2 worldBoundsMax = bounds.spriteBounds.worldRightUp;
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

    protected override void Reset()
    {
        base.Reset();

        FitToChildrenSpriteBounds();
    }

    public void UpdateElementTransforms(Direction direction, float time, float speed)
    {
        float additionalDistance = time * speed;
        Vector2 arrayLocalMin = localLeftDown;
        Vector2 arrayLocalMax = localRightUp;

        foreach (ChildSpriteBounds spriteBounds in childSpriteBounds)
        {
            switch (direction)
            {
                case Direction.ToLeft:
                {
                    float prevUnRepeatedX = spriteBounds.prevUnRepeatedLocal.x;
                    float unRepeatedX = spriteBounds.originLocalMin.x - additionalDistance;

                    float x = MathUtility.RepeatInMinMax(arrayLocalMin.x, arrayLocalMax.x, unRepeatedX);
                    float y = spriteBounds.originLocal.y;
                    Vector2 world = transform.TransformPoint(x, y, 0);
                    spriteBounds.spriteBounds.SnapLeftEdge(world);

                    int prevRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.x, arrayLocalMax.x, prevUnRepeatedX);
                    int changedRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.x, arrayLocalMax.x, unRepeatedX);
                    if (prevRepeatIndex != changedRepeatIndex)
                    {
                        spriteBounds.prevUnRepeatedLocal.x = unRepeatedX;
                        spriteBounds.spriteBounds.OnChangedRepeatIndex();
                    }
                }
                break;

                case Direction.ToRight:
                {
                    float prevUnRepeatedX = spriteBounds.prevUnRepeatedLocal.x;
                    float unRepeatedX = spriteBounds.originLocalMax.x - additionalDistance;

                    float x = MathUtility.RepeatInMinMax(arrayLocalMin.x, arrayLocalMax.x, spriteBounds.originLocalMax.x + additionalDistance);
                    float y = spriteBounds.originLocal.y;
                    Vector2 world = transform.TransformPoint(x, y, 0);
                    spriteBounds.spriteBounds.SnapRightEdge(world);

                    int prevRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.x, arrayLocalMax.x, prevUnRepeatedX);
                    int changedRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.x, arrayLocalMax.x, unRepeatedX);
                    if (prevRepeatIndex != changedRepeatIndex)
                    {
                        spriteBounds.prevUnRepeatedLocal.x = unRepeatedX;
                        spriteBounds.spriteBounds.OnChangedRepeatIndex();
                    }
                }
                break;

                case Direction.ToDown:
                {
                    float prevUnRepeatedY = spriteBounds.prevUnRepeatedLocal.y;
                    float unRepeatedY = spriteBounds.originLocalMin.y - additionalDistance;

                    float x = spriteBounds.originLocal.x;
                    float y = MathUtility.RepeatInMinMax(arrayLocalMin.y, arrayLocalMax.y, spriteBounds.originLocalMin.y - additionalDistance);
                    Vector2 world = transform.TransformPoint(x, y, 0);
                    spriteBounds.spriteBounds.SnapDownEdge(world);

                    int prevRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.y, arrayLocalMax.y, prevUnRepeatedY);
                    int changedRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.y, arrayLocalMax.y, unRepeatedY);
                    if (prevRepeatIndex != changedRepeatIndex)
                    {
                        spriteBounds.prevUnRepeatedLocal.y = unRepeatedY; 
                        spriteBounds.spriteBounds.OnChangedRepeatIndex();
                    }
                }
                break;

                case Direction.ToUp:
                {
                    float prevUnRepeatedY = spriteBounds.prevUnRepeatedLocal.y;
                    float unRepeatedY = spriteBounds.originLocalMax.y - additionalDistance;

                    float x = spriteBounds.originLocal.x;
                    float y = MathUtility.RepeatInMinMax(arrayLocalMin.y, arrayLocalMax.y, spriteBounds.originLocalMax.y + additionalDistance);
                    Vector2 world = transform.TransformPoint(x, y, 0);
                    spriteBounds.spriteBounds.SnapUpEdge(world);

                    int prevRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.y, arrayLocalMax.y, prevUnRepeatedY);
                    int changedRepeatIndex = MathUtility.GetRepeatIndex(arrayLocalMin.y, arrayLocalMax.y, unRepeatedY);
                    if (prevRepeatIndex != changedRepeatIndex)
                    {
                        spriteBounds.prevUnRepeatedLocal.y = unRepeatedY;
                        spriteBounds.spriteBounds.OnChangedRepeatIndex();
                    }
                }
                break;
            }
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

    private void Update()
    {
        UpdateElementTransforms(direction, time, speed);
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(SpriteBoundsArray))]
[CanEditMultipleObjects]
public class SpriteBoundsArrayEditor : SpriteBoundsEditor
{
    SpriteBoundsArray targetSpriteBoundsArray = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        targetSpriteBoundsArray = (SpriteBoundsArray)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Fit To Children Sprite Bounds"))
        {
            Undo.RegisterCompleteObjectUndo(targetSpriteBoundsArray, "Edit bounds");
            Undo.FlushUndoRecordObjects();

            targetSpriteBoundsArray.FitToChildrenSpriteBounds();
        }
    }
}
#endif
