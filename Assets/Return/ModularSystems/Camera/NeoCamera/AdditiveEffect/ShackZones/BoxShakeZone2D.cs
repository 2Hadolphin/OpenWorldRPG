using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS.ShakeZones
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-boxshakezone2d.html")]
    public class BoxShakeZone2D : MonoBehaviour, IShakeZone
    {
        [SerializeField, Tooltip("The strength of the shake")]
        private float m_Strength = 1f;

        [SerializeField, Tooltip("The dimensions of the box along each axis (centered on position)")]
        private Vector2 m_Size = new Vector3(1f, 1f);

        [SerializeField, Tooltip("The distance outside of the box that the strength falls off to zero")]
        private float m_FalloffDistance = 5f;

        private Transform m_LocalTransform = null;

        private float m_Multiplier = 1f;
        public float multiplier
        {
            get { return m_Multiplier; }
            set { m_Multiplier = Mathf.Clamp01(value); }
        }

        protected void OnValidate()
        {
            m_Strength = Mathf.Clamp(m_Strength, 0f, 10f);
        }

        protected void Awake()
        {
            m_LocalTransform = transform;
        }

        protected void OnEnable()
        {
            ShakeHandler.AddShaker(this);
        }

        protected void OnDisable()
        {
            ShakeHandler.RemoveShaker(this);
        }

        public float GetStrengthAtPosition(Vector3 position)
        {
            // Get bounds
            Vector2 halfSize = m_Size * 0.5f;
            Vector2 extended = halfSize;
            extended.x += m_FalloffDistance;
            extended.y += m_FalloffDistance;

            // Get absolute local position
            Vector3 relative = m_LocalTransform.InverseTransformPoint(position);
            Vector2 absRelative = new Vector2(Mathf.Abs(relative.x), Mathf.Abs(relative.z));

            // Get per-axis alpha
            Vector2 alpha = Vector2.zero;
            if (absRelative.x < extended.x)
            {
                if (absRelative.x < halfSize.x)
                    alpha.x = 1f;
                else
                    alpha.x = 1f - (absRelative.x - halfSize.x) / m_FalloffDistance;
            }
            if (absRelative.y < extended.y)
            {
                if (absRelative.y < halfSize.y)
                    alpha.y = 1f;
                else
                    alpha.y = 1f - (absRelative.y - halfSize.y) / m_FalloffDistance;
            }

            // Return scaled
            return m_Strength * m_Multiplier * alpha.x * alpha.y;
        }

#if UNITY_EDITOR
        void OnDrawGizmos()
        {
            ExtendedGizmos.DrawBoxMarker2D(transform.position, transform.rotation, m_Size, Color.cyan);
            if (m_FalloffDistance > 0f)
            {
                Vector2 expanded = m_Size;
                expanded.x += m_FalloffDistance * 2f;
                expanded.y += m_FalloffDistance * 2f;
                ExtendedGizmos.DrawBoxMarker2D(transform.position, transform.rotation, expanded, Color.cyan * 0.5f);
            }
        }
#endif
    }
}