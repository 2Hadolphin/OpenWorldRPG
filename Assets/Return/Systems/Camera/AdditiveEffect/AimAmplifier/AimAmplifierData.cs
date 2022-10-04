using UnityEngine;
using System;

namespace Return.Cameras
{
    public class AimAmplifierData : PresetDatabase
    {
        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the resulting weapon rotation side to side")]
        protected float m_HorizontalMultiplier = 1f;
        public float HorizontalMultiplier => m_HorizontalMultiplier;

        [SerializeField, Range(-2f, 2f), Tooltip("The multiplier for the resulting weapon rotation up and down")]
        protected float m_VerticalMultiplier = 1f;
        public float VerticalMultiplier => m_VerticalMultiplier;

        [SerializeField, Range(0f, 1f), Tooltip("How sensitive the sway is to camera rotation. Higher sensitivity means the sway approaches its peak with slower rotations")]
        protected float m_Sensitivity = 0.5f;
        public float Sensitivity => m_Sensitivity;

        [SerializeField, Range(0.1f, 1f), Tooltip("Approximately the time it will take to reach the target rotation. A smaller value will reach the target faster")]
        protected float m_DampingTime = 0.2f;
        public float DampingTime => m_DampingTime;

        [SerializeField, Range(0f, 50), Tooltip("The max difference degree aimer can provide, will remap into ratio.")]
        protected float m_HorizontalClamp = 30;
        public float HorizontalClamp => m_HorizontalClamp;

        [SerializeField, Range(0f, 90), Tooltip("The max difference degree aimer can provide, will remap into ratio.")]
        protected float m_VerticalClamp = 10;
        public float VerticalClamp => m_VerticalClamp;


        [SerializeField, Range(0f, 1f), Tooltip("The max distance target can move align horizontal axis.")]
        protected float m_HorizontalDisplacement = 0.01f;
        public float HorizontalDisplacement => m_HorizontalDisplacement;

        [SerializeField, Range(0f, 1f), Tooltip("The max distance target can move align vertical axis.")]
        protected float m_VerticalDisplacement = 0.01f;
        public float VerticalDisplacement => m_VerticalDisplacement;

        [SerializeField, Tooltip("The max rotation target can rotate align local axis.")]
        protected Vector3 m_HorizontalRotation = new(10, 30, 30);
        public Vector3 HorizontalRotation => m_HorizontalRotation;
    }

}