using Return.Modular;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Assertions;
using System;

namespace Return.Items
{

    /// <summary>
    /// Mono instance Item module.
    /// </summary>
    public abstract class MonoItemModule: MonoModule<IItem>, IItemModule
    {
        #region Hanlder

        //public override void SetHandler(object handler)
        //{
        //    base.SetHandler(handler);
        //}

        public IItem Item { get; protected set; }

        public override void SetHandler(IItem module)
        {
            Debug.Log($"Set item handler {module}.");
            Assert.IsFalse(module == null);
            Item = module;
        }

        #endregion

        #region Routine

        /// <summary>
        /// Phase 2, Enable active function **RegisterHandler when awake, not here
        /// </summary>
        protected override void Activate()
        {
            base.Activate();

            if (CycleOption.HasFlag(ControlMode.Deactivate))
            {
                Item.OnItemDeactivate -= Deactivate;
                Item.OnItemDeactivate += Deactivate;
            }
        }
 
        #endregion


    }

    /// <summary>
    /// Non-Mono item module.
    /// </summary>
    public abstract class ItemModule : EnabledCheck, IItemModule,IDisposable
    {
        #region Hanlder

        public IItem Item { get; protected set; }

        public virtual void SetHandler(IItem module)
        {
            Assert.IsFalse(module == null,$"{module}");
            Item = module;
        }

        #endregion


        #region IModule

        void IModular.Register()
        {
            try
            {
                Register();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            initialized = true;
        }

        void IModular.UnRegister()
        {
            try
            {
                UnRegister();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                initialized = false;
            }
        }

        void IModular.Activate()
        {
            try
            {
                Activate();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            enabled = true;
        }

        void IModular.Deactivate()
        {
            try
            {
                Deactivate();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return;
            }

            enabled = false;
        }

        #endregion


        #region Routine

        public bool initialized { get; protected set; }


        public virtual ControlMode CycleOption => ControlMode.All;


        public virtual void Register() { }

        public virtual void UnRegister() { }

        /// <summary>
        /// Will been invoke when activate.
        /// </summary>
        protected virtual void Activate()
        {
            enabled = true;
        }

        /// <summary>
        /// Disable all active control function
        /// </summary>
        protected virtual void Deactivate()
        {
            enabled = false;
        }


        protected override void OnEnable()
        {
            if (initialized)
                Activate();
        }

        protected override void OnDisable()
        {
            if (initialized)
                Deactivate();
        }

        public void Dispose()
        {
            Debug.LogError($"{this} destroy valid : {(bool)this}.");
            UnRegister();
        }


        #endregion

    }

}