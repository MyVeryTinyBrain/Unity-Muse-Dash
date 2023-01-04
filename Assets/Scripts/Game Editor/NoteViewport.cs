using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public enum SelectMouseType
{
    MouseDown,
    MouseUp,
}

public class NoteViewport : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler, IPointerDownHandler, IPointerUpHandler
{
    protected Mediator mediator => Mediator.i;

    struct VisibleBeat
    {
        public int index;
        public float localCenterX;
        public float localLeftX;
        public float localRightX;
        public float localUpYTop;
        public float localUpYBot;
        public float localDownYTop;
        public float localDownYBot;
    }

    // 선택된 비트의 시간과 상하 위치가 전달됩니다.
    public delegate void OnSelectBeatDelegate(float time, NotePosition position);

    // 선택된 노트들과 어떤 입력에 의해 선택되었는지가 전달됩니다.
    // 만약 첫 번째로 선택된 노트 콜라이더에 노트 요소 핸들이 부착되어 있다면 해당 컴포넌트 또한 전달됩니다.
    public delegate void OnSelectNotesDelegate(List<Note> notes, NoteElementHandle elementHandle, SelectMouseType eventType);

    // 아무 것도 선택되지 않은 경우에 호출됩니다.
    public delegate void OnSelectEmptyDelegate(SelectMouseType eventType);

    public event OnSelectBeatDelegate OnSelectBeat;
    public event OnSelectNotesDelegate OnSelectNotes;
    public event OnSelectEmptyDelegate OnSelectEmpty;

    public float localGridHeight = 1.0f;
    public float localGridWidth = 0.3f;
    public BeatType beatType = BeatType.B4;

    public bool enableDrag = false;

    RectTransform rectTransform = null;
    Material glMaterial = null;
    Material glMaterialNonMasking = null;
    List<VisibleBeat> visibleBeats;

    UnityEngine.UI.Image emptyImage = null;
    bool pointerHovering = false; // 마우스가 이 패널 위에 있습니다.
    bool mouseDrag = false; // 이 패널에서 마우스가 클릭되었고, 아직 마우스를 떼지 않았습니다.
    float mouseDownTime; // 마우스가 클릭 시작한 재생 시간입니다.
    float mouseDownWorldY; // 마우스가 클릭 시작한 로컬y 입니다.

    bool overlappedBeat = false; // 오버랩된 비트가 있습니다.
    int reactBeatIndex = -1; // 오버랩된 비트가 없으면 -1입니다.
    NotePosition reactBeatPosition = 0;

    public void IncreaseBeat()
    {
        beatType = Utility.LoopEnum(beatType);
    }

    public bool ClosetBeatOnScreen(Vector2 screenPosition, out int closetBeatIndex, out NotePosition closetBeatPosition)
    {
        Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return ClosetBeatOnWorld(worldMouse, out closetBeatIndex, out closetBeatPosition);
    }

    public bool ClosetBeatOnWorld(Vector2 worldPosition, out int closetBeatIndex, out NotePosition closetBeatPosition)
    {
        closetBeatIndex = -1;
        closetBeatPosition = 0;

        Vector2 localMouse = rectTransform.InverseTransformPoint(worldPosition);

        // 마우스의 x위치와 가까운 비트 인덱스입니다.
        int closetIndex = -1;
        float closetDistance = float.MaxValue;
        for (int i = 0; i < visibleBeats.Count; ++i)
        {
            float distance = Mathf.Abs(localMouse.x - visibleBeats[i].localCenterX);
            if (distance < closetDistance)
            {
                closetDistance = distance;
                closetIndex = i;
            }
        }

        if (closetIndex != -1)
        {
            VisibleBeat closetBeat = visibleBeats[closetIndex];

            float upY = (closetBeat.localUpYTop + closetBeat.localUpYBot) * 0.5f;
            float centerY = (closetBeat.localUpYBot + closetBeat.localDownYTop) * 0.5f;
            float downY = (closetBeat.localDownYTop + closetBeat.localDownYBot) * 0.5f;

            float[] yPos = { upY, centerY, downY };
            NotePosition[] notePos = { NotePosition.Air, NotePosition.Center, NotePosition.Road };
            float minYDistance = float.MaxValue;
            NotePosition minYDistancePos = NotePosition.Air;

            for(int i = 0; i < 3; ++i)
            {
                float yDistance = Mathf.Abs(localMouse.y - yPos[i]);
                if(yDistance < minYDistance)
                {
                    minYDistance = yDistance;
                    minYDistancePos = notePos[i];
                }
            }

            closetBeatIndex = visibleBeats[closetIndex].index;
            closetBeatPosition = minYDistancePos;
            return true;
        }

        return false;
    }

    public bool OverlapBeatOnScreen(Vector2 screenPosition, out int overlapedBeatIndex, out NotePosition overlapedPosition)
    {
        Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        return OverlapBeatOnWorld(worldMouse, out overlapedBeatIndex, out overlapedPosition);
    }

    public bool OverlapBeatOnWorld(Vector2 worldPosition, out int overlapedBeatIndex, out NotePosition overlapedPosition)
    {
        overlapedBeatIndex = -1;
        overlapedPosition = 0;

        float localUpHitPointY = rectTransform.InverseTransformPoint(0, mediator.hitPoint.airPos.y, 0).y;
        float localDownHitPointY = rectTransform.InverseTransformPoint(0, mediator.hitPoint.roadPos.y, 0).y;
        float localGridWidth = rectTransform.InverseTransformVector(this.localGridWidth, 0, 0).x;
        float localGridHeight = rectTransform.InverseTransformVector(0, this.localGridHeight, 0).y;

        Vector2 localMouse = rectTransform.InverseTransformPoint(worldPosition);

        // 마우스의 x위치와 가까운 비트 인덱스입니다.
        int closetIndex = -1;
        float closetDistance = float.MaxValue;
        for (int i = 0; i < visibleBeats.Count; ++i)
        {
            float distance = Mathf.Abs(localMouse.x - visibleBeats[i].localCenterX);
            if (distance < closetDistance)
            {
                closetDistance = distance;
                closetIndex = i;
            }
        }

        if (closetIndex != -1)
        {
            VisibleBeat closetBeat = visibleBeats[closetIndex];

            if (localMouse.x > closetBeat.localLeftX && localMouse.x < closetBeat.localRightX)
            {
                if (localMouse.y > closetBeat.localUpYBot && localMouse.y < closetBeat.localUpYTop)
                {
                    overlapedBeatIndex = visibleBeats[closetIndex].index;
                    overlapedPosition = NotePosition.Air;
                    return true;
                }
                else if (localMouse.y > closetBeat.localDownYTop && localMouse.y < closetBeat.localUpYBot)
                {
                    overlapedBeatIndex = visibleBeats[closetIndex].index;
                    overlapedPosition = NotePosition.Center;
                    return true;
                }
                else if (localMouse.y > closetBeat.localDownYBot && localMouse.y < closetBeat.localDownYTop)
                {
                    overlapedBeatIndex = visibleBeats[closetIndex].index;
                    overlapedPosition = NotePosition.Road;
                    return true;
                }
            }
        }

        return false;
    }

    void UpdateVisibleBeats()
    {
        float localUpHitPointTop = rectTransform.InverseTransformPoint(0, mediator.hitPoint.airPos.y + localGridHeight, 0).y;
        float localUpHitPointBot = rectTransform.InverseTransformPoint(0, mediator.hitPoint.airPos.y - localGridHeight, 0).y;
        float localDownHitPointTop = rectTransform.InverseTransformPoint(0, mediator.hitPoint.roadPos.y + localGridHeight, 0).y;
        float localDownHitPointBot = rectTransform.InverseTransformPoint(0, mediator.hitPoint.roadPos.y - localGridHeight, 0).y;
        float localGridWidth = rectTransform.InverseTransformVector(this.localGridWidth, 0, 0).x;

        float panelWidth = rectTransform.rect.width;
        float panelHalfWidth = panelWidth * 0.5f;

        List<float> beats = mediator.music.GetBeats(beatType);

        // 이진 탐색할 시간을 구합니다.
        // 패널의 맨 왼쪽 위치를 재생 시간으로 변환하여 해당 시간부터 비트를 그리기 위함입니다.
        // 월드 공간에서 패널의 왼쪽x와 타격점의 x 사이의 거리를 비율로 변환합니다.
        // 해당 비율을 시간으로 바꾸면 패널의 왼쪽x부터 타격점까지의 시간을 구할 수 있습니다.
        // 해당 시간을 현재 재생 시간에서 빼면(왼쪽으로 옮기기) 비트 그리기의 시작 시간을 구할 수 있습니다.
        float worldLeftX = rectTransform.TransformPoint(rectTransform.rect.xMin, 0, 0).x;
        float worldDistX = Mathf.Abs(worldLeftX - mediator.hitPoint.Pos.x);
        float deltaRatio = worldDistX / mediator.gameSettings.lengthPerSeconds;
        //float deltaTime = MusicUtility.RatioToTime(deltaRatio, 1, 0);
        float deltaTime = MusicUtility.RatioToTime(deltaRatio, mediator.music.currentGlobalSpeedScale, 0);
        float searchBeginTime = mediator.music.playingTime - deltaTime;  
        int closetIndex = mediator.music.GetClosetBeatIndexAtTime(searchBeginTime, beatType);

        visibleBeats.Clear();
        for (int i = closetIndex; i < beats.Count; ++i)
        {
            // 판정선에 일치하도록 비트 위치를 구합니다.
            // 현재 비트가 일치하는 경우에는 판정선에 정확히 위치해야 하므로 비트를 판정선의 x위치에 ratio가 0일때 위치하도록 합니다.
            float ratio = mediator.music.TimeToRatioAtCurrentTime(beats[i], 1.0f);
            float worldX = mediator.hitPoint.Pos.x + (float)ratio * mediator.gameSettings.lengthPerSeconds;
            float localX = rectTransform.InverseTransformPoint(worldX, 0, 0).x;

            VisibleBeat visibleBeat = new VisibleBeat();
            visibleBeat.index = i;
            visibleBeat.localCenterX = localX;
            visibleBeat.localLeftX = localX - localGridWidth;
            visibleBeat.localRightX = localX + localGridWidth;
            visibleBeat.localUpYTop = localUpHitPointTop;
            visibleBeat.localUpYBot = localUpHitPointBot;
            visibleBeat.localDownYTop = localDownHitPointTop;
            visibleBeat.localDownYBot = localDownHitPointBot;

            if (visibleBeat.localLeftX > rectTransform.rect.xMax && visibleBeat.localRightX > rectTransform.rect.xMax)
            {
                break;
            }

            visibleBeats.Add(visibleBeat);
        }
    }

    void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
    {
        pointerHovering = true;
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        pointerHovering = false;
        overlappedBeat = false;
    }

    void IPointerClickHandler.OnPointerClick(UnityEngine.EventSystems.PointerEventData eventData)
    {
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        mouseDrag = enableDrag;
        Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 hitPointSpaceLocalMouse = mediator.hitPoint.transform.InverseTransformPoint(worldMouse);
        mouseDownTime = mediator.music.LocalXToTime(hitPointSpaceLocalMouse.x, mediator.music.playingTime);
        mouseDownWorldY = worldMouse.y;

        if (overlappedBeat)
        {
            float time = mediator.music.GetBeats(beatType)[reactBeatIndex];
            NotePosition position = reactBeatPosition;
            OnSelectBeat?.Invoke(time, position);

            mouseDrag = false;
        }   

        {
            NoteElementHandle elementHandle;
            Note note = NoteCollider.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition), out elementHandle);
            if (note)
            {
                List<Note> notes = new List<Note>();
                notes.Add(note);
                OnSelectNotes?.Invoke(notes, elementHandle, SelectMouseType.MouseDown);

                mouseDrag = false;
            }
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        if (mouseDrag)
        {
            float hitPointLocalFirstMouseX = mediator.music.TimeToLocalXAtCurrentTime(mouseDownTime, 1);
            float worldFirstMouseX = mediator.hitPoint.transform.TransformPoint(hitPointLocalFirstMouseX, 0, 0).x;
            Vector2 worldFirstMouse = new Vector2(worldFirstMouseX, mouseDownWorldY);
            Vector2 worldMouse = ClampedWorldMousePos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            List<Note> overlaped = NoteCollider.OverlapRect(worldFirstMouse, worldMouse);

            if(overlaped.Count > 0)
            {
                OnSelectNotes?.Invoke(overlaped, null, SelectMouseType.MouseUp);
            }
            else
            {
                OnSelectEmpty?.Invoke(SelectMouseType.MouseUp);
            }
        }

        mouseDrag = false;
    }

    Vector2 ClampedWorldMousePos(Vector2 worldMouse)
    {
        Vector2 worldHalfExtents = Camera.main.OrthographicExtents() * 0.5f;
        Vector2 camPos = Camera.main.transform.position;
        worldMouse.x = Mathf.Clamp(worldMouse.x, camPos.x - worldHalfExtents.x, camPos.x + worldHalfExtents.x);
        worldMouse.y = Mathf.Clamp(worldMouse.y, camPos.y - worldHalfExtents.y, camPos.y + worldHalfExtents.y);
        return worldMouse;
    }

    void MouseScroll()
    {
        const float scrollTick = 0.1f;
        float deltaY = Input.mouseScrollDelta.y;
        if (Mathf.Abs(deltaY) > 0)
        {
            mediator.music.playingTime += -deltaY * scrollTick;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // 인풋 이벤트를 받기 위해 이미지를 추가합니다.
        emptyImage = gameObject.GetComponent<UnityEngine.UI.Image>();
        if (emptyImage == null)
        {
            emptyImage = gameObject.AddComponent<UnityEngine.UI.Image>();
            emptyImage.color = Color.clear;
        }

        Shader shader = Shader.Find("GL/GL");

        glMaterial = new Material(shader);
        glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        glMaterial.SetInt("_ZWrite", 0);

        glMaterial.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
        glMaterial.SetInt("_Stencil", 0);
        glMaterial.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);

        glMaterialNonMasking = new Material(shader);
        glMaterialNonMasking.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterialNonMasking.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterialNonMasking.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        glMaterialNonMasking.SetInt("_ZWrite", 0);

        visibleBeats = new List<VisibleBeat>();
    }

    private void Update()
    {
        UpdateVisibleBeats();

        if (pointerHovering)
        {
            overlappedBeat = OverlapBeatOnScreen(Input.mousePosition, out reactBeatIndex, out reactBeatPosition);

            MouseScroll();
        }
    }

    private void OnRenderObject()
    {
        float localUpHitPointTop = rectTransform.InverseTransformPoint(0, mediator.hitPoint.airPos.y + localGridHeight, 0).y;
        float localUpHitPointBot = rectTransform.InverseTransformPoint(0, mediator.hitPoint.airPos.y - localGridHeight, 0).y;
        float localDownHitPointTop = rectTransform.InverseTransformPoint(0, mediator.hitPoint.roadPos.y + localGridHeight, 0).y;
        float localDownHitPointBot = rectTransform.InverseTransformPoint(0, mediator.hitPoint.roadPos.y - localGridHeight, 0).y;
        float localGridWidth = rectTransform.InverseTransformVector(this.localGridWidth, 0, 0).x;

        float panelWidth = rectTransform.rect.width;
        float panelHalfWidth = panelWidth * 0.5f;
        float panelHeight = rectTransform.rect.height;
        float panelHalfHiehgt = panelHeight * 0.5f;

        glMaterial.SetPass(0);

        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw guide line
        GL.Begin(GL.LINES);
        {
            GL.Color(Color.white);

            GL.Vertex3(-panelHalfWidth, localUpHitPointTop, 0);
            GL.Vertex3(+panelHalfWidth, localUpHitPointTop, 0);

            GL.Vertex3(-panelHalfWidth, localUpHitPointBot, 0);
            GL.Vertex3(+panelHalfWidth, localUpHitPointBot, 0);

            GL.Vertex3(-panelHalfWidth, localDownHitPointTop, 0);
            GL.Vertex3(+panelHalfWidth, localDownHitPointTop, 0);

            GL.Vertex3(-panelHalfWidth, localDownHitPointBot, 0);
            GL.Vertex3(+panelHalfWidth, localDownHitPointBot, 0);
        }
        GL.End();

        // Draw beats
        GL.Begin(GL.QUADS);
        {
            foreach(VisibleBeat beat in visibleBeats)
            {
                Color upBeatColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);
                Color centerBeatColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);
                Color downBeatColor = new Color(1.0f, 1.0f, 1.0f, 0.25f);

                if (overlappedBeat && reactBeatIndex == beat.index)
                {
                    if (reactBeatPosition == NotePosition.Air)
                    {
                        upBeatColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                    else if (reactBeatPosition == NotePosition.Center)
                    {
                        centerBeatColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                    else if (reactBeatPosition == NotePosition.Road) 
                    {
                        downBeatColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);
                    }
                }

                GL.Color(upBeatColor);
                GL.Vertex3(beat.localLeftX, beat.localUpYTop, 0);
                GL.Vertex3(beat.localRightX, beat.localUpYTop, 0);
                GL.Vertex3(beat.localRightX, beat.localUpYBot, 0);
                GL.Vertex3(beat.localLeftX, beat.localUpYBot, 0);

                GL.Color(centerBeatColor);
                GL.Vertex3(beat.localLeftX, beat.localUpYBot, 0);
                GL.Vertex3(beat.localRightX, beat.localUpYBot, 0);
                GL.Vertex3(beat.localRightX, beat.localDownYTop, 0);
                GL.Vertex3(beat.localLeftX, beat.localDownYTop, 0);

                GL.Color(downBeatColor);
                GL.Vertex3(beat.localLeftX, beat.localDownYTop, 0);
                GL.Vertex3(beat.localRightX, beat.localDownYTop, 0);
                GL.Vertex3(beat.localRightX, beat.localDownYBot, 0);
                GL.Vertex3(beat.localLeftX, beat.localDownYBot, 0);
            }
        }
        GL.End();

        glMaterialNonMasking.SetPass(0);

        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw selection rect
        if (mouseDrag)
        {
            float hitPointLocalFirstMouseX = mediator.music.TimeToLocalXAtCurrentTime(mouseDownTime, 1);
            float worldFirstMouseX = mediator.hitPoint.transform.TransformPoint(hitPointLocalFirstMouseX, 0, 0).x;
            Vector2 worldFirstMouse = new Vector2(worldFirstMouseX, mouseDownWorldY);
            Vector2 screenMouse = Input.mousePosition;
            Vector2 worldMouse = ClampedWorldMousePos(Camera.main.ScreenToWorldPoint(screenMouse));
            Vector2 localMouseA = rectTransform.InverseTransformPoint(worldFirstMouse);
            Vector2 localMouseB = rectTransform.InverseTransformPoint(worldMouse);
            Vector2 min = new Vector2(Mathf.Min(localMouseA.x, localMouseB.x), Mathf.Min(localMouseA.y, localMouseB.y));
            Vector2 max = new Vector2(Mathf.Max(localMouseA.x, localMouseB.x), Mathf.Max(localMouseA.y, localMouseB.y));

            Color selectionRectFillColor = new Color(0, 1, 0, 0.5f);
            Color selectionRectLineColor = new Color(0, 1, 0, 1);

            GL.Begin(GL.QUADS);
            GL.Color(selectionRectFillColor);
            GL.Vertex3(min.x, max.y, 0);
            GL.Vertex3(max.x, max.y, 0);
            GL.Vertex3(max.x, min.y, 0);
            GL.Vertex3(min.x, min.y, 0);
            GL.End();

            GL.Begin(GL.LINE_STRIP);
            GL.Color(selectionRectLineColor);
            GL.Vertex3(min.x, max.y, 0);
            GL.Vertex3(max.x, max.y, 0);
            GL.Vertex3(max.x, min.y, 0);
            GL.Vertex3(min.x, min.y, 0);
            GL.Vertex3(min.x, max.y, 0);
            GL.End();
        }
    }
}
