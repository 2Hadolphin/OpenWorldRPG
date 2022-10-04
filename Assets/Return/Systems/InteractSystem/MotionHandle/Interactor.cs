using Return.Humanoid;
using System.Collections.Generic;
using Return.Humanoid.IK;
using Sirenix.OdinInspector;
using Return.Creature;
using Return.Motions;

namespace Return
{
    /// <summary>
    /// Motion module to process hand IK motion, inculde adjust IK pose and avoid collision.
    /// </summary>
    public class Interactor : MonoMotionModule_Humanoid // MonoMotionModule
    {
        #region Preset

        public InteractorPreset Data { get; protected set; }

        public override MotionModulePreset_Humanoid GetData => Data;

        protected override void LoadData(MotionModulePreset data)
        {
            Data = data as InteractorPreset;
        }

        #endregion


        public override void SetHandler(IMotionSystem motionSystem)
        {
            base.SetHandler(motionSystem);
            Init_IK();
        }

        LimbIKWrapper RightHand;
        LimbIKWrapper LeftHand;

        [ShowInInspector, ReadOnly]
        Stroke _RightHand;
        [ShowInInspector, ReadOnly]
        Stroke _LeftHand;

        void Init_IK()
        {
            RightHand = IMotions.GetIK(HashCode, Limb.RightHand);
            LeftHand = IMotions.GetIK(HashCode, Limb.LeftHand);
        }

        void BuildRightHandStroke(object sender)
        {
            _RightHand = new Stroke(RightHand) { sender = sender };
            ZeroIKWeight(RightHand);
        }

        void BuildLeftHandStroke(object sender)
        {
            _LeftHand = new Stroke(LeftHand) { sender = sender };
            ZeroIKWeight(LeftHand);
        }

        void ZeroIKWeight(LimbIKWrapper wrapper)
        {
            wrapper.weight = 0;
        }

        #region Func

        public Symmetry CheckHandle(Symmetry require)
        {
            var deny = Symmetry.Deny;
            var enableHand = deny;

            switch (require)
            {
                case Symmetry.Right:

                    enableHand |= _RightHand ? CheckIK(ref _RightHand) : CheckIK(Limb.RightHand);
                    break;

                case Symmetry.Left:
                    enableHand |= _LeftHand ? CheckIK(ref _LeftHand) : CheckIK(Limb.RightHand);
                    break;

                default:
                    enableHand = enableHand
                        | (_RightHand ? CheckIK(ref _RightHand) : CheckIK(Limb.RightHand))
                        | (_LeftHand ? CheckIK(ref _LeftHand) : CheckIK(Limb.RightHand));
                    break;
            }

            return enableHand == Symmetry.Deny ? deny : enableHand ^ deny;
        }


        Symmetry CheckIK(ref Stroke stroke)
        {
            var enableHand = Symmetry.Deny;
            if (stroke.Ready2Use())
            {
                var limb = stroke.Limb;
                var sequence = IMotions[limb];
                if (sequence.CanMotion(this))
                    switch (limb)
                    {
                        case Limb.RightHand:
                            enableHand = Symmetry.Right;
                            break;
                        case Limb.LeftHand:
                            enableHand = Symmetry.Left;
                            break;
                    }
            }

            return enableHand;
        }

        Symmetry CheckIK(Limb limb)
        {
            var enableHand = Symmetry.Deny;

            var sequence = IMotions[limb];
            if (sequence.CanMotion(this))
                switch (limb)
                {
                    case Limb.RightHand:
                        enableHand = Symmetry.Right;
                        break;
                    case Limb.LeftHand:
                        enableHand = Symmetry.Left;
                        break;
                }

            return enableHand;
        }

        /// <summary>
        /// Subscribe IK event and dispose
        /// </summary>
        public virtual void RegisterAdapter(InteractAdapter adapter)
        {
            var sender = adapter.Owner;
            switch (adapter.PermissionInteractor)
            {
                case Symmetry.Right:
                    BuildRightHandStroke(sender);
                    break;
                case Symmetry.Left:
                    BuildLeftHandStroke(sender);
                    break;
                case Symmetry.Both:
                    BuildRightHandStroke(sender);
                    BuildLeftHandStroke(sender);
                    break;
            }

            adapter.IKHandle += UpdateIKAdapter;

            adapter.OnDispose += DisposeHandle;
        }

        private void UpdateIKAdapter(object sender, TwoBoneIKDataArg e)
        {
            Stroke stroke = default;
            switch (e.Limb)
            {
                case Limb.RightHand:
                    stroke = _RightHand;
                    break;

                case Limb.LeftHand:
                    stroke = _LeftHand;
                    break;

                default:
                    throw new KeyNotFoundException(e.Limb.ToString());
            }

            if (!stroke.sender.Equals(sender))
                return;

            e.Init(stroke.IK.Root, stroke.IK.Tip);
            stroke.StartStroke(e);
        }

        protected virtual void DisposeHandle(object sender, Limb limb)
        {
            switch (limb)
            {
                case Limb.RightHand:

                    if (_RightHand && _RightHand.sender == sender)
                        DisposeStroke(ref _RightHand);
                    break;
                case Limb.LeftHand:
                    if (_LeftHand && _LeftHand.sender == sender)
                        DisposeStroke(ref _LeftHand);
                    break;
            }
        }

        void DisposeStroke(ref Stroke stroke)
        {
            stroke = null;
        }
        #endregion

        public override int CompareTo(IHumanoidMotionModule other)
        {
            return -2;
        }
    }
}