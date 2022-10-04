using System;

namespace Return.Humanoid
{
    /// <summary>
    /// Character controller interface.
    /// </summary>
    public interface IHumanoidController
    {
        /// <summary>
        /// Perfact adjust.
        /// </summary>
        public event Action<PR> OnAfterMotion;
    }
}
