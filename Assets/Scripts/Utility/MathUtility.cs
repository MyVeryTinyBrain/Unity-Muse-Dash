using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class MathUtility
{
    public static float SmoothStep(float min, float max, float x)
    {
        float t = Mathf.Clamp((x - min) / (max - min), 0f, 1f);
        return t * t * (3f - 2f * t);
    }

    public static float InverseLerp(float min, float max, float value)
    {
        return (value - min) / (max - min);
    }

    public static float RepeatInMinMax(float min, float max, float x)
    {
        if(max == min)
        {
            return min;
        }

        return Mathf.Repeat(x - min, max - min) + min;
    }

    public static int GetRepeatIndex(float min, float max, float yFromRepeatInMinMax)
    {
        if(max == min)
        {
            return 0;
        }

        return Mathf.FloorToInt((yFromRepeatInMinMax - min) / (max - min));
    }
}
