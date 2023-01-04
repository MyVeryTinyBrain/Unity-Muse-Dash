using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;
using System;
using System.Linq;

/// <summary>
/// 노트를 선택하기 위한 콜라이더 컴포넌트입니다.<para>
/// 기재된 정적 함수를 사용하여 노트를 쿼리할 수 있습니다.</para><para>
/// 생성된 콜라이더는 쿼리 이외의 상황에서는 비활성화 상태로 존재합니다.</para>
/// </summary>
[DisallowMultipleComponent]
public class NoteCollider : NoteComponent
{
    static List<NoteCollider> noteColliders = new List<NoteCollider>();

    public static void EnableAndUpdateNoteColliders()
    {
        noteColliders.ForEach((x) =>
        {
            x.EnableNoteCollider();
            x.UpdateNoteCollider();
        });
    }

    public static void UpdateNoteColliders()
    {
        noteColliders.ForEach((x) => x.UpdateNoteCollider());
    }

    public static void DisableNoteColliders()
    {
        noteColliders.ForEach((x) =>
        {
            x.DisableNoteCollider();
        });
    }

    public static int GetNoteColliderLayer()
    {
        return LayerMask.NameToLayer(Layers.NoteCollider);
    }

    public static Note OverlapPoint(Vector2 worldPosition)
    {
        List<Note> notes = OverlapPointAll(worldPosition, true);
        return notes.Count == 0 ? null : notes[0];
    }

    public static Note OverlapPoint(Vector2 worldPosition, out NoteElementHandle elementHandle)
    {
        List<Note> notes = OverlapPointAll(worldPosition, out elementHandle, true);
        return notes.Count == 0 ? null : notes[0];
    }

    public static List<Note> OverlapPointAll(Vector2 worldPosition, bool sortByX = true)
    {
        NoteElementHandle elementHandle;
        return OverlapPointAll(worldPosition, out elementHandle, sortByX);
    }

    public static List<Note> OverlapPointAll(Vector2 worldPosition, out NoteElementHandle elementHandle, bool sortByX = true)
    {
        EnableAndUpdateNoteColliders();

        int layerMask = 1 << GetNoteColliderLayer();
        Ray ray = new Ray(new Vector3(worldPosition.x, worldPosition.y, -100), Vector3.forward);
        RaycastHit[] hits = Physics.RaycastAll(ray, float.MaxValue, layerMask);
        Collider2D[] hits2D = Physics2D.OverlapPointAll(worldPosition, layerMask);

        List<Component> colliders = new List<Component>();
        Array.ForEach(hits, (x) => colliders.Add(x.collider));
        Array.ForEach(hits2D, (x) => colliders.Add(x));

        DisableNoteColliders();

        // 오른쪽에 있는 순서대로 내림차순 정렬합니다.
        if (sortByX)
        {
            colliders.Sort((lhs, rhs) => rhs.transform.position.x.CompareTo(lhs.transform.position.x));
        }

        // 처음 오버랩된 콜라이더에 노트 요소 핸들이 부착되어 있으면 해당 컴포넌트를 반환합니다.
        elementHandle = null;
        if (colliders.Count > 0)
        {
            elementHandle = colliders[0].GetComponent<NoteElementHandle>();
        }

        List<Note> notes = new List<Note>();
        foreach (Component collider in colliders)
        {
            NoteCollider noteCollider = collider.gameObject.GetComponentInParent<NoteCollider>(true);
            if (noteCollider && noteCollider.note)
            {
                notes.Add(noteCollider.note);
            }
        }

        notes = notes.Distinct().ToList();
        return notes;
    }

    public static List<Note> OverlapRect(Vector2 pointA, Vector2 pointB)
    {
        EnableAndUpdateNoteColliders();

        int layerMask = 1 << GetNoteColliderLayer();
        Vector3 min = new Vector3(Mathf.Min(pointA.x, pointB.x), Mathf.Min(pointA.y, pointB.y), -100);
        Vector3 max = new Vector3(Mathf.Max(pointA.x, pointB.x), Mathf.Max(pointA.y, pointB.y), +100);
        Vector3 center = (min + max) * 0.5f;
        Vector3 extents = max - min;
        Vector3 halfExtents = extents * 0.5f;
        Collider[] overlapResult = Physics.OverlapBox(center, halfExtents, Quaternion.identity, layerMask);
        Collider2D[] overlapResult2D = Physics2D.OverlapAreaAll(min, max, layerMask);

        List<Component> colliders = new List<Component>();
        Array.ForEach(overlapResult, (x) => colliders.Add(x));
        Array.ForEach(overlapResult2D, (x) => colliders.Add(x));

        DisableNoteColliders();

        List<Note> notes = new List<Note>();
        foreach (Component collider in colliders)
        {
            NoteCollider noteCollider = collider.gameObject.GetComponentInParent<NoteCollider>(true);
            if (noteCollider && noteCollider.note)
            {
                notes.Add(noteCollider.note);
            }
        }
        return notes.Distinct().ToList();
    }

    abstract class ColliderSet
    {
        public abstract void Close();
        public abstract void Update();
        public abstract void ManualUpdate();
        public abstract void EnableCollider();
        public abstract void DisableCollider();
    }

    class SpineColliderSet : ColliderSet
    {
        SkeletonAnimation    spine = null;
        MeshFilter           spineMeshFilter = null;
        MeshCollider         spineCollider = null;

        public SpineColliderSet(SkeletonAnimation skeletonAnimation)
        {
            spine = skeletonAnimation;
            spineCollider = spine.gameObject.AddComponent<MeshCollider>();
            spineCollider.cookingOptions =
                MeshColliderCookingOptions.CookForFasterSimulation |
                MeshColliderCookingOptions.WeldColocatedVertices |
                MeshColliderCookingOptions.UseFastMidphase;
            // 메쉬 클리닝을 사용하지 않습니다.
            // 특정 스파인 애니메이션에서 오류 발생.
            spineMeshFilter = spine.gameObject.GetComponent<MeshFilter>();
            ManualUpdate();
            DisableCollider();
        }

        public override void Close()
        {
            if (spineCollider)
            {
                Destroy(spineCollider);
            }

            spine = null;
            spineMeshFilter = null;
            spineCollider = null;
        }

        public override void Update()
        {
            ManualUpdate();
        }

        public bool IsUpdatable()
        {
            // 특정 스파인 애니메이션은 아래 조건을 만족하지 못하는 경우에
            // spineCollider.enabled 또는 spineCollider.sharedMesh = spineMeshFilter.sharedMesh 를 사용하여
            // 콜라이더를 업데이트 할 때 문제가 발생합니다.
            // 따라서 이 함수로 검사 후 콜라이더 활성화 또는 업데이트를 진행합니다.

            if (!spineMeshFilter || !spineMeshFilter.sharedMesh)
            {
                return false;
            }

            Vector2 localBoundSize = spineMeshFilter.sharedMesh.bounds.extents;
            Vector2 worldBoundSize = spine.transform.TransformVector(localBoundSize);

            if (worldBoundSize.x > 0.1f && worldBoundSize.y > 0.1f)
            {
                return true;
            }
            return false;
        }

        public override void ManualUpdate()
        {
            if (spineMeshFilter.sharedMesh == null)
            {
                spineCollider.sharedMesh = null;
                return;
            }

            //if (spine.skeleton.GetColor().a == 0.0f)
            //{
            //    spineCollider.sharedMesh = null;
            //    return;
            //}

            Vector2 localBoundSize = spineMeshFilter.sharedMesh.bounds.extents;
            Vector2 worldBoundSize = spine.transform.TransformVector(localBoundSize);

            if (IsUpdatable())
            {
                spineCollider.sharedMesh = spineMeshFilter.sharedMesh;
            }
            else
            {
                spineCollider.sharedMesh = null;
            }
        }

        public override void EnableCollider()
        {
            if (IsUpdatable())
            {
                spineCollider.enabled = true;
            }
        }

        public override void DisableCollider()
        {
            spineCollider.enabled = false;
        }
    }

    class SpriteColliderSet : ColliderSet
    {
        SpriteRenderer       sprite = null;
        PolygonCollider2D    spriteCollider = null;

        public SpriteColliderSet(SpriteRenderer spriteRenderer)
        {
            sprite = spriteRenderer;
            spriteCollider = sprite.gameObject.AddComponent<PolygonCollider2D>();
            DisableCollider();
        }

        public override void Close()
        {
            if (spriteCollider)
            {
                Destroy(spriteCollider);
            }

            sprite = null;
            spriteCollider = null;
        }

        public override void Update() { }
        public override void ManualUpdate() { }

        public override void EnableCollider()
        {
            spriteCollider.enabled = true;
        }

        public override void DisableCollider()
        {
            spriteCollider.enabled = false;
        }
    }

    class LineColliderSet : ColliderSet
    {
        LineRenderer        line = null;
        MeshCollider        lineCollider = null;
        Mesh                lineMesh = null;

        public LineColliderSet(LineRenderer lineRenderer)
        {
            line = lineRenderer;
            lineCollider = line.gameObject.AddComponent<MeshCollider>();
            lineMesh = new Mesh();
            ManualUpdate();
            DisableCollider();
        }

        public override void Close()
        {
            if (lineCollider)
            {
                Destroy(lineCollider);
            }

            if (lineMesh)
            {
                Destroy(lineMesh);
            }

            line = null;
            lineCollider = null;
            lineMesh = null;
        }

        public override void Update()
        {
            ManualUpdate();
        }

        public override void ManualUpdate()
        {
            line.BakeMesh(lineMesh);

            Vector2 boundExtents = lineMesh.bounds.extents;
            if (boundExtents.x > 0.0f && boundExtents.y > 0.0f)
            {
                lineCollider.sharedMesh = lineMesh;
            }
            else
            {
                lineCollider.sharedMesh = null;
            }
        }

        public override void EnableCollider()
        {
            lineCollider.enabled = true;
        }

        public override void DisableCollider()
        {
            lineCollider.enabled = false;
        }
    }

    class SubHandleColliderSet : ColliderSet
    {
        NoteSubHandle handle = null;
        CircleCollider2D handleCollider = null;

        public SubHandleColliderSet(NoteSubHandle subHandle)
        {
            handle = subHandle;
            handleCollider = handle.subHandleObject.AddComponent<CircleCollider2D>();
            ManualUpdate();
            DisableCollider();
        }

        public override void Close()
        {
            if (handleCollider)
            {
                Destroy(handleCollider);
            }

            handle = null;
            handleCollider = null;
        }

        public override void Update() 
        {
            ManualUpdate();
        }

        public override void ManualUpdate()
        {
            if(Mathf.Abs(handle.width * 0.5f - handleCollider.radius) > 0.01f)
            {
                handleCollider.radius = handle.width * 0.5f;
            }
        }

        public override void EnableCollider()
        {
            handleCollider.enabled = true;
        }

        public override void DisableCollider()
        {
            handleCollider.enabled = false;
        }
    }

    List<SpineColliderSet> spineColliderSets = null;
    List<SpriteColliderSet> spriteColliderSets = null;
    List<LineColliderSet> lineColliderSets = null;
    List<SubHandleColliderSet> subHandleColliderSets = null;

    protected override void OnInit(Note note)
    {

    }

    protected override void OnPostInit()
    {
        SkeletonAnimation[] skeletonAnimations = GetComponentsInChildren<SkeletonAnimation>(true);
        spineColliderSets = skeletonAnimations.Length > 0 ? new List<SpineColliderSet>() : null;
        Array.ForEach(skeletonAnimations, delegate (SkeletonAnimation skeletonAnimation)
        {
            skeletonAnimation.gameObject.layer = GetNoteColliderLayer();
            skeletonAnimation.gameObject.tag = Tags.NoteCollider;
            spineColliderSets.Add(new SpineColliderSet(skeletonAnimation));
        });

        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        spriteColliderSets = spriteRenderers.Length > 0 ? new List<SpriteColliderSet>() : null;
        Array.ForEach(spriteRenderers, delegate (SpriteRenderer spriteRenderer)
        {
            spriteRenderer.gameObject.layer = GetNoteColliderLayer();
            spriteRenderer.gameObject.tag = Tags.NoteCollider;
            spriteColliderSets.Add(new SpriteColliderSet(spriteRenderer));
        });

        LineRenderer[] lineRenderers = GetComponentsInChildren<LineRenderer>(true);
        lineColliderSets = lineRenderers.Length > 0 ? new List<LineColliderSet>() : null;
        Array.ForEach(lineRenderers, delegate (LineRenderer lineRenderer)
        {
            lineRenderer.gameObject.layer = GetNoteColliderLayer();
            lineRenderer.gameObject.tag = Tags.NoteCollider;
            lineColliderSets.Add(new LineColliderSet(lineRenderer));
        });

        NoteSubHandle[] subHandles = GetComponentsInChildren<NoteSubHandle>(true);
        subHandleColliderSets = subHandles.Length > 0 ? new List<SubHandleColliderSet>() : null;
        Array.ForEach(subHandles, delegate (NoteSubHandle subHandle)
        {
            subHandle.subHandleObject.layer = GetNoteColliderLayer();
            subHandle.gameObject.tag = Tags.NoteCollider;
            subHandleColliderSets.Add(new SubHandleColliderSet(subHandle));
        });

        noteColliders.Add(this);
    }

    private void OnDestroy()
    {
        noteColliders.Remove(this);

        spineColliderSets?.ForEach((x) => x.Close());
        spineColliderSets?.Clear();

        spriteColliderSets?.ForEach((x) => x.Close());
        spriteColliderSets?.Clear();

        lineColliderSets?.ForEach((x) => x.Close());
        lineColliderSets?.Clear();

        subHandleColliderSets?.ForEach((x) => x.Close());
        subHandleColliderSets?.Clear();
    }

    public void EnableNoteCollider()
    {
        spineColliderSets?.ForEach((x) => x.EnableCollider());
        spriteColliderSets?.ForEach((x) => x.EnableCollider());
        lineColliderSets?.ForEach((x) => x.EnableCollider());
        subHandleColliderSets?.ForEach((x) => x.EnableCollider());
    }

    public void DisableNoteCollider()
    {
        spineColliderSets?.ForEach((x) => x.DisableCollider());
        spriteColliderSets?.ForEach((x) => x.DisableCollider());
        lineColliderSets?.ForEach((x) => x.DisableCollider());
        subHandleColliderSets?.ForEach((x) => x.DisableCollider());
    }

    public void UpdateNoteCollider()
    {
        spineColliderSets?.ForEach((x) => x.Update());
        spriteColliderSets?.ForEach((x) => x.Update());
        lineColliderSets?.ForEach((x) => x.Update());
        subHandleColliderSets?.ForEach((x) => x.Update());
    }
}
