using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Spine.Unity;

public static class Utility
{
    /// <summary>
    /// 해당 열거형의 다음 값을 반환합니다.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static T LoopEnum<T>(T enumValue) where T : Enum
    {
        IEnumerable<T> enums = Enum.GetValues(typeof(T)).Cast<T>();
        for (int i = 0; i < enums.Count(); ++i)
        {
            if (enums.ElementAt(i).Equals(enumValue))
            {
                if (i == enums.Count() - 1)
                {
                    return enums.First();
                }
                else
                {
                    return enums.ElementAt(i + 1);
                }
            }
        }
        return enums.First();
    }

    /// <summary>
    /// 해당 열거형의 인덱스를 반환합니다.<para>
    /// 열거형의 값이 아닌 순서를 반환합니다.</para>
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static int GetIndexOfEnum<T>(T enumValue) where T : Enum
    {
        IEnumerable<T> enums = Enum.GetValues(typeof(T)).Cast<T>();
        for (int i = 0; i < enums.Count(); ++i)
        {
            if (enums.ElementAt(i).Equals(enumValue))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 해당 열거형의 인덱스를 반환합니다.<para>
    /// 열거형의 값이 아닌 순서를 반환합니다.</para>
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="enumValue"></param>
    /// <returns></returns>
    public static int GetIndexOfEnum(Type enumType, Enum enumValue)
    {
        IEnumerable<Enum> enums = Enum.GetValues(enumType).Cast<Enum>();
        for (int i = 0; i < enums.Count(); ++i)
        {
            if (enums.ElementAt(i).Equals(enumValue))
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 해당 열거형의 인덱스와 일치하는 값을 반환합니다.
    /// </summary>
    /// <param name="enumType"></param>
    /// <param name="enumIndex"></param>
    /// <returns></returns>
    public static Enum GetEnumOfIndex(Type enumType, int enumIndex)
    {
        IEnumerable<Enum> enums = Enum.GetValues(enumType).Cast<Enum>();
        return enums.ElementAt(enumIndex);
    }

    public static T RandomEnum<T>()
    {
        IEnumerable<T> enums = Enum.GetValues(typeof(T)).Cast<T>();
        int index = UnityEngine.Random.Range(0, enums.Count() - 1);
        return enums.ElementAt(index);
    }

    public static List<string> GetNamesOfEnum<T>() where T : Enum
    {
        return GetNamesOfEnum(typeof(T));
    }

    public static List<string> GetNamesOfEnum(Type enumType)
    {
        IEnumerable<Enum> enums = Enum.GetValues(enumType).Cast<Enum>();
        List<string> enumNames = new List<string>();
        foreach (Enum e in enums)
        {
            enumNames.Add(e.ToString());
        }
        return enumNames;
    }

    public static T Loop<T>(int index, params T[] array)
    {
        if(array == null || array.Length == 0)
        {
            return default;
        }
        return array[index % array.Length];
    }

    public static Vector2 OrthographicExtents(this Camera camera)
    {
        //Vector2 camExtents = Vector2.zero;
        //camExtents.x = camera.aspect * 2f * camera.orthographicSize;
        //camExtents.y = 2f * camera.orthographicSize;
        //return camExtents;
        return OrthographicExtents(camera.aspect, camera.orthographicSize);
    }

    public static Vector2 OrthographicExtents(float aspect, float orthographicSize)
    {
        Vector2 camExtents = Vector2.zero;
        camExtents.x = aspect * 2f * orthographicSize;
        camExtents.y = 2f * orthographicSize;
        return camExtents;
    }

    public static bool UsingInputField()
    {
        GameObject currentFocus = EventSystem.current.currentSelectedGameObject;
        if (currentFocus != null)
        {
            TMP_InputField tmp_if = null;
            TMP_Dropdown tmp_dd = null;
            if (currentFocus.TryGetComponent<TMP_InputField>(out tmp_if) ||
                currentFocus.TryGetComponent<TMP_Dropdown>(out tmp_dd))
            {
                return true;
            }
        }
        return false;
    }

    public static void Swap<T>(this List<T> list, int indexA, int indexB)
    {
        T temp = list[indexA];
        list[indexA] = list[indexB];
        list[indexB] = temp;
    }

    public static string ToStandardPathFormat(string path)
    {
        return path.Replace('/', '\\');
    }

    public static string GetFileSaveDirectory()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.WindowsEditor:
            case RuntimePlatform.WindowsPlayer:
            return ToStandardPathFormat($"{Application.dataPath}/GameFiles");

            default:
            return ToStandardPathFormat($"{Application.persistentDataPath}/GameFiles");
        }
    }

    public static void CreateSaveDirectory()
    {
        if (!Directory.Exists(GetFileSaveDirectory()))
        {
            Directory.CreateDirectory(GetFileSaveDirectory());
        }
    }

    public static void Init(this SkeletonAnimation skeletonAnimation, SkeletonDataAsset asset, string skin = "default")
    {
        skeletonAnimation.ClearState();
        skeletonAnimation.initialSkinName = skin;
        skeletonAnimation.skeletonDataAsset = asset;
        skeletonAnimation.Initialize(true, true);
    }

    public static void Init(this SkeletonGraphic skeletonGraphic, SkeletonDataAsset asset, string skin = "default")
    {
        skeletonGraphic.AnimationState?.ClearTracks();
        skeletonGraphic.initialSkinName = skin;
        skeletonGraphic.skeletonDataAsset = asset;
        skeletonGraphic.Initialize(true);
    }

    public static void ApplyAnimation_Internal(this SkeletonAnimation skeletonAnimation, Spine.Animation animation, float time, bool loop, float deltaTime)
    {
        animation.Apply(skeletonAnimation.skeleton, 0, time, loop, null, 1, Spine.MixBlend.Replace, Spine.MixDirection.In);
        skeletonAnimation.skeleton.Update(deltaTime);
        skeletonAnimation.skeleton.UpdateWorldTransform();
    }

    public static void ApplyAnimation(this SkeletonAnimation skeletonAnimation, Spine.Animation animation, float time, bool loop)
    {
        ApplyAnimation_Internal(skeletonAnimation, animation, time, loop, Time.deltaTime);
    }

    public static void BlendAnimation_Internal(this SkeletonAnimation skeletonAnimation, Spine.Animation animation, float time, bool loop, float alpha, float deltaTime)
    {
        animation.Apply(skeletonAnimation.skeleton, 0, time, loop, null, alpha, Spine.MixBlend.First, Spine.MixDirection.In);
        skeletonAnimation.skeleton.Update(deltaTime);
        skeletonAnimation.skeleton.UpdateWorldTransform();
    }

    public static void BlendAnimation(this SkeletonAnimation skeletonAnimation, Spine.Animation animation, float time, bool loop, float alpha)
    {
        BlendAnimation_Internal(skeletonAnimation, animation, time, loop, alpha, Time.deltaTime);
    }

    public static void EnablePMAAtMaterial(Material material, bool enablePMA)
    {
        int STRAIGHT_ALPHA_PARAM_ID = Shader.PropertyToID("_StraightAlphaInput");
        const string ALPHAPREMULTIPLY_ON_KEYWORD = "_ALPHAPREMULTIPLY_ON";
        const string STRAIGHT_ALPHA_KEYWORD = "_STRAIGHT_ALPHA_INPUT";

        if (material.HasProperty(STRAIGHT_ALPHA_PARAM_ID))
        {
            material.SetInt(STRAIGHT_ALPHA_PARAM_ID, enablePMA ? 0 : 1);
            if (enablePMA)
                material.DisableKeyword(STRAIGHT_ALPHA_KEYWORD);
            else
                material.EnableKeyword(STRAIGHT_ALPHA_KEYWORD);
        }
        else
        {
            if (enablePMA)
                material.EnableKeyword(ALPHAPREMULTIPLY_ON_KEYWORD);
            else
                material.DisableKeyword(ALPHAPREMULTIPLY_ON_KEYWORD);
        }
    }
}
