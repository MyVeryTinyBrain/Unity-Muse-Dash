using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[ExecuteInEditMode]
public class UISpineElementFitter : MonoBehaviour
{
    public enum UIElementFit
    {
        None,
        FitByParent,
    }

    SkeletonGraphic skeletonGraphic = null;

    [SerializeField]
    UIElementFit elementFit = UIElementFit.FitByParent;

    [SerializeField]
    [Range(0, 1)]
    float ratio = 1.0f;

    private void Start()
    {
        skeletonGraphic = GetComponent<SkeletonGraphic>();
    }

    void FitByParent()
    {
        if (!skeletonGraphic)
        {
            return;
        }

        RectTransform parent = skeletonGraphic.rectTransform.parent as RectTransform;

        if (!parent)
        {
            return;
        }

        Vector2 curSize = new Vector2(skeletonGraphic.SkeletonData.Width, skeletonGraphic.SkeletonData.Height);
        Vector2 curCenter = new Vector2(skeletonGraphic.SkeletonData.X, skeletonGraphic.SkeletonData.Y) + curSize * 0.5f;

        Vector2 parentSize = parent.sizeDelta;
        Vector2 worldCurSize = skeletonGraphic.rectTransform.TransformVector(curSize);
        Vector2 parentSpaceCurSize = parent.InverseTransformVector(worldCurSize);

        float xScale = skeletonGraphic.rectTransform.localScale.x * parentSize.x / parentSpaceCurSize.x;
        float yScale = skeletonGraphic.rectTransform.localScale.y * parentSize.y / parentSpaceCurSize.y;

        if (xScale == 0 || yScale == 0)
        {
            skeletonGraphic.enabled = false;
            return;
        }
        else
        {
            skeletonGraphic.enabled = true;
        }

        skeletonGraphic.rectTransform.localScale = new Vector3(xScale * ratio, yScale * ratio, 1);

        skeletonGraphic.rectTransform.localPosition = -curCenter * new Vector2(xScale, yScale);
    }

    private void LateUpdate()
    {
        switch (elementFit)
        {
            case UIElementFit.None:
            break;

            case UIElementFit.FitByParent:
            FitByParent();
            break;
        }
    }
}
