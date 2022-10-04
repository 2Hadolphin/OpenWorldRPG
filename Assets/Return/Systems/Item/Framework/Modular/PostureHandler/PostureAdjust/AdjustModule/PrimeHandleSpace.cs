using Return.Editors;
using System;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;
using Return;
using Return.Animations;

namespace Return.Items
{
    /// <summary>
    /// StreamTransformHandle space which calculate offsetPosition then offsetRotation, final solve ik if required
    /// </summary>
    public class PrimeHandleSpace : PostureAdjustHandle
    {
        public override IAnimationStreamHandler CreateAdjustJob(PR offset, Transform handle, AbstractItem item, Animator animator)
        {
            var handler = animator.BindStreamTransform(handle);

            offset = PR.Default;

            var itemHandle = item.transform.parent;

            if (itemHandle.IsNull())
                itemHandle = item.transform;

            var adjustJob = new StreamAdditiveAlignHandle()
            {
                AlignHandle = handler,
                ItemHandle = animator.BindStreamTransform(itemHandle),
                Position = offset,
                Rotation = offset,
            };

            // insert ik m_duringTransit ? or unity batch at lateupdate
            //TwoBoneIKAnimationJob.Create(anim, handle);
            return adjustJob;
        }

#if UNITY_EDITOR

        public override void DrawPostureWizer(ItemPostureAdjustWizer wizer, HandleConfig config, Transform handle, string handleName)
        {
            ItemPostureAdjustWizer.DrawHandle(config,handle, handleName);
        }
#endif
    }
}

