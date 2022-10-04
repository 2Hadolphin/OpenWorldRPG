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
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-headduck.html")]
	public class HeadDuck : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField, Tooltip("The distance to duck the head downwards.")]
        private string m_JumpChargeKey = "jumpCharge";
        [SerializeField, Tooltip("The distance to duck the head downwards.")]
        private float m_DuckHeight = 0.05f;

        private static readonly NeoSerializationKey k_DuckKey = new NeoSerializationKey("duck");

        private IAdditiveTransformHandler m_Handler = null;
        //private FloatParameter m_ChargeParameter = null;
        private float m_CurrentDuck = 0f;
        private float m_TargetDuck = 0f;

		public Quaternion rotation
		{
			get { return Quaternion.identity; }
		}

		private Vector3 m_Position = Vector3.zero;
		public Vector3 position
		{
			get { return m_Position; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

#if UNITY_EDITOR
        protected void OnValidate()
        {
            m_DuckHeight = Mathf.Clamp(m_DuckHeight, 0.05f, 0.5f);
        }
#endif

        protected void Awake ()
		{
			//m_Handler = GetComponent<IAdditiveTransformHandler>();

   //         if (!string.IsNullOrEmpty(m_JumpChargeKey))
   //         {
   //             var mc = GetComponentInParent<MotionController>();
   //             if (mc != null && mc.motionGraph != null)
   //             {
   //                 m_ChargeParameter = mc.motionGraph.GetFloatProperty(m_JumpChargeKey);
   //                 if (m_ChargeParameter != null)
   //                     m_ChargeParameter.onValueChanged += OnJumpChargeChanged;
   //             }
   //         }
        }

        private void OnJumpChargeChanged(float charge)
        {
            m_TargetDuck = Mathf.Clamp01(charge);
        }

        protected void OnEnable ()
		{
			m_Handler.ApplyAdditiveEffect (this);
		}

        protected void OnDisable ()
		{
			m_Handler.RemoveAdditiveEffect (this);
		}

		public void UpdateTransform ()
        {
            if (m_CurrentDuck == m_TargetDuck)
                return;

            m_CurrentDuck = Mathf.Lerp(m_CurrentDuck, m_TargetDuck, Time.deltaTime * 10f);

            float delta = 0f;
            if (m_CurrentDuck > m_TargetDuck)
                delta = m_CurrentDuck - m_TargetDuck;
            else
                delta = m_TargetDuck - m_CurrentDuck;

            if (delta < 0.001f)
                m_CurrentDuck = m_TargetDuck;

            m_Position.y = m_CurrentDuck * -m_DuckHeight;
        }

		public float duckAmount
		{
			get { return m_CurrentDuck; }
			set { m_TargetDuck = Mathf.Clamp01 (value); }
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            writer.WriteValue(k_DuckKey, m_CurrentDuck);
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            if (reader.TryReadValue(k_DuckKey, out m_CurrentDuck, m_CurrentDuck))
                m_Position = Vector3.down * m_CurrentDuck * m_DuckHeight;
        }
    }
}
