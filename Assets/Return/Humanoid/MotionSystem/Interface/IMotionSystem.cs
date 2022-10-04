using UnityEngine;
using Return.Humanoid;
using Return.Humanoid.Controller;
using MxMGameplay;
using System.Collections.Generic;
using System;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using Return.Framework.Parameter;
using Return.Modular;

namespace Return.Motions
{
    public interface IMotionSystem : IAnimator//, IAgentModular
    {
        //ReadOnlyTransform ITransform { get; }

        PlayableGraph GetGraph { get; }
        Parameters<float> LocalSelfVelocity { get; }

        //float LocalSelfVelocity { get; }      

        /// <summary>
        /// Direction of motion velocity(global).
        /// </summary>
        Vector3 MotionVector { get; }

        /// <summary>
        /// Vector of motion velocity(global).
        /// </summary>
        Vector3 MotionVelocity { get; }

        void Rebind();

        #region Coordinate

        /// <summary>
        /// Set controller coordinate.
        /// </summary>
        void SetCoordinate(Vector3 pos, Quaternion rot);

        #endregion


        #region Resolver

        /// <summary>
        /// Container to hold all agent module. **Motions **Inventory **Interact
        /// </summary>
        IModularResolver Resolver { get; }

        #endregion


        IMotionModule MotionModule { get; }

        #region Motion System States


       /// <summary>
       /// get other system blackboard
       /// </summary>
       /// <typeparam name="T">Blackboard</typeparam>
       /// <typeparam name="U">Blackboard value</typeparam>
       /// <param name="hash">global state hash</param>
       /// <param name="blackboard">require blackboard</param>
       /// <returns>return true if contains this state</returns>
       //[Obsolete]
       // bool GetBlackBoard<T, U>(int hash, out T blackboard) where T : GenericBlackBoard<U>;

        /// <summary>
        /// RegisterHandler motion state blackboard
        /// </summary>
        /// <typeparam name="T">The type of motion state.</typeparam>
        /// <param name="requireBlackBoard">Motion state preset. (Require)</param>
        /// <returns>Return exist motion state or instance new one.</returns>
        //TSerializable GetMotionState<TSerializable>(TSerializable requireBlackBoard) where TSerializable : BlackBoard;
        //[Obsolete]
        //bool GetMotionState<T, U>(T stateWrapper, out U blackboard) where T : StateWrapper where U : BlackBoard;
        //void GetMotionState<TRegister, TRquire>(TRegister registerBlackBoard, out TRquire rquire) where TRegister : BlackBoard where TRquire : TRegister;


        #endregion
    }
}