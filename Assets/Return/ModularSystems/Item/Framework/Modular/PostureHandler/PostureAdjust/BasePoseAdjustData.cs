using UnityEngine;
using UnityEngine.Playables;

namespace Return.Items
{
    public abstract class BasePoseAdjustData : NullCheck
    {
        //public abstract Playable CreateStreamAdjustJob(PlayableGraph graph);
        //public abstract (HumanBodyBones, Vector3) PositionAdjust();
        //public abstract (HumanBodyBones, Quaternion) RotationAdjust();

        /// <summary>
        /// Create humanoid posture stream adjust m_duringTransit.
        /// </summary>
        public abstract IPlayableDisposable CreateAdjustJob(AbstractItem item,Animator animator, PlayableGraph graph, bool loadTwoBoneIK = false);
    }
}

