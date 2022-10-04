using Return.Creature;
using UnityEngine;

namespace Return.Humanoid
{
    public static partial class mHumanExtension
    {
        /// <summary>
        /// Get human limb root bone (upward)
        /// </summary>
        public static HumanBodyBones ParseRootBone(this Limb limb)
        {
            return limb switch
            {
                Limb.Body=>HumanBodyBones.Spine,
                Limb.Chest=>HumanBodyBones.UpperChest,
                Limb.RightHand=>HumanBodyBones.RightUpperArm,
                Limb.LeftHand => HumanBodyBones.LeftUpperArm,
                Limb.RightLeg => HumanBodyBones.RightUpperLeg,
                Limb.LeftLeg => HumanBodyBones.LeftUpperLeg,
                Limb.RightFinger=>HumanBodyBones.RightHand,
                Limb.LeftFinger => HumanBodyBones.LeftHand,

                _ => HumanBodyBones.Hips,
            };
        }

        public static Limb ParseLimb(this HumanBodyBones humanBodyBones)
        {
            return humanBodyBones switch
            {
                HumanBodyBones.Hips => Limb.Body,
                HumanBodyBones.Spine => Limb.Body,

                HumanBodyBones.Chest => Limb.Chest,
                HumanBodyBones.UpperChest => Limb.Chest,

                HumanBodyBones.Neck => Limb.Head,
                HumanBodyBones.Head => Limb.Head,
                HumanBodyBones.Jaw => Limb.Head,
                HumanBodyBones.LeftEye => Limb.Head,
                HumanBodyBones.RightEye => Limb.Head,

                HumanBodyBones.RightUpperArm => Limb.RightHand,
                HumanBodyBones.RightLowerArm => Limb.RightHand,

                HumanBodyBones.LeftUpperArm => Limb.LeftHand,
                HumanBodyBones.LeftLowerArm => Limb.LeftHand,

                HumanBodyBones.RightUpperLeg => Limb.RightLeg,
                HumanBodyBones.RightLowerLeg => Limb.RightLeg,
                HumanBodyBones.RightFoot => Limb.RightLeg,
                HumanBodyBones.RightToes => Limb.RightLeg,

                HumanBodyBones.LeftUpperLeg => Limb.LeftLeg,
                HumanBodyBones.LeftLowerLeg => Limb.LeftLeg,
                HumanBodyBones.LeftFoot => Limb.LeftLeg,
                HumanBodyBones.LeftToes => Limb.LeftLeg,

                _ => humanBodyBones.ParseFinger(),
            };
        }

        public static Limb ParseFinger(this HumanBodyBones humanBodyBones)
        {
            if ((int)humanBodyBones > (int)HumanBodyBones.LeftRingDistal)
                return Limb.RightFinger;
            else
                return Limb.LeftFinger;
        }

        public static int BoneWeightIndexArray(this BoneWeight boneWeight,ref int[] indexArray)
        {
            var num = 0;

            if (boneWeight.boneIndex0!=default)
            {
                indexArray[num] = boneWeight.boneIndex0;
                num++;
            }

            if (boneWeight.boneIndex1 != default)
            {
                indexArray[num] = boneWeight.boneIndex0;
                num++;
            }

            if (boneWeight.boneIndex2 != default)
            {
                indexArray[num] = boneWeight.boneIndex0;
                num++;
            }

            if (boneWeight.boneIndex3 != default)
            {
                indexArray[num] = boneWeight.boneIndex0;
                num++;
            }

            return num;
        }
    }

}