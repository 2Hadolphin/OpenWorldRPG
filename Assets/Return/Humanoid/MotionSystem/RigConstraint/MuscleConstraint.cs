using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Animations.Rigging;

namespace Return.Humanoid.Animation
{
    public class MuscleConstraint : RigConstraint<MuscleClampJob, MuscleClampData, MuscleClampBinder>
    {

    }

    public struct MuscleClampJob : IWeightedAnimationJob
    {
        public FloatProperty jobWeight { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public void ProcessAnimation(AnimationStream stream)
        {

        }

        public void ProcessRootMotion(AnimationStream stream)
        {

        }
    }

    public struct MuscleClampData : IAnimationJobData
    {
        public MuscleData[] Datas;
        public bool IsValid()
        {
            return Datas != null && Datas.Length > 0;
        }

        public void SetDefaultValues()
        {
            Datas = new MuscleData[0];
        }
    }

    public class MuscleClampBinder : AnimationJobBinder<MuscleClampJob, MuscleClampData>
    {

        public MuscleClampBinder() { }
        public override MuscleClampJob Create(Animator animator, ref MuscleClampData data, Component component)
        {

            return new MuscleClampJob();
        }

        public override void Destroy(MuscleClampJob job)
        {
            
        }
    }


}
