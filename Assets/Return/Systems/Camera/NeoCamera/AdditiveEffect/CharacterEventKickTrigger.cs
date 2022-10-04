using UnityEngine;
using NeoFPS.CharacterMotion;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-charactereventkicktrigger.html")]
    public class CharacterEventKickTrigger : MonoBehaviour
    {
        [SerializeField, Tooltip("The additive kicker used to react to the force")]
        private AdditiveKicker m_Kicker = null;
        
        [SerializeField, Range(0.1f, 1f), Tooltip("The time taken to recover from the kick")]
        private float m_KickDuration = 0.5f;

        [SerializeField, Range(0f, 1f), Tooltip("The downward position kick distance at max impulse")]
        private float m_MaxKickDistance = 0.05f;

        [SerializeField, Range(5f, 30f), Tooltip("The forward kick angle at max impulse")]
        private float m_MaxKickAngle = 15f;
        
        [Header ("Ground Impacts")]

        [SerializeField, Tooltip("A ground impact impulse with magnitude lower than this will be ignored")]
        private float m_MinGroundImpact = 7.5f;

        [SerializeField, Tooltip("The ground impact impulse magnitude that gives the maximum kick")]
        private float m_MaxGroundImpact = 30f;
        
        [Header ("View Impacts")]
        
        [SerializeField, Tooltip("A head impact impulse with magnitude lower than this will be ignored")]
        private float m_MinHeadImpact = 2.5f;

        [SerializeField, Tooltip("The head impact impulse magnitude that gives the maximum kick")]
        private float m_MaxHeadImpact = 12.5f;
        
        [Header ("Body Impacts")]
        
        [SerializeField, Tooltip("A body impact impulse with magnitude lower than this will be ignored")]
        private float m_MinBodyImpact = 15f;

        [SerializeField, Tooltip("The body impact impulse magnitude that gives the maximum kick")]
        private float m_MaxBodyImpact = 30f;

        private Transform m_RootTransform = null;

#if UNITY_EDITOR
        protected void OnValidate()
        {
            if (m_Kicker == null)
                m_Kicker = GetComponent<AdditiveKicker>();

            // Clamp values
            m_MinGroundImpact = Mathf.Clamp(m_MinGroundImpact, 1f, 100f);
            m_MaxGroundImpact = Mathf.Clamp(m_MaxGroundImpact, 1f, 100f);
            m_MinHeadImpact = Mathf.Clamp(m_MinHeadImpact, 1f, 100f);
            m_MaxHeadImpact = Mathf.Clamp(m_MaxHeadImpact, 1f, 100f);
            m_MinBodyImpact = Mathf.Clamp(m_MinBodyImpact, 1f, 100f);
            m_MaxBodyImpact = Mathf.Clamp(m_MaxBodyImpact, 1f, 100f);
        }
#endif

        protected void Start()
        {
            //MotionController m = GetComponentInParent<MotionController>();
            //if (m != null)
            //{
            //    m_RootTransform = m.transform;
            //    m.onGroundImpact += OnGroundImpact;
            //    m.onBodyImpact += OnBodyImpact;
            //    m.onHeadImpact += OnHeadImpact;
            //}
        }

        public void OnGroundImpact (Vector3 impulse)
        {
            if (m_Kicker == null)
                return;

            float mag = impulse.sqrMagnitude;
            if (mag > m_MinGroundImpact * m_MinGroundImpact)
            {
                // Calculate lerp
                float lerp = Mathf.Clamp01((Mathf.Sqrt(mag) - m_MinGroundImpact) / (m_MaxGroundImpact - m_MinGroundImpact));

                // Kick
                if (m_MaxKickDistance > Mathf.Epsilon)
                    m_Kicker.KickPosition(Vector3.down * lerp * m_MaxKickDistance, m_KickDuration);
                m_Kicker.KickRotation(Quaternion.AngleAxis (Mathf.Lerp(0f, m_MaxKickAngle, lerp), Vector3.right), m_KickDuration);
            }
        }

        public void OnHeadImpact (Vector3 impulse)
        {
            if (m_Kicker == null)
                return;

            float mag = impulse.sqrMagnitude;
            if (mag > m_MinHeadImpact * m_MinHeadImpact)
            {
                float lerp = Mathf.Clamp01((Mathf.Sqrt(mag) - m_MinHeadImpact) / (m_MaxHeadImpact - m_MinHeadImpact));

                // Convert to local space
                impulse = m_RootTransform.InverseTransformVector(impulse);

                // Kick position
                if (m_MaxKickDistance > Mathf.Epsilon)
                    m_Kicker.KickPosition(impulse.normalized * lerp * m_MaxKickDistance, m_KickDuration);

                // Kick rotation
                if (impulse.y < -0.9f)
                {
                    // Use a random angle
                    Vector3 axis = Quaternion.AngleAxis (UnityEngine.Random.Range (0f, 360f), Vector3.up) * Vector3.forward;
                    m_Kicker.KickRotation(Quaternion.AngleAxis (m_MaxKickAngle * lerp, axis), m_KickDuration);
                }
                else
                {
                    // Get an axis from the impulse
                    impulse.y = impulse.x;
                    impulse.x = impulse.z;
                    impulse.z = -impulse.y;
                    impulse.y = 0f;
                    impulse.Normalize();
                    m_Kicker.KickRotation(Quaternion.AngleAxis (m_MaxKickAngle * lerp, impulse), m_KickDuration);
                }
            }
        }

        public void OnBodyImpact (Vector3 impulse)
        {
            if (m_Kicker == null)
                return;

            float mag = impulse.sqrMagnitude;
            if (mag > m_MinBodyImpact * m_MinBodyImpact)
            {
                float lerp = Mathf.Clamp01((Mathf.Sqrt(mag) - m_MinBodyImpact) / (m_MaxBodyImpact - m_MinBodyImpact));

                // Convert to local space
                impulse = m_RootTransform.InverseTransformVector(impulse);

                // Kick position
                if (m_MaxKickDistance > Mathf.Epsilon)
                    m_Kicker.KickPosition(impulse.normalized * lerp * -m_MaxKickDistance, m_KickDuration);

                // Kick rotation
                impulse.y = impulse.x;
                impulse.x = -impulse.z;
                impulse.z = impulse.y;
                impulse.y = 0f;
                impulse.Normalize();
                m_Kicker.KickRotation(Quaternion.AngleAxis (m_MaxKickAngle * lerp, impulse), m_KickDuration);
            }
        }
    }
}
