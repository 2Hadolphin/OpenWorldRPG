using KinematicCharacterController;
using Sirenix.OdinInspector;
using UnityEngine;
using Return.Humanoid;
using Return.Modular;

namespace Return.Motions
{
    [DisallowMultipleComponent]
    public partial class LadderMotion : MotionModule_Humanoid, IControllerVelocityWrapper
    {
        public override MotionModulePreset_Humanoid GetData => pref;

        protected ControlModule_Ladder pref;

        public override void LoadData(MotionModulePreset data)
        {
            data.Parse(out pref);
        }

        public override void SetHandler(IMotionSystem module)
        {
            base.SetHandler(module);


            Transform = Motor.Transform;
        }

        IModule ControlHandle;
        

        [Inject]
        IMxMHandler IMxMHandler;


        #region Routine

        protected override void Register()
        {
            base.Register();

            if (IMotions.isMine)
            {
                var handle = new UserInputHandle();
                handle.RegisterInput(InputManager.Input);
                handle.SetHandler(this);
                ControlHandle = handle;
            }

            ControlHandle?.Register();
        }

        protected override void Unregister()
        {
            ControlHandle?.UnRegister();
            base.Unregister();
        }

        protected override void Activate()
        {
            base.Activate();

            ControlHandle?.Activate();
        }

        protected override void Deactivate()
        {
            ControlHandle?.Deactivate();
            base.Deactivate();
        }

        public override void SubscribeRountine(bool enable = false)
        {
            Motions.UpdatePost += Update;
        }

        private void Update(float deltaTime)
        {
            if (Ladder && LadderTF)
            {
                var dis = (LadderTF.position - Transform.position).sqrMagnitude;
                if (Mathf.Pow(Ladder.LadderSegmentLength, 2) * 2 < dis)
                {
                    FinishModuleMotion();
                }
            }

            if (EnableMotion)
            {
                LadderUserDirection = Ladder.GetLadderDirection(Transform.up);
                return;
                //if (Motions.mxm.CheckEventPlaying(pref.m_DropDefinition.Id))//&&Motor.mxm.IsEventComplete)
                //{
                //    // handle module speed and IK hand position +ragdoll position

                //}
                //else //if (Motor.mxm.IsEventComplete)
                //{
                //    Debug.LogError("Not Sliding");
                //    Motions.mxm.ForceExitEvent();
                //    FinishModuleMotion();
                //}
            }
        }


        #endregion


        #region MotionWrapper

        public bool ApplyGravity => false;

        public bool Override => true;

        public void Main(ref Vector3 currentVelocity, float deltaTime)
        {
            LadderUserDirection = Ladder.GetLadderDirection(Transform.up);

            if (CurrentClimbSpeed == 0)
                currentVelocity = Vector3.zero;
            else
                currentVelocity = LadderUserDirection.Multiply(CurrentClimbSpeed);// wrap distance

            ModuleVector = LadderUserDirection;
            ModuleVelocity = currentVelocity;
        }

        public bool Finish(ref Vector3 currentVelocity, float deltaTime)
        {
            HandleLadder();
            return !OnLadder;
        }

        #endregion


        #region Parameters

        Transform Transform;
        public Ladder Ladder;
        Transform LadderTF;


        private float _onLadderSegmentState = 0;

        public Vector3 LadderUserDirection;
        public float CurrentClimbSpeed;

        public Vector3 _ladderTargetPosition;
        public Quaternion _ladderTargetRotation;
        public Quaternion _rotationBeforeClimbing;

        #endregion

        #region LimbSequence

        protected override void FinishModuleMotion()
        {
            base.FinishModuleMotion();

            OnLadder = false;

            
            IMotions.UnistallModule(this);
        }

        #endregion


        public void SetClimbSpeed(float v)
        {
            CurrentClimbSpeed = v.Sign() * pref.LadderClimbSpeed;
            Debug.Log(CurrentClimbSpeed);
        }

        public virtual void ValidOnLadder()
        {
            if (EnableMotion)
                return;

            // sensor ladder direction
            if (Ladder && Ladder.IsLadderFocus(Transform.position, Transform.rotation))
            {
                var dir = IMotions.MotionVelocity.GetEachMaxWith(IMotions.MotionVector);

                if (CurrentClimbSpeed == 0)
                    CurrentClimbSpeed = Vector3.Dot(dir, Transform.forward).Sign() * pref.LadderClimbSpeed;

                Debug.Log(CurrentClimbSpeed);

                if (true && CurrentClimbSpeed != 0)//auto climb{
                {
                    if (ValidModuleMotion(pref.GetMotionLimbs))
                    {
                        OnLadder = true;
                    }
                }
                else if (false)// show use ladder ui tip
                {

                }
            }

            Debug.Log(Ladder ? Ladder.IsLadderFocus(Transform.position, Transform.rotation) : Ladder);

        }

        bool _OnLadder;


        [ShowInInspector]
        public bool OnLadder
        {
            get => _OnLadder;
            set
            {
                if (!_OnLadder.SetAs(value))
                    return;

                Motions.EnableRootMotion = !value;
                //Motions.EnableCollider = !value;

                //var Motor = Motions.Motor;

                //Motor.SetMovementCollisionsSolvingActivation(!value);
                //Motor.SetGroundSolvingActivation(!value);

                if (value)
                {
                    IMxMHandler.mxm.AddRequiredTag(pref.m_climbTagName);
                    Motions.SetControllerWrapper(this);

                    _rotationBeforeClimbing = Motor.TransientRotation;

                    // Store the target position and rotation to snap to
                    if (Ladder.Collider is Collider col && col && !col.isTrigger)
                        Motions.IgnoredColliders.Add(col);

                    var ladderNormal = Ladder.GetLadderNormal(Transform.forward);
                    _ladderTargetRotation = Quaternion.LookRotation(-ladderNormal, Ladder.GetLadderDirection(Transform.up));

                    _ladderTargetPosition = ladderNormal.Multiply(Motions.ControllerRadius);
                    _ladderTargetPosition += Ladder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);

                }
                else
                {
                    if (Ladder.Collider is Collider col && col && !col.isTrigger)
                        Motions.IgnoredColliders.Remove(col);

                    IMxMHandler.mxm.RemoveRequiredTag(pref.m_climbTagName);
                    if (!Ladder.Contains(Motions.Collider))
                    {

                        //Destroy(this, 1f);
                    }

                    Ladder = null;
                }

                Motions.SetPositionAndRotation(_ladderTargetPosition, _ladderTargetRotation);
                Motor.ForceUnground(0.1f);
            }
        }


        public virtual void HandleLadder()
        {
            Ladder.ClosestPointOnLadderSegment(Motor.TransientPosition, out _onLadderSegmentState);

            if (Mathf.Abs(_onLadderSegmentState) > 0.05f)
            {
                // If we're higher than the ladder top point
                if (_onLadderSegmentState > 0)
                {
                    _ladderTargetPosition = Ladder.TopReleasePoint.position;
                    _ladderTargetRotation = Ladder.TopReleasePoint.rotation;
                }
                // If we're lower than the ladder bottom point
                else if (_onLadderSegmentState < 0)
                {
                    _ladderTargetPosition = Ladder.BottomReleasePoint.position;
                    _ladderTargetRotation = Ladder.BottomReleasePoint.rotation;
                }

                // => finish ladder
                FinishModuleMotion();
            }
            else
            {
                var groundingStatus = Motor.GroundingStatus;

                if (groundingStatus.IsStableOnGround && groundingStatus.GroundCollider)
                    FinishModuleMotion();

                Debug.Log(groundingStatus.IsStableOnGround + "**" + groundingStatus.GroundCollider);
            }


        }

    }
}