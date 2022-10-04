//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using MxMGameplay;
//using UnityEngine.Assertions;
//using KinematicCharacterController;
//using MxM;

namespace Return.Humanoid.Controller
{

}
//{
//    /// <summary>
//    /// Custom MxM controller wrapper
//    /// </summary>
//    [RequireComponent(typeof(KinematicCharacterMotor))]
//    public class m_ControllerWrapper : GenericControllerWrapper, ICharacterController, IMxMRootMotion
//    {
//        public float _Height = 1.7f;
//        public Vector3 _Center;
//        public float _Radius = 0.3f;

//        public Rigidbody RB;

//        [SerializeField]
//        private bool m_applyGravityWhenGrounded = false;

//        private KinematicCharacterMotor Motor;

//        private bool m_enableCollision = true;

//        public override bool IsGrounded { get { return Motor.GroundingStatus.FoundAnyGround; } }
//        public override bool ApplyGravityWhenGrounded { get { return m_applyGravityWhenGrounded; } }
//        public override Vector3 ModuleVelocity { get { return Motor.ModuleVelocity; } }
//        public override float MaxStepHeight
//        {
//            get { return Motor.MaxStepHeight; }
//            set { Motor.MaxStepHeight = value; }
//        }
//        public override float Height
//        {
//            get { return Motor.Capsule.rows; }
//            set
//            {
//                Motor.Capsule.rows = value;
//                //m_characterController.SetCapsuleDimensions(_Radius,value,_Center.height) ;
//            }
//        }

//        public override Vector3 Center
//        {
//            get { return Motor.Capsule.center; }
//            set { Motor.Capsule.center = value; }
//        }
//        public override float Radius
//        {
//            get { return Motor.Capsule.radius; }
//            set { Motor.Capsule.radius = value; }
//        }

//        public override void Initialize()
//        {
//            //m_characterController.enableOverlapRecovery = false;
//        }

//        public Vector3 LastPos;
//        public Quaternion LastQua;

//        public Vector3 LastMov;
//        public Quaternion LastRot;
//        protected ReadOnlyTransform ReadOnlyTransform;


//        //private void Update()
//        //{
//        //    if (ReadOnlyTransform.hasChanged)
//        //    {
//        //        var pos = ReadOnlyTransform.position;
//        //        var rot = ReadOnlyTransform.rotation;

//        //        LastMov =  pos- LastPos;
//        //        LastRot = rot*Quaternion.Inverse(LastRot);

//        //        LastPos = pos;
//        //        LastRot = rot;

//        //        ReadOnlyTransform.hasChanged = false;
//        //    }
//        //}

//        public override Vector3 GetCachedMoveDelta() { return LastMov; }
//        public override Quaternion GetCachedRotDelta() { return LastRot; }

//        //Gets or sets whether collision is enabled (GenericControllerWrapper override)
//        public override bool CollisionEnabled
//        {
//            get { return m_enableCollision; }
//            set
//            {
//                if (m_enableCollision != value)
//                {
//                    if (m_enableCollision)
//                        Motor.enabled = false;
//                    else
//                        Motor.enabled = true;
//                }

//                m_enableCollision = value;
//            }
//        }

//        //============================================================================================
//        /**
//        *  @brief Sets up the wrapper, getting a reference to an attached character controller
//        *         
//        *********************************************************************************************/
//        private void Awake()
//        {
//            ReadOnlyTransform = transform;
//            LastPos = ReadOnlyTransform.position;
//            LastQua = ReadOnlyTransform.rotation;
//            ReadOnlyTransform.hasChanged = false;
//            CharacterRoot.InstanceIfNull(ref RB);
//            CharacterRoot.InstanceIfNull(ref Motor);
//            Motor.CharacterController = this;

//            Assert.IsNotNull(Motor, "Error (UnityControllerWrapper): Could not find" +
//                "CharacterController component");

//            //m_characterController.enableOverlapRecovery = true;
//        }

//        //============================================================================================
//        /**
//        *  @brief Moves the controller by an absolute Vector3 tranlation. Delta time must be applied
//        *  before passing a_move to this function.
//        *  
//        *  @param [Vector3] a_move - the movement vector
//        *         
//        *********************************************************************************************/
//        public override void Move(Vector3 a_move)
//        {
//            //LastMov = a_move;
//            Debug.Log(a_move);
//            if (m_enableCollision)
//            {
//                Motor.MoveCharacter(a_move);
//            }
//            else
//            {
//                Motor.transform.Translate(a_move, Space.World);
//            }
//        }

//        //============================================================================================
//        /**
//        *  @brief Moves the controller by an absolute Vector3 tranlation and sets the new rotation
//        *  of the controller. Delta time must be applied before passing a_move to this function.
//        *  
//        *  @param [Vector3] a_move - the movement vector
//        *  @param [Quaternion] a_newRotation - the new rotation for the character
//        *         
//        *********************************************************************************************/
//        public override void MoveAndRotate(Vector3 a_move, Quaternion a_rotDelta)
//        {
//            Debug.Log(a_move);
//            //LastMov = a_move;
//            //LastRot = a_rotDelta;
//            //m_characterController.transform.rotation *= a_rotDelta;
//            Motor.RotateCharacter(a_rotDelta);
//            if (m_enableCollision)
//            {
//                Motor.MoveCharacter(a_move);
//            }
//            else
//            {
//                Motor.transform.Translate(a_move, Space.World);
//            }
//        }

//        //============================================================================================
//        /**
//        *  @brief Sets the rotation of the character contorller with interpolation if supported
//        *  
//        *  @param [Quaternion] a_newRotation - the new rotation for the character
//        *         
//        *********************************************************************************************/
//        public override void Rotate(Quaternion a_rotDelta)
//        {
//            //LastRot = a_rotDelta;
//            Motor.transform.rotation *= a_rotDelta;
//        }

//        //============================================================================================
//        /**
//        *  @brief Monobehaviour OnEnable function which simply passes the enabling status onto the
//        *  controller compopnnet
//        *         
//        *********************************************************************************************/
//        private void OnEnable()
//        {
//            Motor.enabled = true;
//        }

//        //============================================================================================
//        /**
//        *  @brief Monobehaviour OnDisable function which simply passes the disabling status onto the
//        *  controller compopnnet
//        *         
//        *********************************************************************************************/
//        private void OnDisable()
//        {
//            Motor.enabled = false;
//        }

//        //============================================================================================
//        /**
//        *  @brief Sets the position of the character controller (teleport)
//        *  
//        *  @param [Vector3] a_position - the new position
//        *         
//        *********************************************************************************************/
//        public override void SetPosition(Vector3 a_position)
//        {
//            Motor.SetPosition(a_position);
//            //transform.position = a_position;
//        }

//        //============================================================================================
//        /**
//        *  @brief Sets the rotation of the character controller (teleport)
//        *         
//        *  @param [Quaternion] a_rotation - the new rotation        
//        *         
//        *********************************************************************************************/
//        public override void SetRotation(Quaternion a_rotation)
//        {
//            Motor.SetRotation(a_rotation);
//            //transform.rotation = a_rotation;
//        }

//        //============================================================================================
//        /**
//        *  @brief Sets the position and rotation of the character controller (teleport)
//        *  
//        *  @param [Vector3] a_position - the new position
//        *  @param [Quaternion] a_rotation - the new rotation    
//        *         
//        *********************************************************************************************/
//        public override void SetPositionAndRotation(Vector3 a_position, Quaternion a_rotation)
//        {
//            //m_characterController.SetPositionAndRotation(a_position, a_rotation);
//            transform.SetPositionAndRotation(a_position, a_rotation);
//        }

//        [Header("Stable Movement")]
//        public float MaxStableMoveSpeed = 10f;
//        public float StableMovementSharpness = 15;
//        public float OrientationSharpness = 10;

//        [Header("Air Movement")]
//        public float MaxAirMoveSpeed = 10f;
//        public float AirAccelerationSpeed = 5f;
//        public float Drag = 0.1f;

//        [Header("Misc")]
//        public bool RotationObstruction;
//        public Vector3 GravityForce = new Vector3(0, -30f, 0);
//        public ReadOnlyTransform MeshRoot;

//        private Vector3 _moveInputVector;
//        private Vector3 _lookInputVector;


//        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
//        {
//            if (_lookInputVector != Vector3.zero && OrientationSharpness > 0f)
//            {
//                // Smoothly interpolate from current to target look direction
//                Vector3 smoothedLookInputDirection = Vector3.Slerp(Motor.CharacterForward, ReadOnlyTransform.rotation*LastRot*ReadOnlyTransform.forward/*_lookInputVector*/, 1 - Mathf.Exp(-OrientationSharpness * deltaTime)).normalized;

//                // Set the current rotation (which will be used by the KinematicCharacterMotor)
//                currentRotation = Quaternion.LookRotation(smoothedLookInputDirection, Motor.CharacterUp);
//            }
//        }

//        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
//        {
//            Vector3 targetMovementVelocity = Vector3.zero;
//            if (Motor.GroundingStatus.IsStableOnGround)
//            {
//                // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
//                currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, Motor.GroundingStatus.GroundNormal) * currentVelocity.magnitude;

//                // Calculate target velocity
//                Vector3 inputRight = Vector3.Cross(_moveInputVector, Motor.CharacterUp);
//                Vector3 reorientedInput = Vector3.Cross(Motor.GroundingStatus.GroundNormal, inputRight).normalized * _moveInputVector.magnitude;
//                targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
//                targetMovementVelocity = LastMov;
//                // Smooth movement ModuleVelocity
//                currentVelocity = Vector3.Lerp(currentVelocity, targetMovementVelocity, 1 - Mathf.Exp(-StableMovementSharpness * deltaTime));
//            }
//            else
//            {
//                // Add move Input
//                if (_moveInputVector.sqrMagnitude > 0f)
//                {
//                    targetMovementVelocity = _moveInputVector * MaxAirMoveSpeed;

//                    // Prevent climbing on un-stable slopes with air movement
//                    if (Motor.GroundingStatus.FoundAnyGround)
//                    {
//                        Vector3 perpenticularObstructionNormal = Vector3.Cross(Vector3.Cross(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal), Motor.CharacterUp).normalized;
//                        targetMovementVelocity = Vector3.ProjectOnPlane(targetMovementVelocity, perpenticularObstructionNormal);
//                    }

//                    Vector3 velocityDiff = Vector3.ProjectOnPlane(targetMovementVelocity - currentVelocity, GravityForce);
//                    currentVelocity += velocityDiff * AirAccelerationSpeed * deltaTime;
//                }

//                // GravityForce
//                currentVelocity += GravityForce * deltaTime;

//                // Drag
//                currentVelocity *= (1f / (1f + (Drag * deltaTime)));
//            }
//        }

//        public void BeforeCharacterUpdate(float deltaTime)
//        {
            
//        }

//        public void PostGroundingUpdate(float deltaTime)
//        {
            
//        }

//        public void AfterCharacterUpdate(float deltaTime)
//        {
            
//        }

//        public bool IsColliderValidForCollisions(ColliderBounds coll)
//        {
//            return true;
//        }

//        public void OnGroundHit(ColliderBounds hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
//        {
            
//        }

//        public void OnMovementHit(ColliderBounds hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
//        {
            
//        }

//        public void ProcessHitStabilityReport(ColliderBounds hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
//        {
           
//        }

//        public void OnDiscreteCollisionDetected(ColliderBounds hitCollider)
//        {
            
//        }

//        public void HandleRootMotion(Vector3 a_rootPosition, Quaternion a_rootRotation, Vector3 a_warp, Quaternion a_warpRot, float a_deltaTime)
//        {
//            LastMov = a_rootPosition;
//            LastRot = a_rootRotation;
//            Motor.MoveCharacter(transform.position+a_rootPosition);
//            Motor.RotateCharacter(transform.rotation*a_rootRotation);
//        }

//        public void HandleAngularErrorWarping(Quaternion a_warpRot)
//        {
            
//        }
//    }
//}