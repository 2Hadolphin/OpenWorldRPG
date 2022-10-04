using UnityEngine.InputSystem;
using Return.Inputs;

namespace Return.Items.Weapons
{
    public partial class AmmunitionModule   // **User Input
    {

        #region User

        public class InputHandle : UserInputExtendHandle<AmmunitionModule>
        {

            #region Input

            protected override void SubscribeInput()
            {
                // register invertory habbit 
                // link battle system
                var action = Input.Battle;
                action.Enable();
                action.Reload.performed += Reload;
                action.ChargingBolt.performed += ChargingBolt;
            }

            protected override void UnsubscribeInput()
            {
                var action = Input.Battle;
                action.Reload.performed -= Reload;
                action.ChargingBolt.performed -= ChargingBolt;
                action.Disable();
            }

            protected virtual void Reload(InputAction.CallbackContext ctx)
            {
                module.Reload();
            }

            protected virtual void ChargingBolt(InputAction.CallbackContext ctx)
            {
                module.ChargingBolt();
            }

            #endregion

         }

        #endregion
    }
}