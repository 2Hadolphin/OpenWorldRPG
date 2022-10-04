using UnityEngine;
using Cinemachine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;


namespace Return.Cameras
{
    /// <summary>
    /// Input module for Cinemachine
    /// </summary>
    [RequireComponent(typeof(CinemachineFreeLook))]
    public class VirtualCamInputModule : MonoBehaviour, AxisState.IInputAxisProvider
    {
        [SerializeField]
        public CameraSetting Setting;
        Transform Transform;

        [ShowInInspector]
        CinemachineFreeLook Cam;
        Vector2 value;

        [ShowInInspector]
        Vector2 Values;
        float Room;
        private void Start()
        {
            gameObject.InstanceIfNull(ref Cam);
            Cam.m_XAxis.SetInputAxisProvider(0, this);
            Cam.m_YAxis.SetInputAxisProvider(1, this);
        }

        public void MouseMove_performed(InputAction.CallbackContext obj)
        {
            value = obj.ReadValue<Vector2>();
        }

        public void Room_performed(InputAction.CallbackContext obj)
        {
            Room = obj.ReadValue<float>();
        }

        private void _LateUpdate()
        {
            //Cam.m_XAxis.SetInputAxisProvider(0, this);
            //Cam.m_YAxis.SetInputAxisProvider(1, this);
        }

        void LateUpdate()
        {
            // Create the look Input vector for the camera
            float mouseLookAxisUp = value.y;
            if (Cam.m_XAxis.m_InvertInput)
                mouseLookAxisUp *= -1;

            float mouseLookAxisRight = value.x;
            if (Cam.m_YAxis.m_InvertInput)
                mouseLookAxisRight *= -1;

            // Inputs for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = -Room;

            var delta = ConstCache.deltaTime;

            Values.x = mouseLookAxisRight * delta * Setting.Sensitivity_V;
            Values.y = mouseLookAxisUp * delta * Setting.Sensitivity_H;



            //Cam.m_XAxis.m_InputAxisValue = mouseLookAxisUp * delta*Setting.Sensitivity_V;
            //Cam.m_YAxis.m_InputAxisValue = mouseLookAxisRight * delta*Setting.Sensitivity_H;
        }

        public float GetAxisValue(int axis)
        {
            return Values[axis];
        }
    }
}