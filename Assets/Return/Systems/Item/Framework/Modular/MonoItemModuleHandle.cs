using UnityEngine;

namespace Return.Items
{
    /// <summary>
    /// Mono 
    /// </summary>
    public abstract class MonoItemModuleHandle<T> : ItemModuleHandle where T: MonoItemModule
    {
        public T Module { get; protected set; }

        /// <summary>
        /// SetHandler handle via object.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="itemmodule"></param>
        public override void SetModule(IItem item, object itemmodule)
        {
             if (itemmodule.Parse(out T module))
                ActivateHandle(item, module);
            else
                Debug.LogError("Faliure to parse module " + itemmodule);
        }

        /// <summary>
        /// SetHandler handle(cast).
        /// </summary>
        /// <param name="item">item</param>
        /// <param name="module">parent module</param>
        protected virtual void ActivateHandle(IItem item, T module)
        {
            if (module.NotNull())
            {
                Module = module;
                base.Activate();
            }
            else
                enabled = false;
        }
    }

}