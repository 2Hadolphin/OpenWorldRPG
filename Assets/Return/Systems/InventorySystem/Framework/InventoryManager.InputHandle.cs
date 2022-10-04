using Return.Inputs;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

namespace Return.Inventory
{
    public partial class InventoryManager   // InputHandle
    {

        /// <summary>
        /// Quick use.
        /// </summary>
        public class InputHandle : UserInputExtendHandle<InventoryManager>
        {

            protected override void SubscribeInput()
            {
                var input = Input.GUI;

                input.Inventory.Subscribe(InspectInventoryUI);

                // dev manual binding
                //foreach (var inputAction in Input.asset.actionMaps)
                //{
                    
                //} 
            }

            protected override void UnsubscribeInput()
            {
                var input = Input.GUI;

                input.Inventory.Unsubscribe(InspectInventoryUI);
            }

            

            protected virtual void InspectInventoryUI(InputAction.CallbackContext obj)
            {
                // show single inventory or inventory manager?
                // open manager
                if (false)
                {
                    Debug.LogException(new NotImplementedException("Inventory manager ui not finish."));
                }
                else
                {
                    foreach (var inv in module.Inventories)
                    {
                        if (inv.NotNull())
                        {
                            if(inv is Inventory inventory)
                            {
                                inventory.ShowUI();
                                return;
                            }
                        }
                    }
                }
            }



        }
    }
}