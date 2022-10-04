using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using Return;
using Return.Cameras;
using UnityEngine;


namespace NeoFPS.CharacterMotion
{

}
namespace NeoFPS.CharacterMotion.Parameters
{

}

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-bodylean.html")]
    public class BodyLean : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The maximum angle the character can lean")]
        private float m_MaxLeanAngle = 20f;
        [SerializeField, Tooltip("The vertical offset of the pivot")]
        private float m_PivotOffset = -1f;
        [SerializeField, Range(0f, 1f), Tooltip("The speed the character can change lean amount")]
        private float m_LeanSpeed = 0.4f;
        [SerializeField, Range(0f, 1f), Tooltip("How much of the lean rotation is reflected in the weapon")]
        private float m_WeaponTilt = 1f;
        [SerializeField, Range(0f, 1f), Tooltip("A counter rotation of the head compared to the weapon.")]
        private float m_HeadCounterTilt = 0.5f;

        [Header("Clearance")]
        [SerializeField, Tooltip("The clearance required on the lean side to reach full lean")]
        private float m_RequiredClearance = 1f;
        [SerializeField, Tooltip("Should the lean be cancelled if physically blocked")]
        private bool m_CancelIfBlocked = true;

        [Header("Motion Graph")]
        [SerializeField, Tooltip("The maximum speed the character can travel before the lean is cancelled. (0 = no max speed)")]
        private float m_ResetSpeedStanding = 4f;
        [SerializeField, Tooltip("The maximum speed the character can travel before the lean is cancelled. (0 = no max speed)")]
        private float m_ResetSpeedCrouching = 1.5f;
        [SerializeField, Tooltip("The key to a motion graph switch parameter that dictates if the character can lean or not")]
        private string m_CanLeanKey = "canLean";
        [SerializeField, Tooltip("The key to a motion graph switch parameter that dictates if the character is crouching or standing")]
        private string m_IsCrouchingKey = "isCrouching";

        private static readonly NeoSerializationKey k_CurrentLeanKey = new NeoSerializationKey("currentLean");
        private static readonly NeoSerializationKey k_TargetKey = new NeoSerializationKey("targetLean");
        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");

        private IAdditiveTransformHandler m_Handler = null;
        //private MotionController m_MotionController = null;
        //private SwitchParameter m_CanLeanSwitch = null;
        //private SwitchParameter m_IsCrouchingSwitch = null;
        private Transform m_ParentTransform = null;
        private Vector3 m_LeanPosition = Vector3.zero;
        private Quaternion m_LeanRotation = Quaternion.identity;
        private RaycastHit s_Hit = new RaycastHit();
        private float m_CurrentLean = 0f;
        private float m_TargetLean = 0f;
        private float m_LeanVelocity = 0f;
        private float m_Radius = 0.5f;

        public Vector3 position
        {
            get { return m_LeanPosition; }
        }

        public Quaternion rotation
        {
            get { return m_LeanRotation;  }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public float currentLean
        {
            get { return -m_CurrentLean; }
        }

        public float targetLean
        {
            get { return -m_TargetLean; }
            set { m_TargetLean = -Mathf.Clamp(value, -1f, 1f); }
        }

        protected void OnValidate()
        {
            m_MaxLeanAngle = Mathf.Clamp(m_MaxLeanAngle, 1f, 45f);
            m_PivotOffset = Mathf.Clamp(m_PivotOffset, -2f, 0f);
            m_RequiredClearance = Mathf.Clamp(m_RequiredClearance, 0f, 2f);
            m_ResetSpeedCrouching = Mathf.Clamp(m_ResetSpeedCrouching, 0f, 50f);
        }

        protected void Awake()
        {
            m_Handler = GetComponent<IAdditiveTransformHandler>();
            //m_MotionController = GetComponentInParent<MotionController>();
            m_ParentTransform = transform.parent;
        }

        protected void Start()
        {
            //if (m_MotionController != null)
            //{
            //    m_Radius = m_MotionController.characterController.radius;
            //    m_CanLeanSwitch = m_MotionController.motionGraph.GetSwitchProperty(m_CanLeanKey);
            //    m_IsCrouchingSwitch = m_MotionController.motionGraph.GetSwitchProperty(m_IsCrouchingKey);
            //}

            if (m_HeadCounterTilt > 0.001f)
            {
                //var character = GetComponentInParent<ICharacter>();
                //if (character.headTransformHandler != null)
                //{
                //    var counter = character.headTransformHandler.CharacterRoot.AddComponent<BodyLeanCounterRotation>();
                //    counter.AttachToBodyLean(this);
                //    character.headTransformHandler.ApplyAdditiveEffect(counter);
                //}
            }
        }

        protected void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            ResetLean();
            m_Handler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {
            float constrainedLean = m_TargetLean;
            
            if (Mathf.Approximately(constrainedLean, 0f) && (Mathf.Abs(constrainedLean - m_CurrentLean) < 0.001f))
            {
                m_LeanPosition = Vector3.zero;
                m_LeanRotation = Quaternion.identity;
            }
            else
            {
                // CheckAdd if can lean
                bool blocked = false;
                //if (m_CanLeanSwitch != null)
                //    blocked = !m_CanLeanSwitch.on;

                if (blocked)
                {
                    constrainedLean = 0f;
                    m_TargetLean = 0f;
                }
                else
                {
                    // CheckAdd if speed limit reached
                    //if (m_MotionController != null)
                    //{
                    //    float resetSpeed = (m_IsCrouchingSwitch != null && m_IsCrouchingSwitch.on) ? m_ResetSpeedCrouching : m_ResetSpeedStanding;

                    //    if (m_MotionController.characterController.velocity.sqrMagnitude > (resetSpeed * resetSpeed))
                    //    {
                    //        constrainedLean = 0f;
                    //        m_TargetLean = 0f;
                    //    }
                    //}

                    // CheckAdd left
                    if (m_RequiredClearance > 0.0001f && m_TargetLean > 0.05f)
                    {
                        if (Physics.Raycast(m_ParentTransform.position, -m_ParentTransform.right, out s_Hit, m_Radius + m_RequiredClearance, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore))
                        {
                            constrainedLean = (s_Hit.distance - m_Radius) / m_RequiredClearance;
                            if (m_CancelIfBlocked && constrainedLean < 0.05f)
                            {
                                constrainedLean = 0f;
                                m_TargetLean = 0f;
                            }
                        }
                    }

                    // CheckAdd right
                    if (m_RequiredClearance > 0.0001f && m_TargetLean < -0.05f)
                    {
                        if (Physics.Raycast(m_ParentTransform.position, m_ParentTransform.right, out s_Hit, m_Radius + m_RequiredClearance, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore))
                        {
                            constrainedLean = -(s_Hit.distance - m_Radius) / m_RequiredClearance;
                            if (m_CancelIfBlocked && constrainedLean > -0.05f)
                            {
                                constrainedLean = 0f;
                                m_TargetLean = 0f;
                            }
                        }
                    }
                }

                // Get damping parameters
                float maxSpeed = Mathf.Lerp(2.5f, 50f, m_LeanSpeed * m_LeanSpeed);
                float leanTime = Mathf.Lerp(0.25f, 0.01f, m_LeanSpeed);

                // Get damped lean
                m_CurrentLean = Mathf.SmoothDamp(m_CurrentLean, constrainedLean, ref m_LeanVelocity, leanTime, maxSpeed, Time.deltaTime);

                // Calculate position and rotation
                m_LeanRotation = Quaternion.Euler(0f, 0f, m_MaxLeanAngle * m_CurrentLean);
                Vector3 leanPivot = new Vector3(0f, m_PivotOffset, 0f);
                m_LeanPosition = (m_LeanRotation * -leanPivot) + leanPivot;
                // Counter tilt
                if (m_WeaponTilt < 0.99f)
                    m_LeanRotation = Quaternion.Slerp(Quaternion.identity, m_LeanRotation, m_WeaponTilt);
            }
        }

        public void LeanLeft (float amount)
        {
            m_TargetLean = amount;
        }

        public void LeanRight (float amount)
        {
            m_TargetLean = -amount;
        }

        public void ResetLean()
        {
            m_TargetLean = 0f;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_CurrentLeanKey, m_CurrentLean);
            writer.WriteValue(k_TargetKey, m_TargetLean);
            writer.WriteValue(k_VelocityKey, m_LeanVelocity);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            reader.TryReadValue(k_CurrentLeanKey, out m_CurrentLean, m_CurrentLean);
            reader.TryReadValue(k_TargetKey, out m_TargetLean, m_TargetLean);
            reader.TryReadValue(k_VelocityKey, out m_LeanVelocity, m_LeanVelocity);
        }

        #region COUNTER-ROTATION

        class BodyLeanCounterRotation : MonoBehaviour, IAdditiveTransform
        {
            public bool bypassPositionMultiplier { get { return true; } }
            public bool bypassRotationMultiplier { get { return true; } }

            public Vector3 position { get { return Vector3.zero; } }

            public void UpdateTransform() { }

            private BodyLean m_BodyLean = null;

            public Quaternion rotation
            {
                get
                {
                    if (m_BodyLean != null && m_BodyLean.currentLean != 0f)
                        return Quaternion.Slerp(Quaternion.identity, Quaternion.Inverse(m_BodyLean.m_LeanRotation), m_BodyLean.m_HeadCounterTilt);
                    else
                        return Quaternion.identity;
                }
            }

            public void AttachToBodyLean(BodyLean body)
            {
                m_BodyLean = body;
            }
        }

        #endregion
    }
}