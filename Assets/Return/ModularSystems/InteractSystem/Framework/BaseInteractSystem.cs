using UnityEngine.Animations.Rigging;
using System.Linq;
using Return.InteractSystem;
using Return.Inputs;
using UnityEngine.InputSystem;
using System;
using Cysharp.Threading.Tasks;
using Return.Modular;
using UnityEngine;

namespace Return
{
    [DisallowMultipleComponent]
    public abstract class BaseInteractSystem : ModuleHandler, IInteractHandler
    {
        public virtual event Action<InteractWrapper> OnInteractMission;
        public virtual event Action<InteractWrapper> OnCancelMission;

        /// <summary>
        /// Order interact system interact with target. (self agent or child module)
        /// </summary>
        public abstract UniTask<bool> Interact(object obj);

        /// <summary>
        /// await interactable dispose and schedule remaining option **drop **storage
        /// </summary>
        public abstract void Release();




        //public class InputHandle : UserInputExtendHandle<BaseInteractSystem>
        //{
        //    protected override void SubscribeInput()
        //    {
        //        var input = Input.Agent;
        //        input.Release.Subscribe(Release);


        //    }

        //    protected override void UnsubscribeInput()
        //    {
        //        var input = Input.Agent;
        //        input.Release.Unsubscribe(Release);


        //    }


        //    protected virtual void Release(InputAction.CallbackContext ctx)
        //    {
        //        // drop or storage
        //        module.Release();
        //    }
        //}
    }



}
