using System;
using Return.Modular;

namespace Return.Items
{
    [Serializable]
    public abstract class ItemModuleHandle<T>: EnabledCheck, IItemModuleHandle where T : class
    {

        public T Module { get; protected set; }

        #region IItemModuleHandle

        public virtual void SetHandler(IItem module)
        {
            
        }

        public virtual void SetModule(IItem item, object itemmodule)
        {
            if (itemmodule.Parse(out T module))
                ActivateHandle(item, module);
            else
                throw new InvalidCastException("Invalid module handle cast.");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="module"></param>
        protected virtual void ActivateHandle(IItem item, T module)
        {
            if (module.NotNull())
                Module = module;
            else
                enabled = false;
        }

        #endregion

        #region IModule

        public virtual ControlMode CycleOption => ControlMode.All;

        public virtual void Register()
        {

        }

        public virtual void UnRegister()
        {

        }

        public virtual void Activate()
        {
            enabled = true;
        }

        public virtual void Deactivate()
        {
            enabled = false;
        }







        #endregion
    }

}