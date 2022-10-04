using UnityEngine;
using Sirenix.OdinInspector;
using Return.Creature;
using Return.Modular;
using Return.Motions;
using Return.Framework.Parameter;
using System;

namespace Return.Humanoid
{
    [DefaultExecutionOrder(ExecuteOrderList.ChildModules)]
    public abstract class MonoMotionModule : MonoModule, IMotionModule, IInjectable//,IComparable<MotionModule>
    {
        #region Hanlder

        IMotionSystem MotionSystem;


        public virtual void SetHandler(IMotionSystem handler)
        {
            MotionSystem = handler;
        }


        #endregion

        #region IMotionModule

        public virtual Vector3 ModuleVector { get; protected set; }
        public virtual Vector3 ModuleVelocity { get; protected set; }
        public virtual Quaternion ModuleRotation { get; protected set; }

        #endregion

        #region Routine


        /// <summary>
        /// Invoke on activate.
        /// </summary>
        public virtual void SubscribeRountine(bool enable)
        {

        }

        #endregion


        #region SequenceQueue

        /// <summary>
        /// ????
        /// </summary>
        public int HashCode { get; protected set; }

        /// <summary>
        /// Whether this module executing any motion
        /// </summary>
        public virtual bool HasMotion => m_EnableMotion;//{ get; protected set; }

        /// <summary>
        /// Whether enable module solve the motion **not sensor
        /// </summary>
        protected virtual bool EnableModuleMotion
        { 
            get=>m_EnableMotion;
            set=>m_EnableMotion=value;
        }
 

        [ShowInInspector,ReadOnly]
        bool m_EnableMotion=false;

        /// <summary>
        /// <para>(<0)Takes precedence over other</para>
        /// <para>(=0)Coexist with other</para>
        /// <para>(>0)Lower priority than other</para>
        /// </summary>
        public virtual int CompareTo(IHumanoidMotionModule other)
        {
            return 1;
        }

        /// <summary>
        /// Is this module can wait in queue, pass max queue number if true
        /// </summary>
        public virtual bool CanQueue(out int priority) 
        {
            priority = 0;
            return false;
            //return GetData.Order.MaxQueueNumber; 
        }


        /// <summary>
        /// If module motion finish invoke this func
        /// </summary>
        protected abstract void FinishModuleMotion();

        public void Dispose()
        {
            
        }

        public int CompareTo(IMotionModule other)
        {
            return 1;
        }


        #endregion
    }
}

