using Return.Agents;
using Return.Inputs;
using Return.Preference;
using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Return.Items
{
    /// <summary>
    /// Player Input controlable item module
    /// </summary>
    [Obsolete]
    public abstract class UserInputItemModule : MonoItemModule, IItemInputModule
    {
        #region Setup

        protected override void Register()
        {
            base.Register();



            try
            {
                Assert.IsFalse(Item.Agent == null);

                var resolver = Item.resolver;

                Assert.IsFalse(resolver == null);

                resolver.RegisterModule(this);

                resolver.Inject(this);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                initialized = false;
            }
        }

        /// <summary>
        /// Enable  controlable function  
        /// </summary>
        protected override void Activate()
        {
            base.Activate();


        }



        protected override void OnEnable()
        {
            base.OnEnable();

            if (Input != null)
            {
                UnsubscribeInput();
                SubscribeInput();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (Input != null)
                UnsubscribeInput();
        }

        #endregion


        #region Input

        /// <summary>
        /// Apply user preference while setting change. 
        /// </summary>
        protected virtual void LoadGamePlay(SettingsManager manager) { }


        /// <summary>
        /// RegisterHandler owner to agent whitboard and get control Input
        /// </summary>
        protected mPlayerInput Input;


        /// <summary>
        /// Register input with custom input manager.
        /// </summary>
        [Inject]
        public virtual void RegisterInput(mPlayerInput inputActions)
        {
            var subscribe = Input != inputActions;

            Input = inputActions;

            if(subscribe)
            {
                UnsubscribeInput();
                SubscribeInput();
            }
        }

        protected virtual void SubscribeInput()
        {

        }
        protected virtual void UnsubscribeInput()
        {

        }

        #endregion


    }

    /// <summary>
    /// Use control but attach as extra handle of the module.
    /// </summary>
    //[Obsolete]
    //public abstract class UserInputItemModuleExtraHandle<T>: UserInputExtendHandle<T>, IItemModuleHandle where T:class
    //{
    //    public T MonoModule { get; protected set; }

    //    //protected virtual bool EnableAtActivate => true;

    //    public void ActivateHandle(IItem item,object itemmodule)
    //    {
    //        if (itemmodule.Parse(out T module))
    //            SetHandler(item, module);
    //        else
    //            Debug.LogError("Failure to load module : " + itemmodule);
    //    }

    //    protected virtual void SetHandler(IItem item, T module)
    //    {
    //        if (module.NotNull())
    //        {
    //            MonoModule = module;
    //            enabled = true;
    //        }
    //        else
    //            enabled = false;
    //    }

    //}

}