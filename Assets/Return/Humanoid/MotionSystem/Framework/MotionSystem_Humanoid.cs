using UnityEngine;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Return.Humanoid.Motion;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using Return.Agents;
using Return.Modular;
using Return.Humanoid;

namespace Return.Motions
{
    /// <summary>
    /// Motion system to handle humanoid character controller.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public partial class MotionSystem_Humanoid : MotionSystem, IHumanoidMotionSystem
    {
        #region IResolveHandle

        public override void InstallResolver(IModularResolver resolver)
        {
            base.InstallResolver(resolver);

            resolver.RegisterModule<IHumanoidMotionSystem>(this);

            InstallResolver_KCC();
            InstallResolver_MxM();

        }


        #endregion


        #region Agent

        [Inject]
        protected virtual void Inject_Agent(IAgent agent)
        {
            IsLocalUser = agent.IsLocalUser();
        }

        public virtual bool IsLocalUser { get; protected set; }

        #endregion

        #region Preset

        [TabGroup(MainTab)]
        public HumanoidMotionSystemPreset Preset;

        #endregion

        #region IMotionSystem

        public override IMotionModule MotionModule => Module;

        public override void SetCoordinate(Vector3 pos, Quaternion rot)
        {
            SetPositionAndRotation(pos, rot, true);
        }

        #endregion

        #region Cache

        public Transform Transform { get; protected set; }

        #endregion


        public virtual void Activate_MotionSystem()
        {
            Transform = transform;
            Init_Bounds();
        }


        #region Routine

        public event Action<float> UpdatePost;

        public event Action<float> OnFixedUpdate;


        protected override void Register()
        {
            base.Register();

            Register_PlayableGraph();

            // if use mxm or animator controller
            Register_MxM();


            Register_Net();

            //UpdatePost = new UEvent<float>(this);
            //OnFixedUpdate = new UEvent<float>(this);

            Register_KCC();

            Register_Trajectory();

            Register_MxMRootmotion();

            //AddPlayableIK(StepIK.CreateJob(MotionGraph));

            //if (UseRigging)
            //    BuildRiggingIK();


        }

        protected override void Unregister()
        {
            UnistallModules();

            Unregister_Grpah();

            OnDestroy_Trajectory();

            base.Unregister();
        }

        protected override void Activate()
        {
            base.Activate();

            try
            {
                //  Idle state
                {
                    if (IdleModule == null)
                        IdleModule = new IdleMotion();

                    InstallModule(IdleModule);

                    SetModule(IdleModule);

                    IdleModule.Activate();
                }

                Activate_MotionSystem();

                Activate_MxMRootMotion();

                Activate_MxM();

                //OnEnable_KCC();
                OnEnable_Trajectory();

                OnEnable_Net();


                Activate_KCC();


                // m_blendSpaceLayers = GetComponent<MxMBlendSpaceLayers>();
                //m_mxmAnimator.BlendInLayer(m_blendSpaceLayers.LayerId, 4, 1f);

                //m_Trajectory.InputProfile = m_generalLocomotion;

                //Event_TrajectoryMoveMode = new Event<ETrajectoryMoveMode>(this);

                //BuildRiggingIK();


                //Debug.LogException(new InvalidOperationException("SetHandler Motions."));

                LoadPresetModules();

                ActivateModules();

                //SetModules(false);

                //Invoke(nameof(ActivateModules),0.1f);

                MotionGraph.Play();
                //MotionGraph.Evaluate();

                EnableGravity = true;

            }
            catch (Exception e)
            {
                enabled = false;
                Debug.LogException(e);
                return;
            }
            finally
            {
                Routine.OnFixedUpdateEarly.Subscribe(FixedTick);
                Routine.OnUpdateEarly.Subscribe(Tick);
            }

        }


        protected override void Deactivate()
        {
            DeactivateModules();

            Routine.OnFixedUpdateEarly.Unsubscribe(FixedTick);
            Routine.OnUpdateEarly.Unsubscribe(Tick);

            OnDisable_Trajectory();

            base.Deactivate();
        }



        protected override void OnDestroy()
        {
            Unregister_Grpah();

            OnDestroy_Trajectory();

            base.OnDestroy();
        }

        /// <summary>
        /// Load module from presets.
        /// </summary>
        protected virtual void LoadPresetModules()
        {
            foreach (var moduleData in Preset.MotionModules)
            {
                try
                {
                    Debug.Log($"Loading motion module : {moduleData.name}");

                    if (InstallModule(moduleData, out IHumanoidMotionModule module))
                    {
                        Debug.Log($"Install Module {module}");
                        //module.enabled = true;
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }

        }

        protected virtual void ActivateModules()
        {
            var modules = Modules.CacheLoop();

            foreach (IHumanoidMotionModule motion in modules)
                if (motion.CycleOption.HasFlag(ControlMode.Activate))
                    motion.Activate();
        }

        protected virtual void DeactivateModules()
        {
            var modules = Modules.CacheLoop();

            foreach (IHumanoidMotionModule motion in modules)
                if (motion.CycleOption.HasFlag(ControlMode.Deactivate))
                    motion.Deactivate();
        }

        protected virtual void UnistallModules()
        {
            var modules = Modules.CacheLoop();

            foreach (IHumanoidMotionModule motion in modules)
                if (motion.CycleOption.HasFlag(ControlMode.Unregister))
                {
                    motion.UnRegister();
                    Modules.Remove(motion);
                }

        }

        protected virtual void Tick()
        {
            try
            {
                Update_TrajectoryInput();

                Update_KCC();

                var deltatime = ConstCache.deltaTime;

                UpdatePost?.Invoke(deltatime);

                Assert.IsNotNull(Motor.CharacterController);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                enabled = false;
            }
        }

        protected virtual void FixedTick()
        {
            try
            {
                CaculateBounds();
                FixedUpdate_Trajectory();
                FixedUpdate_KCC();

                var deltaTime = ConstCache.fixeddeltaTime;

                OnFixedUpdate?.Invoke(deltaTime);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                enabled = false;
            }
        }



        #region Debug

        [TabGroup(MainTab)]
        public bool Debug_Tag = true;

        private void OnGUI()
        {
            if (!Debug_Tag)
                return;

            TrajectoryGUI();
        }

        [TabGroup(MainTab)]
        public bool DebugGizmos;

        private void OnDrawGizmos()
        {
            GizmosNetTarget();

            GizmosMovementHit();

            if (!DebugGizmos)
                return;

            GizmosBounds();
        }


        #endregion

        #endregion


        #region Modular

        /// <summary>
        /// Current motion module.
        /// </summary>
        public IMotionModule Module { get; protected set; }

        [ShowInInspector]
        [TabGroup(MainTab)]
        HashSet<IHumanoidMotionModule> m_Modules = new();

        public HashSet<IHumanoidMotionModule> Modules { get => m_Modules; set => m_Modules = value; }

        public IEnumerator<IHumanoidMotionModule> GetModules => Modules.GetEnumerator();


        public virtual bool InstallModule<T>(MotionModulePreset motionModuleData, out T module) where T : IMotionModule
        {
            if (motionModuleData.Create(gameObject).Parse(out module))
            {
                return InstallModule(module);
            }
            else
            {
                Debug.LogError(module + " is missing IHumanoidMotionModule interface.");

                if (module is Component cp)
                    Destroy(cp);
                else
                    module.Dispose();

                return false;
            }
        }

        /// <summary>
        /// Set motion handler and inject parameters.  **IsMine    **IsLocalUser
        /// </summary>
        public virtual bool InstallModule(IMotionModule module)
        {
            Assert.IsNotNull(module);

            try
            {
                module.SetHandler(this);

                Resolver.Inject(module);

                module.Register();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                //if(module)
                //    Modules.Remove(module);
                return false;
            }

            if (module is IHumanoidMotionModule humanoidMotion)
                Modules.Add(humanoidMotion);
            else
            {
                Debug.LogError($"Unknow motion module installed : {module}.");
                return false;
            }


            return true;
        }


        public virtual void UnistallModule(IHumanoidMotionModule module)
        {
            if (!Modules.Remove(module))
                return;

            //Resolver.UnregisterModule(module);
        }

        public virtual bool TryGetModule<T>(out T module) where T : IHumanoidMotionModule
        {
            return Modules.TryGetValue(out module);
        }

        public virtual void SetModule(IMotionModule motionModule)
        {
            Debug.Log($"Swap motion module {Module} to {motionModule}.");
            if (motionModule == null)
                motionModule = IdleModule;

            Module = motionModule;
        }

        #endregion



        #region Bounds
        public Bounds CharacterBounds { get; protected set; }

        protected Transform[] BoundsTarget;

        protected virtual void Init_Bounds()
        {
            var targets = new[]
            {
                HumanBodyBones.Head,
                HumanBodyBones.LeftLowerArm,
                HumanBodyBones.LeftHand,
                HumanBodyBones.RightLowerArm,
                HumanBodyBones.RightHand,
                HumanBodyBones.Spine,
                HumanBodyBones.Hips,
                HumanBodyBones.LeftLowerLeg,
                HumanBodyBones.LeftFoot,
                HumanBodyBones.RightLowerLeg,
                HumanBodyBones.RightFoot
            };

            var list = new List<Transform>(targets.Length);

            foreach (var bone in targets)
            {
                var tf = Animator.GetBoneTransform(bone);
                if (tf)
                    list.Add(tf);
            }

            BoundsTarget = list.ToArray();
        }
        protected virtual void CaculateBounds()
        {
            var bounds = Motor.Capsule.bounds;
            var length = BoundsTarget.Length;
            for (int i = 0; i < length; i++)
            {
                bounds.Encapsulate(BoundsTarget[i].position);
            }
            CharacterBounds = bounds;
        }

        void GizmosBounds()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(CharacterBounds.center, CharacterBounds.size);
        }

        #endregion


        #region delete bounds
        /// <summary>
        /// bounce controller collision *not grounded
        /// </summary>
        //public void _FixedUpdate()
        //{
        //    /*
        //    var handles = new TransformStreamHandle[]
        //    {
        //        Agent.Animator.GetBindingBone(HumanBodyBones.View),
        //        Agent.Animator.GetBindingBone(HumanBodyBones.RightHand),
        //        Agent.Animator.GetBindingBone(HumanBodyBones.LeftHand),
        //        Agent.Animator.GetBindingBone(HumanBodyBones.RightLeg),
        //        Agent.Animator.GetBindingBone(HumanBodyBones.LeftLeg),
        //        Agent.Animator.GetBindingBone(HumanBodyBones.Hips),
        //    };

        //    var top = new KeyValuePair<float, Vector3>(float.MinValue,default);
        //    var bottom = new KeyValuePair<float, Vector3>(float.MaxValue,default);
        //    var stream = new AnimationStream();
        //    var anim =  Agent.Animator.GetAnimator;
        //    anim.OpenAnimationStream(ref stream);

        //    var length = handles.Length;
        //    var normal = Agent.root.up;
        //    for (int i = 0; i < length; i++)
        //    {
        //        var pos = handles[i].GetIndexPosition(stream);
        //        var rows = Vector3.Project(pos, normal).magnitude;
        //        if (top.Key < rows)
        //            top = new KeyValuePair<float, Vector3>(rows, pos);

        //        if (bottom.Key > rows)
        //            bottom = new KeyValuePair<float, Vector3>(rows, pos);
        //    }

        //    var controllerHeight = top.Key - bottom.Key;
        //    var center = normal.Multiply(controllerHeight / 2+bottom.Key);

        //    anim.CloseAnimationStream(ref stream);

        //    Controller.center = center;
        //    Controller.rows = controllerHeight;
        //    */
        //}
        #endregion
    }

}