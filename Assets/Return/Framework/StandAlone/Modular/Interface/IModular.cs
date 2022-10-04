using Return.Modular;

namespace Return
{
    /// <summary>
    /// Abstract interface of IItemModule, more clean to code without module bhv.
    /// </summary>
    public interface IModular
    {
        /// <summary>
        /// Cycle operation configs.    **Register **Activate **Deactivate **Unregister
        /// </summary>
        ControlMode CycleOption { get; }


        /// <summary>
        /// Register before activation.
        /// </summary>
        void Register();

        /// <summary>
        /// Unregister before destory.(dispose).
        /// </summary>
        void UnRegister();

        /// <summary>
        /// Activated after registration and before interaction.
        /// </summary>
        void Activate();

        /// <summary>
        /// Deactivate to disable module after interaction and before unregister.
        /// </summary>
        void Deactivate();
    }




}