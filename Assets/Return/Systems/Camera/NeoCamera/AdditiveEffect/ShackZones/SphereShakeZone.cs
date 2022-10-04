using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ShakeZones
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-sphereshakezone.html")]
    public class SphereShakeZone : MonoBehaviour, IShakeZone
    {
        [SerializeField, Tooltip("The strength of the shake.")]
        private float m_Strength = 1f;

        [SerializeField, Tooltip("The inner radius of the sphere. Inside this, the strength is constant.")]
        private float m_InnerRadius = 5f;

        [SerializeField, Tooltip("The distance outside of the inner sphere that the strength falls off to zero.")]
        private float m_FalloffDistance = 5f;

        private Transform m_LocalTransform = null;

        private float m_Multiplier = 1f;
        public float multiplier
        {
            get { return m_Multiplier; }
            set { m_Multiplier = Mathf.Clamp01(value); }
        }

        protected void OnValidate()
        {
            m_Strength = Mathf.Clamp(m_Strength, 0f, 10f);
        }

        protected void Awake()
        {
            m_LocalTransform = transform;
        }

        protected void OnEnable()
        {
            ShakeHandler.AddShaker(this);
        }

        protected void OnDisable()
        {
            ShakeHandler.RemoveShaker(this);
        }

        public float GetStrengthAtPosition(Vector3 position)
        {
            // Get the outer radius squared
            float outerRadiusSquared = m_InnerRadius + m_FalloffDistance;
            outerRadiusSquared *= outerRadiusSquared;

            // CheckAdd if position overlaps shake radius
            float sqrDistance = (m_LocalTransform.position - position).sqrMagnitude;
            if (sqrDistance < outerRadiusSquared)
            {
                // CheckAdd if inside inner radius
                float innerRadiusSquared = m_InnerRadius * m_InnerRadius;
                if (sqrDistance <= innerRadiusSquared)
                    return m_Strength * m_Multiplier;
                else
                {
                    // Linear falloff
                    float alpha = 1f - ((Mathf.Sqrt(sqrDistance) - m_InnerRadius) / m_FalloffDistance);
                    return m_Strength * m_Multiplier * alpha;
                }
            }

            return 0f;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, m_InnerRadius);
            if (m_FalloffDistance > 0f)
            {
                Gizmos.color = Color.cyan * 0.5f;
                Gizmos.DrawWireSphere(transform.position, m_InnerRadius + m_FalloffDistance);
            }
            Gizmos.color = Color.white;
        }
#endif
    }
}