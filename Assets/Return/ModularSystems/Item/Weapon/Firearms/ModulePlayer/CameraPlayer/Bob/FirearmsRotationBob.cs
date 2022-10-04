

using Sirenix.OdinInspector;
using UnityEngine;
using System;

namespace Return.Cameras
{
    public class FirearmsRotationBob : BaseRotationBob
    {
        [SerializeField, Range(0f, 1f), Tooltip("A multiplier for the rotation when aiming down sights. This can be used to prevent excessive crosshair wander.")]
        private float m_AimingMultiplier = 0.25f;

        [ShowInInspector, NonSerialized]
        public bool isAiming;

        protected void FixedUpdate()
        {
            // Interpolate user strength
            float multiplier = isAiming ? m_AimingMultiplier : 1f;
            m_CurrentStrength = Mathf.Lerp(m_CurrentStrength, multiplier * m_TargetStrength, ConstCache.deltaTime * 5f);
        }



    }

}