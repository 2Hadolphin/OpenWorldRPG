using Return;
using Return.Cameras;
using System.Collections;
using UnityEngine;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-transformmatcher.html")]
    public class TransformMatcher : MonoBehaviour, IAdditiveTransform
    {
        [SerializeField, Range(0f, 1f), Tooltip("The strength of the effect. 1 matches the movement absolutely, while 0 is no movement.")]
        private float m_Weight = 1f;

        [SerializeField, Range(0f, 1f), Tooltip("The time it takes to blend in or out of the movement when the transforms are changed.")]
        private float m_BlendDuration = 0.25f;

        private float m_CurrentWeight = 1f;
        private Quaternion m_FromRotation = Quaternion.identity;
        private Vector3 m_FromPosition = Vector3.zero;
        private Quaternion m_CurrentRotation = Quaternion.identity;
        private Vector3 m_CurrentPosition = Vector3.zero;
        private Coroutine m_BlendCoroutine = null;
        private float m_BlendAmount = 0f;

        public Transform target
        {
            get;
            private set;
        }

        public Transform relativeTo
        {
            get;
            private set;
        }

        public Quaternion rotation
        {
            get { return m_CurrentRotation; }
        }

        public Vector3 position
        {
            get { return m_CurrentPosition; }
        }

        public bool bypassPositionMultiplier
        {
            get { return true; }
        }

        public bool bypassRotationMultiplier
        {
            get { return true; }
        }

        public IAdditiveTransformHandler transformHandler
        {
            get;
            private set;
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
            Vector3 targetPosition = Vector3.zero;
            Quaternion targetRotation = Quaternion.identity;

            float w = m_CurrentWeight * m_Weight;
            if (target != null && relativeTo != null && !Mathf.Approximately(w, 0f))
            {
                Quaternion inverse = Quaternion.Inverse(relativeTo.rotation);
                Quaternion fullRotation = inverse * target.rotation;
                Vector3 fullPosition = inverse * (target.position - relativeTo.position);

                if (Mathf.Approximately(w, 1f))
                {
                    targetPosition = fullPosition;
                    targetRotation = fullRotation;
                }
                else
                {
                    targetPosition = Vector3.Lerp(Vector3.zero, fullPosition, w);
                    targetRotation = Quaternion.Lerp(Quaternion.identity, fullRotation, w);
                }
            }

            if (m_BlendCoroutine != null)
            {
                m_CurrentPosition = Vector3.Lerp(m_FromPosition, targetPosition, m_BlendAmount);
                m_CurrentRotation = Quaternion.Lerp(m_FromRotation, targetRotation, m_BlendAmount);
            }
            else
            {
                m_CurrentPosition = targetPosition;
                m_CurrentRotation = targetRotation;
            }
        }

        public void SetTargetTransforms(Transform target, Transform relativeTo, float weight = 1f)
        {
            // Set transforms
            this.target = target;
            this.relativeTo = relativeTo;

            // Set weight
            m_CurrentWeight = weight;

            // Start blend
            if (m_BlendDuration > 0.001f)
            {
                m_FromRotation = rotation;
                m_FromPosition = position;
                m_BlendAmount = 0f;
                if (m_BlendCoroutine == null && isActiveAndEnabled)
                    m_BlendCoroutine = StartCoroutine(BlendCoroutine());
            }
        }

        public void ClearTargetTransforms()
        {
            // Set transforms
            this.target = null;
            this.relativeTo = null;

            // Set weight
            m_CurrentWeight = 1f;

            // Start blend
            if (m_BlendDuration > 0.001f)
            {
                m_FromRotation = rotation;
                m_FromPosition = position;
                m_BlendAmount = 0f;
                if (m_BlendCoroutine == null && isActiveAndEnabled)
                    m_BlendCoroutine = StartCoroutine(BlendCoroutine());
            }
        }

        IEnumerator BlendCoroutine()
        {
            while (m_BlendAmount < 1f)
            {
                yield return null;
                m_BlendAmount += Time.deltaTime / m_BlendDuration;
                if (m_BlendAmount > 1f)
                {
                    m_BlendAmount = 1f;
                    m_BlendCoroutine = null;
                    break;
                }
            }
        }
    }
}