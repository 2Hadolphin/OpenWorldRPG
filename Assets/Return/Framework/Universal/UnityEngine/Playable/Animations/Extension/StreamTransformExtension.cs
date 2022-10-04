using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;


namespace Return
{
    public static class StreamTransformExtension
    {
        public static bool GetStreamHandle(this Animator animator, HumanBodyBones bone,out TransformStreamHandle handle)
        {
            var tf = animator.GetBoneTransform(bone);

            if (tf)
                handle = animator.BindStreamTransform(tf);
            else
                handle = default;

            return tf;
        }

        public static TransformStreamHandle GetStreamHandle(this Animator animator, HumanBodyBones bone)
        {
            var tf = animator.GetBoneTransform(bone);

            if (tf)
                return animator.BindStreamTransform(tf);
            else
                return default;
        }


        public static TransformStreamHandle[] GetHandles(this Animator animator,params Transform[] tfs)
        {
            var length = tfs.Length;

            var handles = new TransformStreamHandle[length];

            for (int i = 0; i < length; i++)
                handles[i] = animator.BindStreamTransform(tfs[i]);

            return handles;
        }


        public static TransformStreamHandle[] GetHandles(this Animator animator, params HumanBodyBones[] bones)
        {
            var length = bones.Length;

            var handles = new List<TransformStreamHandle>(length);

            for (int i = 0; i < length; i++)
            {
                var tf = animator.GetBoneTransform(bones[i]);

                if(tf)
                    handles.Add(animator.BindStreamTransform(tf));
            }

            return handles.ToArray();
        }

    }
}