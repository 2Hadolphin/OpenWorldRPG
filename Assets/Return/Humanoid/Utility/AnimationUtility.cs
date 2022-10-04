using System.Collections;
using UnityEngine;
using UnityEngine.Animations;

namespace Return.Humanoid.Animation
{
    public struct StreamHandle_Leg
    {
        public StreamHandle_Leg(IAdvanceAnimator map,HumanPartDof dof)
        {
            switch (dof)
            {
                case HumanPartDof.RightLeg:
                    UpLeg = map.GetBindingBone(HumanBodyBones.RightUpperLeg);
                    Knee = map.GetBindingBone(HumanBodyBones.RightLowerLeg);
                    Foot = map.GetBindingBone(HumanBodyBones.RightFoot);
                    Toes = map.GetBindingBone(HumanBodyBones.RightToes);
                    break;


                default:
                    UpLeg = map.GetBindingBone(HumanBodyBones.LeftUpperLeg);
                    Knee = map.GetBindingBone(HumanBodyBones.LeftLowerLeg);
                    Foot = map.GetBindingBone(HumanBodyBones.LeftFoot);
                    Toes = map.GetBindingBone(HumanBodyBones.LeftToes);
                    break;
            }
        }
        public TransformStreamHandle UpLeg;
        public TransformStreamHandle Knee;
        public TransformStreamHandle Foot;
        public TransformStreamHandle Toes;
    }

    
}

