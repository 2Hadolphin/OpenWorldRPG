using UnityEngine;
using System.Collections.Generic;
using Sirenix.OdinInspector;

namespace NeoFPS
{
    [HelpURL("https://docs.neofps.com/manual/fpcamref-mb-shakehandler.html")]
	public abstract class ShakeHandler : MonoBehaviour
    {
        private static List<IShakeZone> s_Shakers = new List<IShakeZone>(8);
        private static List<ShakeHandler> s_ShakeHandlers = new List<ShakeHandler>(4);

        private Transform m_LocalTransform = null;

        private const float k_MinimumDurationMultiplier = 0.25f;

        private static float s_GlobalShake = 0f;
        public static float globalShake
        {
            get { return s_GlobalShake; }
            set { s_GlobalShake = Mathf.Clamp(value, 0f, 10f); }
        }

        private float m_ContinuousShake = 0f;
        public float continuousShake
        {
            get { return m_ContinuousShake; }
            set { m_ContinuousShake = Mathf.Clamp(value, 0f, 10f); }
        }

        private float m_ContinuousMultiplier = 1f;
        public float continuousMultiplier
        {
            get { return m_ContinuousMultiplier; }
            set { m_ContinuousMultiplier = Mathf.Clamp01(value); }
        }

        private float m_ConcussionMultiplier = 1f;
        public float concussionMultiplier
        {
            get { return m_ConcussionMultiplier; }
            set { m_ConcussionMultiplier = Mathf.Clamp01(value); }
        }

        protected virtual void Awake()
        {
            m_LocalTransform = transform;
        }

        protected virtual void OnEnable()
        {
            s_ShakeHandlers.Add(this);
        }

        protected virtual void OnDisable()
        {
            s_ShakeHandlers.Remove(this);
        }

        protected virtual void TickShakeHandler()
        {
            // Get highest constant shake
            float max = Mathf.Max(globalShake, continuousShake);

            // CheckAdd overlapping shakers
            Vector3 pos = m_LocalTransform.position;
            for (int i = 0; i < s_Shakers.Count;++i)
            {
                float shakerStrength = s_Shakers[i].GetStrengthAtPosition(pos);
                if (shakerStrength > max)
                    max = shakerStrength;
            }

            // Apply continuous shake
            DoShakeContinuous(max);
        }

        [Button]
        protected abstract void DoShake(float strength, float duration, bool requiresGrounding); 

        [Button]
        protected abstract void DoShakeContinuous(float strength);

        public static void AddShaker(IShakeZone shaker)
        {
            if (shaker != null && !s_Shakers.Contains(shaker))
                s_Shakers.Add(shaker);
        }

        public static void RemoveShaker(IShakeZone shaker)
        {
            if (s_Shakers.Contains(shaker))
                s_Shakers.Remove(shaker);
        }

        public static void Shake(Vector3 position, float innerRadius, float falloffDistance, float strength, float duration, bool requiresGrounding = false)
        {
            // Get the outer radius squared
            float outerRadiusSquared = innerRadius + falloffDistance;
            outerRadiusSquared *= outerRadiusSquared;

            for (int i = 0; i < s_ShakeHandlers.Count; ++i)
            {
                // CheckAdd if handler overlaps shake radius
                float sqrDistance = (s_ShakeHandlers[i].m_LocalTransform.position - position).sqrMagnitude;
                if (sqrDistance < outerRadiusSquared)
                {
                    float innerRadiusSquared = innerRadius * innerRadius;
                    if (sqrDistance <= innerRadiusSquared)
                        s_ShakeHandlers[i].DoShake(strength, duration, requiresGrounding);
                    else
                    {
                        // Linear falloff
                        float alpha = 1f - ((Mathf.Sqrt(sqrDistance) - innerRadius) / falloffDistance);
                        s_ShakeHandlers[i].DoShake(alpha * strength, Mathf.Lerp(k_MinimumDurationMultiplier * duration, duration, alpha), requiresGrounding);
                    }
                }
            }
        }
    }
}
