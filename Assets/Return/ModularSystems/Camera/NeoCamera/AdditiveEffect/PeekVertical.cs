using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using NeoFPS.CharacterMotion;
using NeoFPS.CharacterMotion.Parameters;
using System;
using Return;
using Return.Cameras;

namespace NeoFPS
{
    public class PeekVertical : MonoBehaviour, IAdditiveTransform//, INeoSerializableComponent
    {
        [SerializeField, Range(0f, 1f), Tooltip("The distance up or down to move the camera when peeking up or down.")]
        private float m_PeekDistance = 0.15f;

        [SerializeField, Range(0f, 1f), Tooltip("The speed the character can change lean amount")]
        private float m_PeekSpeed = 0.4f;
        
        [SerializeField, Tooltip("The maximum speed the character can travel before the peek is cancelled. (0 = no max speed)")]
        private float m_MaxMoveSpeed = 5.5f;

        [SerializeField, Tooltip("The key to a motion graph switch parameter that dictates if the character can peek or not")]
        private string m_MotionGraphKey = "CanPeek";

        private IAdditiveTransformHandler m_Handler = null;
        //private MotionController m_MotionController = null;
        //private SwitchParameter m_CanPeekSwitch = null;
        private float m_CurrentPeek = 0f;
        private float m_TargetPeek = 0f;
        private float m_PeekVelocity = 0f;

        public Quaternion rotation
        {
            get { return Quaternion.identity; }
        }

        public Vector3 position
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
            get { return true; }
        }

        public float currentPeek
        {
            get { return m_CurrentPeek; }
        }

        public float targetPeek
        {
            get { return m_TargetPeek; }
        }

        protected void OnValidate()
        {
            m_MaxMoveSpeed = Mathf.Clamp(m_MaxMoveSpeed, 0f, 50f);
        }

        protected void Awake()
        {
            //m_Handler = GetComponent<IAdditiveTransformHandler>();
            //m_MotionController = GetComponentInParent<MotionController>();

        }

        protected void Start()
        {
            //if (m_MotionController != null)
            //{
            //    m_CanPeekSwitch = m_MotionController.motionGraph.GetSwitchProperty(m_MotionGraphKey);
            //    if (m_CanPeekSwitch != null)
            //        m_CanPeekSwitch.onValueChanged += OnCanPeekChanged;
            //}
        }

        private void OnCanPeekChanged(bool canPeek)
        {
            if (!canPeek)
                m_TargetPeek = 0f;
        }

        protected void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            ResetPeek();
            m_Handler.RemoveAdditiveEffect(this);
        }

        public void SetPeek(float amount)
        {
            //if (m_CanPeekSwitch == null || m_CanPeekSwitch.on)
            //    m_TargetPeek = Mathf.Clamp(amount, -1f, 1f);
        }

        public void ResetPeek()
        {
            m_TargetPeek = 0f;
        }

        public void UpdateTransform()
        {
            // CheckAdd if speed limit reached
            //if (m_TargetPeek != 0f && m_MaxMoveSpeed > 0.0001f && m_MotionController != null)
            //{
            //    if (m_MotionController.characterController.velocity.sqrMagnitude > (m_MaxMoveSpeed * m_MaxMoveSpeed))
            //        m_TargetPeek = 0f;
            //}

            if (!Mathf.Approximately(m_CurrentPeek, m_TargetPeek))
            {
                // Get damping parameters
                float maxSpeed = Mathf.Lerp(2.5f, 50f, m_PeekSpeed * m_PeekSpeed);
                float leanTime = Mathf.Lerp(0.25f, 0.01f, m_PeekSpeed);

                m_CurrentPeek = Mathf.SmoothDamp(m_CurrentPeek, m_TargetPeek, ref m_PeekVelocity, leanTime, maxSpeed, Time.deltaTime);

                position = new Vector3(0f, m_CurrentPeek * m_PeekDistance, 0f);
            }
        }

        //public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        //{
        //}

        //public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        //{
        //}
    }
}
