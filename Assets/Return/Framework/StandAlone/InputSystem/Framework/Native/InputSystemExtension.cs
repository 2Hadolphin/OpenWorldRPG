using UnityEngine.InputSystem;
using System;

namespace Return.Inputs
{

    public static class InputSystemExtension
    {
        /// <summary>
        /// Safe subscribe.
        /// </summary>
        public static void Subscribe(this InputAction input, Action<InputAction.CallbackContext> callback,SubscribeType type=SubscribeType.performed)
        {
            Unsubscribe(input, callback, type);

            switch (type)
            {
                case SubscribeType.started:
                    input.started += callback;
                    break;

                case SubscribeType.performed:
                    input.performed += callback;
                    break;

                case SubscribeType.canceled:
                    input.canceled += callback;
                    break;
            }
        }

        public static void Unsubscribe(this InputAction input, Action<InputAction.CallbackContext> callback, SubscribeType type = SubscribeType.performed)
        {
            switch (type)
            {
                case SubscribeType.started:
                    input.started -= callback;
                    break;

                case SubscribeType.performed:
                    input.performed -= callback;
                    break;

                case SubscribeType.canceled:
                    input.canceled -= callback;
                    break;
            }
        }
    }
}
