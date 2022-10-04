using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using Unity.Collections;
using UnityEngine.Playables;
using UnityEditor;

namespace Return.Humanoid.Animation
{
    public struct Job_Humanoid_Mirror :  IAnimationJob
    {
        public static AnimationScriptPlayable Create(PlayableGraph graph, AnimationClip clip)
        {
            var mirrorJob = new Job_Humanoid_Mirror();

            var playable=AnimationScriptPlayable.Create<Job_Humanoid_Mirror>(graph, mirrorJob);
            mirrorJob.Source = AnimationClipPlayable.Create(graph, clip);
            playable.AddInput(mirrorJob.Source, 0, 1);
            return playable;
        }
        /// <summary>
        /// Enable mirror bones by  AvatarMask
        /// </summary>
        private Job_Humanoid_Mirror(AvatarMask mask)
        {
            throw new Exception();
            var length = (int)AvatarMaskBodyPart.LastBodyPart;
            for (int i = 0; i < length; i++)
            {
                var part = (AvatarMaskBodyPart)i;
                if (!mask.GetHumanoidBodyPartActive(part))
                    continue;

                switch (part)
                {
                    case AvatarMaskBodyPart.Root:
                        break;
                    case AvatarMaskBodyPart.Body:
                        break;
                    case AvatarMaskBodyPart.Head:
                        break;
                    case AvatarMaskBodyPart.LeftLeg:
                        break;
                    case AvatarMaskBodyPart.RightLeg:
                        break;
                    case AvatarMaskBodyPart.LeftArm:
                        break;
                    case AvatarMaskBodyPart.RightArm:
                        break;
                    case AvatarMaskBodyPart.LeftFingers:
                        break;
                    case AvatarMaskBodyPart.RightFingers:
                        break;
                    case AvatarMaskBodyPart.LeftFootIK:
                        break;
                    case AvatarMaskBodyPart.RightFootIK:
                        break;
                    case AvatarMaskBodyPart.LeftHandIK:
                        break;
                    case AvatarMaskBodyPart.RightHandIK:
                        break;
                }

            }

            MirrorBones = new NativeArray<TransformStreamHandle>();
            MirrorPairBones = new NativeArray<HandlePair>();
        }
        public struct HandlePair
        {
            public static HandlePair[] Pairs(TransformStreamHandle[] handles)
            {
                var length = handles.Length;

                List<HandlePair> pairs = new List<HandlePair>();
                for (int i = 0; i < length; i += 2)
                {
                    pairs.Add(new HandlePair() { A = handles[i], B = handles[i + 1] });
                }

                return pairs.ToArray();
            }
            public TransformStreamHandle A;
            public TransformStreamHandle B;
        }

        public NativeArray<HandlePair> MirrorPairBones;
        public NativeArray<TransformStreamHandle> MirrorBones;
        public AnimationClipPlayable Source;

        public static Vector3 Mirrored(Vector3 value)
        {
            return new Vector3(-value.x, value.y, value.z);
        }


        public static Quaternion Mirrored(Quaternion value)
        {
            return Quaternion.Euler(value.eulerAngles.x, -value.eulerAngles.y, -value.eulerAngles.z);
        }

        public float Weight;
        public void ProcessRootMotion(AnimationStream stream)
        {

            var angularVelocity = stream.angularVelocity;
            angularVelocity.y *= -1;
            stream.angularVelocity = angularVelocity;

            var velocity = stream.velocity;
            velocity.x *= -1;
            stream.velocity = velocity;

            return;
            var humanStream = stream.AsHuman();
            // mirror position (invert X coordinate)
            var rootMotionPos = humanStream.bodyLocalPosition;
            humanStream.bodyLocalPosition = new Vector3(-rootMotionPos.x, rootMotionPos.y, rootMotionPos.z);

            // mirror rotation (invert Y axis angle)
            var rootMotionRot = humanStream.bodyLocalRotation.eulerAngles;
            humanStream.bodyLocalRotation = Quaternion.Euler(rootMotionRot.x, -rootMotionRot.y, rootMotionRot.z);
        }
        public void ProcessAnimation(AnimationStream stream)
        {

            var humanstream = stream.AsHuman();

            /*
            #region DrawRoot
            mHumanoidUtility.DrawRoot(humanstream, AvatarIKGoal.RightLeg);
            mHumanoidUtility.DrawRoot(humanstream, AvatarIKGoal.LeftLeg);
            mHumanoidUtility.DrawRoot(humanstream, AvatarIKGoal.RightHand);
            mHumanoidUtility.DrawRoot(humanstream, AvatarIKGoal.LeftHand);
            #endregion
            */
            {
                var length = MirrorBones.Length;
                for (int i = 0; i < length; i++)
                {
                    var bone = MirrorBones[i];

                    bone.GetLocalTRS(stream, out var posA, out var rotA, out var sclA);

                    posA.x *= -1;
                    rotA.ReflectRotation(Vector3.right);

                    bone.SetLocalTRS(stream, posA, rotA, sclA, false);
                }
            }


            {
                var length = MirrorPairBones.Length;
                for (int i = 0; i < length; i++)
                {
                    var pair = MirrorPairBones[i];

                    pair.A.GetLocalTRS(stream, out var posA, out var rotA, out var sclA);
                    pair.B.GetLocalTRS(stream, out var posB, out var rotB, out var sclB);

                    posA.x *= -1;
                    posB.x *= -1;

                    rotA.ReflectRotation(Vector3.right);
                    rotB.ReflectRotation(Vector3.right);

                    pair.A.SetLocalTRS(stream, posB, rotB, sclB, false);
                    pair.B.SetLocalTRS(stream, posA, rotA, sclA, false);

                }
            }




            var leftPos = humanstream.GetGoalLocalPosition(AvatarIKGoal.LeftFoot);
            var rightPos = humanstream.GetGoalLocalPosition(AvatarIKGoal.RightFoot);


            //rightPos.width *= -1;
            //leftPos.width *= -1;

            humanstream.SetGoalLocalPosition(AvatarIKGoal.RightFoot, rightPos);
            humanstream.SetGoalLocalPosition(AvatarIKGoal.LeftFoot, leftPos);
       

            var leftRot = humanstream.GetGoalRotation(AvatarIKGoal.LeftFoot);
            var rightRot = humanstream.GetGoalRotation(AvatarIKGoal.RightFoot);
            

            leftRot.ReflectRotation(Vector3.right);
            rightRot.ReflectRotation(Vector3.right);

            humanstream.SetGoalRotation(AvatarIKGoal.RightFoot, rightRot);
            humanstream.SetGoalRotation(AvatarIKGoal.LeftFoot, leftRot);

            HumanoidAnimationUtility.DrawCoordinate(humanstream, AvatarIKGoal.RightFoot);
            HumanoidAnimationUtility.DrawCoordinate(humanstream, AvatarIKGoal.LeftFoot);

            humanstream.SetGoalLocalPosition(AvatarIKGoal.RightFoot, Vector3.zero);
            humanstream.SetGoalLocalRotation(AvatarIKGoal.RightFoot, Quaternion.identity);

            return;

            humanstream.SetGoalWeightPosition(AvatarIKGoal.RightFoot, 1);
            humanstream.SetGoalWeightRotation(AvatarIKGoal.RightFoot, 1);
            humanstream.SetGoalWeightPosition(AvatarIKGoal.LeftFoot, 1);
            humanstream.SetGoalWeightRotation(AvatarIKGoal.LeftFoot, 1);

            humanstream.SetHintWeightPosition(AvatarIKHint.RightKnee, 0);
            humanstream.SetHintWeightPosition(AvatarIKHint.LeftKnee, 0);

            var rightKneePos = humanstream.GetHintPosition(AvatarIKHint.RightKnee);
            var leftKneePos = humanstream.GetHintPosition(AvatarIKHint.LeftKnee);

            
            

            humanstream.SetHintPosition(AvatarIKHint.RightKnee, leftKneePos);
            humanstream.SetHintPosition(AvatarIKHint.LeftKnee, rightKneePos);
            return;

            //Dof Test
            {
                for (int i = 0; i < (int)ArmDof.LastArmDof; i++)
                {
                    var muscle = new MuscleHandle(HumanPartDof.RightArm, (ArmDof)i);
                    var value = humanstream.GetMuscle(muscle);
                    value *= -1;
                    humanstream.SetMuscle(muscle, value);
                }

                for (int i = 0; i < (int)LegDof.LastLegDof; i++)
                {
                    var muscle = new MuscleHandle(HumanPartDof.RightLeg, (LegDof)i);
                    var value = humanstream.GetMuscle(muscle);
                    value *= -1;
                    humanstream.SetMuscle(muscle, value);
                }

                //Body
                {
                    var dofs = bodyDofs;
                    var length = dofs.Length;
                    for (int i = 0; i < length; i++)
                    {
                        FlippingMuscle(humanstream, new MuscleHandle(dofs[i]));
                    }
                }

                //Body
                {
                    var headDof = headDofs;
                    var length = headDof.Length;
                    for (int i = 0; i < length; i++)
                    {
                        FlippingMuscle(humanstream, new MuscleHandle(headDof[i]));
                    }
                }



                //Arm
                {
                    var length = (int)ArmDof.LastArmDof;
                    for (int i = 0; i < length; i++)
                    {
                        var dof = (ArmDof)i;
                        var muscleR = new MuscleHandle(HumanPartDof.RightArm, dof);
                        var muscleL = new MuscleHandle(HumanPartDof.LeftArm, dof);

                        SwitchMuscle(humanstream, muscleR, muscleL);
                    }
                }


                //Leg
                {
                    var dofs = LegDofs;
                    var length = dofs.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var dof = dofs[i];
                        var muscleR = new MuscleHandle(HumanPartDof.RightLeg, dof);
                        var muscleL = new MuscleHandle(HumanPartDof.LeftLeg, dof);

                        SwitchMuscle(humanstream, muscleR, muscleL);
                    }
                }

                //finger
                {
                    var fingers = FingerDofs;
                    var length = fingers.Length;

                    for (int i = 0; i < length; i++)
                    {
                        var partDof = fingers[i];

                        for (int k = 0; k < length; k++)
                        {
                            var muscleR = new MuscleHandle(partDof, (FingerDof)k);
                            var muscleL = new MuscleHandle(partDof, (FingerDof)k);

                            SwitchMuscle(humanstream, muscleR, muscleL);
                        }
                    }
                }

            }



        }


        void SwitchMuscle(AnimationHumanStream stream, MuscleHandle right, MuscleHandle left)
        {
            var tempCatch = stream.GetMuscle(right);
            stream.SetMuscle(right, stream.GetMuscle(left));
            stream.SetMuscle(left, tempCatch);
        }

        void FlippingMuscle(AnimationHumanStream stream, MuscleHandle muscleHandle)
        {
            var dof = stream.GetMuscle(muscleHandle);
            stream.SetMuscle(muscleHandle, 0);
        }


        BodyDof[] bodyDofs
        {
            get
            {
                //return (BodyDof[])System.Enum.GetValues(typeof(BodyDof));

                return new BodyDof[]
                {
                BodyDof.SpineLeftRight,
                BodyDof.SpineRollLeftRight,
                BodyDof.ChestLeftRight,
                BodyDof.ChestRollLeftRight,
                BodyDof.UpperChestLeftRight,
                BodyDof.UpperChestRollLeftRight,
                };

            }
        }

        HeadDof[] headDofs
        {
            get
            {
                return new HeadDof[]
                {
                HeadDof.NeckLeftRight,
                HeadDof.NeckRollLeftRight,
                HeadDof.HeadLeftRight,
                HeadDof.HeadRollLeftRight,
                HeadDof.LeftEyeInOut,
                HeadDof.RightEyeInOut,
                HeadDof.JawLeftRight,
                 };
            }
        }

        LegDof[] LegDofs
        {
            get
            {
                return new LegDof[]
                {

                LegDof.UpperLegFrontBack,
                LegDof.UpperLegInOut,
                LegDof.UpperLegRollInOut,
                LegDof.LegCloseOpen,
                LegDof.LegRollInOut,
                LegDof.FootCloseOpen,
                LegDof.FootInOut,
                LegDof.ToesUpDown,
                };
            }
        }
        HumanPartDof[] FingerDofs
        {
            get
            {
                return new HumanPartDof[]
                {
                HumanPartDof.LeftThumb,
                HumanPartDof.LeftIndex,
                HumanPartDof.LeftMiddle,
                HumanPartDof.LeftRing,
                HumanPartDof.LeftLittle,
                HumanPartDof.RightThumb,
                HumanPartDof.RightIndex,
                HumanPartDof.RightMiddle,
                HumanPartDof.RightRing,
                HumanPartDof.RightLittle,
                };
            }
        }



    }
}