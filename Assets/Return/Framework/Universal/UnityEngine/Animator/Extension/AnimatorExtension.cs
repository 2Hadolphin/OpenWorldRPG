using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.Assertions;

namespace Return
{
    public static partial class AnimatorExtension
    {

        #region AnimationClip

        /// <summary>
        /// Returns the Animation Clip with the corresponding name.
        /// </summary>
        /// <param name="animator"></param>
        /// <param name="name">The Animation Clip name.</param>
        /// <returns></returns>
        public static AnimationClip GetAnimationClip(this Animator animator, string name)
        {
            if (!animator)
                return null;

            for (int i = 0; i < animator.runtimeAnimatorController.animationClips.Length; i++)
            {
                if (animator.runtimeAnimatorController.animationClips[i].name == name)
                    return animator.runtimeAnimatorController.animationClips[i];
            }
            return null;
        }

        #endregion

        /// <summary>
        /// Get skeleton data from the avatar of animator.
        /// </summary>
        public static SkeletonBone GetSkeleton(this Animator animator,HumanBodyBones bodybone)
        {
            var avatar = animator.avatar;
            Assert.IsFalse(avatar == null);

            var bone = animator.GetBoneTransform(bodybone);

            if (bone != null)
            {
                var boneName = bone.name;

                var skeletons = avatar.humanDescription.skeleton;


                foreach (var skeleton in skeletons)
                {
                    if (skeleton.name.Equals(boneName))
                        return skeleton;
                }
            }

            return default;
        }


        /// <summary>
        /// Set character to TSerializable-Pose via avatar of the anim. 
        /// </summary>
        public static void ForceTPose(this Animator animator)
        {
            var avatar = animator.avatar;
            Assert.IsFalse(avatar == null);
            var skeletons = avatar.humanDescription.skeleton;

            var root = animator.transform;
            // cache character coordinate
            //var pos = root.position;
            //var rot = root.rotation;

            var tfs = root.Traverse();

            var dir = new Dictionary<string, Transform>(tfs.Count());
            foreach (var tf in tfs)
                dir.SafeAdd(tf.name, tf);

            dir.Remove(root.name);

            foreach (var skeleton in skeletons)
            {
                if (!dir.TryGetValue(skeleton.name, out var bone))
                    continue;

                bone.localPosition = skeleton.position;
                bone.localRotation = skeleton.rotation;
                bone.localScale = skeleton.scale;
            }

            //root.SetPositionAndRotation(pos, rot);
        }


        public static void TPose_Zero(Animator animator)
        {
            if (!animator || !animator.avatar)
                return;

            Quaternion a = Quaternion.Euler(Vector3.one);
            a *= Quaternion.Euler(Vector3.zero);
            var tfs = animator.GetComponentsInChildren<Transform>();
            var d = tfs.Select(x => x).ToDictionary(x => x.name, x => x);

            d.Remove(animator.transform.name);

            var skeletons = animator.avatar.humanDescription.skeleton;



            foreach (var skeleton in skeletons)
            {
                if (d.TryGetValue(skeleton.name, out var tf))
                {
                    tf.localPosition = skeleton.position;
                    tf.localRotation = skeleton.rotation;
                    tf.localScale = skeleton.scale;
                }
            }
        }



    }
}
