using UnityEngine;
using System.Collections;
using Spine.Unity;

[ExecuteInEditMode]
[RequireComponent(typeof(SkeletonRenderer))]
public class SpineGauge : MonoBehaviour
{
    [Range(0, 1)]
    public float ratio = 0;

    [ReadOnly]
    public float time = 0;

    [SpineAnimation]
    public string fillAnimationName;
    Spine.Animation fillAnimation;

    [SerializeField]
    SkeletonAnimation skeletonAnimation;

    void Update()
    {
        if(skeletonAnimation == null)
        {
            skeletonAnimation = GetComponentInChildren<SkeletonAnimation>(true);
        }

        if (skeletonAnimation && skeletonAnimation.skeleton != null && fillAnimationName != null)
        {
            if(fillAnimation != null && fillAnimation.Name != fillAnimationName)
            {
                fillAnimation = null;
            }

            if (fillAnimation == null)
            {
                fillAnimation = skeletonAnimation.skeleton.Data.FindAnimation(fillAnimationName);
                if (fillAnimation == null)
                    return;
            }

            time = fillAnimation.Duration * ratio;
            skeletonAnimation.ApplyAnimation(fillAnimation, time, false);
        }
    }
}