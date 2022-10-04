using System;

namespace Return
{
    /// <summary>
    /// Linking option. 
    /// </summary>
    [Flags]
    public enum InjectOption : byte
    {
        /// <summary>
        /// Local.
        /// </summary>
        Handler = 1 << 0,

        /// <summary>
        /// Root handler.
        /// </summary>
        Agent = 1 << 1,

        /// <summary>
        /// Space zone.
        /// </summary>
        Zone = 1 << 2,

        /// <summary>
        /// Global framework.
        /// </summary>
        Framework = 1 << 3,

        /// <summary>
        /// Custom register.
        /// </summary>
        Feature = 1 << 4,
    }
}