using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class UIElementFitter : MonoBehaviour
{
    public enum UIElementFit
    {
        None,
        FitByWidth,
        FitByHeight,
        FitByParent,
        FitByParent_OnlyWidth,
        FitByParent_OnlyHeight,
        FitByParent_2D,
    }

    RectTransform rectTransform = null;

    public UIElementFit elementFit = UIElementFit.None;

    [HideInInspector]
    public float ratio = 1.0f;

    [HideInInspector]
    public float ratio2DX = 1.0f;

    [HideInInspector]
    public float ratio2DY = 1.0f;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void LateUpdate()
    {
        if (!rectTransform)
        {
            return;
        }

        switch (elementFit)
        {
            case UIElementFit.None:
            break;

            case UIElementFit.FitByWidth:
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, rectTransform.sizeDelta.x * ratio);
            break;

            case UIElementFit.FitByHeight:
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.y * ratio, rectTransform.sizeDelta.y);
            break;

            case UIElementFit.FitByParent:
            {
                RectTransform parent = rectTransform.parent as RectTransform;
                if (parent)
                {
                    rectTransform.sizeDelta = parent.sizeDelta * ratio;
                }
            }
            break;

            case UIElementFit.FitByParent_OnlyWidth:
            {
                RectTransform parent = rectTransform.parent as RectTransform;
                if (parent)
                {
                    Vector2 sizeDelta = rectTransform.sizeDelta;
                    sizeDelta.x = parent.sizeDelta.x * ratio;
                    rectTransform.sizeDelta = sizeDelta;
                }
            }
            break;

            case UIElementFit.FitByParent_OnlyHeight:
            {
                RectTransform parent = rectTransform.parent as RectTransform;
                if (parent)
                {
                    Vector2 sizeDelta = rectTransform.sizeDelta;
                    sizeDelta.y = parent.sizeDelta.y * ratio;
                    rectTransform.sizeDelta = sizeDelta;
                }
            }
            break;

            case UIElementFit.FitByParent_2D:
            {
                RectTransform parent = rectTransform.parent as RectTransform;
                if (parent)
                {
                    rectTransform.sizeDelta = new Vector2(parent.sizeDelta.x * ratio2DX, parent.sizeDelta.y * ratio2DY);
                }
            }
            break;
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(UIElementFitter))]
[CanEditMultipleObjects]
public class UIElementFitterEditor : Editor
{
    UIElementFitter targetUIElementFitter = null;

    private void OnEnable()
    {
        targetUIElementFitter = (UIElementFitter)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        switch (targetUIElementFitter.elementFit)
        {
            default:
            {
                targetUIElementFitter.ratio =
                    EditorGUILayout.Slider(nameof(targetUIElementFitter.ratio), targetUIElementFitter.ratio, 0, 1);
            }
            break;

            case UIElementFitter.UIElementFit.FitByParent_2D:
            {
                targetUIElementFitter.ratio2DX =
                    EditorGUILayout.Slider(nameof(targetUIElementFitter.ratio2DX), targetUIElementFitter.ratio2DX, 0, 1);
                targetUIElementFitter.ratio2DY =
                    EditorGUILayout.Slider(nameof(targetUIElementFitter.ratio2DY), targetUIElementFitter.ratio2DY, 0, 1);
            }
            break;
        }
    }
}
#endif
