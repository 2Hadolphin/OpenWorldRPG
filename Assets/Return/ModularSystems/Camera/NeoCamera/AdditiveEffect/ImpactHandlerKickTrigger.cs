using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-impacthandlerkicktrigger.html")]
    public class ImpactHandlerKickTrigger : MonoBehaviour//, IImpactHandler
    {
        [SerializeField, Tooltip("The additive kicker used to react to the force")]
        private AdditiveKicker m_Kicker = null;

        [SerializeField, Tooltip("The strength cap. Above this strength, the kick will not increase")]
        private float m_MaxStrength = 75f;

        [SerializeField, Tooltip("The time taken to recover from the kick")]
        private float m_KickDuration = 0.25f;

        [SerializeField, Tooltip("The camera moves along the direction of the force by strength * position multiplier")]
        private float m_PositionKickMultiplier = 0.005f;

        [SerializeField, Tooltip("The rotation kick angle at full strength")]
        private float m_RotationKickMaxAngle = 15f;

        private Transform m_LocalTransform = null;

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (m_Kicker == null)
                m_Kicker = GetComponent<AdditiveKicker>();
            // Clamp values
            m_MaxStrength = Mathf.Clamp(m_MaxStrength, 1f, 100f);
            m_KickDuration = Mathf.Clamp(m_KickDuration, 0f, 10f);
            m_PositionKickMultiplier = Mathf.Clamp(m_PositionKickMultiplier, 0f, 0.1f);
            m_RotationKickMaxAngle = Mathf.Clamp(m_RotationKickMaxAngle, 0f, 45f);
        }
#endif

        protected void Awake()
        {
            m_LocalTransform = transform;
        }

        public void HandlePointImpact(Vector3 position, Vector3 force)
        {
            if (m_Kicker == null)
                return;
                
            // Get force in local space
            force = Quaternion.Inverse(m_LocalTransform.rotation) * force;

            // Clamp the strength
            float strength = force.magnitude;
            if (strength > m_MaxStrength)
            {
                force *= m_MaxStrength / strength;
                strength = m_MaxStrength;
            }

            // Kick position
            if (m_PositionKickMultiplier > Mathf.Epsilon)
                m_Kicker.KickPosition(force * m_PositionKickMultiplier, m_KickDuration);

            // Kick rotation
            if (m_RotationKickMaxAngle > Mathf.Epsilon)
            {
                float angle = Mathf.Lerp(0f, m_RotationKickMaxAngle, strength / m_MaxStrength);
                force.y = force.x;
                force.x = force.z;
                force.z = -force.y;
                force.y = 0f;
                force.Normalize();
                m_Kicker.KickRotation(Quaternion.AngleAxis(angle, force), m_KickDuration);
            }
        }
    }
}
