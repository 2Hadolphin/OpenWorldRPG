using System;
using UnityEngine;

namespace Return.Modular
{
    /// <summary>
    /// abstract class for mono module to inherit.
    /// </summary>
    public abstract class AbstractMonoModule : BaseComponent, IModular
    {

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
                Unregister();
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

        /// <summary>
        /// Default as true, enable module at activate 
        /// </summary>
        public virtual bool DisableAtRegister => true;

        public bool initialized { get; protected set; }

#pragma warning disable IDE1006
        public new bool enabled
#pragma warning restore IDE1006
        {
            get => base.enabled;
            set
            {
                if (base.enabled == value)
                    return;

                base.enabled = value;
            }
        }

        [SerializeField]
        ControlMode m_cycleOption = ControlMode.All;
        public virtual ControlMode CycleOption => m_cycleOption;


        protected virtual void Register()
        {
            if (DisableAtRegister)
                enabled = false;
        }

        protected virtual void Unregister() { }

        /// <summary>
        /// Will been invoke when activate.
        /// </summary>
        protected virtual void Activate()
        {
            this.CheckEnable();
        }

        /// <summary>
        /// Disable all active control function
        /// </summary>
        protected virtual void Deactivate()
        {
            this.CheckEnable(false);
        }

        /// <summary>
        /// Before <see cref="Activate"/>
        /// </summary>
        protected virtual void OnEnable()
        {

        }

        /// <summary>
        /// Before <see cref="Deactivate"/>
        /// </summary>
        protected virtual void OnDisable()
        {

        }

        void OnDestroy()
        {
            Debug.LogError($"{this} destroy valid : {(bool)this}.");
            //if (this)
            Unregister();
        }

        #endregion
    }
}