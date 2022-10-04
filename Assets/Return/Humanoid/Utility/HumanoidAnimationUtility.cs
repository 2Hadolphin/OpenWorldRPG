using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Animations;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.Animations.Rigging;
using RiggingRig = UnityEngine.Animations.Rigging.Rig;

namespace Return
{
    public static class HumanoidAnimationUtility
    {
        #region Rigging

        public static RiggingRig BuildRightHandRig(Animator animator, PR offset, out TwoBoneIKConstraint rightHand, bool destory = false)
        {
            return BuildTwoBoneIKRig(
                animator,
                offset,
                out rightHand,
                destory,
                HumanBodyBones.RightUpperArm,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.RightHand
                );
        }
        public static RiggingRig BuildLeftHandRig(Animator animator, PR offset, out TwoBoneIKConstraint rightHand, bool destory = false)
        {
            return BuildTwoBoneIKRig(
                animator,
                offset,
                out rightHand,
                destory,
                HumanBodyBones.LeftUpperArm,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LeftHand
                );
        }
        public static RiggingRig BuildRightLegRig(Animator animator, PR offset, out TwoBoneIKConstraint rightHand, bool destory = false)
        {
            return BuildTwoBoneIKRig(
                animator,
                offset,
                out rightHand,
                destory,
                HumanBodyBones.RightUpperLeg,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.RightFoot
                );
        }
        public static RiggingRig BuildLeftLegRig(Animator animator, PR offset, out TwoBoneIKConstraint rightHand, bool destory = false)
        {
            return BuildTwoBoneIKRig(
                animator,
                offset,
                out rightHand,
                destory,
                HumanBodyBones.LeftUpperLeg,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.LeftFoot
                );
        }

        static RiggingRig BuildTwoBoneIKRig(Animator animator, PR offset, out TwoBoneIKConstraint rigging, bool destory = false, params HumanBodyBones[] bodyBones)
        {
            Assert.IsNotNull(animator);

            var builder = animator.gameObject.InstanceIfNull<RigBuilder>();

            var rig = new GameObject("Rig_"+bodyBones[2].ToString()).InstanceIfNull<RiggingRig>();
            builder.layers.Add(new RigLayer(rig, true));


            rigging = BuildTwoBoneIK(
                animator,
                rig,
                offset,
                bodyBones[0],
                bodyBones[1],
                bodyBones[2]
                );


            if (destory)
            {
                rig.gameObject.hideFlags = HideFlags.DontSave;
            }

            return rig;
        }
        public static TwoBoneIKConstraint BuildTwoBoneIK(Animator animator, RiggingRig rig, PR offset, params HumanBodyBones[] bones)
        {
            rig.transform.SetParent(animator.transform, false);

            var ikRig = new GameObject(bones[2].ToString()+"_TwoBoneIK").AddComponent<TwoBoneIKConstraint>();
            ikRig.transform.SetParent(rig.transform, false);
            ikRig.data.maintainTargetPositionOffset = true;
            ikRig.data.maintainTargetRotationOffset = true;

            var arm = animator.GetBoneTransform(bones[0]);
            var elbow = animator.GetBoneTransform(bones[1]);
            var hand = animator.GetBoneTransform(bones[2]);


            ikRig.data.root = arm;
            ikRig.data.mid = elbow;
            ikRig.data.tip = hand;

            var target = new GameObject("target").transform;
            target.SetParent(ikRig.transform, false);
            target.CopyViaOffset(hand, offset);
            ikRig.data.target = target;


            var hint = new GameObject("hint").transform;
            hint.SetParent(ikRig.transform, false);
            hint.Copy(elbow);
            ikRig.data.hint = hint;

            return ikRig;
        }

        #endregion

        #region TransformStreamHandle

        public static TransformStreamHandle[] GetStreamBones(Animator animator)
        {
            return GetStreamBones(animator, animator.GetAllHumanoidBones());
        }

        public static TransformStreamHandle[] GetStreamBones(Animator animator, Transform[] bones)
        {
            return bones.Select(x => animator.BindStreamTransform(x)).ToArray();
        }

        #endregion


        #region Muscle

        public static MuscleHandle[] GetArmMuscleHandles(HumanPartDof humanPartDof)
        {
            if (!humanPartDof.Equals(HumanPartDof.RightArm) &&
                !humanPartDof.Equals(HumanPartDof.LeftArm))
                return null;

            var count = (int)ArmDof.LastArmDof;
            List<MuscleHandle> handles = new(count);
            foreach (ArmDof dof in Enum.GetValues(typeof(ArmDof)))
                handles.Add(new MuscleHandle(humanPartDof, dof));

            return handles.ToArray();
        }

        public static MuscleHandle[] GetFingerMuscleHandles(params HumanPartDof[] fingers)
        {
            var count = (int)FingerDof.LastFingerDof;
            List<MuscleHandle> handles = new(count * fingers.Length);
            foreach (var finger in fingers)
                foreach (FingerDof dof in Enum.GetValues(typeof(FingerDof)))
                    handles.Add(new MuscleHandle(finger, dof));

            return handles.ToArray();
        }

        #endregion
        #region Definition


        #endregion

 


        #region Debug

        public static void DrawCoordinate(AnimationHumanStream humanstream, AvatarIKGoal avatarIKGoal)
        {
            var rotationG = humanstream.GetGoalRotation(avatarIKGoal);
            var positionG = humanstream.GetGoalPosition(avatarIKGoal);

            Debug.DrawRay(positionG, rotationG * Vector3.forward, Color.blue);
            Debug.DrawRay(positionG, rotationG * Vector3.right, Color.red);
            Debug.DrawRay(positionG, rotationG * Vector3.up, Color.green);
        }

        public static void DrawRoot(AnimationHumanStream humanstream, AvatarIKGoal avatarIKGoal)
        {
            var rotationG = humanstream.GetGoalRotation(avatarIKGoal);
            var rotationL = humanstream.GetGoalLocalRotation(avatarIKGoal);

            //var rootR = rotationG.SolveParent(rotationL);
            var rootR = rotationG * Quaternion.Inverse(rotationL);


            var positionG = humanstream.GetGoalPosition(avatarIKGoal);
            var positionL = humanstream.GetGoalLocalPosition(avatarIKGoal);

            var rootP = positionG - rootR * positionL;

            Debug.DrawRay(rootP, rootR * Vector3.forward, Color.blue);
            Debug.DrawRay(rootP, rootR * Vector3.right, Color.red);
            Debug.DrawRay(rootP, rootR * Vector3.up, Color.green);


        }

        #endregion
    }

}
