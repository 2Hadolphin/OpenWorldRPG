//using NeoCC;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using Return;
using Return.Cameras;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-camerashake.html")]
    public class CameraShake : ShakeHandler, IAdditiveTransform
    {
        [SerializeField, Tooltip("The distance the camera can move either side of the origin on each axis at a shake strength of 1.")]
        private Vector3 m_ShakeDistance = new Vector3(0.002f, 0.002f, 0f);

        [SerializeField, Tooltip("The max rotation (in each direction) for a shake strength of 1.")]
        private Vector3 m_ShakeTwist = new Vector3(1f, 1f, 5f);

        [SerializeField, Range(0f, 1f), Tooltip("Damping smooths the blend between different continuous shake strengths.")]
        private float m_ContinuousDamping = 0.1f;

        [SerializeField, Tooltip("The time it takes for the shake to go from 0 to 1.")]
        private float m_ConcussionLeadIn = 0.1f;

        [SerializeField, Tooltip("Should continuous shake only be applied while the character this is attached to is grounded (if there is a character).")]
        private bool m_ContinuousOnlyGrounded = true;

        private const int k_NumRandomValues = 512;
        private const float k_MinSpeed = 80f;
        private const float k_MaxSpeed = 120f;
        private const float k_MinDampingMultiplier = 2f;
        private const float k_MaxDampingMultiplier = 20f;

        private IAdditiveTransformHandler m_Handler = null;
        //private INeoCharacterController m_CharacterController = null;
        private Vector3 m_PosOffsets = Vector3.zero;
        private Vector3 m_RotOffsets = Vector3.zero;
        private Vector3 m_PosSpeed = Vector3.zero;
        private Vector3 m_RotSpeed = Vector3.zero;
        private float[] m_RandomValues = null;
        private float m_ConcussionStrength = 0f;
        private float m_ConcussionStart = 0f;
        private float m_ConcussionTarget = 0f;
        private float m_ConcussionLerp = 0f;
        private bool m_ConcussionIn = false;
        private float m_ConcussionInTimeMultiplier = 0f;
        private float m_ConcussionOutTimeMultiplier = 0f;
        private float m_ContinuousTarget = 0f;
        private float m_ContinuousStrength = 0f;
        private float m_CurrentUserStrength = 0f;
        private float m_TargetUserStrength = 1f;

        private Quaternion m_Rotation = Quaternion.identity;
        public Quaternion rotation
        {
            get { return m_Rotation; }
        }

        private Vector3 m_Position = Vector3.zero;
        public Vector3 position
        {
            get { return m_Position; }
        }

        public float strength
        {
            get { return m_TargetUserStrength; }
            set { m_TargetUserStrength = value; }
        }

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return false; }
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
        }
#endif

        protected override void Awake()
        {
            base.Awake();
            m_Handler = GetComponent<IAdditiveTransformHandler>();

            GenerateNoiseMap();

            m_PosOffsets.x = Random.Range(0f, k_NumRandomValues);
            m_PosOffsets.y = Random.Range(0f, k_NumRandomValues);
            m_PosOffsets.z = Random.Range(0f, k_NumRandomValues);
            m_RotOffsets.x = Random.Range(0f, k_NumRandomValues);
            m_RotOffsets.y = Random.Range(0f, k_NumRandomValues);
            m_RotOffsets.z = Random.Range(0f, k_NumRandomValues);
            m_PosSpeed.x = Random.Range(k_MinSpeed, k_MaxSpeed);
            m_PosSpeed.y = Random.Range(k_MinSpeed, k_MaxSpeed);
            m_PosSpeed.z = Random.Range(k_MinSpeed, k_MaxSpeed);
            m_RotSpeed.x = Random.Range(k_MinSpeed, k_MaxSpeed);
            m_RotSpeed.y = Random.Range(k_MinSpeed, k_MaxSpeed);
            m_RotSpeed.z = Random.Range(k_MinSpeed, k_MaxSpeed);

            m_ConcussionInTimeMultiplier = 1f / m_ConcussionLeadIn;

            //m_CharacterController = GetComponentInParent<INeoCharacterController>();
        }

        void GenerateNoiseMap ()
        {
            float inverseLength = Mathf.PI * 2f / k_NumRandomValues;

            m_RandomValues = new float[k_NumRandomValues];
            for (int i = 0; i < k_NumRandomValues; ++i)
            {
                float total = 0f;

                // Octave 1
                float point = inverseLength * i * 4f;
                total += Mathf.Sin(point) * 0.25f;

                // Octave 2
                point = inverseLength * i * 11f + 0.1f;
                total += Mathf.Sin(point) * 0.25f;

                // Octave 3
                point = inverseLength * i * 19f + 0.03f;
                total += Mathf.Sin(point) * 0.25f;

                // Octave 4
                point = inverseLength * i * 31f + 1.7f;
                total += Mathf.Sin(point) * 0.25f;

                m_RandomValues[i] = total;
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            m_Handler.ApplyAdditiveEffect(this);
        }

        protected override void OnDisable()
        {
            m_Handler.RemoveAdditiveEffect(this);
            base.OnDisable();
        }

        public void UpdateTransform()
        {
            TickShakeHandler();

            // Interpolate user strength
            m_CurrentUserStrength = Mathf.Lerp(m_CurrentUserStrength, m_TargetUserStrength, Time.deltaTime * 5f);

            // Update concussion strength
            if (m_ConcussionIn)
            {
                // Apply position and rotation offsets
                //float eased = EasingFunctions.EaseOutQuadratic(m_ConcussionLerp);
                //m_ConcussionStrength = Mathf.Lerp(m_ConcussionStart, m_ConcussionTarget, eased);

                // Animate
                m_ConcussionLerp += Time.deltaTime * m_ConcussionInTimeMultiplier;
                if (m_ConcussionLerp > 1f)
                {
                    m_ConcussionLerp = 1f;
                    m_ConcussionIn = false;
                }
            }
            else
            {
                if (m_ConcussionLerp > 0f)
                {
                    // Apply position and rotation offsets
                    //float eased = EasingFunctions.EaseInOutQuadratic(m_ConcussionLerp);
                    //m_ConcussionStrength = Mathf.Lerp(0f, m_ConcussionTarget, eased);

                    // Animate
                    m_ConcussionLerp -= Time.deltaTime * m_ConcussionOutTimeMultiplier;
                    if (m_ConcussionLerp < 0f)
                        m_ConcussionLerp = 0f;
                }
                else
                    m_ConcussionStrength = 0f;
            }

            // Get the continuous target strength
            float ct = m_ContinuousTarget;
            //if (m_ContinuousOnlyGrounded && m_CharacterController != null && !m_CharacterController.isGrounded)
            //    ct = 0f;

            // Update continuous strength
            if (!Mathf.Approximately(ct, m_ContinuousStrength))
            {
                // Get the damping multiplier
                float dampingMult = Mathf.Lerp(k_MinDampingMultiplier, k_MaxDampingMultiplier, m_ContinuousDamping);
                m_ContinuousStrength = Mathf.Lerp(m_ContinuousStrength, ct, Time.deltaTime * dampingMult);

                // Damped lerp and set if close enough
                if (Mathf.Abs(ct - m_ContinuousStrength) < 0.001f)
                    m_ContinuousStrength = ct;
            }

            // Get current shake strength from continuous and concussion
            float totalStrength = (m_ContinuousStrength + m_ConcussionStrength) * m_CurrentUserStrength;

            // Apply shake
            if (totalStrength > 0.0001f)
            {
                m_Position = new Vector3(
                    Mathf.Approximately(m_ShakeDistance.x, 0f) ? 0f : m_ShakeDistance.x * totalStrength * m_RandomValues[Mathf.FloorToInt(Mathf.Repeat(Time.time * m_PosSpeed.x + m_PosOffsets.x, k_NumRandomValues))],
                    Mathf.Approximately(m_ShakeDistance.y, 0f) ? 0f : m_ShakeDistance.y * totalStrength * m_RandomValues[Mathf.FloorToInt(Mathf.Repeat(Time.time * m_PosSpeed.y + m_PosOffsets.y, k_NumRandomValues))],
                    Mathf.Approximately(m_ShakeDistance.z, 0f) ? 0f : m_ShakeDistance.z * totalStrength * m_RandomValues[Mathf.FloorToInt(Mathf.Repeat(Time.time * m_PosSpeed.z + m_PosOffsets.z, k_NumRandomValues))]
                    );

                m_Rotation = Quaternion.Euler(
                    Mathf.Approximately(m_ShakeTwist.x, 0f) ? 0f : m_ShakeTwist.x * totalStrength * m_RandomValues[Mathf.FloorToInt(Mathf.Repeat(Time.time * m_RotSpeed.x + m_RotOffsets.x, k_NumRandomValues))],
                    Mathf.Approximately(m_ShakeTwist.y, 0f) ? 0f : m_ShakeTwist.y * totalStrength * m_RandomValues[Mathf.FloorToInt(Mathf.Repeat(Time.time * m_RotSpeed.y + m_RotOffsets.y, k_NumRandomValues))],
                    Mathf.Approximately(m_ShakeTwist.z, 0f) ? 0f : m_ShakeTwist.z * totalStrength * m_RandomValues[Mathf.FloorToInt(Mathf.Repeat(Time.time * m_RotSpeed.z + m_RotOffsets.z, k_NumRandomValues))]
                    );
            }
            else
            {
                m_Position = Vector3.zero;
                m_Rotation = Quaternion.identity;
            }
        }

        protected override void DoShakeContinuous(float strength)
        {
            m_ContinuousTarget = strength;
        }

        protected override void DoShake(float strength, float duration, bool requiresGrounding)
        {
            // Only add concussion if it's stronger than current shake level?
            if (m_ConcussionStrength > strength)
                return;

            // CheckAdd for grounding requirements
            //if (requiresGrounding && m_CharacterController != null && !m_CharacterController.isGrounded)
            //    return;

            m_ConcussionTarget = strength;
            m_ConcussionOutTimeMultiplier = 1f / duration;

            // Set starting state
            if (m_ConcussionLeadIn > 0f)
            {
                m_ConcussionIn = true;
                m_ConcussionLerp = 0f;
                m_ConcussionStart = m_ConcussionStrength;
            }
            else
            {
                m_ConcussionLerp = 1f;
                m_ConcussionIn = false;
            }
        }
    }
}