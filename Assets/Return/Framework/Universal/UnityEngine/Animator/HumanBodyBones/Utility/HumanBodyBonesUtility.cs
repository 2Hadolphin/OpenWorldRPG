using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Return
{
    public static class HumanBodyBonesUtility
    {
        public static HumanBodyBones[] MirrorBones = new HumanBodyBones[]
        {
        HumanBodyBones.Hips,
        HumanBodyBones.Spine,
        HumanBodyBones.Chest,
        HumanBodyBones.UpperChest,
        HumanBodyBones.Neck,
        HumanBodyBones.Head,
        };

        public static HumanBodyBones[] MirrorBonePairs = new HumanBodyBones[]
        {
            #region Arm
            HumanBodyBones.RightShoulder,
            HumanBodyBones.LeftShoulder,

            HumanBodyBones.RightUpperArm,
            HumanBodyBones.LeftUpperArm,

            HumanBodyBones.RightLowerArm,
            HumanBodyBones.LeftLowerArm,

            HumanBodyBones.RightHand,
            HumanBodyBones.LeftHand,
            #endregion

            #region Fingers
            HumanBodyBones.RightThumbProximal,
            HumanBodyBones.LeftThumbProximal,
            HumanBodyBones.RightThumbIntermediate,
            HumanBodyBones.LeftThumbIntermediate,
            HumanBodyBones.RightThumbDistal,
            HumanBodyBones.LeftThumbDistal,

            HumanBodyBones.RightIndexProximal,
            HumanBodyBones.LeftIndexProximal,
            HumanBodyBones.RightIndexIntermediate,
            HumanBodyBones.LeftIndexIntermediate,
            HumanBodyBones.RightIndexDistal,
            HumanBodyBones.LeftIndexDistal,

            HumanBodyBones.RightMiddleProximal,
            HumanBodyBones.LeftMiddleProximal,
            HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.RightMiddleDistal,
            HumanBodyBones.LeftMiddleDistal,

            HumanBodyBones.RightRingProximal,
            HumanBodyBones.LeftRingProximal,
            HumanBodyBones.RightRingIntermediate,
            HumanBodyBones.LeftRingIntermediate,
            HumanBodyBones.RightRingDistal,
            HumanBodyBones.LeftRingDistal,

            HumanBodyBones.RightLittleProximal,
            HumanBodyBones.LeftLittleProximal,
            HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.LeftLittleIntermediate,
            HumanBodyBones.RightLittleDistal,
            HumanBodyBones.LeftLittleDistal,
            #endregion


            #region Leg
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.LeftUpperLeg,

            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.LeftLowerLeg,

            HumanBodyBones.RightFoot,
            HumanBodyBones.LeftFoot,

            HumanBodyBones.RightToes,
            HumanBodyBones.LeftToes,

                #endregion

        };

        #region Hand

        /// <summary>
        /// Right upperArm + lowerArm + hand + fingers
        /// </summary>
        public readonly static HumanBodyBones[] RightArmBones = new HumanBodyBones[]
        {
            #region Arm
            //HumanBodyBones.RightShoulder,

            HumanBodyBones.RightUpperArm,

            HumanBodyBones.RightLowerArm,

            HumanBodyBones.RightHand,
            #endregion

            #region Fingers
            HumanBodyBones.RightThumbProximal,
            HumanBodyBones.RightThumbIntermediate,
            HumanBodyBones.RightThumbDistal,

            HumanBodyBones.RightIndexProximal,
            HumanBodyBones.RightIndexIntermediate,
            HumanBodyBones.RightIndexDistal,

            HumanBodyBones.RightMiddleProximal,
            HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightMiddleDistal,

            HumanBodyBones.RightRingProximal,
            HumanBodyBones.RightRingIntermediate,
            HumanBodyBones.RightRingDistal,

            HumanBodyBones.RightLittleProximal,
            HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.RightLittleDistal,
            #endregion
        };

        /// <summary>
        /// Left upperArm + lowerArm + hand + fingers
        /// </summary>
        public readonly static HumanBodyBones[] LeftArmBones = new HumanBodyBones[]
        {
            #region Arm
            //HumanBodyBones.LeftShoulder,

            HumanBodyBones.LeftUpperArm,

            HumanBodyBones.LeftLowerArm,

            HumanBodyBones.LeftHand,
            #endregion

            #region Fingers
            HumanBodyBones.LeftThumbProximal,
            HumanBodyBones.LeftThumbIntermediate,
            HumanBodyBones.LeftThumbDistal,

            HumanBodyBones.LeftIndexProximal,
            HumanBodyBones.LeftIndexIntermediate,
            HumanBodyBones.LeftIndexDistal,

            HumanBodyBones.LeftMiddleProximal,
            HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.LeftMiddleDistal,

            HumanBodyBones.LeftRingProximal,
            HumanBodyBones.LeftRingIntermediate,
            HumanBodyBones.LeftRingDistal,

            HumanBodyBones.LeftLittleProximal,
            HumanBodyBones.LeftLittleIntermediate,
            HumanBodyBones.LeftLittleDistal,
            #endregion
        };

        /// <summary>
        /// 
        /// </summary>
        public static IEnumerable<HumanBodyBones> RightHandBones
        {
            get
            {
                yield return HumanBodyBones.RightHand;

                foreach (var finger in RightFingers)
                     yield return finger;
            }
        }

        public static IEnumerable<HumanBodyBones> LeftHandBones
        {
            get
            {
                yield return HumanBodyBones.LeftHand;

                foreach (var finger in LeftFingers)
                    yield return finger;
            }
        }

        public static IEnumerable<HumanBodyBones> AllFingers()
        {
            var leftFingers = LeftFingers;

            foreach (var finger in leftFingers)
                yield return finger;

            var rightFingers = RightFingers;

            foreach (var finger in rightFingers)
                yield return finger;
        }

        public static IEnumerable<HumanBodyBones> RightFingers = new[]
        {
            #region Fingers
            HumanBodyBones.RightThumbProximal,
            HumanBodyBones.RightThumbIntermediate,
            HumanBodyBones.RightThumbDistal,

            HumanBodyBones.RightIndexProximal,
            HumanBodyBones.RightIndexIntermediate,
            HumanBodyBones.RightIndexDistal,

            HumanBodyBones.RightMiddleProximal,
            HumanBodyBones.RightMiddleIntermediate,
            HumanBodyBones.RightMiddleDistal,

            HumanBodyBones.RightRingProximal,
            HumanBodyBones.RightRingIntermediate,
            HumanBodyBones.RightRingDistal,

            HumanBodyBones.RightLittleProximal,
            HumanBodyBones.RightLittleIntermediate,
            HumanBodyBones.RightLittleDistal,
            #endregion
        };

        public readonly static HumanBodyBones[] LeftFingers = new[]
        {
            #region Fingers
            HumanBodyBones.LeftThumbProximal,
            HumanBodyBones.LeftThumbIntermediate,
            HumanBodyBones.LeftThumbDistal,

            HumanBodyBones.LeftIndexProximal,
            HumanBodyBones.LeftIndexIntermediate,
            HumanBodyBones.LeftIndexDistal,

            HumanBodyBones.LeftMiddleProximal,
            HumanBodyBones.LeftMiddleIntermediate,
            HumanBodyBones.LeftMiddleDistal,

            HumanBodyBones.LeftRingProximal,
            HumanBodyBones.LeftRingIntermediate,
            HumanBodyBones.LeftRingDistal,

            HumanBodyBones.LeftLittleProximal,
            HumanBodyBones.LeftLittleIntermediate,
            HumanBodyBones.LeftLittleDistal,
            #endregion
        };

        #endregion

        #region Leg

        public readonly static IEnumerable<HumanBodyBones> LeftLegBones = new HumanBodyBones[]
        {
            HumanBodyBones.LeftUpperLeg,
            HumanBodyBones.LeftLowerLeg,
            HumanBodyBones.LeftFoot,
            HumanBodyBones.LeftToes,
        };

        public readonly static IEnumerable<HumanBodyBones> RightLegBones = new HumanBodyBones[]
        {
            HumanBodyBones.RightUpperLeg,
            HumanBodyBones.RightLowerLeg,
            HumanBodyBones.RightFoot,
            HumanBodyBones.RightToes,
        };



        #endregion

        public static HumanBodyBones[] AllHumanBodyBones
        {
            get
            {
                return ((HumanBodyBones[])Enum.GetValues(typeof(HumanBodyBones))).Where(x => !x.Equals(HumanBodyBones.LastBone)).ToArray();
            }
        }

        public static string[] AllHumanBodyBoneNames
        {
            get { return AllHumanBodyBones.Select(x => x.ToString()).ToArray(); }
        }

        public static string[] AllHumanBodyBoneNamesInTitlecaseLetter => AllHumanBodyBoneNames.Select(x => x.ToUpper()).ToArray();


        /// <summary>
        /// str_HumanBodyBone<->HumanBodyBones 
        /// </summary>
        public static Dictionary<string, HumanBodyBones> HumanNameMap
        {
            get
            {
                return AllHumanBodyBones.ToDictionary(x => x.ToString(), x => x);
            }
        }
    }
}