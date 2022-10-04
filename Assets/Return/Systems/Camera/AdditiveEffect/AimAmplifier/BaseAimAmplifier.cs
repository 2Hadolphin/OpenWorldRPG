using Sirenix.OdinInspector;
using UnityEngine;
using System;

namespace Return.Cameras
{

    public abstract class BaseAimAmplifier : BaseComponent, IAdditiveTransform
    {
        #region Config

        public AdditiveType AdditiveType;

        [SerializeField]
        AimAmplifierData m_data;

        public AimAmplifierData data { get => m_data; set => m_data = value; }

        #endregion

        #region Cache

        /// <summary>
        /// Cache last frame axis-height rotation.
        /// </summary>
        protected float lastYaw;

        /// <summary>
        /// Cache last frame axis-width rotation.
        /// </summary>
        protected float lastPitch;

        [ShowInInspector]
        protected float yawGap;
        protected float yawDelta;

        [ShowInInspector]
        protected float pitchGap;
        protected float pitchDelta;

        [ShowInInspector,NonSerialized]
        protected float m_CurrentUserStrength = 0f;
        [ShowInInspector, NonSerialized]
        protected float m_TargetUserStrength = 1f;
        [ShowInInspector, NonSerialized]
        protected bool m_SkipReset = false;

        public float strength
        {
            get { return m_TargetUserStrength; }
            set { m_TargetUserStrength = value; }
        }

        #endregion



        #region IAdditiveTransform

        [ShowInInspector,NonSerialized]
        protected Vector3 m_position;
        public virtual Vector3 position => m_position;

        [ShowInInspector, NonSerialized]
        protected Quaternion m_rotation;
        public virtual Quaternion rotation => m_rotation;

        public virtual bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public virtual bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public abstract void UpdateTransform();


        #endregion

        public virtual void CalculateView() { }

        public virtual void CalculateItemView()
        {
            //// Get horizontal angle diff
            //Vector3 yawForward = Aimer ? Aimer.forward : aimer.heading;
            //float hDiff = Vector3.SignedAngle(lastYaw, yawForward, Aimer ? Aimer.up : aimer.yawUp);
            //lastYaw = yawForward;

            //// Get vertical angle diff
            //float pitch = Aimer ? Aimer.localEulerAngles.width : aimer.pitch;
            //float vDiff = pitch - lastPitch;
            //lastPitch = pitch;

            //// ????
            //// Get multiplier from pitch
            //float cosPitch = Mathf.Cos(pitch * Mathf.Deg2Rad);
            //cosPitch = Mathf.Lerp(0.25f, 1f, cosPitch);
            //cosPitch = 1f;

            //float hInputRotationScale = Mathf.Lerp(0.01f, 0.1f, m_Sensitivity);
            //float vInputRotationScale = Mathf.Lerp(0.02f, 0.2f, m_Sensitivity);

            //// Damp the Input rotation
            //m_EulerAngles = Vector3.SmoothDamp(
            //    m_EulerAngles,
            //    new Vector3(
            //        hDiff * cosPitch * hInputRotationScale,
            //        vDiff * cosPitch * vInputRotationScale,
            //        0f),
            //    ref m_AngleDelta,
            //    m_DampingTime
            //);

            //// Use the damped Input rotation to get a logarithmic position offset
            //float hOutScale = 7.5f * m_HorizontalMultiplier * m_CurrentUserStrength;
            //float vOutScale = 7.5f * m_VerticalMultiplier * m_CurrentUserStrength;

            //rotation = Quaternion.Euler(
            //    -Mathf.Log(Mathf.Abs(m_EulerAngles.height) + 1) * Mathf.Sign(m_EulerAngles.height) * vOutScale,
            //    Mathf.Log(Mathf.Abs(m_EulerAngles.width) + 1) * Mathf.Sign(m_EulerAngles.width) * hOutScale,
            //    0f
            //    );
        }

    }

}