using UnityEngine.Animations;
using UnityEngine;
using UnityEngine.Playables;
using Unity.Collections;

namespace Return.Animations
{
    /// <summary>
    /// Animation mixer blend with TransformStreamHandles from A to B.
    /// </summary>
    public struct AnimationMixerJob : IAnimationJob//,IAnimationJobPlayable
    {
        public static AnimationMixerJob Create()
        {
            var job = new AnimationMixerJob()
            {
                StreamA = 0,
                StreamB = 1,
            };

            return job;
        }

        public int StreamA;
        public int StreamB;

        /// <summary>
        /// handle of bone in layer mask to apply animation
        /// </summary>
        public NativeArray<TransformStreamHandle> handles;
        public NativeArray<float> boneWeights;

        /// <summary>
        /// Weight to control root motion.
        /// </summary>
        public float rootMotionWeight;

        /// <summary>
        /// Weight to blend handle animation.
        /// </summary>
        public float weight;

        public void ProcessRootMotion(AnimationStream stream)
        {
            var stNum = stream.inputStreamCount;

            for (int i = 0; i < stNum; i++)
            {
                var st = stream.GetInputStream(i);
                if (st.isValid)
                {
                    // Do blend Tree & caculate motion
                }
            }

            var streamA = stream.GetInputStream(StreamA);
            var streamB = stream.GetInputStream(StreamB);

            var velocity = Vector3.Lerp(streamA.velocity, streamB.velocity, rootMotionWeight);
            var angularVelocity = Vector3.Lerp(streamA.angularVelocity, streamB.angularVelocity, rootMotionWeight);

            stream.velocity = velocity;
            stream.angularVelocity = angularVelocity;
        }

        public void ProcessAnimation(AnimationStream stream)
        {
#if UNITY_EDITOR
            if (!handles.IsCreated)
                return;
#endif

            var streamA = stream.GetInputStream(StreamA);
            var streamB = stream.GetInputStream(StreamB);

            //var go = stream.AsHuman();

            var numHandles = handles.Length;
            for (var i = 0; i < numHandles; ++i)
            {
                var handle = handles[i];

                if (!handle.IsValid(stream))
                    continue;

                var posA = handle.GetLocalPosition(streamA);
                var posB = handle.GetLocalPosition(streamB);
                handle.SetLocalPosition(stream, Vector3.Lerp(posA, posB, weight * boneWeights[i]));

                var rotA = handle.GetLocalRotation(streamA);
                var rotB = handle.GetLocalRotation(streamB);
                handle.SetLocalRotation(stream, Quaternion.Slerp(rotA, rotB, weight * boneWeights[i]));
            }
        }
    }

    public struct AnimationRootMotionRemoveJob : IAnimationJob
    {
        public void ProcessAnimation(AnimationStream stream)
        {

        }

        public void ProcessRootMotion(AnimationStream stream)
        {
            stream.velocity = Vector3.zero;
            stream.angularVelocity = Vector3.zero;
        }
    }
}