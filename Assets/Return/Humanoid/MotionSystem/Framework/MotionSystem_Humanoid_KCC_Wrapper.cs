using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using Sirenix.OdinInspector;
using Return.Framework.PhysicController;
using ReadOnlyAttribute = Sirenix.OdinInspector.ReadOnlyAttribute;
using System.Linq;

namespace Return.Motions
{
    /// <summary>
    /// order anim(MxM) => reveive root motion => odrer movement(KCC)=>_checkCache state & collision
    /// </summary>
    public partial class MotionSystem_Humanoid /*KCC => Apply Motor*/ : ICharacterController, ICharaterMotionHandler
    {
        #region MotionSystem
        /// <summary>
        /// Perfect?
        /// </summary>
        public event Action<PR> OnAfterMotion;

        #endregion

        #region Resolver

        protected virtual void InstallResolver_KCC()
        {
            Resolver.RegisterModule<ICharaterMotionHandler>(this);
        }


        #endregion




        #region ICharaterMotionHandler

        [TabGroup(Controller)]
        [ShowInInspector, ReadOnly]
        public ICharacterControllerMotor Motor { get; protected set; }

        /// <summary>
        /// Module which can provide controller velocity.
        /// </summary>
        protected List<IKCCVelocityModule> VelocityModules = new();

        /// <summary>
        /// Module which can provide controller angular velocity.
        /// </summary>
        protected List<IKCCAngularVelocityModule> AngularVelocityModules = new();


        public void SetMotionModule(IKCCVelocityModule module)
        {
            if (VelocityModules.CheckAdd(module))
                if (module is IMotionModule motionModule)
                    Module = motionModule;
        }

        public void RemoveMotionModule(IKCCVelocityModule module)
        {
            if (VelocityModules.Remove(module))
                if (Module == module)
                {
                    if (VelocityModules.FirstOrDefault(x => x is IMotionModule) is IMotionModule motionModule)
                        Module = motionModule;
                    else
                        Module = IdleModule;
                }
                 


        }


        public void SetMotionModule(IKCCAngularVelocityModule module)
        {
            AngularVelocityModules.CheckAdd(module);
        }

        public void RemoveMotionModule(IKCCAngularVelocityModule module)
        {
            AngularVelocityModules.Remove(module);
        }

        #endregion

        #region Controller

        public override float ControllerHeight
        {
            get => Motor.Capsule.height;
            protected set => Motor.Capsule.height = value;
        }

        public CapsuleCollider Collider => Motor.Capsule;

        public bool EnableCollider
        {
            get => Motor.Capsule.enabled && !Motor.Capsule.isTrigger;
            set => Motor.SetCapsuleCollisionsActivation(value);
        }

        public override float ControllerRadius
        {
            get => Motor.Capsule.radius;
            protected set => Motor.Capsule.radius = value;
        }

        #endregion

        #region KCC.ICharacterController

        const string Controller="Controller";

        [TabGroup(Controller)]
        public bool ApplyMotion=true;
        [TabGroup(Controller)]
        public bool ApplyMovement=true;
        [TabGroup(Controller)] 
        public bool ApplyTurnround=true;

        [TabGroup(Controller)]
        public List<Collider> IgnoredColliders = new();

        private float m_defaultControllerHeight;
        private float m_defaultControllerRadius;
        private float m_defaultControllerCenter;

        private float p_currentControllerHeight;
        private float p_currentControllerRadius;
        private float p_currentControllerCenter;

        void CacheControllerData()
        {
            p_currentControllerHeight = Motor.Capsule.height;
            p_currentControllerRadius = Motor.Capsule.radius;
            p_currentControllerCenter = Motor.Capsule.center.y;
        }

        [TabGroup(Controller)]
        public bool EnableGravity;

        #region Routine

        /// <summary>
        /// Install character controller motor
        /// </summary>
        protected virtual void Register_KCC()
        {
            if (Motor == null)
                Motor = InstanceIfNull<ICharacterControllerMotor,CharacterControllerMotor>();


            Motor.CharacterController = this;
            Motor.enabled = false;
        }

        protected virtual void Activate_KCC()
        {
            {
                {
                    //Parameters.RegisterSetter(Preset.ViewPortPlanarDirection);
                    StrafeDirection = Vector3.forward;
                }


                m_defaultControllerRadius = Motor.Capsule.radius;
                m_defaultControllerHeight = Motor.Capsule.height;
                m_defaultControllerCenter = Motor.Capsule.center.y;

                CacheControllerData();
                GravityForce = new Vector3(0, -10, 0);//PhysicConfig.Gravity;


                if (LocalSelfVelocity.IsNull())
                    LocalSelfVelocity = new(() => Motor.BaseVelocity.magnitude);

            }


            var pr = Transform.GetWorldPR();
            SetCoordinate(pr, pr);

            Motor.enabled = true;
        }

        protected virtual void Deactivate_KCC()
        {
            Motor.enabled = false;
        }

        protected virtual void Unregister_KCC()
        {
            Destroy(Motor);
        }


        protected virtual void Update_KCC()
        {
            var characterUp = Motor.CharacterUp;
            //var camRotaion = CustomCamera.ReadOnlyTransform.rotation;

            //// Calculate camera direction and rotation on the character plane
            //var cameraPlanarDirection = Vector3.ProjectOnPlane(camRotaion * Vector3.forward, characterUp).normalized;
            //if (cameraPlanarDirection.sqrMagnitude == 0f)
            //{
            //    cameraPlanarDirection = Vector3.ProjectOnPlane(camRotaion * Vector3.up, characterUp).normalized;
            //}
            //var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, characterUp);

            //Parameters.SetQuaternion(Preset.PlanarViewRotation, cameraPlanarRotation);
            //Parameters.SetVector3(Preset.PlanarViewDirection, cameraPlanarDirection);
            //Parameters.SetVector3(Preset.CharacterUp, characterUp);
        }

        protected virtual void FixedUpdate_KCC()
        {
            if (GroundingStatus.IsStableOnGround)
                return;

            // GravityForce
            var gravity = GravityForce.Multiply(ConstCache.deltaTime);
            GravityAcceleration += gravity;

            //var bouds =CharacterBounds;

            //Motor.SetCapsuleDimensions(
            //    Math.Max(bouds.size.width+bouds.size.z,m_defaultControllerRadius),
            //    Mathf.Min(bouds.size.height,0.2f,m_defaultControllerHeight),
            //    bouds.center.height
            //    );
        }


        #endregion


        #region blackboard ????

        /// <summary>
        /// Current motion movent sharpness (**soil **ice **mud )
        /// </summary>
        [TabGroup(Controller)] 
        public float StableMovementSharpness = 15;

        [TabGroup(Controller)] 
        public float OrientationSharpness = 10;

        //[Header("Air Movement")]
        /// <summary>
        /// Terminal speed (human 55m/s)
        /// </summary>
        [TabGroup(Controller)] 
        public float BaseMaxAirMoveSpeed = 10f;

        // unknow ????
        [TabGroup(Controller)] 
        public float AirAccelerationSpeed = 5f;

        /// <summary>
        /// Air drag
        /// </summary>
        [TabGroup(Controller)] 
        public float Drag = 0.1f;


        //[Header("Misc")]
        //public bool RotationObstruction;

        /// <summary>
        /// World space gravity force.
        /// </summary>
        public Vector3 GravityForce { get; set; }

        /// <summary>
        /// Inertia gravity acceleration.
        /// </summary>
        public Vector3 GravityAcceleration;


        #region VelocityWrapper

    
        //VelocityWrapper OverrideVelocity;
        IControllerVelocityWrapper Wrapper;
        public virtual void SetControllerWrapper(IControllerVelocityWrapper wrapper)
        {
            Wrapper = wrapper;
        }

        #endregion


        #endregion


        #region KCC.ICharacterController


        /// <summary>
        /// Inertia.
        /// </summary>
        [NonSerialized]
        Vector3 _Acceleration;
        public Vector3 Acceleration { get => _Acceleration; set => _Acceleration = value; }


        [TabGroup(Controller)]
        public bool WrapInAnimSpeed=true;



        // Blend module velocity and animation velocity.
        // Do not apply Vector3.zero to velocity or motor will freeze when grounded == false.
        public virtual void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            

            GroundingStatus = Motor.GroundingStatus;

            //Debug.Log($"Upate velocity Frame {Time.frameCount} Time {deltaTime*1000}");

            if (deltaTime < float.Epsilon)
            {
                currentVelocity = Vector3.zero;
                return;
            }

            // remote lerp
            if (!isMine)
            {
                var vel = MotorVelocity;
                    //Parameters.GetVector3(Preset.MotorVelocity);
                currentVelocity.LerpTo(vel,deltaTime*20f);
                return;
            }

            #region Overwrite Controller => Wrapper

            if (null != Wrapper)
            {
                //Debug.Log(" grounded : "+Motor.GroundingStatus.FoundAnyGround + " vel : " + currentVelocity);

                Wrapper.Main(ref currentVelocity, deltaTime);

                if (Wrapper.ApplyGravity)
                    ApplyInertiaGravity(ref currentVelocity, deltaTime);



                if (Wrapper.Finish(ref currentVelocity, deltaTime))
                {
                    Wrapper = null;
                    PushVelocity(currentVelocity);
                    return;
                }
                else if (Wrapper.Override)
                {
                    PushVelocity(currentVelocity);
                    return;
                }
            }

            #endregion

            #region new

            if (EnableGravity)
                ApplyInertiaGravity(ref _Acceleration, deltaTime);

            // position bias?
            currentVelocity = Vector3.Lerp(currentVelocity, Acceleration,1);

            foreach (var module in VelocityModules)
                module.UpdateVelocity(ref currentVelocity, deltaTime);

            //Debug.Log($"Motor velocity : {currentVelocity}");

            //Debug.DrawRay(Transform.position, currentVelocity.normalized, Color.yellow,5);

            
            //Parameters.SetVector3(Preset.ModuleVelocity, currentVelocity);


            //if (!EnableCollider)
            //    Debug.Log(currentVelocity);

            //Debug.Log("Motor current : "+currentVelocity);
            // Apply motor velocity to parameter
            //Parameters.SetVector3(Preset.MotorVelocity, currentVelocity);
            MotorVelocity = currentVelocity;

            return;

            #endregion

            var movementVel = Vector3.zero;

            if (!ApplyMotion || !ApplyMovement)
            {
                movementVel += currentVelocity;
                PushVelocity(movementVel);
                return;
            }

            Vector3 targetVelocity;
            //VelocityBlendRatio = Parameters.GetFloat(Preset.VelocityBlendRatio);
            var animVel = AnimationDeltaPosition / deltaTime;
            
            if (VelocityBlendRatio > 0)
            {
                if (VelocityBlendRatio == 1)
                    targetVelocity = animVel;
                else
                {
                    var moduleVelocity = currentVelocity;//MotionModule.ModuleVelocity;
                    targetVelocity = Vector3.Lerp(moduleVelocity, animVel, VelocityBlendRatio);
                }
            }
            else // full module vel
            {
                targetVelocity = currentVelocity;//MotionModule.ModuleVelocity;
                
                if (WrapInAnimSpeed)
                {
                    var speed = animVel.magnitude;

                    if (speed > 1)
                        targetVelocity=targetVelocity.normalized.Multiply(speed);
                }
  
                Debug.DrawRay(Transform.position, targetVelocity,Color.red);
                currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }

            currentVelocity = Motor.GetDirectionTangentToSurface(targetVelocity, Motor.GroundingStatus.GroundNormal) * targetVelocity.magnitude;

            Debug.DrawRay(Transform.position + new Vector3(0, 0.4f), currentVelocity, Color.blue);

            #region UsualPhysic

            //if (!Motor.GroundingStatus.IsStableOnGround)
            //    ApplyAirDrag(ref currentVelocity, deltaTime);

            ApplyInertiaGravity(ref _Acceleration, deltaTime);
            currentVelocity += Acceleration;

            movementVel += currentVelocity;


            PushVelocity(movementVel);

            #endregion

            return;

            var cameraPlanarDirection = StrafeDirection; //Parameters.GetVector3(Preset.ViewPortPlanarDirection);

            Vector3 targetMovementVelocity;

            //var animDeltaPosition = Parameters.GetVector3(Preset.AnimationDeltaPosition);



            //Debug.Log(velocityBlendRatio + " : " + targetMovementVelocity);
            //Debug.Log(
            //    string.Format("Grounded {0} {1}", Motor.GroundingStatus.IsStableOnGround, targetMovementVelocity)
            //    );
            if (Motor.GroundingStatus.IsStableOnGround)
            {
                // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

                // Smooth movement ModuleVelocity
                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
            }
            else //if (EnableGravity)
            {
                // Add move Input
                if (cameraPlanarDirection.sqrMagnitude > 0f)
                {
                    targetMovementVelocity = cameraPlanarDirection * BaseMaxAirMoveSpeed;

                    // Prevent climbing on un-stable slopes with air movement
                    if (Motor.GroundingStatus.FoundAnyGround)
                    {
                        var perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
                    }

                    var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, GravityForce);
                    currentVelocity += AirAccelerationSpeed * deltaTime * velocityDiff;
                }

                ApplyInertiaGravity(ref currentVelocity, deltaTime);
            }

            //if (MotionUpdate)
            //{
            //    Debug.Log(currentVelocity);
            //    MotionUpdate = false;
            //}
        }

        //public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        //{
        //    if (deltaTime < float.Epsilon)
        //    {
        //        currentVelocity = Vector3.zero;
        //        return;
        //    }

        //    if (!isMine)
        //    {
        //        var vel = Parameters.GetVector3(Preset.MotorVelocity);
        //        currentVelocity.LerpTo(vel, deltaTime * 20f);
        //        return;
        //    }

        //    var movementVel = Vector3.zero;

        //    if (!ApplyMotion || !ApplyMovement)
        //    {
        //        movementVel += currentVelocity;
        //        PushVelocity(movementVel);
        //        return;
        //    }

        //    #region Overwrite Controller => Wrapper

        //    if (null != Wrapper)
        //    {
        //        //Debug.Log(" grounded : "+Motor.GroundingStatus.FoundAnyGround + " vel : " + currentVelocity);

        //        Wrapper.Main(ref currentVelocity, deltaTime);

        //        if (Wrapper.ApplyGravity)
        //            ApplyInertiaGravity(ref currentVelocity, deltaTime);

        //        if (Wrapper.Finish(ref currentVelocity, deltaTime))
        //        {
        //            Wrapper = null;
        //            PushVelocity(currentVelocity);
        //            return;
        //        }
        //        else if (Wrapper.Override)
        //        {
        //            PushVelocity(currentVelocity);
        //            return;
        //        }
        //    }
        //    #endregion



        //    Vector3 targetVelocity;
        //    //VelocityBlendRatio = Parameters.GetFloat(Preset.VelocityBlendRatio);
        //    var animVel = Parameters.GetVector3(Preset.AnimationDeltaPosition) / deltaTime;

        //    if (VelocityBlendRatio > 0)
        //    {
        //        if (VelocityBlendRatio == 1)
        //            targetVelocity = animVel;
        //        else
        //        {
        //            var moduleVelocity = Parameters.GetVector3(Preset.ModuleVelocity);
        //            targetVelocity = Vector3.Lerp(moduleVelocity, animVel, VelocityBlendRatio);
        //        }
        //    }
        //    else // full module vel
        //    {
        //        targetVelocity = Parameters.GetVector3(Preset.ModuleVelocity);

        //        if (WrapInAnimSpeed)
        //        {
        //            var speed = animVel.magnitude;

        //            if (speed > 1)
        //                targetVelocity = targetVelocity.normalized.Multiply(speed);
        //        }

        //        Debug.DrawRay(ReadOnlyTransform.position, targetVelocity, Color.red);
        //        currentVelocity = Vector3.Lerp(currentVelocity, targetVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
        //    }

        //    currentVelocity = Motor.GetDirectionTangentToSurface(targetVelocity, Motor.GroundingStatus.GroundNormal) * targetVelocity.magnitude;

        //    Debug.DrawRay(ReadOnlyTransform.position + new Vector3(0, 0.4f), currentVelocity, Color.blue);

        //    #region UsualPhysic

        //    //if (!Motor.GroundingStatus.IsStableOnGround)
        //    //    ApplyAirDrag(ref currentVelocity, deltaTime);

        //    ApplyInertiaGravity(ref Acceleration, deltaTime);
        //    currentVelocity += Acceleration;

        //    movementVel += currentVelocity;
        //    PushVelocity(movementVel);

        //    #endregion

        //    return;

        //    var cameraPlanarDirection = Parameters.GetVector3(Preset.ViewPortPlanarDirection);

        //    Vector3 targetMovementVelocity;

        //    //var animDeltaPosition = Parameters.GetVector3(Preset.AnimationDeltaPosition);



        //    //Debug.Log(velocityBlendRatio + " : " + targetMovementVelocity);
        //    //Debug.Log(
        //    //    string.Format("Grounded {0} {1}", Motor.GroundingStatus.IsStableOnGround, targetMovementVelocity)
        //    //    );
        //    if (Motor.GroundingStatus.IsStableOnGround)
        //    {
        //        // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
        //        currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

        //        // Smooth movement ModuleVelocity
        //        currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
        //    }
        //    else //if (EnableGravity)
        //    {
        //        // Add move Input
        //        if (cameraPlanarDirection.sqrMagnitude > 0f)
        //        {
        //            targetMovementVelocity = cameraPlanarDirection * BaseMaxAirMoveSpeed;

        //            // Prevent climbing on un-stable slopes with air movement
        //            if (Motor.GroundingStatus.FoundAnyGround)
        //            {
        //                var perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
        //                targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
        //            }

        //            var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, GravityForce);
        //            currentVelocity += AirAccelerationSpeed * deltaTime * velocityDiff;
        //        }

        //        ApplyInertiaGravity(ref currentVelocity, deltaTime);
        //    }

        //    //if (MotionUpdate)
        //    //{
        //    //    Debug.Log(currentVelocity);
        //    //    MotionUpdate = false;
        //    //}

        //    if (!EnableCollider)
        //        Debug.Log(currentVelocity);
        //    //Debug.Log("Motor current : "+currentVelocity);
        //    // Apply motor velocity to parameter
        //    Parameters.SetVector3(Preset.MotorVelocity, currentVelocity);
        //}


        protected Vector3 LastPosition;

        //protected Vector3 MotorDeltaPosition;

        #region Velocity Inertia Func

        //protected virtual void ApplyAirDrag(ref Vector3 currentVelocity, float deltaTime)
        //{
        //    return;
        //    var cameraPlanarDirection = Parameters.GetVector3(Preset.ViewPortPlanarDirection);

        //    Vector3 targetMovementVelocity;

        //    if (cameraPlanarDirection.sqrMagnitude > 0f)
        //    {
        //        targetMovementVelocity = cameraPlanarDirection * BaseMaxAirMoveSpeed;

        //        // Prevent climbing on un-stable slopes with air movement
        //        if (Motor.GroundingStatus.FoundAnyGround)
        //        {
        //            var perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
        //            targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
        //        }

        //        var velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, GravityForce);
        //        currentVelocity += AirAccelerationSpeed * deltaTime * velocityDiff;
        //    }
        //}

        public void ApplyAirDragVelocity(ref Vector3 currentVelocity, float deltaTime = 0)
        {
            if (deltaTime == 0)
                deltaTime = ConstCache.deltaTime;

            currentVelocity *= (1f / (1f + (Drag * deltaTime)));
        }



        public Vector3 MotorVelocity { get; protected set; }

        /// <summary>
        /// Add inertial velocity.
        /// </summary>
        public void ConvertModuleVelocityToInertia(Vector3 vel = default)
        {
            Debug.Log("Convert inertia "+vel);

            Acceleration += vel;
        }

        protected virtual void ApplyInertiaGravity(ref Vector3 currentVelocity, float deltaTime)
        {

            if (/*Vector3.Dot(currentVelocity,GravityForce)>0&&*/GroundingStatus.IsStableOnGround)
            {
                Debug.DrawRay(Transform.position, currentVelocity.normalized, Color.yellow, 10);
                //Acceleration = default;
                return;
            }


            float sqrDistance;

            sqrDistance = Vector3.Distance(LastGroundPoint, Transform.position);

            if (GroundingStatus.FoundAnyGround)
            {
                Debug.DrawLine(LastGroundPoint, Transform.position, Color.white, 5);
                Debug.Log(sqrDistance);
            }


            //if (Motor.GroundingStatus.FoundAnyGround)
            //    sqrDistance = Vector3.Distance(Motor.GroundingStatus.GroundPoint,Transform.position);
            //else
            //    sqrDistance = 1;



            // do not apply extract gravity when close to ground prevent the velocity larger than upstair.
            //if (sqrDistance < 0.2f)
            //{
            //    if (Vector3.Dot(GravityAcceleration, GravityForce) < 1)
            //    {
            //        currentVelocity += gravity;
            //        Debug.Log($"Close grounding apply gravity {gravity} MotorVelocity {currentVelocity}");
            //    }
            //}
            //else
            {
                currentVelocity += GravityAcceleration;
            }

            //Debug.Log($"Apply gravity {gravity} MotorVelocity {currentVelocity} Distance {sqrDistance}");

            //Debug.LogError(currentVelocity);

            // Drag
            ApplyAirDragVelocity(ref currentVelocity, deltaTime);
            //currentVelocity *= (1f / (1f + (Drag * deltaTime)));
        }


        #endregion

        #region Apply Rotation Motion

        public float VelocityBlendRatio { get; set; }
        public float RotationBlendRatio { get; set; }
        public float PositionBias { get; set; }
        public float RotationBias { get; set; }

        protected Quaternion MotorDeltaRotation;

        [Range(0, 1)]
        [TabGroup(Controller)]
        public float AngularVelocityBlendRatio;


        public Quaternion MotorRotation;

        /// <summary>
        /// Blend module rotation and animation rotation 
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            //Debug.Log($"Upate rotation Frame {Time.frameCount} Time {deltaTime * 1000}");

            if (!ApplyMotion || !ApplyTurnround)
                return;

            //Parameters.GetQuaternion(Preset.MotorRotation);

            // sync from net ?
            if (!isMine)
            {
                var rot = MotorRotation;
                currentRotation = rot*Quaternion.Lerp(currentRotation, rot, deltaTime * 20f);
                return;
            }

            Quaternion targetRotation;
            Quaternion moduleRotation = Transform.rotation;

            foreach (var module in AngularVelocityModules)
                module.UpdateRotation(ref moduleRotation, deltaTime);

            AngularVelocityBlendRatio = VelocityBlendRatio;// Parameters.GetFloat(Preset.RotationBlendRatio);

            if (AngularVelocityBlendRatio > 0)
            {
                var animRotation= AnimationDeltaRotation * currentRotation;

                if (AlignViewMotion)// lock cam
                    targetRotation = moduleRotation;
                else if (AngularVelocityBlendRatio == 1)
                    targetRotation = animRotation;
                else
                {
                    targetRotation = Quaternion.Lerp(moduleRotation, animRotation, AngularVelocityBlendRatio);
                }
            }
            else
            {
                if (AlignViewMotion)// lock cam{
                {
                    targetRotation = Quaternion.LookRotation(moduleRotation * Vector3.forward, Motor.CharacterUp);
                }
                else
                {
                    var dir = MotorVelocity;
                        //Parameters.GetVector3(Preset.MotorVelocity);
                    if (dir == default)
                        dir = Transform.forward;
                    targetRotation = Quaternion.LookRotation(dir, Motor.CharacterUp);
                }
                targetRotation = Quaternion.Lerp(currentRotation, targetRotation, deltaTime * m_dirBias);
            }

            MotorDeltaRotation = targetRotation*Quaternion.Inverse(currentRotation);

            PushRotation(targetRotation);

            currentRotation = targetRotation;

            return;

            if (!Motor.GroundingStatus.FoundAnyGround)
                return;

            //if (MotionModule.ModuleVector.sqrMagnitude < 0.01f)
            //    return;

            var viewPlanarDirection = StrafeDirection; // Parameters.GetVector3(Preset.ViewPortPlanarDirection);

            //var rot=Quaternion.FromToRotation(Motor.CharacterForward, viewPlanarDirection);
            //currentRotation = Quaternion.LookRotation(Quaternion.Lerp(currentRotation, rot* currentRotation, deltaTime * 20)*Vector3.forward,Motor.CharacterUp);

            //return;


            var animDeltaRotation = AnimationDeltaRotation;
            var blendRatio = VelocityBlendRatio;// Parameters.GetFloat(Preset.RotationBlendRatio);
            //currentRotation = Parameters.GetQuaternion(Preset.PlanarViewRotation);


            //return;

            var targetDirectio = animDeltaRotation * Vector3.forward;// viewPlanarDirection;//(animDeltaRotation) * Motor.CharacterForward; //(animDeltaRotation) * Motor.CharacterForward;//Quaternion.Lerp(moduleRotation, animationRotation, 1);

            //currentRotation = Quaternion.LookRotation(( currentRotation* animDeltaRotation) *Vector3.forward, Motor.CharacterUp);

            // Debug.Log(targetDirectio);
            if (viewPlanarDirection != Vector3.zero && OrientationSharpness > 0f)
            {
                // Smoothly interpolate from current to target look direction
                var smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, targetDirectio, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

                // Set the current rotation (which will be used by the KinematicCharacterMotor)
                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
            }
        }


        #endregion


        #endregion

        // ?? cache by motor?
        public CharacterGroundingReport GroundingStatus { get; protected set; }

        [SerializeField]
        float m_IgnoreMovementHitHeight = 0.2f;

        #region ICharacterController

        public void BeforeCharacterUpdate(float deltaTime)
        {
            LastPosition = Transform.position;
        }

        // order 2 ground post
        public void PostGroundingUpdate(float deltaTime)
        {
            GroundingStatus = Motor.GroundingStatus;

            Debug.Log($"{Time.frameCount} : {nameof(PostGroundingUpdate)}  Stable {GroundingStatus.IsStableOnGround}.");
            //Parameters.SetValue(Preset.GroundingStatus, Motor.GroundingStatus);

            if (GroundingStatus.IsStableOnGround && !Motor.LastGroundingStatus.IsStableOnGround)
            {
                //Parameters.SetTrigger(Preset.GroundedPost, true);
                Acceleration = default;
                GravityAcceleration = default;
            }
            else if (!GroundingStatus.IsStableOnGround && Motor.LastGroundingStatus.IsStableOnGround)
            {
                //Parameters.SetTrigger(Preset.GroundedPost, false);
            }
        }

        public void AfterCharacterUpdate(float deltaTime)
        {
            var pos = Transform.position;

            OnAfterMotion?.Invoke(new (pos-LastPosition,MotorDeltaRotation));

            LastPosition = pos;


            // Reset root motion deltas
            AnimationDeltaPosition = Vector3.zero;
            AnimationDeltaRotation = Quaternion.identity;


            //StepIK.OnIK(Motor.GroundingStatus, Motor.ModuleVelocity);

        }

        public bool IsColliderValidForCollisions(Collider col)
        {
            if (IgnoredColliders.Contains(col))
                return false;

            return true;
        }

        public Vector3 GroundedNormal { get; protected set; }

        public Vector3 LastGroundPoint;

        // order 1 valid ground post
        // Valid step smooth
        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            //Debug.LogError($"{Time.frameCount} {nameof(OnGroundHit)} {Vector3.Distance(hitPoint, Transform.position)} Stable {Motor.GroundingStatus.IsStableOnGround}.");


            if (!Motor.GroundingStatus.FoundAnyGround)
            {
                Debug.LogError($"{nameof(OnGroundHit)} {hitCollider.name}.");
                //Debug.DrawRay(hitPoint, hitNormal, Color.cyan, 10);
            }
                

            GM_GroundedHit = new Ray(hitPoint, hitNormal);

            Debug.DrawRay(hitPoint, hitNormal, Color.white, 5f);

            LastGroundPoint = hitPoint;

            //if (hitStabilityReport.FoundOuterNormal)
            //    Debug.DrawRay(Motor.Transform.position, hitStabilityReport.OuterNormal, Color.yellow, 5);

            //if (hitStabilityReport.FoundInnerNormal)
            //    Debug.DrawRay(Motor.Transform.position, hitStabilityReport.InnerNormal, Color.blue, 5);

            //Debug.DrawRay(Motor.Transform.position, Motor.BaseVelocity.normalized, Color.green, 5);

            GroundedNormal = hitNormal;

            return;

            hitStabilityReport.ValidStepDetected = false;

            if (!hitStabilityReport.ValidStepDetected)
                return;


            //cast vel ground rows 
            Debug.Log($"OnGroundHit \nIsStable : {hitStabilityReport.IsStable}\nHitCollider : {hitCollider}\nHitSteppedCollider : {hitStabilityReport.SteppedCollider.name}.");

 

            return;

            hitStabilityReport.IsStable = true;
        }


        // order 4 valid ground post
        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            //Debug.Log($"{Time.frameCount} : {nameof(OnMovementHit)}.");
            //Debug.LogError($"{Time.frameCount} {nameof(OnMovementHit)} {Vector3.Distance(hitPoint, Transform.position)} Stable {Motor.GroundingStatus.IsStableOnGround}.");


            if (!Motor.GroundingStatus.FoundAnyGround)
            {
                Debug.DrawRay(hitPoint, hitNormal, Color.red, 10);
            }


            //Debug.Log(hitCollider);
            //hitStabilityReport.IsStable = true;
            GM_MovementHit = new Ray(hitPoint, hitNormal);

            var height = Vector3.Dot(Transform.InverseTransformPoint(hitPoint), Vector3.up);

            if(Vector3.Angle(Transform.up, hitNormal)>7f)
                if(height>0.05f)
                {
                    hitStabilityReport.ValidStepDetected = false;
                }

            return;


            Debug.Log($"OnMovementHit \nHitHeight : {height}\nValidStepDetected : {hitStabilityReport.ValidStepDetected}\nHitSteppedCollider : {hitStabilityReport.SteppedCollider}.");

            //hitStabilityReport.ValidStepDetected = false;


            //var dir = Vector3.Dot(hitPoint - Transform.position,Transform.up);

            //if (dir< m_IgnoreMovementHitHeight)
            //    hitStabilityReport.IsStable = false;


            // **do HitStabilityReport on blackboard => next Version independent investigation system

            // We can wall jump only if we are not stable on ground and are moving against an obstruction
            //if (AllowWallJump && !Motor.GroundingStatus.IsStableOnGround && !hitStabilityReport.IsStable)
            //{
            //    _canWallJump = true;
            //    _wallJumpNormal = hitNormal;
            //}
        }

        // order 0-3 invalid ground post
        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
            //Debug.LogError($"{Time.frameCount} {nameof(ProcessHitStabilityReport)} {Vector3.Distance(hitPoint, atCharacterPosition)} Stable {Motor.GroundingStatus.IsStableOnGround}.");


            if (!Motor.GroundingStatus.FoundAnyGround)
            {
                Debug.DrawRay(hitPoint, hitNormal, Color.black, 10);
            }



            return;

            Debug.Log($"ProcessHitStabilityReport " +
                $"\nIsStable : {hitStabilityReport.IsStable}" +
                $"\nValidStepDetected : {hitStabilityReport.ValidStepDetected}" +
                $"\nHitSteppedCollider : {hitStabilityReport.SteppedCollider}."
                );
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {

        }

        #endregion

        #endregion


        #region Debug


        Ray GM_GroundedHit;

        Ray GM_MovementHit;

        void GizmosMovementHit()
        {
            //Gizmos.color = Color.blue;

            //Gizmos.DrawRay(GM_GroundedHit);

            //Gizmos.color = Color.yellow;

            //if (GM_GroundedHit.Equals(GM_MovementHit))
            //    GM_MovementHit.origin = GM_MovementHit.origin + transform.right.Multiply(0.1f);

            //Gizmos.DrawRay(GM_MovementHit);
        }

        #endregion

    }
}
