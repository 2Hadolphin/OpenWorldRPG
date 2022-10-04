using System.Collections;
using UnityEngine.InputSystem;
using Return.Humanoid.Animation;
using Sirenix.OdinInspector;
using System;
using Return.Agents;
using UnityEngine.Assertions;
using Return.Creature;
using UnityEngine;
using Return.Modular;

namespace Return.Motions
{
    /// <summary>
    /// non-mono base motion module.
    /// </summary>
    [Serializable]
    public abstract class MotionModule : Module<IMotionSystem>, IMotionModule, IInjectable
    {


        #region Setup

        protected IMotionSystem Motions;

        public override void SetHandler(IMotionSystem module)
        {
            //Debug.Log("Motion module is mine : "+motor.IsMine.GetValue());
            Motions = module;
        }

        #endregion


        #region Routine

        /// <summary>
        /// Invoke on start use to subscribe update phase.
        /// </summary>
        public virtual void SubscribeRountine(bool enable = false) { }


        protected override void OnEnable()
        {
            SubscribeRountine(enabled);
        }

        protected override void OnDisable()
        {
            SubscribeRountine(enabled);
        }

        #endregion


        #region IMotionModule

        public virtual Vector3 ModuleVector { get; protected set; }
        public virtual Vector3 ModuleVelocity { get; protected set; }
        public virtual Quaternion ModuleRotation { get; protected set; }

        #endregion

        #region State Cache

        /// <summary>
        /// Hash id to interact with sequence.
        /// </summary>
        [Obsolete]
        public int HashCode { get; protected set; }

        #endregion


        #region SequenceQueue


        [Obsolete]
        [ShowInInspector]
        bool m_enableMotion = false;

        /// <summary>
        /// Whether this module executing any motion
        /// </summary>
        public virtual bool HasMotion => m_enableMotion;//{ get; protected set; }

        /// <summary>
        /// Whether enable module solve the motion **not sensor
        /// </summary>
        protected virtual bool EnableMotion
        {
            get => m_enableMotion;
            set => m_enableMotion = value;
        }


        /// <summary>
        /// <para>(<0)Takes precedence over other</para>
        /// <para>(=0)Coexist with other</para>
        /// <para>(>0)Lower priority than other</para>
        /// </summary>
        public virtual int CompareTo(IMotionModule other)
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

        #endregion

    }
}

