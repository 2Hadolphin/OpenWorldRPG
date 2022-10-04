using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using Return;
using Return.Cameras;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-additivejiggle.html")]
	public class AdditiveJiggle : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The angle (either side of the axis) of a full strength jiggle.")]
        private float m_FullTwistAngle = 5f;

        [SerializeField, Tooltip("if true, the jiggle could rotate in either direction.")]
        private bool m_CanFlip = true;

        [SerializeField, Tooltip("The time taken to ease into the jiggle.")]
        private float m_LeadIn = 0.1f;

        [SerializeField, Tooltip("The time taken for the jiggle spring to ease out.")]
        private float m_Duration = 0.5f;

        private static readonly NeoSerializationKey k_RotElapsedKey = new NeoSerializationKey("rotElapsed");
        private static readonly NeoSerializationKey k_RotTargetKey = new NeoSerializationKey("rotTarget");

        private float m_CurrentRotation = 0f;
        private float m_RotationStart = 0f;
        private float m_RotationTarget = 0f;
        private float m_RotationElapsed = 1f;
        private bool m_RotationLeadIn = false;
        
        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }
        
        public Vector3 position
        {
            get { return Vector3.zero; }
        }
        
        public Quaternion rotation
        {
            get;
            private set;
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return false; }
        }

        protected void OnValidate()
        {
            m_LeadIn = Mathf.Clamp(m_LeadIn, 0.001f, 1f);
        }

        protected void Awake()
        {
            transformHandler = GetComponent<IAdditiveTransformHandler>();
        }

        protected void OnEnable()
        {
            transformHandler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            transformHandler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {
            m_RotationElapsed += Time.deltaTime;
            if (m_RotationLeadIn)
            {
                if (m_RotationElapsed > m_LeadIn)
                {
                    m_CurrentRotation = m_RotationTarget;
                    rotation = Quaternion.Euler(0f, 0f, m_RotationTarget);
                    m_RotationLeadIn = false;
                    m_RotationElapsed = 0f;
                }
                else
                {
                    float eased = EasingFunctions.EaseOutQuadratic(m_RotationElapsed / m_LeadIn);
                    m_CurrentRotation = Mathf.Lerp(m_RotationStart, m_RotationTarget, eased);
                    rotation = Quaternion.Euler(0f, 0f, m_CurrentRotation);
                }
            }
            else
            {
                if (m_RotationElapsed > m_Duration)
                {
                    rotation = Quaternion.identity;
                }
                else
                {
                    m_CurrentRotation = Mathf.LerpUnclamped(m_RotationTarget, 0f, EasingFunctions.EaseInSpring(m_RotationElapsed / m_Duration));
                    rotation = Quaternion.Euler(0f, 0f, m_CurrentRotation);
                }
            }
        }

        public void Jiggle(float strength)
        {
                m_RotationLeadIn = (m_LeadIn > 0.001f);
                m_RotationStart = m_CurrentRotation;
                m_RotationTarget = m_FullTwistAngle * strength;
                if (m_CanFlip && Random.Range(0f, 1f) < 0.5f)
                    m_RotationTarget = -m_RotationTarget;
                m_RotationElapsed = 0f;
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (m_RotationElapsed <= m_Duration)
            {
                writer.WriteValue(k_RotElapsedKey, m_RotationElapsed);
                writer.WriteValue(k_RotTargetKey, m_RotationTarget);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_RotElapsedKey, out m_RotationElapsed, m_RotationElapsed))
            {
                reader.TryReadValue(k_RotTargetKey, out m_RotationTarget, m_RotationTarget);
            }
            else
                rotation = Quaternion.identity;
        }
    }
}
