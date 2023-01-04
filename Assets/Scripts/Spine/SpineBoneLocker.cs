using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

[ExecuteInEditMode]
public class SpineBoneLocker : MonoBehaviour
{
    [SpineBone]
    public string boneName;
    Spine.Bone bone;
    public Vector3 targetPosition;

    private void Update()
    {
        if (boneName != null && (bone == null || bone.Data.Name != boneName))
        {
            SkeletonAnimation skeletonAnimation = GetComponent<SkeletonAnimation>();
            if (skeletonAnimation)
            {
                Spine.Bone prevBone = bone;
                this.bone = skeletonAnimation.Skeleton.FindBone(boneName);
                
                if(prevBone != null)
                {
                    skeletonAnimation.UpdateLocal -= SkeletonAnimation_UpdateLocal;
                }
                skeletonAnimation.UpdateLocal += SkeletonAnimation_UpdateLocal;
            }
        }

        if (bone != null)
        {
            bone.SetLocalPosition(targetPosition);
        }
    }

    void SkeletonAnimation_UpdateLocal(ISkeletonAnimation animated)
    {
        bone.SetLocalPosition(targetPosition);
    }
}
