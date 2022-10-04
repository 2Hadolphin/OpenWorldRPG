using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Animations;
using Return.Animations;
using UnityEngine.Assertions;

namespace Return.Items
{
    [Serializable]
    [HideLabel]
    public class HandleConfig : NullCheck
    {
        public PostureAdjustHandle AdjustMode;

        //[Sirenix.OdinInspector.ReadOnly]
        //public PostureSpace HandleSpace;
        //public PostureAdjustMode AdjustMode = PostureAdjustMode.AdditiveHandleSpace;

        public virtual IAnimationStreamHandler GetJob(Transform handle,AbstractItem item,Animator animator)
        {
            Assert.IsNotNull(handle);
            Assert.IsNotNull(item);
            Assert.IsNotNull(animator);

            return AdjustMode.CreateAdjustJob(this,handle, item, animator);
        }

        [Tooltip("Is offset additive on base stream")]
        public bool isAdditive = true;

        public Vector3 OffsetPosition;
        public Vector3 OffsetRotation;

        public static implicit operator PR (HandleConfig config)
        {
            return new(config.OffsetPosition, config.OffsetRotation.ToEuler());
        }
    }

    public class HandConfig : HandleConfig
    {
        public HumanBodyBones Bone;
    }
}

