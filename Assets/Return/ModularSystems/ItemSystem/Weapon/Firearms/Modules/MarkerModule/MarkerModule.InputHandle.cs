using UnityEngine.InputSystem;
using Return.Preference;
using Return.Inputs;

namespace Return.Items.Weapons
{
    public partial class MarkerModule   // Input handle **local user
    {
        public class InputHandle : UserInputExtendHandle<MarkerModule>
        {
            public bool AimToggle;

            //protected override void SetHandler(IItem item, MarkerModule module)
            //{
            //    base.SetHandler(item, module);
            //}

            //public override void SetHandler(IItem item)
            //{
            //    base.SetHandler(item);

            //    //SettingsManager.GamePlayUpdate.performed += LoadGamePlay;
            //}

            //public override void Deactivate()
            //{
            //    //SettingsManager.GamePlayUpdate.performed -= LoadGamePlay;
            //    UnsubscribeInput();
            //    base.Deactivate();
            //}


            #region Input

            protected virtual void LoadGamePlay(SettingsManager manager)
            {
                AimToggle = manager.GamePlay.AimStyle == ActionMode.Toggle;
            }


            #region Binding


            protected override void SubscribeInput()
            {
                var action = Input.Battle;

                if(!action.enabled)
                    action.Enable();

                action.Assist.performed += Aim;
                if(!AimToggle)
                    action.Assist.canceled += CancelAim;

                action.Room.performed += Room;
                action.SwitchAssist.performed += SwitchAssist;
            }

            protected override void UnsubscribeInput()
            {
                var action = Input.Battle;
                action.Assist.performed -= Aim;
                action.Assist.canceled -= CancelAim;

                action.Room.performed -= Room;
                action.SwitchAssist.performed -= SwitchAssist;
                action.Disable();
            }



            #endregion

            protected virtual void SwitchAssist(InputAction.CallbackContext ctx)
            {
                module.SwitchAssist();
            }

            protected virtual void Aim(InputAction.CallbackContext ctx)
            {
                //Debug.LogError(nameof(Aim));
                module.Aim();
            }

            protected virtual void CancelAim(InputAction.CallbackContext ctx)
            {
                module.CancelAim();
            }

            protected virtual void Room(InputAction.CallbackContext ctx)
            {
                //cam.Zoom(cam.Fov / data.Scope, 0.35f, 4);
                module.RoomIn(0);
            }
            #endregion
        }
    }

}