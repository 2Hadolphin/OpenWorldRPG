using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using System;
using Return.Humanoid.Animation;

namespace Return.Humanoid.IK
{
    struct LimbPart
    {
        public Vector3 localPosition;    // local position of this limb relative to body position
        public Vector3 goalPosition;
        public float goalWeight;
        public float goalPullWeight;
        public float maximumExtension; // maximum extension of the limb which define when the pull solver start to pull on the body (spring rest lenght)
        public float stiffness;        // stiffness of the limb, at 0 the limb is loosen, at 1 the limb is really stiff
    }


    public interface IIKGoalData
    {
        public AvatarIKGoal AvatarIKGoal { get; }
        public Transform GoalSpaceTransform { get; }
        public Vector3 GoalPosition { get; }
        public Quaternion GoalRotation { get; }
        public bool InputViaLocalGoal { get; set; }
        public bool InputViaLocalRotation { get; set; }
        public bool HasTarget { get; }
    }

    public interface ILimbIKGoalData : IIKGoalData
    {
        public TransformStreamHandle LimbRootHandle { get; }
        public float PullWeight { get; }
        public float MaximumExtension { get; }
        public float GoalWeight { get; }

        public void ZeroGoal(Animator animator);
        public void Solve(ref AnimationHumanStream humanStream);
        void Release(ref AnimationHumanStream humanStream);
    }

    [Serializable]
    public class IKGoal: IIKGoalData
    {
        public IKGoal(IAdvanceAnimator animator,Transform RootSpaceTransform, AvatarIKGoal avatarIKGoal)
        {
            GoalSpaceTransform = RootSpaceTransform;
            AvatarIKGoal = avatarIKGoal;

            AvatarIKHint = (AvatarIKHint)(int)avatarIKGoal;

            HumanBodyBones bodyBone;
            switch (avatarIKGoal)
            {
                case AvatarIKGoal.LeftFoot:
                    bodyBone = HumanBodyBones.LeftFoot;
                    break;
                case AvatarIKGoal.RightFoot:
                    bodyBone = HumanBodyBones.RightFoot;
                    break;
                case AvatarIKGoal.LeftHand:
                    bodyBone = HumanBodyBones.LeftHand;
                    break;
                case AvatarIKGoal.RightHand:
                    bodyBone = HumanBodyBones.RightHand;
                    break;
                default:
                throw new InvalidProgramException();
            }
            Target = animator.GetAnimator.GetBoneTransform(bodyBone);
        }


        public readonly AvatarIKGoal AvatarIKGoal;
        public readonly AvatarIKHint AvatarIKHint;
        /// <summary>
        /// IKGoalRootSpaceTransform 
        /// </summary>
        public Transform GoalSpaceTransform { get; protected set; }
        public Transform Target { get; protected set; }
        public float GoalWeight { get; set; }
        public float HintWeight { get; set; }
        public bool InputViaLocalGoal { get; set; }
        public bool InputViaLocalHint { get; set; }
        /// <summary>
        /// Whether set the rotation in local space
        /// </summary>
        public bool InputViaLocalRotation { get; set; }
        protected Vector3 Position;
        protected Vector3 Hint;

        public bool HasTarget { get; set; }
        /// <summary>
        /// Always get global space value 
        /// </summary>
        public Vector3 GoalPosition
        {
            get { return Position; }
            set { Position = InputViaLocalGoal?GoalSpaceTransform.TransformPoint(value):value; }
        }

        protected Quaternion Rotation;
        public Quaternion GoalRotation
        {
            get { return Rotation; }
            set { Rotation = InputViaLocalRotation ? GoalSpaceTransform.rotation*Quaternion.Inverse(value) : value; }
        }

        public Vector3 HintPosition
        {
            get { return Hint; }
            set { Hint = InputViaLocalHint ? GoalSpaceTransform.TransformPoint(value) : value; }
        }

        AvatarIKGoal IIKGoalData.AvatarIKGoal => AvatarIKGoal;
    }

    [Serializable]
    public class LimbIKGoal : IKGoal, ILimbIKGoalData
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="transform"> IK Goal RootTransform Space ReadOnlyTransform</param>
        public LimbIKGoal(IAdvanceAnimator animator,Transform transform,AvatarIKGoal avatarIKGoal) : base(animator,transform,avatarIKGoal) 
        {
            switch (avatarIKGoal)
            {
                case AvatarIKGoal.LeftFoot:
                    LimbRootHandle = animator.GetBindingBone(HumanBodyBones.LeftUpperLeg);
                    TargetHandle = animator.GetBindingBone(HumanBodyBones.LeftFoot);
                    break;
                case AvatarIKGoal.RightFoot:
                    LimbRootHandle = animator.GetBindingBone(HumanBodyBones.RightUpperLeg);
                    TargetHandle = animator.GetBindingBone(HumanBodyBones.RightFoot);
                    break;
                case AvatarIKGoal.LeftHand:
                    LimbRootHandle = animator.GetBindingBone(HumanBodyBones.LeftUpperArm);
                    TargetHandle = animator.GetBindingBone(HumanBodyBones.LeftHand);
                    break;
                case AvatarIKGoal.RightHand:
                    LimbRootHandle = animator.GetBindingBone(HumanBodyBones.RightUpperArm);
                    TargetHandle = animator.GetBindingBone(HumanBodyBones.RightHand);
                    break;
            }

        }

        public TransformStreamHandle LimbRootHandle { get; protected set; }
        public TransformStreamHandle TargetHandle { get; protected set; }
        #region BodyEffector
        public float PullWeight { get; set; }
        public float MaximumExtension { get; set; }
        #endregion

        public void ZeroGoal(Animator animator)
        {
            var stream = new AnimationStream();
            animator.OpenAnimationStream(ref stream);
            TargetHandle.GetGlobalTR(stream,out Position, out Rotation);
            animator.CloseAnimationStream(ref stream);
        }

        public void Solve(ref AnimationHumanStream humanStream)
        {
            var avatarGoal = AvatarIKGoal;
            humanStream.SetGoalPosition(avatarGoal, Position);
            humanStream.SetGoalRotation(avatarGoal, Rotation);
            humanStream.SetGoalWeightPosition(avatarGoal, GoalWeight);
            humanStream.SetGoalWeightRotation(avatarGoal, GoalWeight);

            var avatarHint = AvatarIKHint;
            humanStream.SetHintPosition(avatarHint, Hint);
            humanStream.SetHintWeightPosition(avatarHint,HintWeight);
        }

        public void Release(ref AnimationHumanStream humanStream)
        {
            var avatarGoal = AvatarIKGoal;
            humanStream.SetGoalPosition(avatarGoal, Position);
            humanStream.SetGoalRotation(avatarGoal, Rotation);
            humanStream.SetGoalWeightPosition(avatarGoal, GoalWeight);
            humanStream.SetGoalWeightRotation(avatarGoal, GoalWeight);
        }

    }

}

