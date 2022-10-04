using Return.Inputs;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Return.Cameras
{
    public class FreeLookCamera : CustomCamera,IAimer
    {

        [SerializeField]
        protected bool ActivateAtStart=true;

        public override Transform HostObject => null;

        [SerializeField]
        protected Transform RootTransform;

        [Title("Yaw")]
        [SerializeField]
        protected Transform YawTransform;
        [ShowInInspector,NonSerialized]
        protected float m_Yaw;

        [Title("Pitch")]
        [SerializeField]
        protected Transform PitchTransform;
        [ShowInInspector, NonSerialized]
        protected float m_Pitch;

        protected void cahcheTransform()
        {
            if (RootTransform.IsNull())
                RootTransform = transform;

            // init yaw
            if (YawTransform.IsNull())
                YawTransform = RootTransform;
            m_Yaw = YawTransform.localEulerAngles.y;

            // init pitch
            if (PitchTransform.IsNull())
                PitchTransform = YawTransform;
            m_Pitch = PitchTransform.localEulerAngles.x;
        }

        #region IAimer

        public float yaw => m_Yaw;

        public Quaternion yawLocalRotation => YawTransform.localRotation;

        public float pitch => m_Pitch;

        public Quaternion pitchLocalRotation => PitchTransform.localRotation;

        #endregion


        public override void SetTarget(Transform tf)
        {
            
        }


        public override void Activate()
        {
            cahcheTransform();

            base.Activate(); 
    
            if(CreateIfNull(ref InputHandle))
            {
                InputHandle.RegisterInput(InputManager.Input);
                InputHandle.SetHandler(this);

                // debug
                InputHandle.enabled = ActivateAtStart;
            }


        }

        /// <summary>
        /// How many degree can turn in one second.
        /// </summary>
        [SerializeField]
        protected float TurnDegree = 270;

        [Title("Sensitivity")]
        [SerializeField]
        [LabelText("Horizontal")]
        [Range(0.01f,3f)]
        protected float Sensitivity_Horizontal=1;

        [SerializeField]
        [LabelText("Vertical")]
        [Range(0.01f, 3f)]
        protected float Sensitivity_Vertical=1;

        [ShowInInspector]
        Vector2 Mouse;

        [Title("Constraint")]

        [LabelText("Pitch")]
        [SerializeField]
        protected Vector2 Clamp_Axis_X = new(-90, 90);

        [LabelText("Yaw")]
        [SerializeField]
        protected Vector2 Clamp_Axis_Y = new(-360, 360);

        [SerializeField]
        protected Vector2 Clamp_Axis_Z = new(0, 0);


        protected override void OnEnable()
        {
            base.OnEnable();
            cahcheTransform();
        }

        private void LateUpdate()
        {
            var deltaTime = ConstCache.deltaTime;

            // yaw
            var hRot = Mouse.x * deltaTime * Sensitivity_Horizontal*TurnDegree;

            // clamp yaw
            if ((hRot+ m_Yaw).Flow(Clamp_Axis_Y.x, Clamp_Axis_Y.y,out var vflow))
                hRot -= vflow;

            // apply
            m_Yaw = (m_Yaw + hRot).WrapAngle();
            var yaw = YawTransform.localEulerAngles;
            yaw.y = m_Yaw;
            YawTransform.localEulerAngles = yaw;
            


            // pitch
            var vRot = -Mouse.y * deltaTime * Sensitivity_Vertical * TurnDegree;

            // clamp pitch
            if ((vRot + m_Pitch).Flow(Clamp_Axis_X.x, Clamp_Axis_X.y, out var hflow))
                vRot -= hflow;

            // apply
            m_Pitch = (m_Pitch+vRot).WrapAngle();// Lerp.Lerp(m_Pitch,m_Pitch+ vRot);
            var pitch = PitchTransform.localEulerAngles;
            pitch.x = m_Pitch;
            PitchTransform.localEulerAngles = pitch;
            //PitchTransform.Rotate(new Vector3(vRot, 0), Space.Self);
        }

        #region Input

        [ShowInInspector,NonSerialized]
        UserInputHandle InputHandle;

        public class UserInputHandle : UserInputExtendHandle<FreeLookCamera>
        {
            

            protected override void SubscribeInput()
            {
                Input.FreeCam.ViewPort.Subscribe(CamRotate);
            }

            protected override void UnsubscribeInput()
            {
                Input.FreeCam.ViewPort.Unsubscribe(CamRotate);
            }


            private void CamRotate(InputAction.CallbackContext ctx)
            {
                module.Mouse = ctx.ReadValue<Vector2>();
            }


            private void Room(InputAction.CallbackContext ctx)
            {
                var room = ctx.ReadValue<float>();
            }

        }

        #endregion
    }
}