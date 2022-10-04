using Return;
using Return.Cameras;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-breathing.html")]
    public class BreathingEffect : MonoBehaviour, IAdditiveTransform
    {
        [SerializeField, Tooltip("The maximum pitch rotation at strength 1.")]
        private float m_PitchMax = 0.5f;
        [SerializeField, Tooltip("The maximum yaw rotation at strength 1.")]
        private float m_YawMax = 0.05f;
        [SerializeField, Tooltip("The maximum vertical offset at strength 1.")]
        private float m_VerticalMax = 0.002f;
        [SerializeField, Tooltip("The maximum horizontal offset at strength 1.")]
        private float m_HorizontalMax = 0.0005f;

        private const float k_PitchTimeOffset = -0.05f;
        private const float k_YawTimeOffset = 0.55f;
        private const float k_VerticalTimeOffset = 0.55f;
        private const float k_HorizontalTimeOffset = 0.45f;

        private IBreathHandler m_BreathHandler = null;
        private float m_CurrentStrength = 0f;
        private float m_TargetStrength = 1f;

        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }

        public Vector3 position
        {
            get;
            private set;
        }

        public Quaternion rotation
        {
            get;
            private set;
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

        protected void Awake()
        {
            transformHandler = GetComponent<IAdditiveTransformHandler>();
        }

        protected void OnEnable()
        {
            m_BreathHandler = GetComponentInParent<IBreathHandler>();
            if (m_BreathHandler != null)
                transformHandler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            if (m_BreathHandler != null)
                transformHandler.RemoveAdditiveEffect(this);
            m_BreathHandler = null;
        }

        public void UpdateTransform()
        {
            // Interpolate user strength
            m_CurrentStrength = Mathf.Lerp(m_CurrentStrength, m_TargetStrength, Time.deltaTime * 5f);

            float strength = m_BreathHandler.breathStrength * m_CurrentStrength;
            if (strength > 0.001f)
            {
                // Calculate rotation
                rotation = Quaternion.Euler(
                    m_PitchMax * strength * m_BreathHandler.GetBreathCycle(k_PitchTimeOffset), // EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(m_BreathingCycle + k_PitchTimeOffset, 1f)),
                    m_YawMax * strength * m_BreathHandler.GetBreathCycle(k_YawTimeOffset, 2f), //EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(2f * (m_BreathingCycle + k_YawTimeOffset), 1f)),
                    0f
                    );

                // Calculate position
                position = new Vector3(
                    m_HorizontalMax * strength * m_BreathHandler.GetBreathCycle(k_HorizontalTimeOffset, 2f), // EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(2f * (m_BreathingCycle + k_HorizontalTimeOffset), 1f)),
                    m_VerticalMax * strength * m_BreathHandler.GetBreathCycle(k_VerticalTimeOffset), // EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(m_BreathingCycle + k_VerticalTimeOffset, 1f)),
                    0f
                    );
            }
            else
            {
                position = Vector3.zero;
                rotation = Quaternion.identity;
            }
        }
    }
}