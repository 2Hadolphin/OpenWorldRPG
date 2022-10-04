using Return.Creature;
using UnityEngine;
using UnityEngine.Assertions;

namespace Return
{
    public static class mHumanExtension
    {
        #region Humanoid

        public static SkeletonBone ToSkeletonBone(this Transform transform)
        {
            return new SkeletonBone()
            {
                name = transform.name,
                position = transform.localPosition,
                rotation = transform.localRotation,
                scale = transform.localScale,
            };
        }

        #endregion

        public static Symmetry TakeMirror(this Symmetry actor)
        {
            switch (actor)
            {
                case Symmetry.Right:
                    return Symmetry.Left;
                case Symmetry.Left:
                    return Symmetry.Right;
            }
            Assert.IsNull<string>(null, "Should not be " + actor.ToString());
            return actor;
        }

        public static Symmetry ToSymmetry(this Side actor)
        {
            switch (actor)
            {
                case Side.Right:
                    return Symmetry.Right;
                case Side.Left:
                    return Symmetry.Left;

                default:
                    return Symmetry.Both;
            }
        }

        public static Limb ToLimb(this Symmetry actor)
        {
            switch (actor)
            {
                 case Symmetry.Right:
                    return Limb.RightHand;
                case Symmetry.Left:
                    return Limb.LeftHand;
                default:

                    return Limb.RightHand;
            }
        }


        public static string ToHandle(this Symmetry actor)
        {
            return actor switch
            {
                Symmetry.Right => Return.Humanoid.Interact.HumanoidHandleSlotConfig.str_RightHandHandleSlot,
                Symmetry.Left => Return.Humanoid.Interact.HumanoidHandleSlotConfig.str_LeftHandHandleSlot,
                _ => default,
            };
        }

    }

}
