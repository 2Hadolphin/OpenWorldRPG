using UnityEngine;
using System;

namespace Return.Humanoid
{
    /// <summary>
    /// ???
    /// </summary>
    public interface IHumanoidAnimator: IAnimator
    {
        /// <summary>
        /// Get bone transform
        /// </summary>
        [Obsolete]
        bool GetBoneTransform(HumanBodyBones bodyBone,out Transform bone)
        {
            bone = Animator.GetBoneTransform(bodyBone);
            return bone == null;
        }


    }
}
