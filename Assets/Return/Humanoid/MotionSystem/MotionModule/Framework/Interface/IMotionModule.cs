using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Modular;
using Return.Creature;
using System;

namespace Return.Motions
{

    public interface IMotionModule : IModule,IDisposable,IComparable<IMotionModule>
    {
        #region Motion State

        /// <summary>
        /// Direction of motion.
        /// </summary>
        //Vector3 ModuleVector { get; }

        /// <summary>
        /// Velocity vector of motion.
        /// </summary>
        //Vector3 ModuleVelocity { get; }

        /// <summary>
        /// Target motion rotation.
        /// </summary>
        //Quaternion ModuleRotation { get; }


        #endregion
    }

    /// <summary>
    /// Motion module interface.
    /// </summary>
    public interface IHumanoidMotionModule : IMotionModule,IModule<IMotionSystem> ,IComparable<IHumanoidMotionModule>
    {

#pragma warning disable IDE1006 // 命名樣式
        [Obsolete]
        bool enabled { get; set; }
#pragma warning restore IDE1006 // 命名樣式

        /// <summary>
        /// ?????? id?
        /// </summary>
        int HashCode { get; }

        bool HasMotion { get; }


        bool CanQueue(out int queuePriority);


        /// <summary>
        /// Invoke while module motion was break by other modules, might cause other sequence with this motion module unload.
        /// </summary>
        /// <param name="limb"> which limb cause this interruption </param>
        void InterruptMotion(Limb limb);

        /// <summary>
        /// CheckAdd whether the other limbs enable to motion
        /// </summary>
        bool Ready2Motion(Limb limb);
    }


}