using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using System;
using Return.Modular;

namespace Return.Framework.PhysicController
{


    public abstract class AbstractPhysicControllerMotor : MonoModule, IControllerMotor, IResolveHandle
    {
        #region Const

        // Warning: Don't touch these constants unless you know exactly what you're doing!
        public const int MaxHitsBudget = 16;
        public const int MaxCollisionBudget = 16;
        public const int MaxGroundingSweepIterations = 2;
        public const int MaxSteppingSweepIterations = 3;
        public const int MaxRigidbodyOverlapsCount = 16;
        public const float CollisionOffset = 0.01f;
        public const float GroundProbeReboundDistance = 0.02f;
        public const float MinimumGroundProbingDistance = 0.005f;
        public const float GroundProbingBackstepDistance = 0.1f;
        public const float SweepProbingBackstepDistance = 0.002f;
        public const float SecondaryProbesVertical = 0.02f;
        public const float SecondaryProbesHorizontal = 0.001f;
        public const float MinVelocityMagnitude = 0.01f;
        public const float SteppingForwardDistance = 0.03f;
        public const float MinDistanceForLedge = 0.05f;
        public const float CorrelationForVerticalObstruction = 0.01f;
        public const float ExtraSteppingForwardDistance = 0.01f;
        public const float ExtraStepHeightPadding = 0.01f;

        #endregion

        #region IResolver

        public virtual void InstallResolver(IModularResolver resolver)
        {
            resolver.RegisterModule<IControllerMotor>(this);
        }




        #endregion


        #region IControllerMotor

        Vector3 IControllerMotor.BaseVelocity { get => BaseVelocity; set => BaseVelocity=value; }


        #endregion

        #region Cache

        Transform _transform;
        /// <summary>
        /// The ReadOnlyTransform of the character motor
        /// </summary>
        public Transform Transform { get => _transform; protected set => _transform = value; }

        /// <summary>
        /// The motor's assigned controller
        /// </summary>
        public ICharacterController CharacterController { get; set; }

        #endregion


        //#pragma warning restore 0414
        #region Routine


        protected virtual void Awake()
        {
            Transform = this.transform;

            TransientPosition = _transform.position;
            TransientRotation = _transform.rotation;

            // Build CollidableLayers mask
            CollidableLayers = 0;
            for (int i = 0; i < 32; i++)
            {
                if (!Physics.GetIgnoreLayerCollision(this.gameObject.layer, i))
                {
                    CollidableLayers |= (1 << i);
                }
            }
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            KinematicCharacterSystem.EnsureCreation();
            KinematicCharacterSystem.RegisterCharacterMotor(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            KinematicCharacterSystem.UnregisterCharacterMotor(this);
        }

        /// <summary>
        /// Update phase 1 is meant to be called after physics movers have calculated their velocities, but
        /// before they have simulated their goal positions/rotations. It is responsible for:
        /// - Initializing all values for update
        /// - Handling MovePosition calls
        /// - Solving initial collision overlaps
        /// - Ground probing
        /// - Handle detecting potential interactable rigidbodies
        /// </summary>
        public abstract void UpdatePhase1(float deltaTime);


        /// <summary>
        /// Update phase 2 is meant to be called after physics movers have simulated their goal positions/rotations. 
        /// At the end of this, the TransientPosition/Rotation values will be up-to-date with where the motor should be at the end of its move. 
        /// It is responsible for:
        /// - Solving Rotation
        /// - Handle MoveRotation calls
        /// - Solving potential attached rigidbody overlaps
        /// - Solving ModuleVelocity
        /// - Applying planar constraint
        /// </summary>
        public abstract void UpdatePhase2(float deltaTime);



        #endregion




        #region Grounding

        [Header("Grounding settings")]
        /// <summary>
        /// Increases the range of ground detection, to allow snapping to ground at very high speeds
        /// </summary>    
        [Tooltip("Increases the range of ground detection, to allow snapping to ground at very high speeds")]
        public float GroundDetectionExtraDistance = 0f;
        /// <summary>
        /// Maximum slope angle on which the character can be stable
        /// </summary>    
        [Range(0f, 89f)]
        [Tooltip("Maximum slope angle on which the character can be stable")]
        public float MaxStableSlopeAngle = 60f;
        /// <summary>
        /// Which layers can the character be considered stable on
        /// </summary>    
        [Tooltip("Which layers can the character be considered stable on")]
        public LayerMask StableGroundLayers = -1;

     
        /// <summary>
        /// Notifies the Character Controller when discrete collisions are detected
        /// </summary>    
        [Tooltip("Notifies the Character Controller when discrete collisions are detected")]
        public bool DiscreteCollisionEvents = false;


        /// <summary>
        /// Contains the current grounding information
        /// </summary>
        [NonSerialized]
        public CharacterGroundingReport GroundingStatus = new ();
        /// <summary>
        /// Contains the previous grounding information
        /// </summary>
        [NonSerialized]
        public CharacterTransientGroundingReport LastGroundingStatus = new ();

        /// <summary>
        /// Did the motor's last swept collision detection find a ground?
        /// </summary>
        [NonSerialized]
        public bool LastMovementIterationFoundAnyGround;

        #endregion

        #region Step

        [Header("Step settings")]
        /// <summary>
        /// UnityHandles properly detecting grounding status on steps, but has a performance cost.
        /// </summary>
        [Tooltip("UnityHandles properly detecting grounding status on steps, but has a performance cost.")]
        public StepHandlingMethod StepHandling = StepHandlingMethod.Standard;
        /// <summary>
        /// Maximum rows of a step which the character can climb
        /// </summary>    
        [Tooltip("Maximum rows of a step which the character can climb")]
        public float MaxStepHeight = 0.4f;
        /// <summary>
        /// Can the character step up obstacles even if it is not currently stable?
        /// </summary>    
        [Tooltip("Can the character step up obstacles even if it is not currently stable?")]
        public bool AllowSteppingWithoutStableGrounding = false;
        /// <summary>
        /// Minimum length of a step that the character can step on (used in Extra stepping method. Use this to let the character step on steps that are smaller that its radius
        /// </summary>    
        [Tooltip("Minimum length of a step that the character can step on (used in Extra stepping method). Use this to let the character step on steps that are smaller that its radius")]
        public float MinRequiredStepDepth = 0.1f;

        #endregion

        #region Ledge

        [Header("Ledge settings")]
        /// <summary>
        /// UnityHandles properly detecting ledge information and grounding status, but has a performance cost.
        /// </summary>
        [Tooltip("UnityHandles properly detecting ledge information and grounding status, but has a performance cost.")]
        public bool LedgeAndDenivelationHandling = true;
        /// <summary>
        /// The distance from the capsule central axis at which the character can stand on a ledge and still be stable
        /// </summary>    
        [Tooltip("The distance from the capsule central axis at which the character can stand on a ledge and still be stable")]
        public float MaxStableDistanceFromLedge = 0.5f;
        /// <summary>
        /// Prevents snapping to ground on ledges beyond a certain velocity
        /// </summary>    
        [Tooltip("Prevents snapping to ground on ledges beyond a certain velocity")]
        public float MaxVelocityForLedgeSnap = 0f;
        /// <summary>
        /// The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground
        /// </summary>    
        [Tooltip("The maximun downward slope angle change that the character can be subjected to and still be snapping to the ground")]
        [Range(1f, 180f)]
        public float MaxStableDenivelationAngle = 180f;

        #endregion

        #region Rigidbody

        [Header("Rigidbody interaction settings")]
        /// <summary>
        /// UnityHandles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies
        /// </summary>
        [Tooltip("UnityHandles properly being pushed by and standing on PhysicsMovers or dynamic rigidbodies. Also handles pushing dynamic rigidbodies")]
        public bool InteractiveRigidbodyHandling = true;
        /// <summary>
        /// How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.
        /// </summary>
        [Tooltip("How the character interacts with non-kinematic rigidbodies. \"Kinematic\" mode means the character pushes the rigidbodies with infinite force (as a kinematic body would). \"SimulatedDynamic\" pushes the rigidbodies with a simulated mass value.")]
        public RigidbodyInteractionType RigidbodyInteractionType;
        [Tooltip("Mass used for pushing bodies")]
        public float SimulatedCharacterMass = 1f;
        /// <summary>
        /// Determines if the character preserves moving platform velocities when de-grounding from them
        /// </summary>
        [Tooltip("Determines if the character preserves moving platform velocities when de-grounding from them")]
        public bool PreserveAttachedRigidbodyMomentum = true;

        #endregion

        #region Constraint

        [Header("Constraints settings")]
        /// <summary>
        /// Determines if the character's movement uses the planar constraint
        /// </summary>
        [Tooltip("Determines if the character's movement uses the planar constraint")]
        public bool HasPlanarConstraint = false;
        /// <summary>
        /// Defines the plane that the character's movement is constrained on, if HasMovementConstraintPlane is active
        /// </summary>
        [Tooltip("Defines the plane that the character's movement is constrained on, if HasMovementConstraintPlane is active")]
        public Vector3 PlanarConstraintAxis = Vector3.forward;

        #endregion

        #region Others

        [Header("Other settings")]
        /// <summary>
        /// How many times can we sweep for movement per update
        /// </summary>
        [Tooltip("How many times can we sweep for movement per update")]
        public int MaxMovementIterations = 5;
        /// <summary>
        /// How many times can we check for decollision per update
        /// </summary>
        [Tooltip("How many times can we check for decollision per update")]
        public int MaxDecollisionIterations = 1;
        /// <summary>
        /// Checks for overlaps before casting movement, making sure all collisions are detected even when already intersecting geometry (has a performance cost, but provides safety against tunneling through colliders)
        /// </summary>
        [Tooltip("Checks for overlaps before casting movement, making sure all collisions are detected even when already intersecting geometry (has a performance cost, but provides safety against tunneling through colliders)")]
        public bool CheckMovementInitialOverlaps = true;
        /// <summary>
        /// Sets the velocity to zero if exceed max movement iterations
        /// </summary>
        [Tooltip("Sets the velocity to zero if exceed max movement iterations")]
        public bool KillVelocityWhenExceedMaxMovementIterations = true;
        /// <summary>
        /// Sets the remaining movement to zero if exceed max movement iterations
        /// </summary>
        [Tooltip("Sets the remaining movement to zero if exceed max movement iterations")]
        public bool KillRemainingMovementWhenExceedMaxMovementIterations = true;

        #endregion


        #region Motor State

        #region Field
        /// <summary>
        /// Whether or not the motor will solve collisions when moving. **Climbing **Up
        /// </summary>
        protected bool _solveMovementCollisions = true;
        protected bool _solveGrounding = true;
        protected bool _movePositionDirty = false;
        protected Vector3 _movePositionTarget = Vector3.zero;
        protected bool _moveRotationDirty = false;
        protected Quaternion _moveRotationTarget = Quaternion.identity;
        protected bool _lastSolvedOverlapNormalDirty = false;
        protected Vector3 _lastSolvedOverlapNormal = Vector3.forward;
        protected int _rigidbodyProjectionHitCount = 0;
        protected bool _isMovingFromAttachedRigidbody = false;
        protected bool _mustUnground = false;
        protected float _mustUngroundTimeCounter = 0f;
        protected Vector3 _cachedWorldUp = Vector3.up;
        protected Vector3 _cachedWorldForward = Vector3.forward;
        protected Vector3 _cachedWorldRight = Vector3.right;
        protected Vector3 _cachedZeroVector = Vector3.zero;

        // ref
        protected Vector3 _transientPosition;
        protected Quaternion _transientRotation;


        protected Vector3 _initialSimulationPosition;
        protected Quaternion _initialSimulationRotation;

        #region Coordinate
        protected Vector3 _characterUp;
        protected Vector3 _characterForward;
        protected Vector3 _characterRight;



        protected Vector3 _characterTransformToCapsuleCenter;
        protected Vector3 _characterTransformToCapsuleBottom;
        protected Vector3 _characterTransformToCapsuleBottomHemi;
        protected Vector3 _characterTransformToCapsuleTop;
        protected Vector3 _characterTransformToCapsuleTopHemi;

        #endregion

        #endregion

        #region Property

        /// <summary>
        /// The character's goal rotation in its movement calculations (always up-to-date during the character update phase)
        /// </summary>
        public Quaternion TransientRotation
        {
            get
            {
                return _transientRotation;
            }
            protected set
            {
                _transientRotation = value;
                _characterUp = _transientRotation * _cachedWorldUp;
                _characterForward = _transientRotation * _cachedWorldForward;
                _characterRight = _transientRotation * _cachedWorldRight;
            }
        }

        /// <summary>
        /// The character's total velocity, including velocity from standing on rigidbodies or PhysicsMover
        /// </summary>
        public Vector3 Velocity
        {
            get=> BaseVelocity + _attachedRigidbodyVelocity;
        }

        /// <summary>
        /// The character's goal position in its movement calculations (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 TransientPosition { get => _transientPosition; protected set => _transientPosition = value; }
        /// <summary>
        /// The character's up direction (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 CharacterUp { get { return _characterUp; } }
        /// <summary>
        /// The character's forward direction (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 CharacterForward { get { return _characterForward; } }
        /// <summary>
        /// The character's right direction (always up-to-date during the character update phase)
        /// </summary>
        public Vector3 CharacterRight { get { return _characterRight; } }
        /// <summary>
        /// The character's position before the movement calculations began
        /// </summary>
        public Vector3 InitialSimulationPosition { get { return _initialSimulationPosition; } }
        /// <summary>
        /// The character's rotation before the movement calculations began
        /// </summary>
        public Quaternion InitialSimulationRotation { get { return _initialSimulationRotation; } }

        /// <summary>
        /// Vector3 from the character transform position to the capsule center
        /// </summary>
        public Vector3 CharacterTransformToCapsuleCenter { get { return _characterTransformToCapsuleCenter; } }
        /// <summary>
        /// Vector3 from the character transform position to the capsule bottom
        /// </summary>
        public Vector3 CharacterTransformToCapsuleBottom { get { return _characterTransformToCapsuleBottom; } }
        /// <summary>
        /// Vector3 from the character transform position to the capsule top
        /// </summary>
        public Vector3 CharacterTransformToCapsuleTop { get { return _characterTransformToCapsuleTop; } }
        /// <summary>
        /// Vector3 from the character transform position to the capsule bottom hemi center
        /// </summary>
        public Vector3 CharacterTransformToCapsuleBottomHemi { get { return _characterTransformToCapsuleBottomHemi; } }
        /// <summary>
        /// Vector3 from the character transform position to the capsule top hemi center
        /// </summary>
        public Vector3 CharacterTransformToCapsuleTopHemi { get { return _characterTransformToCapsuleTopHemi; } }



        /// <summary>
        /// Index of this motor in KinematicCharacterSystem arrays
        /// </summary>
        [NonSerialized]
        public int IndexInCharacterSystem;
        /// <summary>
        /// Remembers initial position before all simulation are done
        /// </summary>
        public Vector3 InitialTickPosition { get; set; }
        /// <summary>
        /// Remembers initial rotation before all simulation are done
        /// </summary>
        public Quaternion InitialTickRotation { get; set; }

        /// <summary>
        /// The character's velocity resulting from direct movement
        /// </summary>

        [NonSerialized]
        public Vector3 BaseVelocity;

        #endregion

        #endregion




        #region Physics

        #region Collision

        /// <summary>
        /// Specifies the LayerMask that the character's movement algorithm can detect collisions with. By default, this uses the rigidbody's layer's collision matrix
        /// </summary>
        public LayerMask CollidableLayers { get; set; } = -1;

        /// <summary>
        /// Cache raycast hit for steps valid.
        /// </summary>
        protected RaycastHit[] internalCharacterHits = new RaycastHit[MaxHitsBudget];
        protected Collider[] _internalProbedColliders = new Collider[MaxCollisionBudget];
        protected List<Rigidbody> _rigidbodiesPushedThisMove = new(16);
        protected RigidbodyProjectionHit[] _internalRigidbodyProjectionHits = new RigidbodyProjectionHit[MaxRigidbodyOverlapsCount];

        /// <summary>
        /// The number of overlaps detected so far during character update (is reset at the beginning of the update)
        /// </summary>
        public int OverlapsCount { get { return _overlapsCount; } }
        protected int _overlapsCount;
        /// <summary>
        /// The overlaps detected so far during character update
        /// </summary>
        public OverlapResult[] Overlaps { get { return _overlaps; } }
        protected OverlapResult[] _overlaps = new OverlapResult[MaxRigidbodyOverlapsCount];


        public void SetMovementCollisionsSolvingActivation(bool movementCollisionsSolvingActive)
        {
            _solveMovementCollisions = movementCollisionsSolvingActive;
        }

        #endregion


        #region Attach Rigidbody

        protected Rigidbody _attachedRigidbody;
        /// <summary>
        /// Represents the Rigidbody to stay attached to
        /// </summary>
        public Rigidbody AttachedRigidbody { get { return _attachedRigidbody; } }
 

        protected Rigidbody _lastAttachedRigidbody;
        /// <summary>
        /// Specifies a Rigidbody to stay attached to
        /// </summary>
        [NonSerialized]
        public Rigidbody AttachedRigidbodyOverride;

        /// <summary>
        /// The character's velocity resulting from standing on rigidbodies or PhysicsMover
        /// </summary>
        public Vector3 AttachedRigidbodyVelocity { get { return _attachedRigidbodyVelocity; } }


        protected Vector3 _attachedRigidbodyVelocity;

        #endregion


        #endregion

        #region Motor

        /// <summary>
        /// Sets the character's position directly
        /// </summary>
        public void SetPosition(Vector3 position, bool bypassInterpolation = true)
        {
            _transform.position = position;
            _initialSimulationPosition = position;
            _transientPosition = position;

            if (bypassInterpolation)
            {
                InitialTickPosition = position;
            }
        }


        /// <summary>
        /// Sets the character's rotation directly
        /// </summary>
        public void SetRotation(Quaternion rotation, bool bypassInterpolation = true)
        {
            _transform.rotation = rotation;
            _initialSimulationRotation = rotation;
            TransientRotation = rotation;

            if (bypassInterpolation)
            {
                InitialTickRotation = rotation;
            }
        }


        /// <summary>
        /// Sets the character's position and rotation directly
        /// </summary>
        public void SetPositionAndRotation(Vector3 position, Quaternion rotation, bool bypassInterpolation = true)
        {
            _transform.SetPositionAndRotation(position, rotation);
            _initialSimulationPosition = position;
            _initialSimulationRotation = rotation;
            _transientPosition = position;
            TransientRotation = rotation;

            if (bypassInterpolation)
            {
                InitialTickPosition = position;
                InitialTickRotation = rotation;
            }
        }


        /// <summary>
        /// Moves the character position, taking all movement collision solving int account. The actual move is done the next time the motor updates are called
        /// </summary>
        public void MoveCharacter(Vector3 toPosition)
        {
            _movePositionDirty = true;
            _movePositionTarget = toPosition;
        }

        /// <summary>
        /// Moves the character rotation. The actual move is done the next time the motor updates are called
        /// </summary>
        public void RotateCharacter(Quaternion toRotation)
        {
            _moveRotationDirty = true;
            _moveRotationTarget = toRotation;
        }

        #endregion

        #region Sensor

        /// <summary>
        /// Sets whether or not grounding will be evaluated for all hits
        /// </summary>
        public void SetGroundSolvingActivation(bool stabilitySolvingActive)
        {
            _solveGrounding = stabilitySolvingActive;
        }


        /// <summary>
        /// Returns all the state information of the motor that is pertinent for simulation
        /// </summary>
        public KinematicCharacterMotorState GetState()
        {
            KinematicCharacterMotorState state = new KinematicCharacterMotorState();

            state.Position = _transientPosition;
            state.Rotation = _transientRotation;

            state.BaseVelocity = BaseVelocity;
            state.AttachedRigidbodyVelocity = _attachedRigidbodyVelocity;

            state.MustUnground = _mustUnground;
            state.MustUngroundTime = _mustUngroundTimeCounter;
            state.LastMovementIterationFoundAnyGround = LastMovementIterationFoundAnyGround;
            state.GroundingStatus.CopyFrom(GroundingStatus);
            state.AttachedRigidbody = _attachedRigidbody;

            return state;
        }

        /// <summary>
        /// Applies a motor state instantly
        /// </summary>
        public void ApplyState(KinematicCharacterMotorState state, bool bypassInterpolation = true)
        {
            SetPositionAndRotation(state.Position, state.Rotation, bypassInterpolation);

            BaseVelocity = state.BaseVelocity;
            _attachedRigidbodyVelocity = state.AttachedRigidbodyVelocity;

            _mustUnground = state.MustUnground;
            _mustUngroundTimeCounter = state.MustUngroundTime;
            LastMovementIterationFoundAnyGround = state.LastMovementIterationFoundAnyGround;
            GroundingStatus.CopyFrom(state.GroundingStatus);
            _attachedRigidbody = state.AttachedRigidbody;
        }




        /// <summary>
        /// Determines if motor can be considered stable on given slope normal
        /// </summary>
        protected bool IsStableOnNormal(Vector3 normal)
        {
            return Vector3.Angle(_characterUp, normal) <= MaxStableSlopeAngle;
        }

        /// <summary>
        /// Determines if motor can be considered stable on given slope normal
        /// </summary>
        protected bool IsStableWithSpecialCases(ref HitStabilityReport stabilityReport, Vector3 velocity)
        {
            if (LedgeAndDenivelationHandling)
            {
                if (stabilityReport.LedgeDetected)
                {
                    if (stabilityReport.IsMovingTowardsEmptySideOfLedge)
                    {
                        // Max snap vel
                        Vector3 velocityOnLedgeNormal = Vector3.Project(velocity, stabilityReport.LedgeFacingDirection);
                        if (velocityOnLedgeNormal.magnitude >= MaxVelocityForLedgeSnap)
                        {
                            return false;
                        }
                    }

                    // Distance from ledge
                    if (stabilityReport.IsOnEmptySideOfLedge && stabilityReport.DistanceFromLedge > MaxStableDistanceFromLedge)
                    {
                        return false;
                    }
                }

                // "Launching" off of slopes of a certain denivelation angle
                if (LastGroundingStatus.FoundAnyGround && stabilityReport.InnerNormal.sqrMagnitude != 0f && stabilityReport.OuterNormal.sqrMagnitude != 0f)
                {
                    float denivelationAngle = Vector3.Angle(stabilityReport.InnerNormal, stabilityReport.OuterNormal);
                    if (denivelationAngle > MaxStableDenivelationAngle)
                    {
                        return false;
                    }
                    else
                    {
                        denivelationAngle = Vector3.Angle(LastGroundingStatus.InnerGroundNormal, stabilityReport.OuterNormal);
                        if (denivelationAngle > MaxStableDenivelationAngle)
                        {
                            return false;
                        }
                    }
                }
            }
            
            return true;
        }

        /// <summary>
        /// Forces the character to unground itself on its next grounding update
        /// </summary>
        public void ForceUnground(float time = 0.1f)
        {
            _mustUnground = true;
            _mustUngroundTimeCounter = time;
        }

        public bool MustUnground()
        {
            return _mustUnground || _mustUngroundTimeCounter > 0f;
        }


        public Vector3 GetDirectionTangentToSurface(Vector3 direction, Vector3 surfaceNormal)
        {
            Vector3 directionRight = Vector3.Cross(direction, _characterUp);
            return Vector3.Cross(surfaceNormal, directionRight).normalized;
        }

        /// <summary>
        /// Moves the character's position by given movement while taking into account all physics simulation, step-handling and 
        /// velocity projection rules that affect the character motor
        /// </summary>
        /// <returns> Returns false if movement could not be solved until the end </returns>
        protected abstract bool InternalCharacterMove(ref Vector3 transientVelocity, float deltaTime);
        

        /// <summary>
        /// Gets the effective normal for movement obstruction depending on current grounding status
        /// </summary>
        protected Vector3 GetObstructionNormal(Vector3 hitNormal, bool stableOnHit)
        {
            // Find hit/obstruction/offset normal
            Vector3 obstructionNormal = hitNormal;
            if (GroundingStatus.IsStableOnGround && !MustUnground() && !stableOnHit)
            {
                Vector3 obstructionLeftAlongGround = Vector3.Cross(GroundingStatus.GroundNormal, obstructionNormal).normalized;
                obstructionNormal = Vector3.Cross(obstructionLeftAlongGround, _characterUp).normalized;
            }

            // Catch cases where cross product between parallel normals returned 0
            if (obstructionNormal.sqrMagnitude == 0f)
            {
                obstructionNormal = hitNormal;
            }

            return obstructionNormal;
        }


        public void SetTransientPosition(Vector3 newPos)
        {
            _transientPosition = newPos;
        }

        /// <summary>
        /// Processes movement projection upon detecting a hit
        /// </summary>
        protected void InternalHandleVelocityProjection(bool stableOnHit, Vector3 hitNormal, Vector3 obstructionNormal, Vector3 originalDirection,
            ref MovementSweepState sweepState, bool previousHitIsStable, Vector3 previousVelocity, Vector3 previousObstructionNormal,
            ref Vector3 transientVelocity, ref float remainingMovementMagnitude, ref Vector3 remainingMovementDirection)
        {
            if (transientVelocity.sqrMagnitude <= 0f)
            {
                return;
            }

            Vector3 velocityBeforeProjection = transientVelocity;

            if (stableOnHit)
            {
                LastMovementIterationFoundAnyGround = true;
                HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
            }
            else
            {
                // Handle projection
                if (sweepState == MovementSweepState.Initial)
                {
                    HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
                    sweepState = MovementSweepState.AfterFirstHit;
                }
                // Blocking crease handling
                else if (sweepState == MovementSweepState.AfterFirstHit)
                {
                    EvaluateCrease(
                        transientVelocity,
                        previousVelocity,
                        obstructionNormal,
                        previousObstructionNormal,
                        stableOnHit,
                        previousHitIsStable,
                        GroundingStatus.IsStableOnGround && !MustUnground(),
                        out bool foundCrease,
                        out Vector3 creaseDirection);

                    if (foundCrease)
                    {
                        if (GroundingStatus.IsStableOnGround && !MustUnground())
                        {
                            transientVelocity = Vector3.zero;
                            sweepState = MovementSweepState.FoundBlockingCorner;
                        }
                        else
                        {
                            transientVelocity = Vector3.Project(transientVelocity, creaseDirection);
                            sweepState = MovementSweepState.FoundBlockingCrease;
                        }
                    }
                    else
                    {
                        HandleVelocityProjection(ref transientVelocity, obstructionNormal, stableOnHit);
                    }
                }
                // Blocking corner handling
                else if (sweepState == MovementSweepState.FoundBlockingCrease)
                {
                    transientVelocity = Vector3.zero;
                    sweepState = MovementSweepState.FoundBlockingCorner;
                }
            }

            if (HasPlanarConstraint)
            {
                transientVelocity = Vector3.ProjectOnPlane(transientVelocity, PlanarConstraintAxis.normalized);
            }

            float newVelocityFactor = transientVelocity.magnitude / velocityBeforeProjection.magnitude;
            remainingMovementMagnitude *= newVelocityFactor;
            remainingMovementDirection = transientVelocity.normalized;
        }

        protected void EvaluateCrease(
            Vector3 currentCharacterVelocity,
            Vector3 previousCharacterVelocity,
            Vector3 currentHitNormal,
            Vector3 previousHitNormal,
            bool currentHitIsStable,
            bool previousHitIsStable,
            bool characterIsStable,
            out bool isValidCrease,
            out Vector3 creaseDirection)
        {
            isValidCrease = false;
            creaseDirection = default;

            if (!characterIsStable || !currentHitIsStable || !previousHitIsStable)
            {
                Vector3 tmpBlockingCreaseDirection = Vector3.Cross(currentHitNormal, previousHitNormal).normalized;
                float dotPlanes = Vector3.Dot(currentHitNormal, previousHitNormal);
                bool isVelocityConstrainedByCrease = false;

                // Avoid calculations if the two planes are the same
                if (dotPlanes < 0.999f)
                {
                    // TODO: can this whole part be made simpler? (with 2d projections, etc)
                    Vector3 normalAOnCreasePlane = Vector3.ProjectOnPlane(currentHitNormal, tmpBlockingCreaseDirection).normalized;
                    Vector3 normalBOnCreasePlane = Vector3.ProjectOnPlane(previousHitNormal, tmpBlockingCreaseDirection).normalized;
                    float dotPlanesOnCreasePlane = Vector3.Dot(normalAOnCreasePlane, normalBOnCreasePlane);

                    Vector3 enteringVelocityDirectionOnCreasePlane = Vector3.ProjectOnPlane(previousCharacterVelocity, tmpBlockingCreaseDirection).normalized;

                    if (dotPlanesOnCreasePlane <= (Vector3.Dot(-enteringVelocityDirectionOnCreasePlane, normalAOnCreasePlane) + 0.001f) &&
                        dotPlanesOnCreasePlane <= (Vector3.Dot(-enteringVelocityDirectionOnCreasePlane, normalBOnCreasePlane) + 0.001f))
                    {
                        isVelocityConstrainedByCrease = true;
                    }
                }

                if (isVelocityConstrainedByCrease)
                {
                    // Flip crease direction to make it representative of the real direction our velocity would be projected to
                    if (Vector3.Dot(tmpBlockingCreaseDirection, currentCharacterVelocity) < 0f)
                    {
                        tmpBlockingCreaseDirection = -tmpBlockingCreaseDirection;
                    }

                    isValidCrease = true;
                    creaseDirection = tmpBlockingCreaseDirection;
                }
            }
        }

        /// <summary>
        /// Allows you to override the way velocity is projected on an obstruction
        /// </summary>
        public virtual void HandleVelocityProjection(ref Vector3 velocity, Vector3 obstructionNormal, bool stableOnHit)
        {
            if (GroundingStatus.IsStableOnGround && !MustUnground())
            {
                // On stable slopes, simply reorient the movement without any loss
                if (stableOnHit)
                {
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                // On blocking hits, project the movement on the obstruction while following the grounding plane
                else
                {
                    Vector3 obstructionRightAlongGround = Vector3.Cross(obstructionNormal, GroundingStatus.GroundNormal).normalized;
                    Vector3 obstructionUpAlongGround = Vector3.Cross(obstructionRightAlongGround, obstructionNormal).normalized;
                    velocity = GetDirectionTangentToSurface(velocity, obstructionUpAlongGround) * velocity.magnitude;
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
            else
            {
                if (stableOnHit)
                {
                    // Handle stable landing
                    velocity = Vector3.ProjectOnPlane(velocity, CharacterUp);
                    velocity = GetDirectionTangentToSurface(velocity, obstructionNormal) * velocity.magnitude;
                }
                // Handle generic obstruction
                else
                {
                    velocity = Vector3.ProjectOnPlane(velocity, obstructionNormal);
                }
            }
        }




        #endregion

        #region Collosion


        /// <summary>
        /// Remembers a rigidbody hit for processing later
        /// </summary>
        protected void StoreRigidbodyHit(Rigidbody hitRigidbody, Vector3 hitVelocity, Vector3 hitPoint, Vector3 obstructionNormal, HitStabilityReport hitStabilityReport)
        {
            if (_rigidbodyProjectionHitCount < _internalRigidbodyProjectionHits.Length)
            {
                if (hitRigidbody.GetComponent<IControllerMotor>() == null)
                {
                    RigidbodyProjectionHit rph = new RigidbodyProjectionHit();
                    rph.Rigidbody = hitRigidbody;
                    rph.HitPoint = hitPoint;
                    rph.EffectiveHitNormal = obstructionNormal;
                    rph.HitVelocity = hitVelocity;
                    rph.StableOnHit = hitStabilityReport.IsStable;

                    _internalRigidbodyProjectionHits[_rigidbodyProjectionHitCount] = rph;
                    _rigidbodyProjectionHitCount++;
                }
            }
        }

        /// <summary>
        /// Allows you to override the way hit rigidbodies are pushed / interacted with. 
        /// ProcessedVelocity is what must be modified if this interaction affects the character's velocity.
        /// </summary>
        public virtual void HandleSimulatedRigidbodyInteraction(ref Vector3 processedVelocity, RigidbodyProjectionHit hit, float deltaTime)
        {

        }

        /// <summary>
        /// Takes into account rigidbody hits for adding to the velocity
        /// </summary>
        protected void ProcessVelocityForRigidbodyHits(ref Vector3 processedVelocity, float deltaTime)
        {
            for (int i = 0; i < _rigidbodyProjectionHitCount; i++)
            {
                RigidbodyProjectionHit bodyHit = _internalRigidbodyProjectionHits[i];

                if (bodyHit.Rigidbody && !_rigidbodiesPushedThisMove.Contains(bodyHit.Rigidbody))
                {
                    if (_internalRigidbodyProjectionHits[i].Rigidbody != _attachedRigidbody)
                    {
                        // Remember we hit this rigidbody
                        _rigidbodiesPushedThisMove.Add(bodyHit.Rigidbody);

                        float characterMass = SimulatedCharacterMass;
                        Vector3 characterVelocity = bodyHit.HitVelocity;

                        KinematicCharacterMotor hitCharacterMotor = bodyHit.Rigidbody.GetComponent<KinematicCharacterMotor>();
                        bool hitBodyIsCharacter = hitCharacterMotor != null;
                        bool hitBodyIsDynamic = !bodyHit.Rigidbody.isKinematic;
                        float hitBodyMass = bodyHit.Rigidbody.mass;
                        float hitBodyMassAtPoint = bodyHit.Rigidbody.mass; // todo
                        Vector3 hitBodyVelocity = bodyHit.Rigidbody.velocity;
                        if (hitBodyIsCharacter)
                        {
                            hitBodyMass = hitCharacterMotor.SimulatedCharacterMass;
                            hitBodyMassAtPoint = hitCharacterMotor.SimulatedCharacterMass; // todo
                            hitBodyVelocity = hitCharacterMotor.BaseVelocity;
                        }
                        else if (!hitBodyIsDynamic)
                        {
                            PhysicsMover physicsMover = bodyHit.Rigidbody.GetComponent<PhysicsMover>();
                            if (physicsMover)
                            {
                                hitBodyVelocity = physicsMover.Velocity;
                            }
                        }

                        // Calculate the ratio of the total mass that the character mass represents
                        float characterToBodyMassRatio = 1f;
                        {
                            if (characterMass + hitBodyMassAtPoint > 0f)
                            {
                                characterToBodyMassRatio = characterMass / (characterMass + hitBodyMassAtPoint);
                            }
                            else
                            {
                                characterToBodyMassRatio = 0.5f;
                            }

                            // Hitting a non-dynamic body
                            if (!hitBodyIsDynamic)
                            {
                                characterToBodyMassRatio = 0f;
                            }
                            // Emulate kinematic body interaction
                            else if (RigidbodyInteractionType == RigidbodyInteractionType.Kinematic && !hitBodyIsCharacter)
                            {
                                characterToBodyMassRatio = 1f;
                            }
                        }

                        ComputeCollisionResolutionForHitBody(
                            bodyHit.EffectiveHitNormal,
                            characterVelocity,
                            hitBodyVelocity,
                            characterToBodyMassRatio,
                            out Vector3 velocityChangeOnCharacter,
                            out Vector3 velocityChangeOnBody);

                        processedVelocity += velocityChangeOnCharacter;

                        if (hitBodyIsCharacter)
                        {
                            hitCharacterMotor.BaseVelocity += velocityChangeOnCharacter;
                        }
                        else if (hitBodyIsDynamic)
                        {
                            bodyHit.Rigidbody.AddForceAtPosition(velocityChangeOnBody, bodyHit.HitPoint, ForceMode.VelocityChange);
                        }

                        if (RigidbodyInteractionType == RigidbodyInteractionType.SimulatedDynamic)
                        {
                            HandleSimulatedRigidbodyInteraction(ref processedVelocity, bodyHit, deltaTime);
                        }
                    }
                }
            }

        }

        public void ComputeCollisionResolutionForHitBody(
            Vector3 hitNormal,
            Vector3 characterVelocity,
            Vector3 bodyVelocity,
            float characterToBodyMassRatio,
            out Vector3 velocityChangeOnCharacter,
            out Vector3 velocityChangeOnBody)
        {
            velocityChangeOnCharacter = default;
            velocityChangeOnBody = default;

            float bodyToCharacterMassRatio = 1f - characterToBodyMassRatio;
            float characterVelocityMagnitudeOnHitNormal = Vector3.Dot(characterVelocity, hitNormal);
            float bodyVelocityMagnitudeOnHitNormal = Vector3.Dot(bodyVelocity, hitNormal);

            // if character velocity was going against the obstruction, restore the portion of the velocity that got projected during the movement phase
            if (characterVelocityMagnitudeOnHitNormal < 0f)
            {
                Vector3 restoredCharacterVelocity = hitNormal * characterVelocityMagnitudeOnHitNormal;
                velocityChangeOnCharacter += restoredCharacterVelocity;
            }

            // solve impulse velocities on both bodies, but only if the body velocity would be giving resistance to the character in any way
            if (bodyVelocityMagnitudeOnHitNormal > characterVelocityMagnitudeOnHitNormal)
            {
                Vector3 relativeImpactVelocity = hitNormal * (bodyVelocityMagnitudeOnHitNormal - characterVelocityMagnitudeOnHitNormal);
                velocityChangeOnCharacter += relativeImpactVelocity * bodyToCharacterMassRatio;
                velocityChangeOnBody += -relativeImpactVelocity * characterToBodyMassRatio;
            }
        }

        /// <summary>
        /// Determines if the Input collider is valid for collision processing
        /// </summary>
        /// <returns> Returns true if the collider is valid </returns>
        protected abstract bool CheckIfColliderValidForCollisions(Collider coll);


        /// <summary>
        /// Determines if the Input collider is valid for collision processing
        /// </summary>
        protected bool InternalIsColliderValidForCollisions(Collider coll)
        {
            Rigidbody colliderAttachedRigidbody = coll.attachedRigidbody;
            if (colliderAttachedRigidbody)
            {
                bool isRigidbodyKinematic = colliderAttachedRigidbody.isKinematic;

                // If movement is made from AttachedRigidbody, ignore the AttachedRigidbody
                if (_isMovingFromAttachedRigidbody && (!isRigidbodyKinematic || colliderAttachedRigidbody == _attachedRigidbody))
                {
                    return false;
                }

                // don't collide with dynamic rigidbodies if our RigidbodyInteractionType is kinematic
                if (RigidbodyInteractionType == RigidbodyInteractionType.Kinematic && !isRigidbodyKinematic)
                {
                    // wake up rigidbody
                    if (coll.attachedRigidbody)
                    {
                        coll.attachedRigidbody.WakeUp();
                    }

                    return false;
                }
            }

            // Custom checks
            bool colliderValid = CharacterController.IsColliderValidForCollisions(coll);
            if (!colliderValid)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Determines if the motor is considered stable on a given hit
        /// </summary>
        public abstract void EvaluateHitStability(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, Vector3 withCharacterVelocity, ref HitStabilityReport stabilityReport);


        protected abstract void DetectSteps(Vector3 characterPosition, Quaternion characterRotation, Vector3 hitPoint, Vector3 innerHitDirection, ref HitStabilityReport stabilityReport);


        protected abstract bool CheckStepValidity(int nbStepHits, Vector3 characterPosition, Quaternion characterRotation, Vector3 innerHitDirection, Vector3 stepCheckStartPos, out Collider hitCollider);
       
        /// <summary>
        /// Get true linear velocity (taking into account rotational velocity) on a given point of a rigidbody
        /// </summary>
        public void GetVelocityFromRigidbodyMovement(Rigidbody interactiveRigidbody, Vector3 atPoint, float deltaTime, out Vector3 linearVelocity, out Vector3 angularVelocity)
        {
            if (deltaTime > 0f)
            {
                linearVelocity = interactiveRigidbody.velocity;
                angularVelocity = interactiveRigidbody.angularVelocity;
                if (interactiveRigidbody.isKinematic)
                {
                    PhysicsMover physicsMover = interactiveRigidbody.GetComponent<PhysicsMover>();
                    if (physicsMover)
                    {
                        linearVelocity = physicsMover.Velocity;
                        angularVelocity = physicsMover.AngularVelocity;
                    }
                }

                if (angularVelocity != Vector3.zero)
                {
                    Vector3 centerOfRotation = interactiveRigidbody.transform.TransformPoint(interactiveRigidbody.centerOfMass);

                    Vector3 centerOfRotationToPoint = atPoint - centerOfRotation;
                    Quaternion rotationFromInteractiveRigidbody = Quaternion.Euler(Mathf.Rad2Deg * angularVelocity * deltaTime);
                    Vector3 finalPointPosition = centerOfRotation + (rotationFromInteractiveRigidbody * centerOfRotationToPoint);
                    linearVelocity += (finalPointPosition - atPoint) / deltaTime;
                }
            }
            else
            {
                linearVelocity = default;
                angularVelocity = default;
                return;
            }
        }

        /// <summary>
        /// Determines if a collider has an attached interactive rigidbody
        /// </summary>
        protected Rigidbody GetInteractiveRigidbody(Collider onCollider)
        {
            Rigidbody colliderAttachedRigidbody = onCollider.attachedRigidbody;
            if (colliderAttachedRigidbody)
            {
                if (colliderAttachedRigidbody.gameObject.GetComponent<PhysicsMover>())
                {
                    return colliderAttachedRigidbody;
                }

                if (!colliderAttachedRigidbody.isKinematic)
                {
                    return colliderAttachedRigidbody;
                }
            }
            return null;
        }

        /// <summary>
        /// Calculates the velocity required to move the character to the target position over a specific deltaTime.
        /// Useful for when you wish to work with positions rather than velocities in the UpdateVelocity callback 
        /// </summary>
        public Vector3 GetVelocityForMovePosition(Vector3 fromPosition, Vector3 toPosition, float deltaTime)
        {
            return GetVelocityFromMovement(toPosition - fromPosition, deltaTime);
        }

        public Vector3 GetVelocityFromMovement(Vector3 movement, float deltaTime)
        {
            if (deltaTime <= 0f)
                return Vector3.zero;

            return movement / deltaTime;
        }

        /// <summary>
        /// Trims a vector to make it restricted against a plane 
        /// </summary>
        protected void RestrictVectorToPlane(ref Vector3 vector, Vector3 toPlane)
        {
            if (vector.x > 0 != toPlane.x > 0)
            {
                vector.x = 0;
            }
            if (vector.y > 0 != toPlane.y > 0)
            {
                vector.y = 0;
            }
            if (vector.z > 0 != toPlane.z > 0)
            {
                vector.z = 0;
            }
        }

        /// <summary>
        /// Detect if the character capsule is overlapping with anything
        /// </summary>
        /// <returns> Returns number of overlaps </returns>
        public abstract int CharacterOverlap(Vector3 position, Quaternion rotation, Collider[] overlappedColliders, LayerMask layers, QueryTriggerInteraction triggerInteraction, float inflate = 0f);






        #endregion

    }
}