using System;

namespace Return.Modular
{
    /// <summary>
    /// Option to define module lifetime, 
    /// **Posture **Performer
    /// RegisterHandler => Activate => Deactivate =>Dispose
    /// </summary>
    [Flags]
    public enum ControlMode
    {
        None = 0,

        /// <summary>
        /// RegisterHandler when module load to handler.
        /// </summary>
        Register = 1 << 0,

        /// <summary>
        /// Execute activation when item activated.
        /// </summary>
        Activate = 1 << 1,

        /// <summary>
        /// Recive deactivate post from item.
        /// </summary>
        Deactivate = 1 << 2,

        /// <summary>
        /// Invoke while item release agnet binding.
        /// </summary>
        Unregister = 1 << 3,


        All = Register | Activate | Deactivate | Unregister
    }

}