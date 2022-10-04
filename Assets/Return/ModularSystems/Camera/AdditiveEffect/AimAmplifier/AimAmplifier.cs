using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

namespace Return.Cameras
{
    public class AimAmplifier :BaseAimAmplifier
    {
        [SerializeField]
        Transform Aimer;

        private IAdditiveTransformHandler m_Handler = null;
        private IAimController m_AimController = null;

        [SerializeField]
        protected bool enablePositionOffset=true;
        [SerializeField]
        protected bool enableRotationOffset=true;

        public override Vector3 position => enablePositionOffset?base.position:Vector3.zero;
        public override Quaternion rotation => enableRotationOffset? base.rotation:Quaternion.identity;

        protected void OnEnable()
        {
            if (TryGetComponent(out m_Handler))
                m_Handler.ApplyAdditiveEffect(this);
            else
                return;

            if (Aimer.NotNull())
            {
                if (!m_SkipReset)
                {
                    // Calculate forward
                    lastYaw = Aimer.localEulerAngles.y;//Aimer.forward;
                    lastPitch = Aimer.localEulerAngles.x;
                }
                else
                    m_SkipReset = false;
            }
            else if(GetComponentInParent<IAimController>().NotNull(out m_AimController))
            {
                if (!m_SkipReset)
                {
                    // Calculate forward
                    lastYaw = 0;//aimer.heading;
                    lastPitch = m_AimController.pitch;
                }
                else
                    m_SkipReset = false;
            }
        }

        protected void OnDisable()
        {
            if (m_Handler != null)
                m_Handler.RemoveAdditiveEffect(this);
        }

        [Button]
        void TestRot(Quaternion rot,float duration=1f,bool additive=true)
        {
            if(additive)
                rot = rot*transform.parent.localRotation;

            transform.parent.DOLocalRotateQuaternion(rot, duration);
        }

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

        /// <summary>
        /// Return aim rotation gap in local horizontal(axis-height).
        /// </summary>
        public float GetHorizontalGap()
        {
            // Get horizontal angle diff (delta gap)
            //Vector3 yawForward = Aimer ? Aimer.forward : aimer.heading;
            //float hDiff = Vector3.SignedAngle(lastYaw, yawForward, Aimer ? Aimer.up : aimer.yawUp);
            var yaw= Aimer.localEulerAngles.y;
            float hDiff = yaw- lastYaw;
            lastYaw = yaw;

            return hDiff.WrapAngle();
        }

        /// <summary>
        /// Return aim rotation gap in local horizontal(axis-width).
        /// </summary>
        public float GetVerticalGap()
        {
            // Get vertical angle diff (delta gap)
            float pitch = Aimer ? Aimer.localEulerAngles.x : m_AimController.pitch;
            float vDiff = pitch - lastPitch;
            lastPitch = pitch;

            return vDiff.WrapAngle();
        }
    }

}