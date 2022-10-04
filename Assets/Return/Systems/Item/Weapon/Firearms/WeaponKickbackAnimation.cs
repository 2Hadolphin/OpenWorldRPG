using System;
using UnityEngine;


    [Serializable]
    public struct WeaponKickbackAnimation
    {
        [SerializeField]
        private bool m_Enabled;

        [SerializeField]
        private float m_UpwardForce;

        [SerializeField]
        [m_MinMaxSlider(-1, 1)]
        private Vector2 m_UpwardRandomness;

        [SerializeField]
        private float m_SidewaysForce;

        [SerializeField]
        [m_MinMaxSlider(-1, 1)]
        private Vector2 m_SidewaysRandomness;

        [SerializeField]
        private float m_KickbackForce;

        [SerializeField]
        [m_MinMaxSlider(-1, 1)]
        private Vector2 m_KickbackRandomness;

        [SerializeField]
        private float m_VerticalRotation;

        [SerializeField]
        [m_MinMaxSlider(-1, 1)]
        private Vector2 m_VerticalRotationRandomness;

        [SerializeField]
        private float m_HorizontalRotation;

        [SerializeField]
        [m_MinMaxSlider(-1, 1)]
        private Vector2 m_HorizontalRotationRandomness;

        [SerializeField]
        private float m_KickbackDuration;

        [SerializeField]
        private float m_KickbackSpeed;

        public WeaponKickbackAnimation(bool enabled, float upwardForce, Vector2 upwardRandomness, float sidewaysForce, Vector2 sidewaysRandomness, float kickbackForce, Vector2 kickbackRandomness, float verticalRotation, Vector2 verticalRotationRandomness, float horizontalRotation, Vector2 horizontalRotationRandomness, float kickbackDuration, float kickbackSpeed)
        {
            m_Enabled = enabled;
            m_UpwardForce = upwardForce;
            m_UpwardRandomness = upwardRandomness;
            m_SidewaysForce = sidewaysForce;
            m_SidewaysRandomness = sidewaysRandomness;
            m_KickbackForce = kickbackForce;
            m_KickbackRandomness = kickbackRandomness;
            m_VerticalRotation = verticalRotation;
            m_VerticalRotationRandomness = verticalRotationRandomness;
            m_HorizontalRotation = horizontalRotation;
            m_HorizontalRotationRandomness = horizontalRotationRandomness;
            m_KickbackDuration = kickbackDuration;
            m_KickbackSpeed = kickbackSpeed;
        }

        #region PROPERTIES

        public bool Enabled => m_Enabled;

        public float UpwardForce => m_UpwardForce;

        public Vector2 UpwardRandomness => m_UpwardRandomness;

        public float SidewaysForce => m_SidewaysForce;

        public Vector2 SidewaysRandomness => m_SidewaysRandomness;

        public float KickbackForce => m_KickbackForce;

        public Vector2 KickbackRandomness => m_KickbackRandomness;

        public float VerticalRotation => m_VerticalRotation;

        public Vector2 VerticalRotationRandomness => m_VerticalRotationRandomness;

        public float HorizontalRotation => m_HorizontalRotation;

        public Vector2 HorizontalRotationRandomness => m_HorizontalRotationRandomness;

        public float KickbackDuration => m_KickbackDuration;

        public float KickbackSpeed => m_KickbackSpeed;

        #endregion
    }
