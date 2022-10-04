using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Cameras;
using Sirenix.OdinInspector;
using DG.Tweening;

namespace Return.Items
{
    public class FirearmsAimAmplifier:BaseAimAmplifier
    {
        [SerializeField]
        protected bool enablePositionOffset = true;
        [SerializeField]
        protected bool enableRotationOffset = true;
        
        #region Aimer

        protected IAimer aimer;

        public virtual void Init(IAimer _aimer)
        {
            aimer = _aimer;
            lastYaw = aimer.yaw;
            lastPitch = aimer.pitch;
        }


        /// <summary>
        /// Return aim rotation gap in local horizontal(axis-height).
        /// </summary>
        public float GetHorizontalGap()
        {
            // Get horizontal angle diff (delta gap)
            //Vector3 yawForward = Aimer ? Aimer.forward : aimer.heading;
            //float hDiff = Vector3.SignedAngle(lastYaw, yawForward, Aimer ? Aimer.up : aimer.yawUp);
            var yaw = aimer.yaw;
            float hDiff = yaw - lastYaw;
            lastYaw = yaw;

            return hDiff.WrapAngle();
        }

        /// <summary>
        /// Return aim rotation gap in local horizontal(axis-width).
        /// </summary>
        public float GetVerticalGap()
        {
            // Get vertical angle diff (delta gap)
            float pitch = aimer.pitch;
            float vDiff = pitch - lastPitch;
            lastPitch = pitch;

            return vDiff.WrapAngle();
        }

        #endregion


        #region IAdditiveTransform

        public override Vector3 position => enablePositionOffset ? base.position : Vector3.zero;
        public override Quaternion rotation => enableRotationOffset ? base.rotation : Quaternion.identity;

        public override void UpdateTransform()
        {
            // Interpolate user strength
            m_CurrentUserStrength = Mathf.Lerp(m_CurrentUserStrength, m_TargetUserStrength, ConstCache.deltaTime * 5f);

            //if (m_CurrentUserStrength < 0.001f)
            //    return;

            // reduce gap via damp time
            yawGap =
                 Mathf.SmoothDamp(
                    yawGap,
                    0,
                    ref yawDelta,
                    data.DampingTime
                );

            pitchGap =
                 Mathf.SmoothDamp(
                    pitchGap,
                    0,
                    ref pitchDelta,
                    data.DampingTime
                );

            switch (AdditiveType)
            {
                case AdditiveType.View:
                    CalculateView();
                    break;

                case AdditiveType.Item:
                    CalculateItemView();
                    break;
            }
        }

        #endregion


        #region Main

        public override void CalculateView()
        {
            var deltaTime = ConstCache.deltaTime;

            var invert = 1;

            var hDiff = GetHorizontalGap();
            var vDiff = GetVerticalGap();

            // store gap
            yawGap = deltaTime.Lerp(yawGap, (yawGap + hDiff).Clamp(data.HorizontalClamp));
            pitchGap = deltaTime.Lerp(pitchGap, (pitchGap + vDiff).Clamp(data.VerticalClamp));

            // remap via ratio
            var hRatio = (yawGap / data.HorizontalClamp).Clamp();
            var vRatio = (pitchGap / data.VerticalClamp).Clamp();

            // trace 

            m_position =
                new Vector3(
                    data.HorizontalDisplacement * hRatio * m_CurrentUserStrength * invert,
                    data.VerticalDisplacement * vRatio * m_CurrentUserStrength * invert * -1,
                    0
                );


            var rot = data.HorizontalRotation.Multiply(
                 vRatio * m_CurrentUserStrength * -1,
                  hRatio * m_CurrentUserStrength,
                  hRatio * m_CurrentUserStrength
                );

            m_rotation = Quaternion.Euler(rot);
        }

        public override void CalculateItemView()
        {
            base.CalculateItemView();
        }

        #endregion


        protected virtual void OnEnable()
        {
            if (aimer.IsNull())
                enabled = false;
        }


    }
}