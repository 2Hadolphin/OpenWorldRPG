using UnityEngine;
using UnityEngine.InputSystem;
using Return;
using Return.Cameras;
using Return.Inputs;

namespace Return.Motions
{

    public partial class Jumping  //User Input
    {
        public class UserInputHandle : UserInputExtendHandle<Jumping>
        {
            public override void SetHandler(Jumping module)
            {
                base.SetHandler(module);

                OnCamerasChange(CameraManager.ActivateCameraTypes, true);
                CameraManager.OnCamerasChange -= OnCamerasChange;
                CameraManager.OnCamerasChange += OnCamerasChange;
            }

            void OnCamerasChange(UTag tag, bool enable)
            {
                if (enable)
                {
                    switch (tag)
                    {
                        case var _ when tag.HasFlag(CameraManager.FirstPersonCamera):
                            module.QuickResponse = true;
                            break;

                        case var _ when tag.HasFlag(CameraManager.ThirdPersonCamera):
                            module.QuickResponse = false;
                            break;
                    }
                }
            }

            #region Input



            #region Binding
            public override void ResetSubscribe()
            {
                //module.ManualVault = !setting.AutoVault;

                base.ResetSubscribe();
            }


            protected override void SubscribeInput()
            {
                Input.Humanoid.Movement.performed += MotionDirection_Input;
                Input.Humanoid.Jump.performed += Jump_Input;
            }

            protected override void UnsubscribeInput()
            {
                Input.Humanoid.Movement.performed -= MotionDirection_Input;
                Input.Humanoid.Jump.performed -= Jump_Input;
            }
            #endregion

            protected virtual void MotionDirection_Input(InputAction.CallbackContext ctx)
            {
                module.MotionDirection = ctx.ReadValue<Vector2>();
            }

            protected virtual void Jump_Input(InputAction.CallbackContext ctx)
            {
                Debug.Log("Inputs Jump");
                //Inertia = Mathf.Sqrt((JumpHeight>1f?JumpHeight:JumpHeight+1f) * 2 * GravityForce);

                // => new sensor vault code
                //if (CanVault()) 
                //    Vault();
                //else
                module.Jump();
            }



            #endregion
        }
    }

}