using UnityEngine;
using Return.InteractSystem;
using Return.Inputs;
using UnityEngine.InputSystem;
using Cysharp.Threading.Tasks;

namespace Return
{
    public partial class HumanoidInteractSystem
    {
        /// <summary>
        /// Local user input handle.
        /// </summary>
        public class UserInputHandle : UserInputExtendHandle<HumanoidInteractSystem>, ISelectionHandle<InteractWrapper>
        {
            protected override void SubscribeInput()
            {
                var input = Input.Agent;
                input.Interact.Subscribe(Interact);
                input.Interact.Subscribe(Interact_Cancel, SubscribeType.canceled);

                input.Release.Subscribe(Release);
            }

            protected override void UnsubscribeInput()
            {
                var input = Input.Agent;
                input.Interact.Unsubscribe(Interact);
                input.Interact.Subscribe(Interact_Cancel, SubscribeType.canceled);

                input.Release.Unsubscribe(Release);
            }

            protected virtual void Interact(InputAction.CallbackContext ctx)
            {
                Debug.Log("Try interact");

                if (!targets.TryPeek(out var wrapper))
                    return;

                module.Interact(wrapper);
            }

            protected virtual void Interact_Cancel(InputAction.CallbackContext ctx)
            {
                Debug.Log("Cancel interact");

                if (!targets.TryPeek(out var wrapper))
                    return;

                module.CancelInteract(wrapper);
            }

            protected virtual void Release(InputAction.CallbackContext ctx)
            {
                Debug.Log(nameof(Release));

                module.Release();
            }


            #region ISelectionHandle

            protected mStack<InteractWrapper> targets = new(1);

            public void SetHandler(ISelectionHandler<InteractWrapper> handler)
            {
                handler.OnSelected -= Handler_OnSelected;
                handler.OnSelected += Handler_OnSelected;

                handler.OnDeselected -= Handler_OnDeselected;
                handler.OnDeselected += Handler_OnDeselected;
            }

            protected virtual void Handler_OnSelected(InteractWrapper obj)
            {
                targets.Push(obj);
            }

            protected virtual void Handler_OnDeselected(InteractWrapper wrapper)
            {
                // out sensor cancel
                if (targets.TryPeek(out var peek) && peek == wrapper)
                    module.CancelInteract(peek);

                targets.Remove(wrapper);
            }

            #endregion
        }
    }
}
