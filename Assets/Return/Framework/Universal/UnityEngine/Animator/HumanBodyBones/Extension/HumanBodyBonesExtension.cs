using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Return.Humanoid.Animation;
//using Return.Humanoid;
using UnityEngine.Animations;
using System;
using System.Linq;
using UnityEngine.Assertions;

namespace Return
{
    public static partial class HumanBodyBonesExtension
    {
        public static IEnumerable<Transform> GetBoneChain(this Animator animator,HumanBodyBones root,HumanBodyBones child,bool allowPassInvalidbone=true)
        {
            Transform rootBone, childBone;

            rootBone = animator.GetBoneTransform(root);

            Assert.IsNotNull(rootBone);

            do
            {
                childBone = animator.GetBoneTransform(child);
                child = GetParentBone(child);
            }
            while (allowPassInvalidbone && childBone==null && child!=HumanBodyBones.LastBone);

            if (childBone == null)
            {
                Debug.LogError("Failur to find child bone of chain : " + child);
                yield break;
            }

            Assert.IsTrue(childBone.IsChildOf(rootBone));

            var stack = new Stack<Transform>(4);

            while (rootBone != childBone)
            {
                stack.Push(childBone);
                childBone = childBone.parent;
            }

            yield return rootBone;

            while (stack.TryPop(out var bone))
                yield return bone;

            yield break;
        }

        public static HumanBodyBones GetParentBone(this HumanBodyBones bone)
        {
            return (HumanBodyBones)HumanTrait.GetParentBone((int)bone);
        }

        /// <summary>
        /// Return cloest transform under root which also above child.
        /// </summary>
        /// <param name="root">to</param>
        /// <param name="child">from</param>
        public static Transform GetCloestRootBone(this Animator animator, HumanBodyBones root, HumanBodyBones child)
        {
            var rootBone = (int)root;
            var curBone = (int)child;

            var boneStack = new Stack<int>(curBone - rootBone > 0 ? curBone - rootBone : 3);

            while (curBone > rootBone)
            {
                boneStack.Push(curBone);
                curBone = HumanTrait.GetParentBone(curBone);
            }

            while (boneStack.TryPop(out var index))
            {
                var tf = animator.GetBoneTransform((HumanBodyBones)index);

                if (tf)
                    return tf;
            }

            return null;
        }


        public static bool MirrorHumanBodyBone(this HumanBodyBones humanBodyBone, out HumanBodyBones mirrorBodyBone)
        {
            var sn = (int)humanBodyBone;
            if (sn > 24)
            {
                if (sn > 38)
                    sn -= 15;
                else
                    sn += 15;
            }
            else
            {
                if (sn.IsOdd())
                    sn++;
                else
                    sn--;
            }

            mirrorBodyBone = (HumanBodyBones)sn;

            switch (humanBodyBone)
            {
                case HumanBodyBones.Hips:
                    break;
                case HumanBodyBones.Spine:
                    break;
                case HumanBodyBones.Chest:
                    break;
                case HumanBodyBones.UpperChest:
                    break;
                case HumanBodyBones.Neck:
                    break;
                case HumanBodyBones.Head:
                    break;
                case HumanBodyBones.Jaw:
                    break;
                case HumanBodyBones.LastBone:
                    break;

                default:
                    return true;
            }

            return false;
        }

        public static Dictionary<HumanBodyBones, Transform> HumanBoneMap(this Animator animator, params HumanBodyBones[] bones)
        {
            if (animator is null)
            {
                throw new ArgumentNullException(nameof(animator));
            }

            if (bones == null || bones.Length == 0)
                bones = HumanBodyBonesUtility.AllHumanBodyBones;

            var length = bones.Length;
            var dic = new Dictionary<HumanBodyBones, Transform>(length);

            for (int i = 0; i < length; i++)
            {
                var tf = animator.GetBoneTransform(bones[i]);
                if (!tf)
                    continue;

                dic.SafeAdd(bones[i], tf);
            }

            return dic;
        }

        public static Transform[] GetAllHumanoidBones(this Animator animator)
        {
            return GetHumanoidBones(animator, HumanBodyBonesUtility.AllHumanBodyBones);
        }


        public static Transform[] GetHumanoidBones(this Animator animator, HumanBodyBones[] bones, bool RequirePair = false)
        {
            var length = bones.Length;

            List<Transform> tfs = new(length);

            if (RequirePair)
            {
                for (int i = 0; i < length; i += 2)
                {
                    Transform a = animator.GetBoneTransform(bones[i]);
                    Transform b = animator.GetBoneTransform(bones[i + 1]);

                    if (a && b)
                        tfs.AddRange(new Transform[] { a, b });
                }
            }
            else
            {
                foreach (var flag in bones)
                {
                    Transform tf = null;
                    try
                    {
                        if (flag != HumanBodyBones.LastBone)
                            tf = animator.GetBoneTransform(flag);
                    }
                    catch (Exception e)
                    {
                        Debug.Log(flag);
                        throw e;
                    }

                    if (tf)
                        tfs.Add(tf);
                }

            }

            return tfs.ToArray();
        }

        

        ///// <summary>
        ///// Return all child humanBodyBones as transform.
        ///// </summary>
        //public static ReadOnlyTransform[] GetChildBones(this Animator animator,HumanBodyBones root)
        //{
            
        //}

        //public static HumanBodyBones[] GetChilds(this HumanBodyBones bone)
        //{
            
        //}




    }
}