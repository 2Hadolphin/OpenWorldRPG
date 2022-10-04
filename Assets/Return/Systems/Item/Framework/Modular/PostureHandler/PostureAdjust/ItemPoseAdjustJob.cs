using UnityEngine.Animations;
using Unity.Collections;
using Return.Animations;
using UnityEngine.Assertions;
using System;
using UnityEngine;

namespace Return.Items
{
    public struct ItemPoseAdjustJob : IAnimationJob
    {
        public IAnimationStreamHandler[] HandleJobs;

        /// <summary>
        /// m_items handle.??
        /// </summary>
        [Obsolete]
        public NativeArray<StreamAdditivePositionHandle> AdditivePositions;

        /// <summary>
        /// Skeleton handle.
        /// </summary>
        public NativeArray<StreamAdditiveRotationHandle> AdditiveRotations;

        public void ProcessAnimation(AnimationStream stream)
        {
            

#if UNITY_EDITOR
            if(!stream.isValid)
            {
                Debug.LogError(stream + " is not valid.");
                return;
            }
#endif

            var length = HandleJobs.Length;

            for (int i = 0; i < length; i++)
            {
                var job = HandleJobs[i];
                if (job.IsNull())
                    continue;
                try
                {
                    //Debug.Log(m_duringTransit==null);

                    continue;

                    job.ProcessStream(ref stream);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            //foreach (var m_duringTransit in AdditivePositions)
            //    m_duringTransit.ProcessPosture(ref stream);

            return;

            if (AdditiveRotations.IsCreated)
            {
                length = AdditiveRotations.Length;
                for (int i = 0; i < length; i++)
                {
                    try
                    {
                        Debug.Log(AdditiveRotations[i]);
                        continue;
                        AdditiveRotations[i].ProcessStream(ref stream);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
                Debug.LogError(AdditiveRotations);
        }

        public void ProcessRootMotion(AnimationStream stream)
        {

        }
    }
}

