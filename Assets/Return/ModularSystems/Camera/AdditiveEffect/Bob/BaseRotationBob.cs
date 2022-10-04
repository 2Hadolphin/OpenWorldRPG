using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace Return.Cameras
{
    public abstract partial class BaseRotationBob : BaseComponent, IAdditiveTransform 
    {
        //private static readonly NeoSerializationKey k_FadeKey = new NeoSerializationKey("fade");
        protected const float k_FadeDuration = 1f;

        public RotationBobData BobData;

        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        protected float m_MinLerpSpeed = 0.5f;

        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        protected float m_MaxLerpSpeed = 2f;


        [SerializeField]
        protected Vector3 m_Euler = Vector3.zero;

        protected float m_FadeTime = 0f;
        [ShowInInspector]
        protected float m_CurrentStrength = 0f;
        protected float m_TargetStrength = 1f;

        #region IAdditiveTransform

        public Quaternion rotation
        {
            get { return Quaternion.Euler(m_Euler * m_CurrentStrength); }
        }

        public Vector3 position
        {
            get { return Vector3.zero; }
        }

        public float strength
        {
            get { return m_TargetStrength; }
            set { m_TargetStrength = value; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return false; }
        }

        public virtual void UpdateTransform()
        {
            float speed = velocityGetter();

            if (speed < m_MinLerpSpeed)
                FadeOut();
            else
            {
                if (speed > m_MaxLerpSpeed)
                    speed = m_MaxLerpSpeed;

                StepCounter += speed * ConstCache.deltaTime;

                m_Euler = BobData.CalculateBob(StepCounter);

                //if (speed < m_MaxLerpSpeed)
                //{
                //    float lerp = (m_Controller.smoothedStepRate - m_MinLerpSpeed) / (m_MaxLerpSpeed - m_MinLerpSpeed);
                //    m_Euler *= lerp;
                //}

                m_FadeTime = 0f;
            }
        }

        #endregion

        #region Main

        [ShowInInspector, NonSerialized]
        protected float StepCounter;

        public Func<float> velocityGetter;

        void FadeOut()
        {
            m_FadeTime += ConstCache.deltaTime;
            if (m_FadeTime > k_FadeDuration)
                m_Euler = Vector3.zero;
            else
                m_Euler *= 1f - ConstCache.deltaTime;
        }

        #endregion


#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (m_MaxLerpSpeed < m_MinLerpSpeed + 0.1f)
                m_MaxLerpSpeed = m_MinLerpSpeed + 0.1f;
        }
#endif

    }


}