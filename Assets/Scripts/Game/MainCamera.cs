using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MainCamera : MonoBehaviour, IGameReset
{
    public const uint NullHandle = uint.MinValue;

    public float sizeChangePower = 20.0f;

    new Camera camera = null;
    float originAspect = 16 / 9.0f;
    float originSize = 7.6f;
    Vector2 originExtents;
    uint newHandleId = NullHandle;

    Vector2 shakeVector = Vector2.zero;
    float shakeStartTime = float.MinValue;
    float shakeDuration = 0.0f;

    // 요청 핸들 번호, 요청 비율로 구성된 딕셔너리입니다.
    Dictionary<uint, float> requiredSizeRatiosDict = new Dictionary<uint, float>();

    // 요청 비율들입니다.
    List<float> requiredSizeRatios = new List<float>();

    void SortRequiredSizeRatios()
    {
        // 오름차순 정렬
        requiredSizeRatios.Sort((lhs, rhs) => lhs.CompareTo(rhs));
    }

    // 요청 핸들을 반환합니다.
    // 이 핸들로 요청을 제거 가능합니다.
    public uint RequireSizeRatio(float ratio)
    {
        if (newHandleId == NullHandle)
        {
            newHandleId = NullHandle + 1;
        }

        uint handle = newHandleId++;
        requiredSizeRatiosDict.Add(handle, ratio);

        requiredSizeRatios.Add(ratio);
        SortRequiredSizeRatios();

        return handle;
    }

    public uint ReserveSizeRatio(float ratio, float duration)
    {
        uint handle = RequireSizeRatio(ratio);
        StartCoroutine(Coroutine_ReserveSizeRatio(handle, duration));
        return handle;
    }

    IEnumerator Coroutine_ReserveSizeRatio(uint handle, float duration)
    {
        yield return new WaitForSeconds(duration);
        RemoveSizeRatio(handle);
    }

    public void RemoveSizeRatio(uint handle)
    {
        if (requiredSizeRatiosDict.TryGetValue(handle, out float ratio))
        {
            requiredSizeRatiosDict.Remove(handle);
            requiredSizeRatios.Remove(ratio);
        }
    }

    public void ClearRequiredSizeRatios()
    {
        requiredSizeRatios.Clear();
        requiredSizeRatiosDict.Clear();
    }

    public void AddShake(float distance, float restoreDuration)
    {
        float radian = UnityEngine.Random.Range(0, 360) * Mathf.Deg2Rad;
        float cos = Mathf.Cos(radian);
        float sin = Mathf.Sign(radian);
        shakeVector = new Vector2(cos, sin) * distance;
        shakeStartTime = Time.time;
        shakeDuration = restoreDuration;
    }

    void UpdateSize()
    {
        // Apply size
        float srcSize = camera.orthographicSize;
        float dstSize = originSize;

        if (requiredSizeRatios.Count == 0)
        {
            dstSize = originSize;
        }
        else
        {
            dstSize = originSize * requiredSizeRatios[0];
        }

        float size = Mathf.Lerp(srcSize, dstSize, sizeChangePower * Time.deltaTime);
        if (Mathf.Abs(size - dstSize) < 0.01f)
        {
            size = dstSize;
        }
        camera.orthographicSize = size;
    }

    void UpdatePosition()
    {
        float shakeRestoreRatio = Mathf.Clamp01((Time.time - shakeStartTime) / shakeDuration);
        if (shakeDuration == 0.0f)
        {
            shakeRestoreRatio = 1.0f;
        }
        Vector2 shake = Vector2.Lerp(shakeVector, Vector2.zero, shakeRestoreRatio);

        Vector2 currentExtents = camera.OrthographicExtents();
        Vector3 localPosition = transform.localPosition;
        localPosition.x = currentExtents.x - originExtents.x + shake.x;
        localPosition.y = shake.y;
        transform.localPosition = localPosition;
    }

    void CaptureOriginCameraState()
    {
        originAspect = camera.aspect;
        originSize = camera.orthographicSize;
        originExtents = camera.OrthographicExtents();
    }

    private void Awake()
    {
        camera = GetComponent<Camera>();
        CaptureOriginCameraState();
    }

    private void Update()
    {
        if (camera.aspect != originAspect)
        {
            CaptureOriginCameraState();
        }

        UpdateSize();
        UpdatePosition();

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            if (requiredSizeRatios.Count == 0)
            {
                RequireSizeRatio(0.8f);
            }
            else
            {
                ClearRequiredSizeRatios();
            }
        }
    }

    public void GameReset()
    {
        shakeVector = Vector2.zero;
        requiredSizeRatiosDict.Clear();
        requiredSizeRatios.Clear();
    }
}
