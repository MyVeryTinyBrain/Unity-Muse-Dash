using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;

public class VolumeInspector : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    protected Mediator mediator => Mediator.i;

    public int skipSamples = 20;
    public float worldBeatLineWidth = 0.1f;
    public float worldVolumeLineWidth = 0.03f;
    public BeatType beatType = BeatType.B4; 

    RectTransform rectTransform = null;
    Material glMaterial = null;
    bool dragging = false;
    Vector2 dragBeginPoint;
    int dragBeginTimeSample;

    public void IncreaseBeat()
    {
        beatType = Utility.LoopEnum(beatType);
    }

    void IPointerDownHandler.OnPointerDown(UnityEngine.EventSystems.PointerEventData eventData)
    {
        Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 localMouse = rectTransform.InverseTransformPoint(worldMouse);

        if (dragging = rectTransform.rect.Contains(localMouse))
        {
            dragBeginPoint = localMouse;
            dragBeginTimeSample = mediator.music.playingTimeSample;
        }
    }

    void IPointerUpHandler.OnPointerUp(PointerEventData eventData)
    {
        dragging = false;
    }

    void Drag()
    {
        if (dragging)
        {
            float gameLeftTime, gameRightTime;
            mediator.music.GetScreenTimes(mediator.music.playingTime, out gameLeftTime, out gameRightTime);

            Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 localMouse = rectTransform.InverseTransformPoint(worldMouse);
            Vector2 delta = dragBeginPoint - localMouse;
            float panelWidth = rectTransform.rect.width;
            float deltaPercent = delta.x / panelWidth;
            float screenTimeLength = gameRightTime - gameLeftTime;
            // UI 로컬 공간에서의 마우스 움직임 거리를 시간으로 변환합니다.
            int deltaXTimeSample = MusicUtility.TimeToTimeSamples(deltaPercent * screenTimeLength, mediator.music.frequency);
            // 드래그한 시간만큼 음악 재생시간에 추가합니다.
            int newTimeSample = dragBeginTimeSample + deltaXTimeSample;
            mediator.music.playingTimeSample = newTimeSample;
        }
    }

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        Shader shader = Shader.Find("GL/GL");

        glMaterial = new Material(shader);
        glMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        glMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        glMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
        glMaterial.SetInt("_ZWrite", 0);

        glMaterial.SetInt("_StencilComp", (int)UnityEngine.Rendering.CompareFunction.Equal);
        glMaterial.SetInt("_Stencil", 0);
        glMaterial.SetInt("_StencilOp", (int)UnityEngine.Rendering.StencilOp.Keep);
    }

    private void Update()
    {
        Drag();
    }

    private void OnRenderObject()
    {
        float gameLeftTime, gameRightTime;
        mediator.music.GetScreenTimes(mediator.music.playingTime, out gameLeftTime, out gameRightTime);

        float panelWidth = rectTransform.rect.width;
        float panelHeight = rectTransform.rect.height;
        float localBeatLineWidth = rectTransform.InverseTransformVector(worldBeatLineWidth, 0, 0).x;
        float localVolumeLineWidth = rectTransform.InverseTransformVector(worldVolumeLineWidth, 0, 0).x / mediator.music.currentGlobalSpeedScale;
        float localHitPointX = rectTransform.InverseTransformPoint(mediator.hitPoint.Pos.x, 0, 0).x;
        float localLengthPerSeconds = rectTransform.InverseTransformVector(mediator.gameSettings.lengthPerSeconds, 0, 0).x;

        glMaterial.SetPass(0);

        GL.MultMatrix(transform.localToWorldMatrix);

        // Draw volume
        GL.Begin(GL.QUADS);
        // 음악의 볼륨 파형 데이터 배열
        float[] volume = mediator.music.leftVolumeData;
        if (volume != null)
        {
            // 이 간격마다 볼륨 데이터를 탐색합니다.
            int skip = Mathf.Clamp(skipSamples, 1, int.MaxValue);
            // 화면 왼쪽 경계 x위치가 나타내는 시간
            int leftTimeSample = MusicUtility.TimeToTimeSamples(gameLeftTime, mediator.music.frequency);
            // 화면 오른쪽 경계 x위치가 나타내는 시간
            int rightTimeSample = MusicUtility.TimeToTimeSamples(gameRightTime, mediator.music.frequency);
            int beginTimeSample = (leftTimeSample / skip) * skip;
            int endTimeSample = (rightTimeSample / skip) * skip;
            // 화면 왼쪽부터 화면 오른쪽까지 skip 간격마다 볼륨 데이터를 읽어 그립니다.
            for (int i = beginTimeSample; i < endTimeSample && i < volume.Length; i += skip)
            {
                float percent = (float)(i - beginTimeSample) / (float)(endTimeSample - beginTimeSample);
                int timeSample = i;
                float x = percent * panelWidth - panelWidth * 0.5f;
                // 볼륨 데이터를 읽어 세로선의 길이로 사용합니다.
                float y = (timeSample > 0 && timeSample < volume.Length - 1) ? Mathf.Abs(volume[timeSample]) : 0;
                y *= panelHeight * 0.5f;
                
                GL.Vertex3(x - localVolumeLineWidth, +y, 0);
                GL.Vertex3(x + localVolumeLineWidth, +y, 0);
                GL.Vertex3(x + localVolumeLineWidth, -y, 0);
                GL.Vertex3(x - localVolumeLineWidth, -y, 0);
            }
        }
        GL.End();

        // Draw beats
        GL.Begin(GL.QUADS);
        {
            List<float> beats = mediator.music.GetBeats(beatType);
            int closetIndex = mediator.music.GetClosetBeatIndexAtTime(gameLeftTime, beatType);

            for (int i = closetIndex; i < beats.Count; ++i)
            {
                float ratio = mediator.music.TimeToRatioAtCurrentTime(beats[i], 1);
                float x = localHitPointX + ratio * localLengthPerSeconds;

                if (beats[i] <= gameRightTime)
                {
                    GL.Color(Color.red);
                    GL.Vertex3(x - localBeatLineWidth * 0.5f, +panelHeight * 0.5f, 0);
                    GL.Vertex3(x + localBeatLineWidth * 0.5f, +panelHeight * 0.5f, 0);
                    GL.Vertex3(x + localBeatLineWidth * 0.5f, -panelHeight * 0.5f, 0);
                    GL.Vertex3(x - localBeatLineWidth * 0.5f, -panelHeight * 0.5f, 0);
                }
            }
        }
        GL.End();

        // Draw Hit line
        GL.Begin(GL.QUADS);
        {
            GL.Color(Color.black);
            GL.Vertex3(localHitPointX - localBeatLineWidth * 0.5f, +panelHeight * 0.5f, 0);
            GL.Vertex3(localHitPointX + localBeatLineWidth * 0.5f, +panelHeight * 0.5f, 0);
            GL.Vertex3(localHitPointX + localBeatLineWidth * 0.5f, -panelHeight * 0.5f, 0);
            GL.Vertex3(localHitPointX - localBeatLineWidth * 0.5f, -panelHeight * 0.5f, 0);
        }
        GL.End();
    }
}
