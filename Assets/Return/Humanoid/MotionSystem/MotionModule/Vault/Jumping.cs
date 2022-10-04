using UnityEngine;
using Return.Humanoid;
using MxMGameplay;
using MxM;
using KinematicCharacterController;
using UnityEngine.Assertions;
using Return.Creature;
using System;
using Return;
using TNet;
using Return.Preference.GamePlay;
using Return.Agents;
using Return.Modular;
using Return.Framework.PhysicController;

namespace Return.Motions
{
    [Serializable]
    public partial class Jumping : MotionModule_Humanoid, IKCCVelocityModule
    {
        #region Preset

        public override MotionModulePreset_Humanoid GetData => pref;
        protected Jump_MotionPreset pref;

        public override void LoadData(MotionModulePreset data)
        {
            data.Parse(out pref);
        }

        #endregion


        #region Resolver

        [Inject]
        IMxMHandler IMxMHandler;

        IModule Control;


        public override void SetHandler(IMotionSystem motionSystem)
        {
            base.SetHandler(motionSystem);
        }

        #endregion

        #region Routine

        protected override void Register()
        {
            base.Register();

            Transform = Motor.Transform;

            //kinematicCharacterMotor = motionSystem.

            Register_Vault();

            if (IMotions.isMine)
            {
                var handle = new UserInputHandle();
                handle.RegisterInput(InputManager.Input);
                handle.SetHandler(this);
                Control = handle;
            }

            Control?.Register();
        }

        protected override void Unregister()
        {
            Control?.UnRegister();

            base.Unregister();
        }

        protected override void Activate()
        {
            base.Activate();

            Motions.VelocityBlendRatio = 1;
            Motions.AngularVelocityBlendRatio = 1;

            Control?.Activate();
        }

        protected override void Deactivate()
        {
            Control?.Deactivate();

            base.Deactivate();
        }

        public override void SubscribeRountine(bool enable)
        {
            if (IMotions.isMine)
            {
                //IMotions.UpdatePost -= Tick;
                IMotions.OnFixedUpdate -= FixedTick;

                if (enable)
                {
                    //IMotions.UpdatePost += Tick;
                    IMotions.OnFixedUpdate += FixedTick;
                }
            }
        }

        protected virtual void Tick(float deltaTime)
        {

        }

        protected virtual void FixedTick(float deltaTime)
        {
            if (EnableMotion)
            {
                //Debug.Log(deltaTime);
                int id = -1;
                Action validFunc = null;
                // handle module speed and IK hand position +ragdoll position
                switch (State)
                {
                    case VaultState.Jumping:
                        validFunc = HandleJumping;
                        id = pref.JumpDefinition.Id;
                        break;

                    case VaultState.Vaulting:
                        //Debug.Log("Vaulting..");
                        validFunc = HandleCurrentVault;
                        if (null != vaultDef)
                            id = vaultDef.EventDefinition.Id;
                        break;

                    default:// wait anim contact arrived
                            //Motions.mxm.ForceExitEvent();
                            //State = VaultState.General;
                        Debug.LogError("Stop " + State + Motor.Velocity);
                        //case VaultState.General => sensor
                        // go fixed update
                        return;
                }

                //Debug.Log(State);

                //if ((QuickResponse||id>=0) && Motions.mxm.CheckEventPlaying(id))
                {
                    Assert.IsNotNull(validFunc);
                    validFunc();
                }
                //else
                {
                    //State = VaultState.General;
                    //Motor.AddImpulseVelocity(LastVelocity);
                    //Debug.LogError("Stop "+State+ Motor.Motor.ModuleVelocity);
                }

                //Parameters.SetBool(pref.InputUpdate);
                //Parameters.SetBool(pref.MotionUpdate);
            }

            return;

            // auto vault _checkCache

            if (ManualVault)
                return;

            if (CanVault())
                Vault();
        }

        #endregion


        #region IKCCVelocityModule

        public virtual void UpdateInertiaVelocity(ref Vector3 currentInertia, float deltaTime)
        {
            // do nothing while in air
            Debug.Log($"Jumnping inertia {currentInertia}");
        }


        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (!Wrapper)
                return;

            Debug.Log($"Jumnping velocity {currentVelocity}");
        }

        #endregion


        #region Animation Event

        public MxMEventDefinition WaitEvent;

        protected virtual void OnAnimEvent(AnimationEvent e)
        {
            Debug.Log("On animation event : " + State);

            if (WaitEvent.IsNull())
                return;

            if (WaitEvent.EventName == e.stringParameter)
            {
                Debug.Log("clip event :" + e.animatorClipInfo.clip.name);

                ExecuteAnim();
                Motions.OnAnimEvent -= OnAnimEvent;


                WaitEvent = null;
            }
        }

        protected virtual void ExecuteAnim()
        {
            Motions.EnableRootMotion = false;
            Motor.ForceUnground(0.1f);
            // activate motion wrapper for custom velocity
            //Wrapper.Enable = true;

            Assert.IsNotNull(Wrapper);

            Motions.ConvertModuleVelocityToInertia(Wrapper.InertiaVelocity + Wrapper.JumpVelocity);

            Wrapper.Dispose();
            Wrapper = null;
        }

        #endregion


        #region Sequence


        protected override bool ValidModuleMotion(params Limb[] limbs)
        {
            return base.ValidModuleMotion(limbs);
            //enabled= base.ValidModuleMotion(limbs);
            //return enabled;
        }

        public override void InterruptMotion(Limb limb)
        {
            base.InterruptMotion(limb);
            State = VaultState.General;
        }

        protected override void FinishModuleMotion()
        {
            base.FinishModuleMotion();


            if (Wrapper)
            {
                ModuleVector = Wrapper.CurrentVelocity.normalized;
                ModuleVelocity = Wrapper.CurrentVelocity;

                Wrapper.Dispose();
                Wrapper = null;
            }

            var moduleVel = ModuleVector;

            if (moduleVel.sqrMagnitude < 0.1)
            {
                Motions.ResetMotion(Transform.eulerAngles.y);
                Debug.LogError($"Finish Jumping vector {moduleVel}");
                //Parameters.SetVector3(pref.ModuleVector, LastVelocity.Multiply(0.3f));
            }

            enabled = false;
        }

        #endregion



        #region Catch

        bool QuickResponse;

        Transform Transform;
        [Inject]
        [Obsolete]
        ICharacterControllerMotor kinematicCharacterMotor;

        RaycastHit[] Hits = new RaycastHit[3];

        [SerializeField]
        public VaultDetectionConfig m_curConfig;

        [SerializeField]
        private float m_minAdvance = 0.1f; //The minimum advance required to trigger a vault.

        [SerializeField]
        private float m_advanceSmoothing = 10f;

        [SerializeField]
        public float m_maxApproachAngle = 60f;

        public Vector3 LastVelocity;

        Vector2 MotionDirection;

        private int m_vaultAnalysisIterations;


        private float m_minVaultRise;
        private float m_maxVaultRise;
        private float m_minVaultDepth;
        private float m_maxVaultDepth;
        private float m_minVaultDrop;
        private float m_maxVaultDrop;

        protected bool ManualVault = false;

        protected enum VaultState
        {
            General = 1,
            Jumping = 3,
            Vaulting = 4
        }

        [SerializeField]
        private bool m_isJumping;
        [SerializeField]
        private bool m_isVaulting;

        protected VaultState m_curState = VaultState.General;

        protected virtual VaultState State
        {
            get => m_curState;
            set
            {
                switch (m_curState)
                {
                    case VaultState.Jumping:
                        IMxMHandler.ExitEvent(pref.JumpDefinition.Id);
                        //Wrapper.IsFinish = true;
                        break;
                    case VaultState.Vaulting:
                        //if (vaultDef)
                        //    Motions.mxm.CheckEventPlaying(vaultDef.EventDefinition.Id);

                        IMxMHandler.ExitEvent(vaultDef.EventDefinition.Id);
                        break;
                }

                switch (value)
                {
                    case VaultState.General:
                        //Motor.EnableGravity = true;

                        //kinematicCharacterMotor.SetMovementCollisionsSolvingActivation(true);

                        Motions.EnableRootMotion = true;
                        //Parameters.SetVector3(pref.ModuleVelocity, Vector3.zero);
                        FinishModuleMotion();

                        //var speed = Motor.Motor.ModuleVelocity;
                        //Parameters.SetVector3(pref.ModuleVelocity, speed);
                        //Motor.AddImpulseVelocity(speed);
                        //Parameters.SetVector3(pref.ModuleVelocity, speed);
                        break;

                    case VaultState.Jumping:


                        // kinematicCharacterMotor.SetMovementCollisionsSolvingActivation(false);
                        Motions.VelocityBlendRatio = 1f;

                        //Parameters.SetVector3(pref.ModuleVelocity,Vector3.up);
                        break;

                    case VaultState.Vaulting:

                        Motor.ForceUnground(0.1f);
                        //kinematicCharacterMotor.SetMovementCollisionsSolvingActivation(false);
                        Motions.VelocityBlendRatio = 1f;
                        break;
                }


                m_isVaulting = value.HasFlag(VaultState.Vaulting);
                m_isJumping = value.HasFlag(VaultState.Jumping);
                m_curState = value;
                Debug.Log("new state : " + value);
            }
        }

        [SerializeField]
        public float Advance { get; set; }
        [SerializeField]
        public float DesiredAdvance { get; set; }

        bool CollisionEnabled
        {
            get => Motions.EnableCollider;
            set => Motions.EnableCollider = value;
        }

        #endregion

        [Obsolete]
        JumpWrapper Wrapper;

        /// <summary>
        /// MotorVelocity wrapper to apply controller update
        /// </summary>
        class JumpWrapper : VelocityWrapper, IDisposable
        {
            public Vector3 OriginVelocity;
            public Vector3 InertiaVelocity { get => Velocity; set => Velocity = value; }
            public Vector3 JumpVelocity;

            // finish?
            public float FuzzyTime = 1f;

            // enable -> init

            public bool init { get; protected set; } = false;
            public bool Enable;

            

            public override void Main(ref Vector3 currentVelocity, float deltaTime)
            {
                if (init)
                {
                    currentVelocity = InertiaVelocity.GetEachMaxWith(currentVelocity);
                    CurrentVelocity = currentVelocity;
                    //Debug.Log(CurrentVelocity);
                    return;
                }
                else if (Enable)
                {
                    // apply burst jump vel
                    Debug.Log("Jump Add " + currentVelocity + JumpVelocity);
                    currentVelocity += JumpVelocity;

                    init = true;
                }
                else
                {
                    CurrentVelocity = InertiaVelocity.GetEachMaxWith(currentVelocity);
                    currentVelocity = CurrentVelocity;
                    //Debug.Log(CurrentVelocity);
                    FuzzyTime -= deltaTime;
                    //IsFinish = FuzzyTime <= 0;
                }

                Debug.Log($"Frame {Time.frameCount} Jumping {currentVelocity}");

            }


            public override bool Finish(ref Vector3 currentVelocity, float deltaTime)
            {
                //if(IsFinish)
                // currentVelocity = OriginVelocity;
                return IsFinish;
            }
        }

        #region Jump

        Vector3 PredictDirection;

        public virtual void Jump()
        {

            if (State != VaultState.General)
            {
                Debug.LogError("Failure to jump while state is : " + State);
                return;
            }

            var grounded = Motor.GroundingStatus;

            if (!grounded.IsStableOnGround && !grounded.FoundAnyGround)
            {
                if (Motor.BaseVelocity != Vector3.zero)
                {
                    Debug.LogError($"Jumping will ignore without stable grounded Any grounded : {Motor.GroundingStatus.FoundAnyGround}");
                    return;
                }
            }

            if (!ValidModuleMotion(pref.RightLeg, pref.LeftLeg))
            {
                Debug.LogError("Failure to jump while module is not valid.");
                return;
            }


            if (pref.JumpDefinition != null)
            {
                pref.JumpDefinition.ClearContacts();

                var speed = Motor.Velocity;

                var planarRotation = Transform.rotation;
                //Parameters.GetQuaternion(pref.ViewPortPlanarRotation)

                var targetVel = planarRotation * MotionDirection.Y2Z().Multiply(pref.JumpDistance);
                speed = speed.GetEachMaxWith(targetVel);


                //Debug.Log(speed);
                //Debug.Log(Motions.Motor.BaseVelocity);
                //Debug.Log(Motions.Motor.MotorVelocity);
                LastVelocity = speed.Multiply(1.2f);

                //if (speed.sqrMagnitude < 0.7f)
                //    speed = speed.Multiply(1.43f);
                //else
                //    speed = speed.Multiply(1.2f);

                var gravity = Motions.GravityForce.magnitude;
                var up = Transform.up;
                var volUp = MathF.Sqrt(2f * pref.JumpHeight * gravity);

                var jumpVel = /*(up+grounded.GroundNormal).normalized*/up.Multiply(volUp);

                Wrapper = new JumpWrapper()
                {
                    OriginVelocity = Motor.Velocity,
                    InertiaVelocity = speed,
                    JumpVelocity = jumpVel,
                    ApplyGravity = true,
                };

                //Motions.SetControllerWrapper(Wrapper);

                Debug.Log($"Jumping inertia : {LastVelocity}");
                PredictDirection = LastVelocity.sqrMagnitude == 0 ?
                    Transform.position :
                    speed.Multiply(volUp / gravity);

                PredictDirection += new Vector3(0, pref.JumpHeight);

                Debug.Log($"predict direction : {PredictDirection} speed : {Wrapper.InertiaVelocity}");

                BeginJumpAnim(Transform.position + PredictDirection);

                if (QuickResponse)
                {
                    ExecuteAnim();
                }
                else
                {
                    WaitEvent = pref.JumpDefinition;

                    Motions.OnAnimEvent -= OnAnimEvent;
                    Motions.OnAnimEvent += OnAnimEvent;
                }

                ModuleVelocity = speed;
            }
        }

        [RFC]
        protected virtual void BeginJumpAnim(Vector3 pos)
        {
            if (IMotions.isMine)
                IMotions.tno.Send(nameof(BeginJumpAnim), Target.Others, pos);

            pref.JumpDefinition.AddEventContact(pos, 0);
            Motions.mxm.BeginEvent(pref.JumpDefinition);

            ref readonly EventContact eventContact = ref IMotions.mxm.NextEventContactRoot_Actual_World;

            State = VaultState.Jumping;

            //Ray ray = new Ray(eventContact.GUIDs + (Vector3.up * 3.5f), Vector3.down);

            //if (Physics.OnSensorUpdate(ray, out var rayHit, 10f)
            //    && rayHit.distance > 1.5f
            //    && rayHit.distance < 5f)
            //{
            //    IMotions.mxm.ModifyDesiredEventContactPosition(rayHit.point);
            //}
            //else
            //{
            //    IMotions.mxm.ModifyDesiredEventContactPosition(eventContact.GUIDs);
            //}
        }

        protected virtual void HandleJumping()
        {
            var vel = Motor.BaseVelocity;
            var grounded = Motor.GroundingStatus;
            var velDir = vel.normalized;
            //Debug.LogError("Grounded : " + grounded.FoundAnyGround + 
            //    "\ndot : " + Vector3.Dot(PredictDirection.normalized, vel.normalized) +
            //    " \nJump ModuleVelocity : " + vel);


            //if (vel.height < 0 && Physics.Raycast(Transform.position, Vector3.down, out var hit))
            //    Debug.Log(hit.distance);

            //if (Wrapper.IsFinish//((WaitEvent == null || (!Wrapper||Wrapper.IsFinish) || Wrapper.init)
            //    &&
            //    Vector3.Dot(velDir, PredictDirection.normalized)<0.7f)//)
            //{
            //    State = VaultState.General;
            //    Debug.LogError("Break Vault via mathf caculate.");
            //} else

            if (grounded.IsStableOnGround)
            {
                State = VaultState.General;
                Debug.LogError("Break Vault via grounded " + grounded.GroundCollider);
            }
            else
            {
                ModuleVector = velDir;
                ModuleVelocity = vel;
            }
            return;

            var sqrVel = vel.sqrMagnitude;
            var radius = Motions.ControllerRadius;

            var count = Physics.SphereCastNonAlloc(Transform.position, radius, vel.normalized, Hits, sqrVel);

            for (int i = 0; i < count; i++)
            {
                var distance = Hits[i].distance;



            }




            //var wrapper = Motor.GetDelegateVelocityWrapper();

            //if(Physics.SphereCast(ReadOnlyTransform.position,0.2f,vel, out var hit))
            {
                //Motor.mxm.GetCurveHandle()
                //(hit.distance)
            }

        }
        #endregion

        #region Vault
        VaultDefinition vaultDef;

        protected virtual void Register_Vault()
        {
            if (pref.VaultConfigurations == null || pref.VaultConfigurations.Length == 0)
            {
                Debug.LogError("VaultDetector: Trying to Awake Vault Detector with null or empty vault configurations (m_vaultConfiguration)");
                Motions.RemoveMotionModule(this);
                return;
            }

            if (pref.VaultConfigurations == null || pref.VaultConfigurations.Length == 0)
            {
                Debug.LogError("VaultDetector: Trying to Awake Vault Detector with null or empty vault definitions (m_vaultDefinitions)");
                Motions.RemoveMotionModule(this);
                return;
            }

            m_minVaultRise = float.MaxValue;
            m_maxVaultRise = float.MinValue;
            m_minVaultDepth = float.MaxValue;
            m_maxVaultDepth = float.MinValue;
            m_minVaultDrop = float.MaxValue;
            m_maxVaultDrop = float.MinValue;

            foreach (var vd in pref.VaultDefinitions)
            {
                switch (vd.VaultType)
                {
                    case EVaultType.StepUp:
                        {
                            if (vd.MinRise < m_minVaultRise) { m_minVaultRise = vd.MinRise; }
                            if (vd.MaxRise > m_maxVaultRise) { m_maxVaultRise = vd.MaxRise; }
                            if (vd.MinDepth < m_minVaultDepth) { m_minVaultDepth = vd.MinDepth; }
                        }
                        break;
                    case EVaultType.StepOver:
                        {
                            if (vd.MinRise < m_minVaultRise) { m_minVaultRise = vd.MinRise; }
                            if (vd.MaxRise > m_maxVaultRise) { m_maxVaultRise = vd.MaxRise; }
                            if (vd.MinDepth < m_minVaultDepth) { m_minVaultDepth = vd.MinDepth; }
                            if (vd.MaxDepth > m_maxVaultDepth) { m_maxVaultDepth = vd.MaxDepth; }
                        }
                        break;
                    case EVaultType.StepOff:
                        {
                            if (vd.MinDepth < m_minVaultDepth) { m_minVaultDepth = vd.MinDepth; }
                            if (vd.MinDrop < m_minVaultDrop) { m_minVaultDrop = vd.MinDrop; }
                            if (vd.MaxDrop > m_maxVaultDrop) { m_maxVaultDrop = vd.MaxDrop; }
                        }
                        break;
                }
            }

            m_curConfig = pref.VaultConfigurations[0];
            DesiredAdvance = Advance = 0f;

            m_vaultAnalysisIterations = (int)(m_maxVaultDepth / m_curConfig.ShapeAnalysisSpacing) + 1;
        }
        private bool CanVault()
        {
            if (m_isVaulting)
                return false;

            var groundingStatus = Motor.GroundingStatus;

            //Do not trigger a vault if the character is not grounded
            if (!groundingStatus.IsStableOnGround)
                return false;

            //CheckAdd that there is user movement Input
            if (IMotions.MotionVector == Vector3.zero)
                return false;

            //CheckAdd that the angle beteen Input and the character facing direction is within an acceptable range to vault
            float inputAngleDelta = Vector3.Angle(Transform.forward/*Parameters.GetVector3(pref.CharacterForward)*/, IMotions.MotionVelocity);
            if (inputAngleDelta > 45f)
                return false;

            //Calculate Advance and determine if it higher than the minimum required advance to perform a vault
            DesiredAdvance = (IMotions.mxm.BodyVelocity * m_curConfig.DetectProbeAdvanceTime).magnitude;
            Advance = Mathf.Lerp(Advance, DesiredAdvance, 1f - Mathf.Exp(-m_advanceSmoothing));
            if (Advance < m_minAdvance)
                return false;

            //if (!ValidModuleMotion(pref.RightLeg, pref.LeftLeg))
            //    return false;

            return true;
        }

        public virtual void Vault()
        {
            //We are going to use the transformposition and forward direction a lot. Caching it here for perf
            Vector3 charPos = Transform.position;
            Vector3 charForward = Transform.forward;
            float approachAngle = 0f;

            //First we must fire a ray forward from the character, slightly above the minimum vault rise (aka the character controller minimum step
            Vector3 probeStart = new Vector3(charPos.x, charPos.y +
                m_curConfig.DetectProbeRadius + m_minVaultRise,
                charPos.z);

            Ray forwardRay = new Ray(probeStart, charForward);
            RaycastHit forwardRayHit;

            if (Physics.SphereCast(forwardRay, m_curConfig.DetectProbeRadius, out forwardRayHit,
                Advance, pref.LayerMask, QueryTriggerInteraction.Ignore))
            {
                //If we hit something closer than the current advance, then we shorten the advance since we want the
                //First downward sphere case to hit the edge.
                if (forwardRayHit.distance < Advance)
                    Advance = forwardRayHit.distance;

                Vector3 obstacleOrient = Vector3.ProjectOnPlane(forwardRayHit.normal, Vector3.up) * -1f;

                approachAngle = Vector3.SignedAngle(charForward, obstacleOrient, Vector3.up);

                //If we encounter an obstacle but at an angle above our max, we don't want to vault so return here.
                //QUESTION: What if the actual vault point (i.e. at the top of the obstacle is within the correct angle?
                if (Mathf.Abs(approachAngle) > m_maxApproachAngle)
                    return;
            }

            //Next we fire a ray vertically downward from the maximum vault rise to the maximum vault Schedule_Drop
            //NOTE: This does not take into consideration a roof or an overhang
            probeStart = Transform.TransformPoint(new Vector3(0f, 0f, Advance));
            probeStart.y += m_maxVaultRise;

            Ray probeRay = new Ray(probeStart, Vector3.down);
            RaycastHit probeHit;
            if (Physics.SphereCast(probeRay, m_curConfig.DetectProbeRadius, out probeHit, m_maxVaultRise + m_maxVaultDrop,
                pref.LayerMask, QueryTriggerInteraction.Ignore))
            {
                //Too high -> cancel the vault
                if (probeHit.distance < Mathf.Epsilon)
                    return;

                //A 'vault over' or 'vault up' may have been detected if the probe distance is between the minimum and maximum vault rise
                if (probeHit.distance < m_maxVaultRise - m_minVaultRise)
                {
                    //A vault may have been detected

                    //CheckAdd if there is enough rows to fit the character
                    if (!CheckCharacterHeightFit(probeHit.point, charForward))
                        return;

                    //Calculate the hit offset. This is the offset on a horizontal 2D plane between the start of the ray and the hit point

                    //Here we conduct a shape analysis of the vaultable object and store that data in a Vaultable Profile
                    VaultableProfile vaultable;
                    VaultShapeAnalysis(in probeHit, out vaultable);

                    if (vaultable.VaultType == EVaultType.Invalid)
                        return;

                    //CheckAdd for enough space on top of the object (for a step up)
                    if (vaultable.VaultType == EVaultType.StepUp && vaultable.Depth < pref.MinStepUpDepth)
                        return;

                    //CheckAdd object surface gradient (Assume ok for now)



                    //Select appropriate vault defenition
                    vaultDef = ComputeBestVault(ref vaultable);

                    if (vaultDef == null)
                        return;

                    // CheckAdd CanPlay
                    if (!ValidModuleMotion(pref.RightLeg, pref.LeftLeg))
                        return;



                    float facingAngle = Transform.rotation.eulerAngles.y;

                    if (vaultDef.LineUpWithObstacle)
                    {
                        facingAngle += approachAngle;
                    }

                    //Pick contacts
                    vaultDef.EventDefinition.ClearContacts();

                    switch (vaultDef.OffsetMethod_Contact1)
                    {
                        case EVaultContactOffsetMethod.Offset: { vaultable.Contact1 += Transform.TransformVector(vaultDef.Offset_Contact1); } break;
                        case EVaultContactOffsetMethod.DepthProportion: { vaultable.Contact1 += Transform.TransformVector(vaultable.Depth * vaultDef.Offset_Contact1); } break;
                    }

                    vaultDef.EventDefinition.AddEventContact(vaultable.Contact1, facingAngle);

                    if (vaultable.VaultType == EVaultType.StepOver)
                    {
                        switch (vaultDef.OffsetMethod_Contact2)
                        {
                            case EVaultContactOffsetMethod.Offset: { vaultable.Contact2 += Transform.TransformVector(vaultDef.Offset_Contact2); } break;
                            case EVaultContactOffsetMethod.DepthProportion: { vaultable.Contact2 += Transform.TransformVector(vaultable.Depth * vaultDef.Offset_Contact2); } break;
                        }

                        vaultDef.EventDefinition.AddEventContact(vaultable.Contact2, facingAngle);
                    }

                    //Trigger event
                    IMotions.mxm.BeginEvent(vaultDef.EventDefinition);

                    IMotions.mxm.PostEventTrajectoryMode = EPostEventTrajectoryMode.Pause;

                    //if (m_rootMotionApplicator != null)
                    //    m_rootMotionApplicator.EnableGravity = false;

                    if (vaultDef.DisableCollision && IMotions != null)
                        CollisionEnabled = false;

                    State = VaultState.Vaulting;
                }
                else //Detect a step off or vault over gap
                {
                    Vector3 flatHitPoint = new Vector3(probeHit.point.x, 0f, probeHit.point.z);
                    Vector3 flatProbePoint = new Vector3(probeStart.x, 0f, probeStart.z);

                    Vector3 dir = flatProbePoint - flatHitPoint; // The direction of the ledge

                    if (dir.sqrMagnitude > m_curConfig.DetectProbeRadius * m_curConfig.DetectProbeRadius / 4f)
                    {
                        //A step off may have occured

                        //Shape analysis 
                        Vector2 start2D = new Vector2(probeStart.x, probeStart.z);
                        Vector2 hit2D = new Vector2(probeHit.point.x, probeHit.point.z);

                        float hitOffset = Vector2.Distance(start2D, hit2D);

                        VaultableProfile vaultable;
                        VaultOffShapeAnalysis(in probeHit, out vaultable, hitOffset);

                        if (vaultable.VaultType == EVaultType.Invalid)
                            return;

                        VaultDefinition vaultDef = ComputeBestVault(ref vaultable);

                        if (vaultDef == null)
                            return;

                        // _checkCache CanPlay
                        if (!ValidModuleMotion(pref.RightLeg, pref.LeftLeg))
                            return;

                        float facingAngle = Transform.rotation.eulerAngles.y;

                        //Pick contacts
                        vaultDef.EventDefinition.ClearContacts();

                        switch (vaultDef.OffsetMethod_Contact1)
                        {
                            case EVaultContactOffsetMethod.Offset: { vaultable.Contact1 += Transform.TransformVector(vaultDef.Offset_Contact1); } break;
                            case EVaultContactOffsetMethod.DepthProportion: { vaultable.Contact1 += Transform.TransformVector(vaultable.Depth * vaultDef.Offset_Contact1); } break;
                        }

                        vaultDef.EventDefinition.AddEventContact(vaultable.Contact1, facingAngle);

                        IMotions.mxm.BeginEvent(vaultDef.EventDefinition);
                        IMotions.mxm.PostEventTrajectoryMode = EPostEventTrajectoryMode.Pause;

                        //if (m_rootMotionApplicator != null)
                        //    m_rootMotionApplicator.EnableGravity = false;

                        //if (m_controllerWrapper != null)
                        //CollisionEnabled = false;
                        //Time.timeScale = 0.1f;

                        State = VaultState.Vaulting;
                    }
                }


            }
        }

        private void HandleCurrentVault()
        {
            // UserTag2 is enable gravity
            //if (m_rootMotionApplicator.EnableGravity == m_Motions.mxm.QueryUserTags(EUserTags.UserTag1))
            //{
            //    m_rootMotionApplicator.EnableGravity = !m_rootMotionApplicator.EnableGravity;
            //}

            // UserTag2 is enable collision
            if (CollisionEnabled == IMotions.mxm.QueryUserTags(EUserTags.UserTag2))
                CollisionEnabled = !CollisionEnabled;

            if (IMotions.mxm.IsEventComplete)
            {
                State = VaultState.General;

                if (IMotions != null)
                    CollisionEnabled = true;

                Advance = 0f;

            }

            return;
        }


        private bool CheckCharacterHeightFit(Vector3 a_fromPoint, Vector3 a_forward)
        {
            float radius = Motions.ControllerRadius;
            Vector3 fromPosition = a_fromPoint + a_forward * radius * 2f + Vector3.up * radius * 1.1f;

            Ray upRay = new Ray(fromPosition, Vector3.up);
            if (Physics.Raycast(upRay, out _, Motions.ControllerHeight, pref.LayerMask, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            return true;
        }

        private void VaultOffShapeAnalysis(in RaycastHit a_rayHit, out VaultableProfile a_vaultProfile, float hitOffset)
        {
            a_vaultProfile = new VaultableProfile();

            Vector3 lastPoint = a_rayHit.point;
            bool stepOffStart = false;
            for (int i = 1; i < m_vaultAnalysisIterations; ++i)
            {
                Vector3 start = Transform.TransformPoint(Vector3.forward *
                    (Advance + hitOffset + i * m_curConfig.ShapeAnalysisSpacing));
                start.y += m_maxVaultRise;

                Ray ray = new Ray(start, Vector3.down);
                RaycastHit rayHit;

                if (Physics.Raycast(ray, out rayHit, m_maxVaultRise + m_maxVaultDrop, pref.LayerMask, QueryTriggerInteraction.Ignore))
                {
                    float deltaHeight = rayHit.point.y - lastPoint.y;

                    if (!stepOffStart)
                    {
                        if (deltaHeight < -m_minVaultDrop)
                        {
                            a_vaultProfile.Drop = Mathf.Abs(deltaHeight);
                            a_vaultProfile.Contact1 = rayHit.point;
                            stepOffStart = true;
                        }
                    }
                    else
                    {
                        if (deltaHeight > m_minVaultRise)
                        {
                            a_vaultProfile.Depth = a_vaultProfile.Depth = (i - 1) * m_curConfig.ShapeAnalysisSpacing;

                            if (a_vaultProfile.Depth > 1f)
                            {
                                a_vaultProfile.Rise = deltaHeight;
                                a_vaultProfile.VaultType = EVaultType.StepOff;
                            }
                            else
                            {
                                a_vaultProfile.VaultType = EVaultType.Invalid;
                            }

                            return;
                        }
                        else if (i == m_vaultAnalysisIterations - 1)
                        {
                            a_vaultProfile.Rise = 0f;
                            a_vaultProfile.Depth = a_vaultProfile.Depth = (i - 1) * m_curConfig.ShapeAnalysisSpacing;
                            a_vaultProfile.VaultType = EVaultType.StepOff;
                            return;
                        }
                    }
                }
                else
                {
                    a_vaultProfile.Depth = a_vaultProfile.Depth = (i - 1) * m_curConfig.ShapeAnalysisSpacing;

                    if (a_vaultProfile.Depth > 1f)
                    {
                        a_vaultProfile.Rise = 0f;
                        a_vaultProfile.VaultType = EVaultType.StepOff;
                    }
                    else
                    {
                        a_vaultProfile.VaultType = EVaultType.Invalid;
                    }

                    return;
                }

                lastPoint = rayHit.point;
            }
        }

        //============================================================================================
        /**
        *  @brief This function is called to analyse the shape of a potentialy vaultable object. By 
        *  analysing the shape with raycasts, it's easy to then match the metrics of that shape to a 
        *  number of vaulable definitions with accompanying animations
        *  
        *  @param [in RaycastHit] a_rayHit - The raycast restuls for the ray that hit the vaultable edge
        *  @param [out VaultableProfile] a_vaultProfile - the container for all metrics of the shape analysis
        *  
        *  @Todo: Implement with Jobs and Burst
        *         
        *********************************************************************************************/
        private void VaultShapeAnalysis(in RaycastHit a_rayHit, out VaultableProfile a_vaultProfile)
        {
            a_vaultProfile = new VaultableProfile();

            a_vaultProfile.Contact1 = a_rayHit.point;
            //  a_vaultProfile.Rise = m_maxVaultRise - a_rayHit.distance;

            Vector3 charPos = Transform.position;
            Vector3 lastPoint = a_rayHit.point;
            Vector3 highestPoint = lastPoint;
            Vector3 lowestPoint = charPos;

            a_vaultProfile.Rise = a_rayHit.point.y - charPos.y;

            //We need to iterate several times, casting rays downwards to determine the shape of the object in
            //a straight rows from the character
            for (int i = 1; i < m_vaultAnalysisIterations; ++i)
            {
                //Each iteration we move the starting point one spacing further
                Vector3 start = a_rayHit.point + Transform.TransformVector(Vector3.forward
                    * i * m_curConfig.ShapeAnalysisSpacing);

                start.y += charPos.y + m_maxVaultRise;
                Ray ray = new Ray(start, Vector3.down);
                RaycastHit rayHit;

                if (Physics.Raycast(ray, out rayHit, m_maxVaultRise + m_maxVaultDrop, pref.LayerMask, QueryTriggerInteraction.Ignore))
                {
                    if (rayHit.point.y > highestPoint.y)
                    {
                        highestPoint = rayHit.point;
                    }
                    else if (rayHit.point.y < lowestPoint.y)
                    {
                        lowestPoint = rayHit.point;
                    }

                    float deltaHeight = rayHit.point.y - lastPoint.y;

                    //If the change in rows from one ray to another is greater than the minimum vault Schedule_Drop, then
                    //we may have detected a step over. However! We can only declare a vault over if there is enough 
                    //space for the character on the other side. This is determined by controller lines and current velocity
                    if (deltaHeight < -m_minVaultDrop)
                    {
                        //Step Over
                        a_vaultProfile.Depth = (i - 1) * m_curConfig.ShapeAnalysisSpacing;

                        a_vaultProfile.Drop = a_rayHit.point.y - rayHit.point.y;
                        a_vaultProfile.VaultType = EVaultType.StepOver;
                        a_vaultProfile.Contact2 = rayHit.point;

                        return; //TODO: Remove this return point. The entire vault needs to be analysed before a decision is made in case the character doesn't fit
                    }
                    else if (i == m_vaultAnalysisIterations - 1)
                    {
                        a_vaultProfile.Depth = (i - 1) * m_curConfig.ShapeAnalysisSpacing;
                        a_vaultProfile.Drop = 0f;
                        a_vaultProfile.VaultType = EVaultType.StepUp;
                    }
                }
                else
                {
                    //Step Over Fall
                    a_vaultProfile.Drop = m_maxVaultDrop;
                    a_vaultProfile.Depth = (i - 1) * m_curConfig.ShapeAnalysisSpacing;
                    a_vaultProfile.VaultType = EVaultType.StepOverFall;
                    return;
                }

                lastPoint = rayHit.point;
            }
        }

        private VaultDefinition ComputeBestVault(ref VaultableProfile a_vaultable)
        {
            foreach (var vaultDef in pref.VaultDefinitions)
            {
                if (vaultDef.VaultType == a_vaultable.VaultType)
                {
                    switch (vaultDef.VaultType)
                    {
                        case EVaultType.StepUp:
                            {
                                if (a_vaultable.Depth < vaultDef.MinDepth)
                                    continue;

                                if (a_vaultable.Rise < vaultDef.MinRise || a_vaultable.Rise > vaultDef.MaxRise)
                                    continue;

                            }
                            break;
                        case EVaultType.StepOver:
                            {
                                if (a_vaultable.Depth < vaultDef.MinDepth || a_vaultable.Depth > vaultDef.MaxDepth)
                                    continue;

                                if (a_vaultable.Rise < vaultDef.MinRise || a_vaultable.Rise > vaultDef.MaxRise)
                                    continue;

                                if (a_vaultable.Drop < vaultDef.MinDrop || a_vaultable.Drop > vaultDef.MaxDrop)
                                    continue;

                            }
                            break;
                        case EVaultType.StepOff:
                            {
                                if (a_vaultable.Depth < vaultDef.MinDepth)
                                    continue;

                                if (a_vaultable.Drop < vaultDef.MinDrop || a_vaultable.Drop > vaultDef.MaxDrop)
                                    continue;

                            }
                            break;
                    }

                    return vaultDef;
                }
            }

            return null;
        }

        #endregion
    }
}