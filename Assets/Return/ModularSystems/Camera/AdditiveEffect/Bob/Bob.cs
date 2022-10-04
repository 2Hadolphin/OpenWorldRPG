using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Return.Cameras
{
    public abstract class Bob : BaseComponent, IAdditiveTransform
    {


        [SerializeField, Tooltip("The bob animation data, shared between the head and the item/weapon")]
        protected PositionBobData m_BobData = null;
        public PositionBobData BobData
        {
            get => m_BobData;
            set => m_BobData = value;
        }

        [SerializeField, Tooltip("Is this bob being applied to the head or the item (allows the effect to blend between the 2 with similar results based on game settings).")]
        protected AdditiveType m_BobType = AdditiveType.View;

        /// <summary>
        /// Weight to apply additvie. **ref to movement
        /// </summary>
        [ShowInInspector]
        protected float weight = 1f;
        float targetWeight=1f;

        [SerializeField]
        [Obsolete]
        protected float LerpRate = 0.2f;

        [ShowInInspector]
        protected Vector3 m_Position = Vector3.zero;

        public Func<float> velocityGetter;
        protected float stepCount;

        [ShowInInspector]
        public virtual float Increase { get; set; } = 1f;

        #region IAdditiveTransform

        public Vector3 position
        {
            get { return m_Position; }
        }

        public Quaternion rotation
        {
            get { return Quaternion.identity; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public virtual void UpdateTransform()
        {
            var vel=CaculateWeight();
            ApplyAdditive(vel);
        }

        #endregion

        #region Calculate

        /// <summary>
        /// Calculate weight and return step speed.
        /// </summary>
        protected virtual float CaculateWeight()
        {
            var vel = velocityGetter();

            // clamp 0.5~2
            if (vel < m_MinLerpSpeed && weight > 0)
            {
                vel = 0;
                targetWeight = 0;
                stepCount = m_BobData.WrapStep(stepCount);
            }
            else if (weight < 1)
            {
                targetWeight = 1;

                if (vel > m_MaxLerpSpeed)
                {
                    vel = m_MaxLerpSpeed;
                }
            }

            return vel;
        }

        /// <summary>
        /// Calculate additive result.
        /// </summary>
        protected virtual void ApplyAdditive(float velocity)
        {
            if (weight > 0.0001f)
            {
                stepCount += velocity * ConstCache.deltaTime;

                // Get the bob amount
                Vector2 bob = m_BobData.GetBobPositionAtTime(stepCount);

                // invert if move via item
                bob = bob.Multiply(weight * Increase* ((m_BobType == AdditiveType.Item)?-1f:1f));

                m_Position = bob;
            }
            else
            {
                m_Position = Vector3.zero;
            }

            //if (MotionSpeed < m_MaxLerpSpeed)
            //{
            //    float lerp = (LerpRate - m_MinLerpSpeed) / (m_MaxLerpSpeed - m_MinLerpSpeed);
            //    m_Position *= lerp;
            //}
        }

        #endregion


        #region Clamp

        [SerializeField, Range(0f, 5f), Tooltip("At or below this speed the bob will be scaled to zero.")]
        protected float m_MinLerpSpeed = 0.5f;

        [SerializeField, Range(0.25f, 10f), Tooltip("At or above this speed the bob will have its full effect.")]
        protected float m_MaxLerpSpeed = 2f;

        protected const float k_FadeLerp = 0.05f;


        protected void FixedUpdate()
        {
            weight = Mathf.Lerp(weight, targetWeight, k_FadeLerp);
        }


#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            if (m_MaxLerpSpeed < m_MinLerpSpeed + 0.1f)
                m_MaxLerpSpeed = m_MinLerpSpeed + 0.1f;
        }
#endif

        #endregion

    }

    public enum AdditiveType
    {
        /// <summary>
        /// Apply additive effect to camera which inspect item.
        /// </summary>
        View,

        /// <summary>
        /// Apply additive effect to item.
        /// </summary>
        Item
    }
}