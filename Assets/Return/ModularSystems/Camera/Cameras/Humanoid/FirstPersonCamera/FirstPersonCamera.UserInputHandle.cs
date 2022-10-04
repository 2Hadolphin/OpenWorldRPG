using UnityEngine;
using Return;
using UnityEngine.InputSystem;
using Return.Inputs;
using Return.Preference.GamePlay;

namespace Return.Cameras
{
    public partial class FirstPersonCamera // **UserInputHandle
    {
        public class UserInputHandle : UserInputExtendHandle<FirstPersonCamera>
        {
            public override void ResetSubscribe()
            {
                base.ResetSubscribe();
            }

            protected override void SubscribeInput()
            {
                Input.Humanoid.MouseRotate.Subscribe(InputControl);
            }

            protected override void UnsubscribeInput()
            {
                Input.Humanoid.MouseRotate.Unsubscribe(InputControl);
            }

            protected void InputControl(InputAction.CallbackContext ctx)
            {
                // valid cursor 
                if (!ConstCache.captureMouseCursor)
                {
                    //Debug.LogWarning("Cursor state invalid.");
                    return;
                }
                //Debug.Log($"Frame : {Time.frameCount} pahse : {ctx.phase} value : {ctx.ReadValue<Vector2>()}.");

                Vector2 input =ctx.ReadValue<Vector2>();
                module.Performe(input);
            }


        }
    }

}