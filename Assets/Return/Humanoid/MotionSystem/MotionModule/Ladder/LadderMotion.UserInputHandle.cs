using UnityEngine;
using UnityEngine.InputSystem;
using Return.Inputs;

namespace Return.Motions
{
    public partial class LadderMotion
    {
        public class UserInputHandle : UserInputExtendHandle<LadderMotion>
        {

            #region Input
            protected override void SubscribeInput()
            {
                Input.Humanoid.Movement.performed += Movement;
                Input.Humanoid.Jump.performed += Jump;
            }


            protected override void UnsubscribeInput()
            {
                Input.Humanoid.Movement.performed -= Movement;
                Input.Humanoid.Jump.performed -= Jump;
            }
            #endregion

            private void Movement(InputAction.CallbackContext ctx)
            {
                var input = ctx.ReadValue<Vector2>();
                module.SetClimbSpeed(input.y);
                module.ValidOnLadder();
            }

            protected virtual void Jump(InputAction.CallbackContext ctx)
            {
                module.OnLadder = false;
            }
        }

    }
}