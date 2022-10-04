using Return.Animations.IK;
using Return.Editors;
using System;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using Return.Animations;

namespace Return.Items
{
    /// <summary>
    /// StreamTransformHandle space which calculate offsetPosition then offsetRotation, final solve ik if required
    /// </summary>
    public class AdditiveHandleSpace : PostureAdjustHandle
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="handle">ReadOnlyTransform of hand.</param>
        /// <param name="item"></param>
        /// <param name="animator"></param>
        /// <returns></returns>
        public override IAnimationStreamHandler CreateAdjustJob(PR offset, Transform handle, AbstractItem item, Animator animator)
        {
            var handler = animator.BindStreamTransform(handle);

            var isPos = offset != Vector3.zero;
            var isRot = offset != Quaternion.identity;

            IAnimationStreamHandler adjustJob;


            if (isPos && !isRot)
            {
                adjustJob = new StreamAdditivePositionHandle
                {
                    Position = offset
                };

            }
            else if (!isPos && isRot)
            {
                adjustJob = new StreamAdditiveRotationHandle
                {
                    Rotation = offset
                };
            }
            else if (isPos && isRot)
            {
                adjustJob = new StreamAdditiveTransformHandle()
                {
                    Position = offset,
                    Rotation = offset
                };
            }
            else
            {
                Debug.LogError(new NotImplementedException("Offset is zero"));
                return null;
            }

            adjustJob.Handle = handler;

            // insert ik m_duringTransit ? or unity batch at lateupdate
            //TwoBoneIKAnimationJob.Create(anim, handle);
            return adjustJob;
        }

#if UNITY_EDITOR

        public override void DrawPostureWizer(ItemPostureAdjustWizer wizer, HandleConfig config,Transform handle, string handleName)
        {
            //var style = new GUIStyle(GUI.skin.label);
            //style.normal.
            //var tooltip = new GUIContent("AdditiveHandle", "Set additive offset on StreamTransformHandle space.");
            //UnityEditor.EditorGUILayout.LabelField(tooltip);
            ItemPostureAdjustWizer.DrawHandle(config, handle, handleName);
        }

#endif
    }
}

