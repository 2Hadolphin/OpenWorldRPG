using UnityEngine;
using UnityEngine.InputSystem;
using Return.Inputs;

namespace Return.Cameras
{
    public partial class FreeCamera // InputHandle
    {
        //private void CamRot()
        //{
        //    float xRot = cam.transform.eulerAngles.width;
        //    if (xRot > 180)
        //    {
        //        xRot -= 360;
        //        xRot -= UnityEngine.Input.GetAxisRaw(LeftRight) * RotSensivitity;
        //        xRot = xRot < -89 ? -89 : xRot;
        //    }
        //    else
        //    {
        //        xRot -= UnityEngine.Input.GetAxisRaw(LeftRight) * RotSensivitity;
        //        xRot = xRot > 89 ? 89 : xRot;
        //    }

        //    Vector3 v = new Vector3(xRot, cam.transform.eulerAngles.height + UnityEngine.Input.GetAxisRaw(UpDown) * RotSensivitity, 0);

        //    cam.transform.eulerAngles = v;
        //}

        public class InputHandle : UserInputExtendHandle<FreeCamera>
        {
            protected override void SubscribeInput()
            {
                Input.FreeCam.Movement.Subscribe(CamMove);
                Input.FreeCam.ViewPort.Subscribe(CamRotate);

                Input.FreeCam.SpeedUp.Subscribe(SpeedUp_Start);
                Input.FreeCam.SpeedUp.Subscribe(SpeedUp_Stop, SubscribeType.canceled);
            }

            protected override void UnsubscribeInput()
            {
                Input.FreeCam.Movement.Unsubscribe(CamMove);
                Input.FreeCam.ViewPort.Unsubscribe(CamRotate);

                Input.FreeCam.SpeedUp.Unsubscribe(SpeedUp_Start);
                Input.FreeCam.SpeedUp.Unsubscribe(SpeedUp_Stop, SubscribeType.canceled);
            }



            private void CamMove(InputAction.CallbackContext ctx)
            {
                var value = ctx.ReadValue<Vector2>();
                module.Movement = value;
            }

            private void CamRotate(InputAction.CallbackContext ctx)
            {
                module.Mouse = ctx.ReadValue<Vector2>();
            }

            private void SpeedUp_Start(InputAction.CallbackContext ctx)
            {
                module.SpeedUp = true;
            }

            private void SpeedUp_Stop(InputAction.CallbackContext ctx)
            {
                module.SpeedUp = false;
            }

            private void Room(InputAction.CallbackContext ctx)
            {
                var room = ctx.ReadValue<float>();

            }

        }
    }
}