using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;
using Return.Creature;
using System;
using Return.Modular;
using Return.Humanoid;

namespace Return.Motions
{
    /// <summary>
    /// Presetable humanoid motion module.
    /// </summary>
    [Obsolete]
    public abstract class MonoMotionModule_Humanoid : MonoMotionModule, IMotionModule_Huamnoid, IComparable<MonoMotionModule_Humanoid>
    {
        #region Preset

        public abstract MotionModulePreset_Humanoid GetData { get; }

        #endregion

        #region Load Preset

        [Obsolete]
        public bool _Init { get; protected set; } = false;


        public static T Create<T>(MotionModulePreset_Humanoid data, GameObject @object) where T : MonoMotionModule_Humanoid
        {
            var module = @object.InstanceIfNull<T>();
            module.LoadData(data);
            //module.hideFlags = HideFlags.DontSave|HideFlags.NotEditable;
            module.HashCode = module.GetHashCode();
            return module;
        }

        /// <summary>
        /// Load module data
        /// </summary>
        protected abstract void LoadData(MotionModulePreset data);

        #endregion

     

        #region IMotionSystem

        [ShowInInspector, ReadOnly]
        protected virtual IHumanoidMotionSystem IMotions { get; set; }

        [Obsolete]
        protected MotionSystem_Humanoid Motions;

        //[Obsolete]
        //protected TrashStateBoard Parameters;

        public override void SetHandler(IMotionSystem motionSystem)
        {
            base.SetHandler(motionSystem);

            IMotions = motionSystem as IHumanoidMotionSystem;
            Assert.IsNotNull(IMotions);

            RegisterLimbSequence();

            if (motionSystem.Parse(out Motions))
            {
                //Parameters = Motions;
                RegisterParameter();
            }
        }

        protected virtual void RegisterLimbSequence()
        {
            Assert.IsNotNull(GetData);
            Assert.IsNotNull(GetData.GetMotionLimbs);
            Assert.IsNotNull(IMotions);

            var limbs = GetData.GetMotionLimbs;
            var sequences = new TNet.List<LimbSequence>(limbs.Length);
            foreach (var limb in limbs)
            {
                var seq = IMotions[limb];
                if (seq)
                {
                    sequences.Add(seq);
                    seq.Verify += Limb_Verify;
                }
            }
            Sequences = sequences.ToArray();
        }

        protected virtual void RegisterParameter() { }

        protected virtual void UnregisterParameter() { }


        protected override bool EnableModuleMotion
        {
            get => base.EnableModuleMotion;
            set
            {
                if (value)
                    ResetParameters();
                base.EnableModuleMotion = value;
            }
        }


        #endregion




        #region Sequence

        [Obsolete]
        /// <summary>
        /// Body sequence from motion system.  **ref to motion system
        /// </summary>
        protected LimbSequence[] Sequences;

        /// <summary>
        /// Whether the module enables motion (call via agent Input)
        /// </summary>
        protected virtual bool ValidModuleMotion(params Limb[] limbs)
        {
            if (!EnableModuleMotion)
            {
                var canMotion = true;
                var length = limbs.Length;

                for (int i = 0; i < length; i++)
                {
                    var seq = IMotions[limbs[i]];
                    if (!seq.CanMotion(this, true))
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
                LimbSequence seq;
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

                EnableModuleMotion = canMotion;
            }
            return EnableModuleMotion;
        }

        /// <summary>
        /// Invoke this func while motion mission has been apply
        /// </summary>
        protected virtual void ResetParameters() { }

        /// <summary>
        /// Receive this arg to set module enable or disable
        /// </summary>
        /// <param name="moduleHash">module.HashCode</param>
        /// <param name="enable">set module enable</param>
        /// <param name="limb">cause by which limb</param>
        protected virtual void Limb_Verify(int moduleHash, bool enable, Limb limb)
        {
            if (HashCode == moduleHash && EnableModuleMotion != enable)
                EvaluateModule(enable, limb);
        }

        /// <summary>
        /// 
        /// </summary>
        protected virtual void EvaluateModule(bool enable, Limb limb)
        {
            //EnableMotion = enable;

            if (CanQueue(out var _))
                Sequences[limb].SubstituteMotion(this);
        }


        public virtual void InterruptMotion(Limb limb)
        {
            // queue this limb and remove others
            foreach (var seqLimb in Sequences)
            {
                if (!seqLimb)
                    continue;

                if (seqLimb.Limb.Equals(limb))
                    continue;

                seqLimb.RemoveMotion(this);
            }

            EnableModuleMotion = false;
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

            if (!EnableModuleMotion)
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

                    Debug.Log("CheckAdd " + seq.Limb + this);
                }

                for (int i = 0; i < length; i++)
                {
                    var seq = Sequences[i];
                    if (seq.Limb == limb)
                        continue;

                    Debug.Log("confirm " + canMotion + seq.Limb + this);
                    if (canMotion)
                        seq.ConfirmMotion(this);
                    else
                        seq.ConfirmMotion(null);
                }

                EnableModuleMotion = canMotion;
            }

            return EnableModuleMotion;


            //var length = Sequences.Length;
            //LimbSequence seq;
            //for (int i = 0; i < length; i++)
            //{
            //    seq = Sequences[i];

            //    if (!seq)
            //        continue;

            //    if (seq.Limb.Equals(limb))
            //        continue;

            //    if (!seq.CanPlay(this, true))
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

        public int CompareTo(MonoMotionModule_Humanoid other)
        {
            return GetData.Order.CompareTo(other.GetData.Order);
        }


        protected override void FinishModuleMotion()
        {
            foreach (var seq in Sequences)
            {
                seq.RemoveMotion(this);
            }

            EnableModuleMotion = false;
        }

        #endregion

        protected override void OnDisable()
        {
            base.OnDisable();

            if (!_Init)
                return;
            foreach (var seqLimb in Sequences)
            {
                seqLimb?.RemoveMotion(this);
            }

            UnregisterParameter();

            return;
            if (IMotions != null)
                foreach (var limb in GetData.MotionLimbs)
                {
                    IMotions[limb]?.RemoveMotion(this);
                }
        }


    }
}

