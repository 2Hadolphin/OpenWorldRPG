using UnityEngine.InputSystem;
using Return.Inputs;

namespace Return.Items.Weapons
{

    public partial class FireControlModule  // InputHandle
    {
        public class InputHandle : UserInputExtendHandle<FireControlModule>
        {
            /// <summary>
            /// 
            /// </summary>
            public HostHandler Control { get; set; }
            public bl_Recoil RecoilModule;

               #region Input

            #region User subscribe

            protected override void SubscribeInput()
            {
                //Debug.Log("Subscribe Inputs : " + Inputs);
                if (null != Input)
                {
                    var action = Input.Battle;
                    action.Enable();
                    action.MainUse.performed += TriggerOn;
                    action.MainUse.canceled += TriggerOff;
                    action.Switch.performed += SwitchMode;
                }
            }

            protected override void UnsubscribeInput()
            {
                if (null != Input)
                {
                    var action = Input.Battle;
                    action.MainUse.performed -= TriggerOn;
                    action.MainUse.canceled -= TriggerOff;
                    action.Switch.performed -= SwitchMode;
                    action.Disable();
                }
            }

            #endregion
 
            protected virtual void SwitchMode(InputAction.CallbackContext ctx)
            {
                module.SwitchMode();
            }

            protected virtual void TriggerOn(InputAction.CallbackContext ctx)
            {
                Control.isTrigger = true;
                Control.Trigger();

//#if UNITY_EDITOR
//                MonoModule.HostHandle.DebugShoot();
//#endif
            }

            protected virtual void TriggerOff(InputAction.CallbackContext ctx)
            {
                Control.isTrigger = false;
                //MonoModule.HostHandle.Trigger();
            }

   

            #endregion

        }
    }


}