using Return.Modular;

namespace Return.Items
{



    /// <summary>
    /// Interface to activate module handle.
    /// </summary>
    public interface IItemModuleHandle : IItemModule
    {
        /// <summary>
        /// Set module handle.
        /// </summary>
        void SetModule(IItem item, object module);


    }

}