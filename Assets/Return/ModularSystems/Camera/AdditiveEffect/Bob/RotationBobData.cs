using Sirenix.OdinInspector;
using UnityEngine;

namespace Return.Cameras
{
    public class RotationBobData : PresetDatabase
    {
        [SerializeField, Tooltip("The curve over one step cycle for the weapon bob.")]
        protected AnimationCurve m_BobCurve;

        [SerializeField, Tooltip("The angle range range around each axis.")]
        protected Vector3 m_RotationRange = new(1f, 0.5f, 1f);

        public Vector3 CalculateBob(float step)
        {
            // Get the bob amount
            return new Vector3(
               m_BobCurve.Evaluate(step * 2f) * m_RotationRange.x * 0.5f,
               m_BobCurve.Evaluate(step) * m_RotationRange.y * 0.5f,
               m_BobCurve.Evaluate(step * 2f + 0.5f) * m_RotationRange.z * 0.5f
               );
        }

#if UNITY_EDITOR
        [Button]
        void NewCurve()
        {
            m_BobCurve =
                new(
                new Keyframe(0f, 0f, 0, 0),
                new Keyframe(0.5f, 1f, 0, 0),
                new Keyframe(1f, 0f, 0, 0),
                new Keyframe(1.5f, -1f, 0, 0),
                new Keyframe(2f, 0f, 0, 0))
                {
                    preWrapMode = WrapMode.Loop,
                    postWrapMode = WrapMode.Loop
                };

            var length = m_BobCurve.length;

            for (int i = 0; i < length; i++)
                m_BobCurve.SmoothTangents(i, 1);
        }

        protected void OnValidate()
        {
            m_RotationRange.x = Mathf.Clamp(m_RotationRange.x, -5f, 5f);
            m_RotationRange.y = Mathf.Clamp(m_RotationRange.y, -5f, 5f);
            m_RotationRange.z = Mathf.Clamp(m_RotationRange.z, -5f, 5f);
            m_BobCurve.preWrapMode = WrapMode.Loop;
            m_BobCurve.postWrapMode = WrapMode.Loop;
        }
#endif

    }



}