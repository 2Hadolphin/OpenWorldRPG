using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using Return;
using Return.Items;
namespace NeoFPS.ModularFirearms
{
    public abstract class BaseRecoilHandlerBehaviour : MonoItemModule, IRecoilHandler//, IFirearmModuleValidity, INeoSerializableComponent
    {
        [Header ("Recoil Settings")]

        [SerializeField, Tooltip("an event that fires every time the weapon recoils")]
        private UnityEvent m_OnRecoil = null;

        [Tooltip("The accuracy decrement per shot in hip fire mode (accuracy has a 0-1 range).")]
        [SerializeField] private float m_HipAccuracyKick = 0.075f;
        [Tooltip("The accuracy recovered per second in hip fire mode (accuracy has a 0-1 range).")]
        [SerializeField] private float m_HipAccuracyRecover = 0.75f;
        [Tooltip("The accuracy decrement per shot in sighted fire mode (accuracy has a 0-1 range).")]
        [SerializeField] private float m_SightedAccuracyKick = 0.025f;
        [Tooltip("The accuracy recovered per second in sighted fire mode (accuracy has a 0-1 range).")]
        [SerializeField] private float m_SightedAccuracyRecover = 0.5f;

        public event UnityAction onRecoil
        {
            add { m_OnRecoil.AddListener(value); }
            remove { m_OnRecoil.RemoveListener(value); }
        }

        private float m_AccuracyKickMultiplier = 1f;
        public float accuracyKickMultiplier
        {
            get { return m_AccuracyKickMultiplier; }
            set { m_AccuracyKickMultiplier = Mathf.Clamp(value, 0f, 2f); }
        }
        
        public float hipAccuracyKick { get { return m_HipAccuracyKick * m_AccuracyKickMultiplier; } }
        public float hipAccuracyRecover { get { return m_HipAccuracyRecover; } }
        public float sightedAccuracyKick { get { return m_SightedAccuracyKick * m_AccuracyKickMultiplier; } }
        public float sightedAccuracyRecover { get { return m_SightedAccuracyRecover; } }

        public virtual bool isModuleValid
        {
            get { return true; }
        }


        protected virtual void OnValidate ()
        {
            m_HipAccuracyKick = Mathf.Clamp01(m_HipAccuracyKick);
            m_HipAccuracyRecover = Mathf.Clamp(m_HipAccuracyRecover, 0.01f, 10f);
            m_SightedAccuracyKick = Mathf.Clamp01(m_SightedAccuracyKick);
            m_SightedAccuracyRecover = Mathf.Clamp(m_SightedAccuracyRecover, 0.01f, 10f);
        }
        protected virtual void Awake()
        {
            
        }

        protected virtual void OnEnable()
        {
            //firearm.SetHandling(this);
        }
        protected virtual void OnDisable()
        {
        }

        public virtual void Recoil()
        {
            m_OnRecoil.Invoke();
        }

        public virtual void SetRecoilMultiplier(float move, float rotation)
        { }

        //private static readonly NeoSerializationKey k_KickMultiplierKey = new NeoSerializationKey("kickMultiplier");

        //public virtual void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        //{
        //    writer.WriteValue(k_KickMultiplierKey, m_AccuracyKickMultiplier);
        //}

        //public virtual void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        //{
        //    reader.TryReadValue(k_KickMultiplierKey, out m_AccuracyKickMultiplier, m_AccuracyKickMultiplier);
        //}
    }
}