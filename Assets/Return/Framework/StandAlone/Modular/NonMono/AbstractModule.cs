using System;
using UnityEngine;

namespace Return.Modular
{
    /// <summary>
    /// abstract class for non-mono module to inherit.
    /// </summary>
    public abstract class AbstractModule : EnabledCheck, IModular,IDisposable
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

        public bool initialized { get; protected set; }


        [SerializeField]
        ControlMode m_cycleOption = ControlMode.All;
        public virtual ControlMode CycleOption => m_cycleOption;


        protected virtual void Register()
        {

        }

        protected virtual void Unregister() { }

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


        public virtual void Dispose()
        {
            Debug.LogError($"{this} destroy valid : {(bool)this}.");
            //if (this)
            Unregister();
        }

        #endregion
    }
}