using UnityEngine;
using UnityEngine.InputSystem;
using Return.Preference.GamePlay;
using Return.Inputs;
using Return;

namespace Return.Humanoid.Motion
{
    public partial class Locomotion //  **User input handle
    {
        #region User Input

        public class InputHandle : UserInputExtendHandle<Locomotion>
        {


            #region Binding

            public override void ResetSubscribe()
            {
                //switch (setting.SprintStyle)
                //{
                //    case ActionMode.Toggle:
                //        ToggleSprint = true;
                //        break;
                //    case ActionMode.Hold:
                //        ToggleSprint = false;
                //        break;
                //}

                base.ResetSubscribe();
            }

        

            protected override void SubscribeInput()
            {
                //Debug.Log("Bind input");
                var input = Input.Humanoid;

                //input.Movement.performed += (width) => { Debug.Log("Locomotion_Movement"); };
                //input.Squat.performed += (width) => { Debug.Log("Locomotion_Crouch"); };

                input.Movement.Subscribe(Input_Movement);
                input.Rush.Subscribe(Input_Sprint);

                if (ToggleSprint)
                    input.Rush.Subscribe(Input_Sprint_Cancel,SubscribeType.canceled);

                input.Squat.Subscribe(Input_Crouch);

                if (ToggleCrouch)
                    input.Squat.Subscribe(Input_Crouch_Cancel, SubscribeType.canceled);

                if(!input.enabled)
                    input.Enable();
            }

            protected override void UnsubscribeInput()
            {
                //Debug.LogError(nameof(UnsubscribeInput));

                var input = Input.Humanoid;

                input.Movement.performed -= Input_Movement;
                input.Rush.performed -= Input_Sprint;

                input.Rush.canceled -= Input_Sprint_Cancel;

                input.Squat.performed -= Input_Crouch;
            }


            #endregion


            Vector2 lastInput;

            protected void Input_Movement(InputAction.CallbackContext ctx)
            {
                var inputMov = ctx.ReadValue<Vector2>();

                Debug.Log($"{nameof(Input_Movement)} : {inputMov}");

                if (lastInput == inputMov)
                    return;
                else
                    lastInput = inputMov;

               var sqrInput = inputMov.sqrMagnitude;

                if (sqrInput == 0)
                    module.duration = 0;

                module.Input_Movement = inputMov;
                module.sqrInput = sqrInput;

                //Debug.Log(inputMov);

                module.ActivateMotion();

                //Debug.Log($"Input Frame : {Time.frameCount} hash : {this.GetHashCode()} {inputMov}.");

            }

            #region Crouch

            protected bool ToggleCrouch;

            protected virtual void Input_Crouch(InputAction.CallbackContext ctx)
            {
                Debug.Log(nameof(Input_Crouch));
                module.StartCrouch();
            }

            protected virtual void Input_Crouch_Cancel(InputAction.CallbackContext ctx)
            {
                module.StopCrouch();
            }

            #endregion

            #region Sprint
            protected bool ToggleSprint;

            protected void Input_Sprint(InputAction.CallbackContext ctx)
            {
                if (ToggleSprint && module.MovementGrade > 0)
                    module.StopSprint();
                else
                    module.StartSprint();
            }

            protected void Input_Sprint_Cancel(InputAction.CallbackContext ctx = default)
            {
                module.StopSprint();
            }


            #endregion

        }

        #endregion
    }
}