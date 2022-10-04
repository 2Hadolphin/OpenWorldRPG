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
    public class OverShoulder : MonoBehaviour, IAdditiveTransform//, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The transform to use as the un altered aim direction.")]
        private Transform m_ReferenceTransform = null;
        [SerializeField, Tooltip("The transform to look down (forwards) when looking over shoulder.")]
        private Transform m_OverShoulderTarget = null;
        [SerializeField, Range(0f, 1f), Tooltip("The time taken to turn")]
        private float m_TurnTime = 0.25f;
        [SerializeField, Tooltip("The key to a motion graph switch parameter that dictates if the character can peek or not")]
        private string m_MotionGraphKey = "canPeek";

        //private static readonly NeoSerializationKey k_LerpKey = new NeoSerializationKey("lerp");

        private IAdditiveTransformHandler m_Handler = null;
        //private MotionController m_MotionController = null;
        //private SwitchParameter m_CanPeekSwitch = null;
        private float m_Lerp = 0f;
        private float m_Target = 0f;

        public Quaternion rotation
        {
            get
            {
                if (m_Lerp == 0f)
                    return Quaternion.identity;
                else
                    return GetLookVector();
            }
        }

        public Vector3 position
        {
            get { return Vector3.zero; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public void LookOverShoulder (bool look)
        {
            if (look)
                m_Target = 1f;
            else
                m_Target = 0f;
        }

        protected void OnValidate()
        {
            if (m_OverShoulderTarget == m_ReferenceTransform && m_OverShoulderTarget != null)
            {
                Debug.Log("Over-shoulder target transform must be different to the reference transform.");
                m_OverShoulderTarget = null;
            }
        }

        protected void Awake()
        {
            //m_Handler = GetComponent<IAdditiveTransformHandler>();
            //m_MotionController = GetComponentInParent<MotionController>();
        }

        protected void Start()
        {
            //if (m_MotionController != null)
            //    m_CanPeekSwitch = m_MotionController.motionGraph.GetSwitchProperty(m_MotionGraphKey);
        }

        protected void OnEnable()
        {
            m_Handler.ApplyAdditiveEffect(this);
        }

        protected void OnDisable()
        {
            m_Lerp = 0f;
            m_Target = 0f;
            m_Handler.RemoveAdditiveEffect(this);
        }

        public void UpdateTransform()
        {
            float target = m_Target;
            //if (m_CanPeekSwitch != null && !m_CanPeekSwitch.on)
            //    target = 0f;

            if (m_Lerp > target)
                m_Lerp = Mathf.Clamp01(m_Lerp - Time.deltaTime / m_TurnTime);
            if (m_Lerp < target)
                m_Lerp = Mathf.Clamp01(m_Lerp + Time.deltaTime / m_TurnTime);
        }

        Quaternion GetLookVector()
        {
            //if (m_ReferenceTransform != null && m_OverShoulderTarget != null)
            //{
            //    Quaternion target = Quaternion.Inverse(m_ReferenceTransform.rotation) * m_OverShoulderTarget.rotation;
            //    return Quaternion.Lerp(Quaternion.identity, target, EasingFunctions.EaseInOutQuadratic(m_Lerp));
            //}
            //else
                return Quaternion.identity;
        }

        //public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        //{
        //    reader.TryReadValue(k_LerpKey, out m_Lerp, m_Lerp);
        //}

        //public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        //{
        //    writer.WriteValue(k_LerpKey, m_Lerp);
        //}
    }
}
