using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class BackgroundElement : SpriteBounds
{
    public enum SortingMethod { None, RightToLeft, LeftToRight, UpToDown, DownToUp }

    [SerializeField]
    SortingGroup sortingGroup = null;

    [SerializeField]
    SortingMethod sortingMethod = SortingMethod.None;

    [SerializeField]
    List<SpriteRenderer> spriteRenderers = new List<SpriteRenderer>();

    [SerializeField]
    List<Sprite> skins;

    public bool setRandomSkinOnAwake = false;

    [ContextMenu("Find Sorting Group")]
    public void FindSortingGroup()
    {
        sortingGroup = GetComponent<SortingGroup>();
    }

    [ContextMenu("Find Sprite Renderers")]
    public void FindSpriteRenderers()
    {
        SpriteRenderer[] finded = GetComponentsInChildren<SpriteRenderer>(true);
        spriteRenderers.Clear();
        spriteRenderers.AddRange(finded);
    }

    public void ApplyRandomSkins()
    {
        if(skins.Count == 0)
        {
            return;
        }

        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            // min: 포함
            // max: 제외
            int skinIndex = UnityEngine.Random.Range(0, skins.Count);
            spriteRenderer.sprite = skins[skinIndex];
        }
    }

    void Sort()
    {
        if(sortingMethod == SortingMethod.None)
        {
            return;
        }

        if (!sortingGroup)
        {
            return;
        }

        Camera cam = Camera.main;
        Vector2 camPos = cam.transform.position;
        Vector2 camSize = cam.OrthographicExtents();
        float camLeft = camPos.x - camSize.x * 0.5f;
        float camRight = camPos.x + camSize.x * 0.5f;
        float camDown = camPos.y - camSize.y * 0.5f;
        float camUp = camPos.y + camSize.y * 0.5f;

        float sortingOrder = 0;
        switch (sortingMethod)
        {
            case SortingMethod.RightToLeft:
            {
                float deltaX = camRight - transform.position.x;
                sortingOrder = deltaX * 100.0f;
            }
            break;

            case SortingMethod.LeftToRight:
            {
                float deltaX = transform.position.x - camLeft;
                sortingOrder = deltaX * 100.0f;
            }
            break;

            case SortingMethod.UpToDown:
            {
                float deltaY = camUp - transform.position.y;
                sortingOrder = deltaY * 100.0f;
            }
            break;

            case SortingMethod.DownToUp:
            {
                float deltaY = transform.position.y - camDown;
                sortingOrder = deltaY * 100.0f;
            }
            break;
        }
        sortingGroup.sortingOrder = (int)sortingOrder;
    }

    public override void OnChangedRepeatIndex()
    {
        base.OnChangedRepeatIndex();

        ApplyRandomSkins();
    }

    protected override void Reset()
    {
        base.Reset();

        FindSortingGroup();
        FindSpriteRenderers();
    }

    protected virtual void Awake()
    {
        if (setRandomSkinOnAwake)
        {
            ApplyRandomSkins();
        }
    }

    private void Update()
    {
        Sort();
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(BackgroundElement))]
[CanEditMultipleObjects]
public class BackgroundElementEditor : SpriteBoundsEditor 
{
    BackgroundElement targetBackgroundElement = null;

    protected override void OnEnable()
    {
        base.OnEnable();

        targetBackgroundElement = (BackgroundElement)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUILayout.Space(10);

        if (GUILayout.Button("Find Sorting Group"))
        {
            Undo.RegisterCompleteObjectUndo(targetBackgroundElement, "Find Sorting Group");
            Undo.FlushUndoRecordObjects();

            targetBackgroundElement.FindSortingGroup();
        }

        if (GUILayout.Button("Find Sprite Renderers"))
        {
            Undo.RegisterCompleteObjectUndo(targetBackgroundElement, "Find Sprite Renderers");
            Undo.FlushUndoRecordObjects();

            targetBackgroundElement.FindSpriteRenderers();
        }
    }
}
#endif
