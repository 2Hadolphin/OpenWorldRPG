//using NeoCC;
using NeoFPS.ModularFirearms;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using Return;
using Return.Cameras;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-charactermovementsway.html")]
    public class CharacterMovementSway : MonoBehaviour, IAdditiveTransform, INeoSerializableComponent
    {
        [SerializeField]
        private SwayLimit m_ForwardsSway = new SwayLimit(new Vector3(0f, -0.05f, 0.05f), 0f, 10f);
        [SerializeField]
        private SwayLimit m_ReverseSway = new SwayLimit(new Vector3(0f, -0.05f, -0.05f), 0f, 5f);
        [SerializeField]
        private SwayLimit m_LeftSway = new SwayLimit(new Vector3(0.1f, -0.05f, 0f), 15f, 7.5f);
        [SerializeField]
        private SwayLimit m_RightSway = new SwayLimit(new Vector3(-0.1f, -0.05f, 0f), -15f, 7.5f);

        [SerializeField, Range(0.1f, 1f), Tooltip("Approximately the time it will take to reach the target sway. A smaller value will reach the target faster")]
        private float m_DampingTime = 0.25f;

        [SerializeField, Range(0f, 1f), Tooltip("A multiplier applied to the sway offsets when aiming down sights")]
        private float m_AimingMultiplier = 0.25f;

        [Serializable]
        private struct SwayLimit
        {
            public Vector3 offset;
            public float roll;
            public float speed;

            public SwayLimit(Vector3 offset, float roll, float speed)
            {
                this.offset = offset;
                this.roll = roll;
                this.speed = speed;
            }
        }

        private static readonly NeoSerializationKey k_PositionKey = new NeoSerializationKey("position");
        private static readonly NeoSerializationKey k_RotationKey = new NeoSerializationKey("rotation");

        private IAdditiveTransformHandler m_Handler = null;
        //private INeoCharacterController m_Controller = null;
        //private IModularFirearm m_Firearm = null;
        //private IAimer m_Aimer = null;
        private Vector3 m_OffsetVelocity = Vector3.zero;
        private float m_RollVelocity = 0f;
        private float m_CurrentRoll = 0f;

        public Quaternion rotation
        {
            get;
            private set;
        }

        public Vector3 position
        {
            get;
            private set;
        }

        //public float strength
        //{
        //    get { return m_TargetUserStrength; }
        //    set { m_TargetUserStrength = value; }
        //}

        public bool bypassPositionMultiplier
        {
            get { return false; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        protected void Awake()
        {
            //m_Handler = GetComponent<IAdditiveTransformHandler>();
            //m_Firearm = GetComponentInParent<IModularFirearm>();
        }

        protected void OnEnable()
        {
            //m_CurrentUserStrength = 0f;
            position = Vector3.zero;

            //m_Controller = GetComponentInParent<INeoCharacterController>();
            //if (m_Controller != null)
            //{
            //    m_Handler.ApplyAdditiveEffect(this);

            //    m_OffsetVelocity = Vector3.zero;
            //    m_RollVelocity = m_CurrentRoll = 0f;

            //    if (m_Firearm != null)
            //    {
            //        m_Firearm.onAimerChange += OnAimerChange;
            //        OnAimerChange(m_Firearm, m_Firearm.aimer);
            //    }
            //}
        }

        protected void OnDisable()
        {
            //if (m_Controller != null)
            //{
            //    m_Handler.RemoveAdditiveEffect(this);

            //    if (m_Firearm != null)
            //    {
            //        m_Firearm.onAimerChange -= OnAimerChange;
            //        m_Aimer = null;
            //    }
            //}
        }

        //private void OnAimerChange(IModularFirearm firearm, IAimer aimer)
        //{
        //    m_Aimer = aimer;
        //}

        public void UpdateTransform()
        {
            // Get the velocity in each direction
            //var forwards = Vector3.Dot(m_Controller.velocity, m_Controller.forward);
            //var right = Vector3.Dot(m_Controller.velocity, m_Controller.right);

            //float lerpForwards = Mathf.Clamp01(forwards / m_ForwardsSway.speed);
            //float lerpReverse = Mathf.Clamp01(-forwards / m_ReverseSway.speed);
            //float lerpRight = Mathf.Clamp01(right / m_RightSway.speed);
            //float lerpLeft = Mathf.Clamp01(-right / m_LeftSway.speed);

            //var targetOffset = Vector3.zero;
            //var targetRoll = 0f;

            //var multiplier = (m_Aimer != null && m_Aimer.isAiming) ? m_AimingMultiplier : 1f;

            //if (lerpForwards > 0f)
            //{
            //    float eased = EasingFunctions.EaseInOutQuadratic(lerpForwards);
            //    targetOffset += m_ForwardsSway.offset * eased * multiplier;
            //    targetRoll += m_ForwardsSway.roll * eased * multiplier;
            //}

            //if (lerpReverse > 0f)
            //{
            //    float eased = EasingFunctions.EaseInOutQuadratic(lerpReverse);
            //    targetOffset += m_ReverseSway.offset * eased * multiplier;
            //    targetRoll += m_ReverseSway.roll * eased * multiplier;
            //}

            //if (lerpRight > 0f)
            //{
            //    float eased = EasingFunctions.EaseInOutQuadratic(lerpRight);
            //    targetOffset += m_RightSway.offset * eased * multiplier;
            //    targetRoll += m_RightSway.roll * eased * multiplier;
            //}

            //if (lerpLeft > 0f)
            //{
            //    float eased = EasingFunctions.EaseInOutQuadratic(lerpLeft);
            //    targetOffset += m_LeftSway.offset * eased * multiplier;
            //    targetRoll += m_LeftSway.roll * eased * multiplier;
            //}

            //position = Vector3.SmoothDamp(position, targetOffset, ref m_OffsetVelocity, m_DampingTime);
            //m_CurrentRoll = Mathf.SmoothDamp(m_CurrentRoll, targetRoll, ref m_RollVelocity, m_DampingTime);
            //rotation = Quaternion.Euler(0f, 0f, m_CurrentRoll);
        }

        public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        {
            if (gameObject.activeSelf)
            {
                writer.WriteValue(k_PositionKey, position);
                writer.WriteValue(k_RotationKey, m_CurrentRoll);
            }
        }

        public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        {
            Vector3 p;
            if (reader.TryReadValue(k_PositionKey, out p, Vector3.zero))
            {
                position = p;
                m_OffsetVelocity = Vector3.zero;

                reader.TryReadValue(k_RotationKey, out m_CurrentRoll, m_CurrentRoll);
                rotation = Quaternion.Euler(0f, 0f, m_CurrentRoll);
                m_RollVelocity = 0f;
            }
        }
    }
}