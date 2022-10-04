using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return.Humanoid.Animation
{
    [CreateAssetMenu(fileName = "HandIKBehaviour", menuName = "MyData/Animation/Bundle/Humanoid_HandIK")]
    public class BehaviourBundle_Humanoid_HandIK : AnimationBehaviourBundle
    {
        public const string Path = "HandIKBehaviour";
        public override void LoadBehaviour(IAdvanceAnimator animator, out IAnimationBehaviour playableBehaviour)
        {
            var behaviour = new Behaviour_HandIK(this);
            behaviour.Init(animator);
            playableBehaviour = behaviour;
        }

        public HandPose[] Poses;

        [Serializable]
        public struct HandPose
        {
            public string Name;
            public HumanBodyBones PoseRootBone;
            /// <summary>
            ///  ratio of human bone distance
            /// width => upchest to upperArm 
            /// height => hip to upChest
            /// z => upperArm to hand
            /// </summary>
            public Vector3 Offset;
            public Vector3 Rotation;
            public KeyValuePair<string,PR>Binding(Vector3 dof)
            {
                var newOffset = Offset.Multiply(dof);

                return new KeyValuePair<string, PR>(Name, new PR() {Position=newOffset,Rotation=Quaternion.Euler(Rotation) });
            }

            /// <summary>
            /// return parameter of IK Bounce
            /// </summary>
            public static Vector3 BindingHandPoseBounce(IAdvanceAnimator animator)
            {
                var newOffset = Vector3.zero;

                var armLength = (
                        animator.GetSkeletonBone(HumanBodyBones.RightLowerArm).position.magnitude +
                        animator.GetSkeletonBone(HumanBodyBones.RightHand).position.magnitude
                    //anim.GetSkeletonBone(HumanBodyBones.RightToes).position
                    );

                var bodyWidth = (
                        animator.GetSkeletonBone(HumanBodyBones.RightShoulder).position.magnitude +
                        animator.GetSkeletonBone(HumanBodyBones.RightUpperArm).position.magnitude
                    );

                var upChestHeight = (
                        animator.GetSkeletonBone(HumanBodyBones.Hips).position.magnitude +
                        animator.GetSkeletonBone(HumanBodyBones.Spine).position.magnitude +
                        animator.GetSkeletonBone(HumanBodyBones.Chest).position.magnitude +
                        animator.GetSkeletonBone(HumanBodyBones.UpperChest).position.magnitude
                  //anim.GetSkeletonBone(HumanBodyBones.RightLowerArm).position +
                  //anim.GetSkeletonBone(HumanBodyBones.RightHand).position
                  );

                newOffset.z = armLength;


                newOffset.x = bodyWidth + armLength;

                newOffset.y =upChestHeight+armLength;


                /*
                var hips = anim.GetSkeletonBone(HumanBodyBones.Hips).position;
                var upperArm = anim.GetSkeletonBone(HumanBodyBones.RightUpperArm).position;

                newOffset.width= Vector3.Project(hips - upperArm, Vector3.right).magnitude;

                var upperChest = anim.GetSkeletonBone(HumanBodyBones.UpperChest).position;
                newOffset.height= Vector3.Project(upperChest - upperArm, Vector3.up).magnitude;

                var hand = anim.GetSkeletonBone(HumanBodyBones.RightHand).position;
                newOffset.z = Vector3.Project(upperArm - hand, Vector3.right).magnitude;
                */
                return newOffset;
            }

        }
    }

}
