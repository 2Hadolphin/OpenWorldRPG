using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using Return.Creature;
using TNet;
using System;
using Return.Motions;
using Return.Modular;
using Return.Framework.PhysicController;

namespace Return.Humanoid
{
    /// <summary>
    /// Motion module to cache humanoid parameters.
    /// </summary>
    [Obsolete]
    public abstract class MotionModule_Humanoid : MotionModule,IMotionModule_Huamnoid
    {
        public abstract MotionModulePreset_Humanoid GetData { get; }

        #region Creation

        [Obsolete]
        public static T Create<T>(MotionModulePreset_Humanoid data) where T : MotionModule_Humanoid,new()
        {
            var module = new T();
            module.LoadData(data);
            //module.hideFlags = HideFlags.DontSave|HideFlags.NotEditable;
            module.HashCode = module.GetHashCode();
            return module;
        }

        /// <summary>
        /// Binding module data from preset.
        /// </summary>
        public abstract void LoadData(MotionModulePreset data);

        #endregion

        #region Resolver

        public override void SetHandler(IMotionSystem module)
        {
            base.SetHandler(module);
            IMotions = module as IHumanoidMotionSystem;
            Assert.IsNotNull(IMotions);

            // obsolete
            if (IMotions.Parse(out MotionSystem_Humanoid system))
                Motions = system;
            else
                Debug.LogError("Missing motion system.");
        }

        #region Cache Handler

        [Inject]
        protected ICharacterControllerMotor Motor;

        [ShowInInspector, ReadOnly]
        protected virtual IHumanoidMotionSystem IMotions { get; set; }

        [Obsolete]
        protected MotionSystem_Humanoid Motions { get; set; }

        //[Obsolete]
        //protected TrashStateBoard Parameters;
        #endregion
        #endregion

        #region Routine

        protected override void Register()
        {
            base.Register();

            RegisterLimbSequence();
        }


        /// <summary>
        /// Whether enable to do active motion.
        /// </summary>
        protected override bool EnableMotion
        {
            get => base.EnableMotion;
            set
            {
                if (EnableMotion == value)
                    return;

                Debug.Log($"Change motion state {this}");

                base.EnableMotion = value;

                if (value)
                    OnMotionStart();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!initialized)
                return;

            foreach (var seqLimb in Sequences)
                seqLimb?.RemoveMotion(this);

            EnableMotion = false;
        }


        public override void Dispose()
        {
            IMotions.Resolver.UnregisterModule(this);
            base.Dispose();
        }


        #endregion




        #region Parameters

        /// <summary>
        /// Invoke when active movement is activated.
        /// </summary>
        protected virtual void OnMotionStart() { }

        #endregion




        #region Sequence

        /// <summary>
        /// Body sequence from motion system.
        /// </summary>
        protected LimbSequence[] Sequences;

        /// <summary>
        /// Setup motion handle to sequence(Limbs).
        /// </summary>
        protected virtual void RegisterLimbSequence()
        {
            Assert.IsNotNull(GetData);
            Assert.IsNotNull(GetData.GetMotionLimbs);
            Assert.IsNotNull(IMotions);

            var limbs = GetData.GetMotionLimbs;
            var sequences = new List<LimbSequence>(limbs.Length);

            // push sequence with motion module limbs
            foreach (var limb in limbs)
            {
                var seq = IMotions[limb];
                if (seq)
                {
                    sequences.Add(seq);
                    seq.Verify += Limb_Verify;
                }
            }

            // cache sequence for this motion module
            Sequences = sequences.ToArray();
        }

        [Obsolete]
        int devLog;

        /// <summary>
        /// Whether the module enables motion (call via agent Input).
        /// </summary>
        protected virtual bool ValidModuleMotion(params Limb[] limbs)
        {
            devLog++;

            if (!EnableMotion)
            {
                Debug.Log($"{nameof(ValidModuleMotion)} {Time.frameCount} : {devLog} {this}.");

                var canMotion = true;
                var length = limbs.Length;

                for (int i = 0; i < length; i++)
                {
                    var sequence = IMotions[limbs[i]];

                    if (!sequence.CanMotion(this, true))
                    {
                        length = i + 1;
                        canMotion = false;
                        break;
                    }
                }

                for (int i = 0; i < length; i++)
                {
                    var seq = IMotions[limbs[i]];

                    if (canMotion)
                        seq.ConfirmMotion(this);
                    else
                        seq.ConfirmMotion(null);
                }
                /**
                Assert.IsTrue(Sequences.Length > 0);

                var canMotion = true;
                var length = Sequences.Length;
                LimbSequence sequence;
                for (int i = 0; i < length; i++)
                {
                    seq = Sequences[i];
                    var sLimb = seq.Limb;
                    foreach (var limb in limbs)
                    {
                        if (sLimb.Equals(limb))
                            canMotion = canMotion && seq.CanMotion(this, true);
                    }
                }

                if (enable != canMotion)
                {
                    enable = canMotion;
                    EnableModuleMotion = canMotion;
                    Debug.Log(this.GetData.Title + " : " + EnableModuleMotion);
                    ResetParameters();

                    for (int i = 0; i < length; i++)
                    {
                        seq = Sequences[i];
                        var sLimb = seq.Limb;
                        foreach (var limb in limbs)
                        {
                            if (sLimb.Equals(limb))
                                seq.ConfirmMotion(this);
                        }
                    }
                }
                **/
                if (!canMotion)
                    Debug.LogError(this + "start motion fail.");

                EnableMotion = canMotion;

            }

            return EnableMotion;
        }

        /// <summary>
        /// Receive sequence arg to set module enable or disable when sequence event fire.
        /// </summary>
        /// <param name="moduleHash">module.HashCode</param>
        /// <param name="enable">set module enable</param>
        /// <param name="limb">cause by which limb</param>
        protected virtual void Limb_Verify(int moduleHash, bool enable, Limb limb)
        {
            if (HashCode == moduleHash && EnableMotion != enable)
                EvaluateModule(enable, limb);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="enable"></param>
        /// <param name="limb"></param>
        protected virtual void EvaluateModule(bool enable, Limb limb)
        {
            //EnableMotion = enable;
        }

        public virtual void InterruptMotion(Limb limb)
        {
            // queue this limb and remove others
            foreach (var seqLimb in Sequences)
            {
                // empty or disable sequence
                if (!seqLimb)
                    continue;

                // interrupt limb
                if (seqLimb.Limb.Equals(limb))
                    continue;

                Debug.Log($"Remove sequence {limb} with {this}");

                // interrupt other sequence from this motion
                seqLimb.RemoveMotion(this);
            }

            EnableMotion = false;

            return;

            var limbs = GetData.GetMotionLimbs;

            foreach (var climb in limbs)
            {
                if (climb.Equals(limb))
                    continue;

                IMotions[climb].RemoveMotion(this);
            }
        }


        public virtual bool Ready2Motion(Limb limb)
        {
            Debug.Log($"{limb} check ready to motion {this}.");

            if (!EnableMotion)
            {
                var canMotion = true;

                var length = Sequences.Length;

                for (int i = 0; i < length; i++)
                {
                    var seq = Sequences[i];
                    if (seq.Limb == limb)
                        continue;

                    if (!seq.CanMotion(this, true))
                    {
                        length = i + 1;
                        canMotion = false;
                        break;
                    }

                    Debug.Log("Check ready motion " + seq.Limb + this);
                }


                for (int i = 0; i < length; i++)
                {
                    var seq = Sequences[i];
                    if (seq.Limb == limb)
                        continue;

                    Debug.Log("Confirm motion " + canMotion + seq.Limb + this);

                    if (canMotion)  // push motion if all sequence ready
                        seq.ConfirmMotion(this);
                    else // clean waiting motion if sequence deny
                        seq.ConfirmMotion(null);
                }

                EnableMotion = canMotion;
            }

            return EnableMotion;


            //var length = Sequences.Length;
            //LimbSequence sequence;
            //for (int i = 0; i < length; i++)
            //{
            //    sequence = Sequences[i];

            //    if (!sequence)
            //        continue;

            //    if (sequence.Limb.Equals(limb))
            //        continue;

            //    if (!sequence.CanPlay(this, true))
            //    {
            //        EnableMotion = false;
            //        return EnableMotion;
            //    }
            //}

            //for (int i = 0; i < length; i++)
            //{
            //    Sequences[i].ConfirmMotion(this);
            //}

            //EnableMotion = true;
            //return EnableMotion;

            //foreach (var climb in GetData.MotionLimbs)
            //{
            //    if (!climb.Equals(limb))
            //        continue;

            //    return IMotions[climb].CanPlay(this);
            //}
            //return false;
        }


        protected override void FinishModuleMotion()
        {
            Debug.Log("FinishMotion " + this);

            foreach (var seq in Sequences)
                seq.RemoveMotion(this);

            EnableMotion = false;
        }

        /// <summary>
        /// Get motion module priority.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public virtual int CompareTo(MonoMotionModule_Humanoid other)
        {
            return GetData.Order.CompareTo(other.GetData.Order);
        }

        public virtual int CompareTo(IHumanoidMotionModule other)
        {
            if (other is MonoMotionModule_Humanoid motions)
                return this.CompareTo(motions);
            else
                return 1;
        }

        #endregion


    }
}

