using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public class SimpleBreathHandler : MonoBehaviour, IBreathHandler//, INeoSerializableComponent
    {
        [SerializeField, Delayed, Tooltip("The time in seconds between breaths.")]
        private float m_BreathInterval = 4f;
        [SerializeField, Range(0f, 1f), Tooltip("The strength of the character's breathing (0 = non-existant, 1 = heaving/panting).")]
        private float m_BreathStrength = 0.25f;

        //private static readonly NeoSerializationKey k_BreathCounterKey = new NeoSerializationKey("counter");
        //private static readonly NeoSerializationKey k_BreathIntervalKey = new NeoSerializationKey("interval");
        //private static readonly NeoSerializationKey k_BreathStrengthKey = new NeoSerializationKey("strength");

        public float breathCounter
        {
            get;
            private set;
        }

        public float breathInterval
        {
            get { return m_BreathInterval; }
            set { m_BreathInterval = Mathf.Clamp(value, 1f, 10f); }
        }

        public float breathStrength
        {
            get { return m_BreathStrength; }
            set { m_BreathStrength = Mathf.Clamp01(value); }
        }

        protected void OnValidate()
        {
            m_BreathInterval = Mathf.Clamp(m_BreathInterval, 0.1f, 10f);
        }

        protected void Update()
        {
            breathCounter += 2f * Time.deltaTime / m_BreathInterval;
        }

        public float GetBreathCycle()
        {
            return EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(breathCounter, 1f)) * 2f - 1f;
        }

        public float GetBreathCycle(float offset)
        {
            return EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(breathCounter + offset, 1f)) * 2f - 1f;
        }

        public float GetBreathCycle(float offset, float multiplier)
        {
            return EasingFunctions.EaseInOutQuadratic(Mathf.PingPong(multiplier * (breathCounter + offset), 1f)) * 2f - 1f;
        }

        //public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        //{
        //    writer.WriteValue(k_BreathCounterKey, breathCounter);
        //    writer.WriteValue(k_BreathIntervalKey, breathInterval);
        //    writer.WriteValue(k_BreathStrengthKey, breathStrength);
        //}

        //public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        //{
        //    float floatValue = 0f;
        //    if (reader.TryReadValue(k_BreathCounterKey, out floatValue, 0f))
        //        breathCounter = floatValue;
        //    if (reader.TryReadValue(k_BreathIntervalKey, out floatValue, m_BreathInterval))
        //        breathInterval = floatValue;
        //    if (reader.TryReadValue(k_BreathStrengthKey, out floatValue, m_BreathStrength))
        //        breathStrength = floatValue;
        //}
    }
}