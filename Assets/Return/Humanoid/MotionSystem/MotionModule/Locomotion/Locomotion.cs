using UnityEngine;
using System;
using MxM;
using Return.Agents;
using KinematicCharacterController;
using UnityEngine.Assertions;
using System.Collections.Generic;
using Return.Creature;
using Return.Preference;
using Return.Cameras;
using Sirenix.OdinInspector;
using Return.Motions;
using Return.Modular;

namespace Return.Humanoid.Motion
{
    /// <summary>
    /// Locomotion module control via local player **Walk **Sprint **Crouch
    /// </summary>
    public partial class Locomotion : MotionModule_Humanoid, IKCCVelocityModule
    {
        #region Preset

        public override void LoadData(MotionModulePreset data)
        {
            pref = data as MotionModulePreset_Locomotion;
        }

        public override MotionModulePreset_Humanoid GetData => pref;

        public MotionModulePreset_Locomotion pref { get; protected set; }

        #endregion

        #region Resolver

        [Inject]
        IMxMHandler IMxMHandler;

        [Inject]
        ICharaterMotionHandler ICharaterMotionHandler;

        /// <summary>
        /// **User Input **AI control
        /// </summary>
        IModule ControlHandle;

        public override void SetHandler(IMotionSystem module)
        {
            base.SetHandler(module);


        }


        #endregion

        #region State

        /// <summary>
        /// ???
        /// </summary>
        protected Vector3 InertiaVelocity;

        #endregion

        #region IKCCVelocityModule

        public virtual void UpdateInertiaVelocity(ref Vector3 currentInertia,float deltaTime)
        {
            if (!enabled)
                return;

            

            // position bias different between fps and tps, snow ground also
            currentInertia = Vector3.zero;
            currentInertia += InertiaVelocity;
        }

        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            Debug.Log($"Frame {Time.frameCount} Module Enable {EnableMotion} Input {Input_Movement}");

            if (!EnableMotion)
                return;

            currentVelocity += ModuleVelocity;
            //Debug.Log($"Frame {Time.frameCount} Module velocity {ModuleVelocity} Grounded {Motor.GroundingStatus.IsStableOnGround}");

            //Debug.DrawRay(Motor.Transform.position, currentVelocity.normalized, Color.green, 5);
        }

        /// <summary>
        /// Push module velocity as inertia.
        /// </summary>
        protected virtual void ConvertInertia()
        {
            if (sqrInput != 0)// turn motor vel to inertia vel
            {
                var dur = (duration / 1.3f).Clamp01();

                //var vel = Parameters.GetVector3(pref.ModuleVelocity).Multiply(dur);
                var vel = ModuleVelocity.Multiply(dur);

                Debug.Log($"Set Inertia : {vel} from {dur}.");

                ModuleVelocity = Vector3.zero;
                InertiaVelocity = vel;

                //Motions.ConvertModuleVelocityToInertia(vel);
            }
        }

        #endregion




        #region Parameters

        protected float MaxMoveSpeed;

        /// <summary>
        /// 0 IdleState 1 Walk 2 Run 3 Sprint 
        /// </summary>
        protected int MovementGrade { get; set; }

        public ETags m_IdleTagHandle;
        public ETags m_walkTagHandle;
        public ETags m_runTagHandle;
        public ETags m_sprintTagHandle;

        #endregion

        #region Routine


        protected override void Register()
        {
            base.Register();

            Assert.IsNotNull(IMotions);
            Assert.IsNotNull(IMotions.mxm);
            Assert.IsNotNull(IMotions.mxm.CurrentAnimData);

            m_IdleTagHandle = IMotions.mxm.CurrentAnimData.FavourTagFromName(pref.IdleTagName);
            m_walkTagHandle = IMotions.mxm.CurrentAnimData.FavourTagFromName(pref.WalkSpeedTagName);
            m_runTagHandle = IMotions.mxm.CurrentAnimData.FavourTagFromName(pref.RunSpeedTagName);
            m_sprintTagHandle = IMotions.mxm.CurrentAnimData.FavourTagFromName(pref.SprintSpeedTagName);


            if (IMotions.IsLocalUser)
            {
                if (ControlHandle == null)
                {
                    var input = new InputHandle();
                    input.RegisterInput();
                    input.SetHandler(this);
                    ControlHandle = input;
                }
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

            Motions.SetMotionModule(this);

            if (IMotions.IsLocalUser)
            {
                CameraManager.SubscribeCamera<FirstPersonCamera>(OnCameraChange);//.Activate();
            }

            ControlHandle?.Activate();

            return;

            Init_IK();
        }

        protected override void Deactivate()
        {
            base.Deactivate();

            ControlHandle?.Deactivate();
            ControlHandle = null;
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


        /// <summary>
        /// Custom grounded module
        /// </summary>
        protected virtual void FixedTick(float deltaTime)
        {
            //if (InertiaVelocity != Vector3.zero)
            //    Motions.ApplyAirDragVelocity(ref InertiaVelocity, deltaTime);

            var groundingStatus = Motions.GroundingStatus; //ICharaterMotionHandler
            //= Parameters.GetValue<CharacterGroundingReport>(pref.GroundingStatus);

            if (EnableMotion)
            {
                duration += deltaTime;
                CaculateLocomotion(groundingStatus);
            }
            else // inertia _checkCache grounded 
            {
                if (groundingStatus.IsStableOnGround && sqrInput == 0)
                {
                    ModuleVelocity = Vector3.zero;
                    //InertiaVelocity = Vector3.zero;
                }
                else if (groundingStatus.IsStableOnGround && sqrInput != 0)
                    return; // ActivateMotion();
            }

            //Debug.Log($"Frame {Time.frameCount} Fixed Move {ModuleVelocity}");
        }

        #endregion

        #region Motion Sequence

        protected override void OnMotionStart()
        {
            MaxMoveSpeed = pref.WalkSpeed;

            {
                Motions.VelocityBlendRatio = pref.VelocityBlendRatio_FirstPerson;
                Motions.AngularVelocityBlendRatio = pref.BlendRatio_AngularVelocity;

                Motions.EnableGravity = true;
                Motions.EnableCollider = true;
            }


            Motions.AlignViewMotion = true;
        }


        protected override bool ValidModuleMotion(params Limb[] limbs)
        {
            return base.ValidModuleMotion(limbs);
        }

        protected override void FinishModuleMotion()
        {
            base.FinishModuleMotion();

            //Parameters.SetBool(pref.InputUpdate, true);

            //var grounded = Parameters.GetValue<CharacterGroundingReport>(pref.GroundingStatus).IsStableOnGround;

            var grounded = Motor.GroundingStatus.FoundAnyGround;

            if (grounded)
            {
                ModuleVelocity = Vector3.zero;
                //Parameters.SetVector3(pref.ModuleVelocity, Vector3.zero);
            }

            ConvertInertia();

            Debug.Log($"{nameof(FinishModuleMotion)} Grounded : {grounded} - {this}");

            Crouch = false;

        }

        public override bool CanQueue(out int priority)
        {
            priority = 100;
            return true;
        }


        public override void InterruptMotion(Limb limb)
        {
            base.InterruptMotion(limb);
            Crouch = false;
            //ConvertInertia();
        }

        #endregion

       

        #region GroundedIK
        LimbIKWrapper RightLeg;
        LimbIKWrapper LeftLeg;

        // set locomotion foot ik => iStep
        void Init_IK()
        {
            RightLeg = IMotions.GetIK(HashCode, Limb.RightLeg);
            LeftLeg = IMotions.GetIK(HashCode, Limb.LeftLeg);
        }


        private void IKGrounded(float deltaTime)
        {
            return;

            RightLeg.GoalPosition = RightLeg.GoalPosition;
            LeftLeg.GoalPosition = LeftLeg.GoalPosition;
        }


        #endregion


        /// <summary>
        /// ??? double phase
        /// </summary>
        protected virtual void CaculateLocomotion(CharacterGroundingReport groundingStatus)
        {
            if (Input_Movement == Vector2.zero)
            {
                ModuleVector = Vector3.zero;
                ModuleVelocity = Vector3.zero;
                return;
            }


            // Clamp Input
            var moveInputVector = Vector3.ClampMagnitude(new Vector3(Input_Movement.x, 0f, Input_Movement.y), 1f);

            //var cameraPlanarRotation = Parameters.GetQuaternion(pref.ViewPortPlanarRotation);
            //var maxSpeed = Parameters.GetFloat(pref.MaxStableMoveSpeed);

            // Move and look inputs
            //Rotate our Input vector relative to the camera

            var forward = Motor.CharacterForward;
                //Parameters.GetVector3(pref.ViewPortPlanarDirection);
            moveInputVector = Quaternion.FromToRotation(Vector3.forward, forward) * moveInputVector;
            //moveInputVector = cameraPlanarRotation * moveInputVector;

            #region InputBias

            //If we are using an Input profile then run it and get our Input scale and bias multipliers
            if (m_mxmInputProfile != null)
            {
                var inputData = m_mxmInputProfile.GetInputScale(moveInputVector);
                moveInputVector.MultiplyWith(inputData.scale);
                IMotions.mxm.LongErrorWarpScale = 1f / inputData.scale;

                // upload Input bais ?
                var input_posBias = inputData.posBias;
                var input_rotBias = inputData.dirBias;
            }
            #endregion

            //Normalized
            //if (sqrInput > 1f)
            moveInputVector.Normalize();

            ModuleVector = moveInputVector;

            var targetMovementVelocity = Vector3.zero;

            // calculate step

            if(groundingStatus.FoundAnyGround)
                Debug.DrawRay(Motor.Transform.position, groundingStatus.GroundNormal.normalized, Color.red, 5);


            if (groundingStatus.IsStableOnGround || Motor.BaseVelocity == Vector3.zero)
            {
                //var inputVector = moveInputVector;//Parameters.GetVector3(pref.ModuleVector);
                var characterUp = Motor.CharacterUp;
                //Parameters.GetVector3(pref.characterUp);

                var maxMovSpeed = MaxMoveSpeed;

                // Calculate target velocity
                Vector3 inputRight = Vector3.Cross(moveInputVector, characterUp);
                Vector3 reorientedInput = Vector3.Cross(groundingStatus.GroundNormal, inputRight).normalized * moveInputVector.magnitude;
                targetMovementVelocity = reorientedInput.Multiply(maxMovSpeed);
                //Debug.Log(groundingStatus.IsStableOnGround + "-" + maxMovSpeed);

                // downward
                var effectiveGroundNormal = groundingStatus.GroundNormal;

                Debug.DrawRay(Motor.Transform.position, effectiveGroundNormal, Color.white, 5);

                targetMovementVelocity = Motor.GetDirectionTangentToSurface(targetMovementVelocity, effectiveGroundNormal).Multiply(targetMovementVelocity.magnitude);


                //Debug.DrawRay(Motor.Transform.position, targetMovementVelocity.normalized, Color.black, 5);

                //Debug.Log($"OnLocomotion velocity {Motor.Transform.InverseTransformDirection(targetMovementVelocity)} normal {effectiveGroundNormal}");

                //Debug.Log("ModuleVelocity " + targetMovementVelocity);
                ModuleVelocity = targetMovementVelocity;
                //Parameters.SetBool(pref.MotionUpdate);

                // via Input or actual maxMovSpeed
                UpdateSpeedRamp();

                //InertiaVelocity = Vector3.zero;
            }
            else // air movement
            {
                var airAccelerationSpeed = MaxMoveSpeed * 1.3f;
                var MaxAirMoveSpeed = airAccelerationSpeed;

                // Add move Input
                if (sqrInput > 0f)
                {
                    Vector3 addedVelocity = moveInputVector.Multiply(airAccelerationSpeed * ConstCache.deltaTime);

                    Vector3 currentVelocityOnInputsPlane = Vector3.ProjectOnPlane(Motor.BaseVelocity, Motor.CharacterUp);

                    // Limit air velocity from inputs
                    if (currentVelocityOnInputsPlane.magnitude < MaxAirMoveSpeed)
                    {
                        // clamp addedVel to make total vel not exceed max vel on inputs plane
                        Vector3 newTotal = Vector3.ClampMagnitude(currentVelocityOnInputsPlane + addedVelocity, MaxAirMoveSpeed);
                        addedVelocity = newTotal - currentVelocityOnInputsPlane;
                    }
                    else
                    {
                        // Make sure added vel doesn't go in the direction of the already-exceeding velocity
                        if (Vector3.Dot(currentVelocityOnInputsPlane, addedVelocity) > 0f)
                        {
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, currentVelocityOnInputsPlane.normalized);
                        }
                    }

                    // Prevent air-climbing sloped walls
                    if (groundingStatus.FoundAnyGround)
                    {
                        if (Vector3.Dot(Motor.BaseVelocity + addedVelocity, addedVelocity) > 0f)
                        {
                            Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, groundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                            addedVelocity = Vector3.ProjectOnPlane(addedVelocity, perpenticularObstructionNormal);
                        }
                    }

                    // Apply added velocity
                    //currentVelocity += addedVelocity;
                    //InertiaVelocity = addedVelocity;
                    ModuleVelocity = addedVelocity;
                }

                Debug.Log($"Non grounded movement {ModuleVelocity}");

            }
            //else
            //{
            //    FinishModuleMotion();
            //    return;
            //    //FinishModuleMotion();
            //    //return;
            //    //targetMovementVelocity = Parameters.GetVector3(pref.ModuleVelocity);
            //    //var drag= Parameters.GetVector3(pref.ModuleVelocity);
            //    //targetMovementVelocity = Vector3.down * 10;
            //}

            return;

            if (sqrInput == 0)
                FinishModuleMotion();
        }


        #region Movement



        /// <summary>
        /// current Input profile
        /// </summary>
        MxMInputProfile m_mxmInputProfile;

        protected Vector2 Input_Movement;

        [ShowInInspector]
        protected float sqrInput;

        protected float duration;


        /// <summary>
        /// Valid motion enable if control handle execute.
        /// </summary>
        void ActivateMotion()
        {
            if (!ValidModuleMotion(pref.GetMotionLimbs))
                return;

            //Parameters.SetBool(pref.InputUpdate, sqrInput > 0);

            UpdateSpeedRamp();
        }

        #endregion


        #region Crouch

        [ShowInInspector]
        protected bool m_Crouch;

        public bool Crouch
        {
            get => m_Crouch;
            set
            {
                if (value)
                {
                    IMxMHandler.AddRequireTag("Crouch");
                    Motions.SetControllerHeight(pref.CrouchHeight);
                }
                else
                {
                    IMxMHandler.RemoveRequireTag("Crouch");
                    Motions.ResetControllerBounds();
                }
                m_Crouch = value;
            }
        }

        protected virtual void StartCrouch()
        {
            if (ValidModuleMotion(pref.GetMotionLimbs))
            {
                if (!Crouch)
                {
                    Crouch = true;
                }
                else
                    StopCrouch();
            }
        }

        protected virtual void StopCrouch()
        {
            Crouch = false;
        }



        #endregion


        #region Sprint

        protected virtual void StartSprint()
        {
            if (!ValidModuleMotion(pref.GetMotionLimbs))
                return;

            BeginSprint();
            MaxMoveSpeed = pref.SprintSpeed;

            //Parameters.SetFloat(pref.PositionBias, 6f);
            //Parameters.SetFloat(pref.RotationBias, 6f);
            Motions.PositionBias = 6f;
            Motions.RotationBias = 6f;

            IMotions.mxm.SetCalibrationData(pref.SprintSpeedTagName);
            //BlackBoard.InputProfile = pref.m_sprintLocomotion;
        }

        protected virtual void StopSprint()
        {
            if (!EnableMotion)
                return;

            ResetFromSprint();
            MaxMoveSpeed = pref.WalkSpeed;
            //Parameters.SetFloat(pref.PositionBias, 10f);
            //Parameters.SetFloat(pref.RotationBias, 10f);
            Motions.PositionBias = 10f;
            Motions.RotationBias = 10f;
            IMotions.mxm.SetCalibrationData("General");
            //BlackBoard.m_Trajectory.InputProfile = pref.m_generalLocomotion;
        }


        public void BeginSprint()
        {
            RemoveLastTag();
            MovementGrade = 3;
            AddFavourTag();
        }

        public void ResetFromSprint()
        {
            if (MovementGrade != 3)
                return;

            RemoveLastTag();
            MovementGrade = 1;
            AddFavourTag();

            UpdateSpeedRamp();

            IMotions.mxm.RemoveFavourTags(m_sprintTagHandle);
        }


        protected virtual void AddFavourTag()
        {
            if (Crouch && sqrInput == 0)
            {
                IMxMHandler.mxm.BeginIdle();
                IMxMHandler.SetFavourTags(m_IdleTagHandle);
            }

            var favour = pref.FavourMultiplier;

            if (MovementGrade == 1)
                IMxMHandler.AddFavourTag(m_walkTagHandle, favour);
            else if (MovementGrade == 2)
                IMxMHandler.AddFavourTag(m_runTagHandle, favour);
            else if (MovementGrade == 3)
                IMxMHandler.AddFavourTag(m_sprintTagHandle, favour);
        }

        protected virtual void RemoveLastTag()
        {
            if (sqrInput != 0)
                IMxMHandler.mxm.RemoveFavourTags(m_IdleTagHandle);

            if (MovementGrade == 1)
                IMxMHandler.RemoveFavourTags(m_walkTagHandle);
            else if (MovementGrade == 2)
                IMxMHandler.RemoveFavourTags(m_runTagHandle);
            else if (MovementGrade == 3)
                IMxMHandler.RemoveFavourTags(m_sprintTagHandle);
        }


        // Update is called once per frame
        protected void UpdateSpeedRamp()
        {
            if (sqrInput == 0)
            {
                RemoveLastTag();
                MovementGrade = 0;
                AddFavourTag();
            }
            else
            {
                switch (MovementGrade)
                {
                    case 0: //IdleState
                        {
                            RemoveLastTag();
                            MovementGrade = 1;
                            AddFavourTag();
                        }
                        break;
                    case 1: //Walk
                        {
                            if (sqrInput > 0.33f)//0.7f*0.7f
                            {
                                RemoveLastTag();
                                MovementGrade = 2;
                                AddFavourTag();
                            }
                        }
                        break;

                    case 2: //Run
                        {
                            if (sqrInput < 0.33f)//0.7f*0.7f
                            {
                                RemoveLastTag();
                                MovementGrade = 1;
                                AddFavourTag();
                            }
                        }
                        break;
                }

            }
        }
        #endregion


        #region User Handle

        /// <summary>
        /// Set trajectory mode with current active camera.
        /// </summary>
        protected virtual void OnCameraChange(CustomCamera camera, UTag camType, bool camEnable)
        {
            Debug.LogError(camera);

            if (camEnable)
            {
                float blendRatio;

                switch (camType)
                {
                    case var _ when camType.HasFlag(pref.FirstPersonCamera):
                        blendRatio = pref.VelocityBlendRatio_FirstPerson;
                        IMxMHandler.mxm.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
                        break;

                    case var _ when camType.HasFlag(pref.ThirdPersonCamera):
                        blendRatio = pref.VelocityBlendRatio_ThirdPerson;
                        IMxMHandler.mxm.PastTrajectoryMode = EPastTrajectoryMode.CopyFromCurrentPose;

                        break;

                    default:
                        Debug.LogException(new NotImplementedException(camType.ToString()));
                        return;
                }

                Motions.VelocityBlendRatio = blendRatio;
            }
        }
        #endregion
    }
}