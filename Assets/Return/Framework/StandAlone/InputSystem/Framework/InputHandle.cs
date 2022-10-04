using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Return.Modular;

namespace Return.Inputs
{

    /// <summary>
    /// Player input module.
    /// </summary>
    public abstract class InputHandle : Module, IUserInputHandle
    {
        /// <summary>
        /// Unsubscribe before subscribe input action.
        /// </summary>
        protected virtual bool safeSubscribe => false;

        /// <summary>
        /// Clean input handle reference before release memory.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            UnsubscribeInput();
        }

        #region Input

        // obsolete
        //protected virtual void LoadGamePlay(SettingsManager manager) { }

        /// <summary>
        /// Apply user preference while setting change, 
        /// reload preference and re-subscribe input actions.
        /// </summary>
        public virtual void ResetSubscribe()
        {
            UnsubscribeInput();
            SubscribeInput();
        }


        /// <summary>
        /// Register owner to agent whitboard and get control Input
        /// </summary>
        protected mPlayerInput Input;

        /// <summary>
        /// Register input actions. **local user **remote player
        /// </summary>
        public virtual void RegisterInput(mPlayerInput inputActions = null)
        {
            if (inputActions == null)
            {
                inputActions = InputManager.Input;
                Debug.LogWarning("Register input invalid, will change as InputManager.");
            }

            Input = inputActions;
        }

        /// <summary>
        /// Subscribe input actions.
        /// </summary>
        protected abstract void SubscribeInput();

        /// <summary>
        /// Unsubscribe input actions.
        /// </summary>
        protected abstract void UnsubscribeInput();


        protected override void OnEnable()
        {
            //Debug.Log("Enable Input Handle " + this);
            if (Input != null)
            {
                if(safeSubscribe)
                    UnsubscribeInput();

                SubscribeInput();
            }
        }

        protected override void OnDisable()
        {
            if (Input != null)
                UnsubscribeInput();
        }

        #endregion

    }

    /// <summary>
    /// Use control but attach as extra handle of the module.
    /// </summary>
    public abstract class UserInputExtendHandle<T> : InputHandle
    {
#pragma warning disable IDE1006 // 命名樣式
        public T module { get; protected set; }
#pragma warning restore IDE1006 // 命名樣式

        /// <summary>
        /// Bind module to input handle and set config.
        /// </summary>
        public virtual void SetHandler(T module)
        {
            this.module = module;
        }
    }
}
