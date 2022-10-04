//using NeoCC;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using Return;
using Return.Cameras;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-bodytilt.html")]
    public class BodyTilt : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The absolute maximum angle the character can lean")]
        private float m_MaxTiltAngle = 20f;
        [SerializeField, Range(0f, 1f), Tooltip("The speed the character can change lean amount")]
        private float m_TiltSpeed = 0.4f;
        [SerializeField, Tooltip("The clearance required in the tilt direction to reach full tilt")]
        private float m_RequiredClearance = 1f;

        //private static readonly NeoSerializationKey k_CurrentLeanKey = new NeoSerializationKey("currentLean");
        //private static readonly NeoSerializationKey k_TargetKey = new NeoSerializationKey("targetLean");
        //private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");

        private IAdditiveTransformHandler m_Handler = null;
        //private INeoCharacterController m_CharacterController = null;
        private Transform m_BaseTransform = null;
        private Transform m_ParentTransform = null;
        private Vector3 m_TiltPosition = Vector3.zero;
        private Quaternion m_TiltRotation = Quaternion.identity;
        private RaycastHit s_Hit = new RaycastHit();
        private Vector2 m_CurrentTilt = Vector2.zero;
        private Vector2 m_TargetTilt = Vector2.zero;
        private Vector2 m_TiltVelocity = Vector2.zero;
        private float m_Radius = 0.5f;
        private float m_NormalisedHeight = 0f;
        
        public enum TiltPoint
        {
            Base,
            Mid,
            Head
        }

        public Vector3 position
        {
            get { return m_TiltPosition; }
        }

        public Quaternion rotation
        {
            get { return m_TiltRotation; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public Vector2 currentLean
        {
            get { return -m_CurrentTilt; }
        }

        public Vector2 targetLean
        {
            get { return -m_TargetTilt; }
        }

        protected void OnValidate()
        {
            m_MaxTiltAngle = Mathf.Clamp(m_MaxTiltAngle, 1f, 45f);
            m_RequiredClearance = Mathf.Clamp(m_RequiredClearance, 0f, 2f);
        }

        protected void Awake()
        {
            m_Handler = GetComponent<IAdditiveTransformHandler>();
            m_ParentTransform = transform.parent;

        }

        protected void Start()
        {
            //var motionController = GetComponentInParent<MotionController>();
            //if (motionController != null)
            //{
            //    m_BaseTransform = motionController.transform;
            //    m_CharacterController = motionController.characterController;
            //    m_Radius = m_CharacterController.radius;
            //}
        }

        protected void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            ResetTilt();
            m_Handler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {

            if (m_TargetTilt == Vector2.zero && (m_TargetTilt - m_CurrentTilt).sqrMagnitude < 0.00001f)
            {
                m_TiltPosition = Vector3.zero;
                m_TiltRotation = Quaternion.identity;
            }
            else
            {
                Vector2 constrainedTilt = m_TargetTilt;

                // CheckAdd clearance
                if (m_RequiredClearance > 0.0001f && m_TargetTilt.sqrMagnitude > 0.00025f)
                {
                    Vector3 direction = (m_ParentTransform.forward * m_TargetTilt.y) + (m_ParentTransform.right * m_TargetTilt.x);
                    if (Physics.Raycast(m_ParentTransform.position, direction.normalized, out s_Hit, m_Radius + m_RequiredClearance, PhysicsFilter.Masks.CharacterBlockers, QueryTriggerInteraction.Ignore))
                    {
                        constrainedTilt = Vector2.ClampMagnitude(constrainedTilt, (s_Hit.distance - m_Radius) / m_RequiredClearance);
                    }
                }

                // Get damping parameters
                float maxSpeed = Mathf.Lerp(10f, 200f, m_TiltSpeed * m_TiltSpeed);
                float leanTime = Mathf.Lerp(0.25f, 0.01f, m_TiltSpeed);

                // Get damped lean
                m_CurrentTilt = Vector2.SmoothDamp(m_CurrentTilt, constrainedTilt, ref m_TiltVelocity, leanTime, maxSpeed, Time.deltaTime);

                // Get the pivot point
                float height = Vector3.Dot(m_ParentTransform.position - m_BaseTransform.position, m_BaseTransform.up);
                Vector3 leanPivot = new Vector3(0f, m_NormalisedHeight * height, 0f);

                // Calculate position and rotation
                m_TiltRotation = Quaternion.Euler(m_CurrentTilt.y, 0f, -m_CurrentTilt.x);
                m_TiltPosition = (m_TiltRotation * -leanPivot) + leanPivot;
            }
        }

        public void SetTilt(Vector2 direction, float angle, float normalisedHeight)
        {
            m_TargetTilt = direction * Mathf.Clamp(angle, -m_MaxTiltAngle, m_MaxTiltAngle);
            m_NormalisedHeight = normalisedHeight;
        }

        public void ResetTilt()
        {
            m_TargetTilt = Vector2.zero;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            //writer.WriteValue(k_CurrentLeanKey, m_CurrentLean);
            //writer.WriteValue(k_TargetKey, m_TargetLean);
            //writer.WriteValue(k_VelocityKey, m_LeanVelocity);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            //reader.TryReadValue(k_CurrentLeanKey, out m_CurrentLean, m_CurrentLean);
            //reader.TryReadValue(k_TargetKey, out m_TargetLean, m_TargetLean);
            //reader.TryReadValue(k_VelocityKey, out m_LeanVelocity, m_LeanVelocity);
        }
    }
}
