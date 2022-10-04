using System;
using UnityEngine;

namespace Return
{
    /// <summary>
    /// 
    /// </summary>
    [Obsolete]
    public interface IControllerVelocityWrapper
    {
        public void Main(ref Vector3 currentVelocity, float deltaTime);

        /// <summary>
        /// Valid module finish
        /// </summary>
        public bool Finish(ref Vector3 currentVelocity, float deltaTime);
        public bool ApplyGravity { get; }

        /// <summary>
        /// ????
        /// </summary>
        public bool Override { get; }
    }
}