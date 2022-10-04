using UnityEngine;
using NeoSaveGames.Serialization;
using NeoSaveGames;
using Return;
using Return.Cameras;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using System.Reflection;
using System;
using TNet;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-additivekicker.html")]
	public class AdditiveKicker : MonoBehaviour, IAdditiveTransform//, INeoSerializableComponent
    {
        [SerializeField]
        Component component;
        [SerializeField]
        [SerializeReference]
        GameObject obj;

        [SerializeField]
        BindingFlags Flag = BindingFlags.Instance | BindingFlags.Public;

        [Button]
        protected  void LoadItem()
        {
            var type = GetType();


            var props = type.GetProperties(Flag);

            LogProp(type, props);

            var prop = type.GetSerializableProperties();

            LogProp(type, prop.ToArray());


        }

        void LogProp(Type type, params PropertyInfo[] prop)
        {
            Debug.Log(type + " " + prop.Length);

            foreach (var item in prop)
            {
                Debug.Log(item.Name);
            }
        }

        [Button]
        void CopyTest(int count=100,bool reflection=true)
        {
            var time = Time.realtimeSinceStartup;

            UniTask.SwitchToTaskPool();

            for (int i = 0; i < count; i++)
            {
                if (reflection)
                    obj.AddComponent(component);
                else
                    obj.AddComponent(component.GetType());
            }

            UniTask.SwitchToMainThread();


            time = Time.realtimeSinceStartup - time;

            Debug.Log(string.Format("Create {0} items with {1} seconds, avg {2}",
                count,
                time,
                time/count
                ));
        }

        [SerializeField, Tooltip("The time taken to ease into the kick.")]
        private float m_LeadIn = 0f;

        [SerializeField, Tooltip("The return spring is an animation curve that dictates how the kicker returns from the kick angle/position (1 on the height-axis) to its originak state (0 on the height axis).")]
        private AnimationCurve m_ReturnSpring = new AnimationCurve(
            new Keyframe(0f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, 0f, 0f)
        );

        //private static readonly NeoSerializationKey k_PosElapsedKey = new NeoSerializationKey("posElapsed");
        //private static readonly NeoSerializationKey k_PosDurationKey = new NeoSerializationKey("posDuration");
        //private static readonly NeoSerializationKey k_PosTargetKey = new NeoSerializationKey("posTarget");
        //private static readonly NeoSerializationKey k_RotElapsedKey = new NeoSerializationKey("rotElapsed");
        //private static readonly NeoSerializationKey k_RotDurationKey = new NeoSerializationKey("rotDuration");
        //private static readonly NeoSerializationKey k_RotTargetKey = new NeoSerializationKey("rotTarget");

        private Quaternion m_RotationStart = Quaternion.identity;
        private Quaternion m_RotationTarget = Quaternion.identity;
        private float m_RotationDuration = 0f;
		private float m_RotationElapsed = 1f;
        private bool m_RotationLeadIn = false;
        private Vector3 m_PositionStart = Vector3.zero;
        private Vector3 m_PositionTarget = Vector3.zero;
        private float m_PositionDuration = 0f;
		private float m_PositionElapsed = 1f;
        private bool m_PositionLeadIn = false;
        
        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
        }
        
		public Vector3 position
        {
            get;
            private set;
        }
        
		public Quaternion rotation
        {
            get;
            private set;
        }


        public bool bypassPositionMultiplier
        {
            get;
            set;
        }

        public bool bypassRotationMultiplier
        {
            get;
            set;
        }

        protected void OnValidate()
        {
            m_LeadIn = Mathf.Clamp(m_LeadIn, 0.001f, 1f);
        }

        protected void Awake ()
		{
			transformHandler = GetComponent<IAdditiveTransformHandler>();
		}

        protected void OnEnable ()
		{
			transformHandler.ApplyAdditiveEffect (this);
		}

        protected void OnDisable ()
		{
			transformHandler.RemoveAdditiveEffect (this);
		}

        public void UpdateTransform()
        {
            m_PositionElapsed += Time.deltaTime;
            if (m_PositionLeadIn)
            {
                if (m_PositionElapsed > m_LeadIn)
                {
                    position = m_PositionTarget;
                    m_PositionLeadIn = false;
                    m_PositionElapsed = 0f;
                }
                else
                {
                    float eased = EasingFunctions.EaseOutQuadratic(m_PositionElapsed / m_LeadIn);
                    position = Vector3.Lerp(m_PositionStart, m_PositionTarget, eased);
                }
            }
            else
            {
                if (m_PositionElapsed > m_PositionDuration)
                {
                    position = Vector3.zero;
                }
                else
                {
                    position = Vector3.LerpUnclamped(
                        Vector3.zero,
                        m_PositionTarget,
                        m_ReturnSpring.Evaluate(m_PositionElapsed / m_PositionDuration)
                    );
                }
            }

            m_RotationElapsed += Time.deltaTime;
            if (m_RotationLeadIn)
            {
                if (m_RotationElapsed > m_LeadIn)
                {
                    rotation = m_RotationTarget;
                    m_RotationLeadIn = false;
                    m_RotationElapsed = 0f;
                }
                else
                {
                    float eased = EasingFunctions.EaseOutQuadratic(m_RotationElapsed / m_LeadIn);
                    rotation = Quaternion.Lerp(m_RotationStart, m_RotationTarget, eased);
                }
            }
            else
            {
                if (m_RotationElapsed > m_RotationDuration)
                {
                    rotation = Quaternion.identity;
                }
                else
                {
                    rotation = Quaternion.LerpUnclamped(
                        Quaternion.identity,
                        m_RotationTarget,
                        m_ReturnSpring.Evaluate(m_RotationElapsed / m_RotationDuration)
                    );
                }
            }
        }

        [Button]
        public void KickRotation (Quaternion target, float duration)
		{
            if (duration > 0.01f)
            {
                m_RotationLeadIn = (m_LeadIn > 0.001f);
                m_RotationStart = rotation;
                m_RotationTarget = rotation * target;
                m_RotationDuration = duration;
                m_RotationElapsed = 0f;
            }
            else
                Debug.LogError("Additive.KickRotation duration too small");
		}

        [Button]
		public void KickPosition (Vector3 target, float duration)
        {
            if (duration > 0.01f)
            {
                m_PositionLeadIn = (m_LeadIn > 0.001f);
                m_PositionStart = position;
                m_PositionTarget = position + target;
                m_PositionDuration = duration;
                m_PositionElapsed = 0f;
            }
            else
                Debug.LogError("Additive.KickPosition duration too small");
        }

        //public void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode)
        //{
        //    if (m_PositionElapsed <= m_PositionDuration)
        //    {
        //        writer.WriteValue(k_PosElapsedKey, m_PositionElapsed);
        //        writer.WriteValue(k_PosDurationKey, m_PositionDuration);
        //        writer.WriteValue(k_PosTargetKey, m_PositionTarget);
        //    }

        //    if (m_RotationElapsed <= m_RotationDuration)
        //    {
        //        writer.WriteValue(k_RotElapsedKey, m_RotationElapsed);
        //        writer.WriteValue(k_RotDurationKey, m_RotationDuration);
        //        writer.WriteValue(k_RotTargetKey, m_RotationTarget);
        //    }
        //}

        //public void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo)
        //{
        //    if (reader.TryReadValue(k_PosElapsedKey, out m_PositionElapsed, m_PositionElapsed))
        //    {
        //        reader.TryReadValue(k_PosDurationKey, out m_PositionDuration, m_PositionDuration);
        //        reader.TryReadValue(k_PosTargetKey, out m_PositionTarget, m_PositionTarget);
        //    }
        //    else
        //        position = Vector3.zero;

        //    if (reader.TryReadValue(k_RotElapsedKey, out m_RotationElapsed, m_RotationElapsed))
        //    {
        //        reader.TryReadValue(k_RotDurationKey, out m_RotationDuration, m_RotationDuration);
        //        reader.TryReadValue(k_RotTargetKey, out m_RotationTarget, m_RotationTarget);
        //    }
        //    else
        //        rotation = Quaternion.identity;
        //}
    }
}
