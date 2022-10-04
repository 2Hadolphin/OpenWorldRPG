using UnityEngine;
using System;
using System.ComponentModel;
using Unity.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Cysharp.Threading.Tasks;
using Return.Framework.Parameter;
using Return.Modular;
using Return.Motions;

namespace Return.Humanoid.Motion
{
    /// <summary>
    /// MotionSystem with anim (not vehicle)
    /// </summary>
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(ExecuteOrderList.ModuleSystem)]
    public abstract class BaseMotionSystem : ModuleHandler, IMotionSystem, IInjectable //ModularBase
    {
        protected const string MainTab = "Motion";

        /// <summary>
        /// Event to pass playable layer IK.
        /// </summary>
        public virtual event Action<int> OnAnimatorIKPhase;

        [Inject]
        public Animator Animator { get; protected set; }


        //public ReadOnlyTransform ITransform { get; protected set; }
        public abstract float ControllerRadius { get; protected set; }
        public abstract float ControllerHeight { get; protected set; }



        public override void InstallResolver(IModularResolver resolver)
        {
            // binding IMotions
            resolver.RegisterModule<IMotionSystem>(this);

            // animator
            {
                if (!resolver.TryGetModule<Animator>(out var animator))
                {
                    InstanceIfNull(ref animator);
                    resolver.RegisterModule(animator);
                }

                Animator = animator;
            }
        }

        protected override void Register()
        {
            base.Register();


            //Parameters = ScriptableObject.CreateInstance<TrashStateBoard>();
            //Parameters.hideFlags = HideFlags.DontSave;
            //Assert.IsNotNull(Parameters);
        }

        protected override void Unregister()
        {
            //Destroy(Parameters);

            base.Unregister();
        }

        #region IMotionSystem

        public virtual PlayableGraph GetGraph { get => Animator.playableGraph; }


        public Parameter<float> LocalSelfVelocity { get; protected set; }
        Parameters<float> IMotionSystem.LocalSelfVelocity => LocalSelfVelocity;


        public virtual void Rebind()
        {
            Animator.Rebind();
        }



        #region Modular

        public virtual Vector3 MotionVector { get; protected set; }
        public virtual Vector3 MotionVelocity { get; protected set; }

        public abstract IMotionModule MotionModule { get; }

        #endregion

        public abstract void SetCoordinate(Vector3 pos, Quaternion rot);

        #endregion

        // todelete=> go state ??　out system call?
        public virtual bool Controllable { get; protected set; }


        #region BlackBoard

        //[ShowInInspector]
        //[Sirenix.OdinInspector.ReadOnly]
        //[TabGroup(MainTab)]
        //[Obsolete]
        //protected TrashStateBoard Parameters;

        //[Obsolete]
        //public static implicit operator TrashStateBoard(BaseMotionSystem baseMotionSystem)
        //{
        //    return baseMotionSystem.Parameters;
        //}

        //[Obsolete]
        //public bool GetBlackBoard<T, U>(int hash, out T blackboard) where T : GenericBlackBoard<U>
        //{
        //    throw new NotImplementedException();
        //}

        /// <summary>
        /// ObsoleteMotionState => Agent State
        /// </summary>
        protected HashSet<IStatusWrapper> States { get; set; } = new HashSet<IStatusWrapper>();


        public bool GetMotionState<T,U>(T stateWrapper,out U blackboard) where T : StateWrapper where U :BlackBoard
        {
            var tokenWrapper= States.mRegisterState(stateWrapper.State as U);
            blackboard = tokenWrapper.Board;
            return tokenWrapper.VerifyToken(stateWrapper.Token);
        }

        //protected TSerializable GetMotionState<TSerializable>(TSerializable requireBlackBoard) where TSerializable : BlackBoard
        //{
        //    return States.RegisterState(requireBlackBoard) as TSerializable;
        //}

        //public void GetMotionState<TRegister,TRquire>(TRegister registerBlackBoard,out TRquire rquire ) where TRegister : BlackBoard where TRquire:TRegister
        //{
        //    if(registerBlackBoard.Parse(out rquire));
        //    rquire= States.RegisterState(rquire) as TRquire;
        //}



        #endregion
    }


    public abstract class MotionSystem : BaseMotionSystem
    {
        public virtual IMotionModule IdleModule { get; protected set; }

    }
}