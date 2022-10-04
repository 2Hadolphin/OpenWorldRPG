
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using Sirenix.OdinInspector;
using Return.Humanoid;
using Return.Audios;
using Return.Preference;
using Return.Physical;

/// <summary>
/// Character States.
/// </summary>
public enum ObsoleteMotionState
{
    Idle,
    Walking,
    Running,
    Crouched,
    Climbing,
    Flying
}



namespace Return.Humanoid
{
    /// <summary>
    /// FirstPersonCharacterController is responsible for handling all movement related functions.
    /// </summary>


    public partial class mFPScontroller:MonoBehaviour
    {
        #region CHARACTER SETTINGS

        /// <summary>
        /// The character’s Capsule ColliderBounds rows in meters.
        /// </summary>
        [SerializeField]
        [Range(1.5f, 2.1f)]
        [Tooltip("The character’s Capsule ColliderBounds rows in meters.")]
        private float m_CharacterHeight = 1.8f;

        /// <summary>
        /// The character’s Capsule ColliderBounds diameter in meters.
        /// </summary>
        [SerializeField]
        [Range(0.6f, 1.4f)]
        [Tooltip("The character’s Capsule ColliderBounds diameter in meters.")]
        private float m_CharacterShoulderWidth = 0.8f;

        /// <summary>
        /// The character’s mass. The movement is based on forces so the mass will affect how fast the character moves.
        /// </summary>
        [SerializeField]
        [Range(50, 100)]
        [Tooltip("The character’s mass. The movement is based on forces so the mass will affect how fast the character moves.")]
        private float m_CharacterMass = 80;

        /// <summary>
        /// Enables jumping functionality for the character.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables jumping functionality for the character.")]
        private bool m_AllowJump = true;

        /// <summary>
        /// Enables running functionality for the character.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables running functionality for the character.")]
        private bool m_AllowRun = true;

        /// <summary>
        /// Enables crouching functionality for the character.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables crouching functionality for the character.")]
        private bool m_AllowCrouch = true;

        /// <summary>
        /// The character’s Capsule ColliderBounds rows while crouching, in meters.
        /// </summary>
        [SerializeField]
        [Range(0.9f, 1.3f)]
        [Tooltip("The character’s Capsule ColliderBounds rows while crouching, in meters.")]
        private float m_CrouchingHeight = 1.25f;

        /// <summary>
        /// Determines how fast the character can change between standing and crouching.
        /// </summary>
        [SerializeField]
        [Range(0.001f, 2f)]
        [Tooltip("Determines how fast the character can change between standing and crouching.")]
        private float m_CrouchingSpeed = 0.5f;

        /// <summary>
        /// The character will step up a stair only if it is closer to the ground than the indicated value.
        /// </summary>
        [SerializeField]
        [Range(0.05f, 0.5f)]
        [Tooltip("The character will step up a stair only if it is closer to the ground than the indicated value.")]
        private float m_StepOffset = 0.25f;

        /// <summary>
        /// Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.
        /// </summary>
        [SerializeField]
        [Range(1, 90)]
        [Tooltip("Limits the collider to only climb slopes that are less steep (in degrees) than the indicated value.")]
        private float m_SlopeLimit = 50;

        #endregion

        #region MOVEMENT

        /// <summary>
        /// Defines how much force will be applied on the character when walking.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 10)]
        [Tooltip("Defines how much force will be applied on the character when walking.")]
        private float m_WalkingForce = 4.25f;

        /// <summary>
        /// Defines how much force will be applied on the character when walking crouching.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 5)]
        [Tooltip("Defines how much force will be applied on the character when walking crouching.")]
        private float m_CrouchForce = 2f;

        /// <summary>
        /// Defines the Running Force by multiplying the indicated value by the Walking Force. (Running Force = Walking Force * Run Multiplier).
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines the Running Force by multiplying the indicated value by the Walking Force. (Running Force = Walking Force * Run Multiplier).")]
        private float m_RunMultiplier = 2.25f;

        /// <summary>
        /// Defines how much the character will be affected by the movement forces while flying.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines how much the character will be affected by the movement forces while flying.")]
        private float m_AirControlPercent = 0.5f;

        /// <summary>
        /// Determines the force applied on the character to perform a jump.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Determines the force applied on the character to perform a jump.")]
        private float m_JumpForce = 9f;

        /// <summary>
        /// A gravity acceleration multiplier to control jumping and falling velocities.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("A gravity acceleration multiplier to control jumping and falling velocities.")]
        private float m_GravityIntensity = 4f;

        /// <summary>
        /// Determines the delay to perform a jump, in seconds.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Determines the delay to perform a jump, in seconds.")]
        private float m_JumpDelay;

        #region FALL

        /// <summary>
        /// The minimum fall distance to calculate the resulting damage.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("The minimum fall distance to calculate the resulting damage.")]
        private float m_HeightThreshold = 4.0f;

        /// <summary>
        /// Multiplies the calculated fall damage by the indicated value to increase the total damage.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Multiplies the calculated fall damage by the indicated value to increase the total damage.")]
        private float m_DamageMultiplier = 3.0f;

        private Vector3 m_FallingStartPos;
        private float m_FallDistance;

        #endregion

        #endregion

        #region CAMERA CONTROLLER

        /// <summary>
        /// The CameraController handles the camera movement and related functions.
        /// </summary>
        [SerializeField]
        [Tooltip("The CameraController handles the camera movement and related functions.")]
        private Camera_ThirdPerson camController;

        /// <summary>
        /// The camera used by this character.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("The camera used by this character.")]
        private Camera m_MainCamera;

        #endregion

        #region CLIMBING

        /// <summary>
        /// Allow the character to climb ladders.
        /// </summary>
        [SerializeField]
        [Tooltip("Allow the character to climb ladders.")]
        private bool m_Ladder = true;

        /// <summary>
        /// Defines how fast the character can climb ladders.
        /// </summary>
        [SerializeField]
        [Range(0.1f, 10)]
        [Tooltip("Defines how fast the character can climb ladders.")]
        private float m_ClimbingSpeed = 3.5f;

        private bool m_Climbing;
        private Vector3 m_LadderTopEdge;

        #endregion

        #region SLIDING

        /// <summary>
        /// Allows the character to slide.
        /// </summary>
        [SerializeField]
        [Tooltip("Allows the character to slide")]
        private bool m_Sliding = true;

        /// <summary>
        /// Defines how much force will be applied on the character when sliding.
        /// </summary>
        [SerializeField]
        [Range(0.1f, Mathf.Infinity)]
        [Tooltip("Defines how much force will be applied on the character when sliding.")]
        private float m_SlidingThrust = 12f;

        /// <summary>
        /// Defines the minimum and maximum distance the character will slide.
        /// </summary>
        [SerializeField]
        [m_MinMaxSliderAttribute(1, 12, "Defines the minimum and maximum distance the character will slide.")]
        private Vector2 m_SlidingDistance = new Vector2(4, 7);

        /// <summary>
        /// Limits the collider to only slide on slopes that are less steep (in degrees) than the indicated value.
        /// </summary>
        [SerializeField]
        [Range(1, 90)]
        [Tooltip("Limits the collider to only slide on slopes that are less steep (in degrees) than the indicated value.")]
        private float m_SlidingSlopeLimit = 15;

        /// <summary>
        /// Defines how much time the character will need to stand up after sliding.
        /// </summary>
        [SerializeField]
        [Range(0.1f, Mathf.Infinity)]
        [Tooltip("Defines how much time the character will need to stand up after sliding.")]
        private float m_DelayToGetUp = 1f;

        /// <summary>
        /// Defines whether the character will stand-up after sliding or will stay crouching.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the character will stand-up after sliding or will stay crouching.")]
        private bool m_StandAfterSliding = true;

        /// <summary>
        /// Override the camera’s vertical rotation limits  (pitch).
        /// </summary>
        [SerializeField]
        [Tooltip("Override the camera’s vertical rotation limits  (pitch).")]
        private bool m_OverrideCameraPitchLimit = true;

        /// <summary>
        /// Defines the minimum and maximum camera pitch while sliding.
        /// </summary>
        [SerializeField]
        [m_MinMaxSliderAttribute(-90, 90, "Defines the minimum and maximum camera pitch while sliding.", "F0")]
        private Vector2 m_SlidingCameraPitch = new Vector2(-40, 60);

        private float m_NextSlidingTime;
        private float m_DesiredSlidingDistance;
        private Vector3 m_SlidingStartPosition;
        private Vector3 m_SlidingStartDirection;

        #endregion

        #region STAMINA

        /// <summary>
        /// Enables stamina feature in character.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables stamina feature in character.")]
        private bool m_Stamina = true;

        /// <summary>
        /// Defines how much stamina the character has.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how much stamina the character has.")]
        private float m_MaxStaminaAmount = 100;

        /// <summary>
        /// Defines how fast the stamina will be regenerated.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how fast the stamina will be regenerated.")]
        private float m_IncrementRatio = 7.5f;

        /// <summary>
        /// Defines how fast the stamina will be depleted.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how fast the stamina will be depleted.")]
        private float m_DecrementRatio = 3.75f;

        /// <summary>
        /// Defines whether the weight the character is carrying will affect their movement speed.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the weight the character is carrying will affect their movement speed.")]
        private bool m_WeightAffectSpeed = true;

        /// <summary>
        /// Defines whether the weight the character is carrying will affect their jump rows.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines whether the weight the character is carrying will affect their jump rows.")]
        private bool m_WeightAffectJump = true;

        /// <summary>
        /// Enables sounds effects to simulate character’s fatigue.
        /// </summary>
        [SerializeField]
        [Tooltip("Enables sounds effects to simulate character’s fatigue.")]
        private bool m_Fatigue = true;

        /// <summary>
        /// Defines the threshold to start playing the breathing sound.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines the threshold to start playing the breathing sound.")]
        private float m_StaminaThreshold = 40;

        /// <summary>
        /// Sound played to indicate stamina depletion.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound played to indicate stamina depletion.")]
        private AudioClip m_BreathSound;

        /// <summary>
        /// The breath volume is defined by (1 - the current percentage of stamina / fatigue threshold). (Less stamina will result in a louder sound).
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("The breath volume is defined by (1 - the current percentage of stamina / fatigue threshold). (Less stamina will result in a louder sound).")]
        private float m_MaximumBreathVolume = 0.5f;

        private float m_StaminaAmount;

        #endregion

        #region PARKOUR

        /// <summary>
        /// Allow the character to vault scene objects.
        /// </summary>
        [SerializeField]
        [Tooltip("Allow the character to vault scene objects.")]
        private bool m_Vault = true;

        /// <summary>
        /// Defines the layers that can be evaluated when checking the dimensions of the obstacle.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the layers that can be evaluated when checking the dimensions of the obstacle.")]
        private LayerMask m_AffectedLayers = 1;

        /// <summary>
        /// Allow the character to vault big obstacles like walls.
        /// </summary>
        [SerializeField]
        [Tooltip("Allow the character to vault big obstacles like walls.")]
        private bool m_AllowWallJumping = true;

        /// <summary>
        /// This curve is used to evaluate how fast the animation should be simulated according to the wave format.
        /// </summary>
        [SerializeField]
        [Tooltip("This curve is used to evaluate how fast the animation should be simulated according to the wave format.")]
        private AnimationCurve m_VaultAnimationCurve = new AnimationCurve(new Keyframe(0, 0), new Keyframe(1, 1));

        /// <summary>
        /// Defines the maximum rows an obstacle can have for the character to be able to vault it.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the maximum rows an obstacle can have for the character to be able to vault it.")]
        [Range(0.75f, 1.75f)]
        private float m_MaxObstacleHeight = 1.2f;

        /// <summary>
        /// Limits the character to only vault slopes that are within the slope range.
        /// </summary>
        [SerializeField]
        [m_MinMaxSliderAttribute(50, 130, "Limits the character to only vault slopes that are within the slope range.")]
        private Vector2 m_ObstacleSlope = new Vector2(70, 120);

        /// <summary>
        /// Defines how long the character will need to vault an obstacle.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how long the character will need to vault an obstacle.")]
        private float m_VaultDuration = 0.55f;

        private Vector3 m_ObstaclePosition;

        #endregion

        #region FOOTSTEPS

        /// <summary>
        /// Allow the character to emit sounds when walking through the scene.
        /// </summary>
        [SerializeField]
        [Tooltip("Allow the character to emit sounds when walking through the scene.")]
        private bool m_Footsteps = true;

        /// <summary>
        /// Allows the footsteps to be automatically calculated based on the character's current speed.
        /// </summary>
        [SerializeField]
        [Tooltip("Allows the footsteps to be automatically calculated based on the character's current speed.")]
        private bool m_AutomaticallyCalculateIntervals;

        /// <summary>
        /// Defines the interval between the footsteps when the character is walking.
        /// </summary>
        [SerializeField]
        [Range(0.001f, Mathf.Infinity)]
        [Tooltip("Defines the interval between the footsteps when the character is walking.")]
        private float m_WalkingBaseInterval = 0.475f;

        /// <summary>
        /// Defines the interval between the footsteps when the character is running.
        /// </summary>
        [SerializeField]
        [Range(0.001f, Mathf.Infinity)]
        [Tooltip("Defines the interval between the footsteps when the character is running.")]
        private float m_RunningBaseInterval = 0.12f;

        /// <summary>
        /// Defines the interval between the footsteps when the character is crouched.
        /// </summary>
        [SerializeField]
        [Range(0.001f, Mathf.Infinity)]
        [Tooltip("Defines the interval between the footsteps when the character is crouched.")]
        private float m_CrouchBaseInterval = 0.7f;

        /// <summary>
        /// Defines the interval between the footsteps when the character is is climbing a ladder.
        /// </summary>
        [SerializeField]
        [Range(0.001f, Mathf.Infinity)]
        [Tooltip("Defines the interval between the footsteps when the character is is climbing a ladder.")]
        private float m_ClimbingInterval = 0.55f;

        /// <summary>
        /// Defines the footsteps volume when walking.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the footsteps volume when walking.")]
        private float m_WalkingVolume = 0.05f;

        /// <summary>
        /// Defines the footsteps volume when walking crouching.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the footsteps volume when walking crouching.")]
        private float m_CrouchVolume = 0.02f;

        /// <summary>
        /// Defines the footsteps volume when running.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the footsteps volume when running.")]
        private float m_RunningVolume = 0.15f;

        /// <summary>
        /// Defines the jumping volume used by the character when performing a jump.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the jumping volume used by the character when performing a jump.")]
        private float m_JumpVolume = 0.1f;

        /// <summary>
        /// Defines the landing volume used by the character when landing on the ground.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the landing volume used by the character when landing on the ground.")]
        private float m_LandingVolume = 0.1f;

        /// <summary>
        /// Sound played when the character goes from standing to crouching.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound played when the character goes from standing to crouching.")]
        private AudioClip m_StandingUpSound;

        /// <summary>
        /// Sound played when the character goes from crouching to standing.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound played when the character goes from crouching to standing.")]
        private AudioClip m_CrouchingDownSound;

        /// <summary>
        /// Defines the crouching volume played when the character goes from crouch or stand up.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the crouching volume played when the character goes from crouch or stand up.")]
        private float m_CrouchingVolume = 0.2f;

        /// <summary>
        /// Defines the sliding sound volume when the character is sliding.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the sliding sound volume when the character is sliding.")]
        private float m_SlidingVolume = 0.2f;

        private float m_NextStep;

        #endregion

        #region EVENTS

        /// <summary>
        /// The LadderEvent is called once per frame while the character is climbing a ladder.
        /// </summary>
        public event Action<bool> LadderEvent;

        /// <summary>
        /// The LandingEvent is called after the character lands on ground due a jump or fall.
        /// </summary>
        public event Action<float> LandingEvent;

        /// <summary>
        /// The PreJumpEvent is called right after the player request to jump, simulating a channeling state to perform the jump.
        /// </summary>
        public event Action PreJumpEvent;

        /// <summary>
        /// The JumpEvent is called right after the character leaves the ground.
        /// </summary>
        public event Action JumpEvent;

        /// <summary>
        /// The VaultEvent is called when the character start jumping an obstacle.
        /// </summary>
        public event Action VaultEvent;

        /// <summary>
        /// The StartSlidingEvent is called right after the character starts sliding.
        /// </summary>
        public event Action StartSlidingEvent;

        /// <summary>
        /// The GettingUpEvent is called right after the character stop sliding.
        /// </summary>
        public event Action GettingUpEvent;

        #endregion

        /// <summary>
        /// The current weight the character is carrying on. Interval : [0, 50]
        /// </summary>
        private float m_Weight;

        /// <summary>
        /// Defines the maximum weight the character can carry.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines the maximum weight the character can carry.")]
        private float m_MaxWeight = 50;

        /// <summary>
        /// Defines the maximum speed loss when carrying the maximum weight. Interval : [0, 1]
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the maximum speed loss when carrying the maximum weight.")]
        private float m_MaxSpeedLoss = 0.7f;

        private Rigidbody m_RigidBody;
        private CapsuleCollider m_Capsule;

        private float m_GroundRelativeAngle;
        private Vector3 m_GroundContactNormal;
        private Vector3 m_GroundContactPoint;

        private SurfaceIdentifier m_Surface;
        private int m_TriangleIndex;

        private bool m_Jump;
        private bool m_PreviouslyGrounded;
        private bool m_PreviouslyJumping;
        private bool m_PreviouslySliding;
        private bool m_Jumping;
        private bool m_SlopedSurface;
        private bool m_Running;
        //private AudioEmitter m_PlayerBreathSource;
        //private AudioEmitter m_PlayerFootstepsSource;

        private InputActionMap m_InputBindings;
        private InputAction m_JumpAction;
        private InputAction m_CrouchAction;
        private InputAction m_MoveAction;
        private InputAction m_SprintingAction;

        #region CONTROLLER PROPERTIES


        /// <summary>
        /// The current camera pitch in the interval : [-1, 1]. (Read Only)
        /// </summary>
        //public float CurrentCameraPitch => camController.CurrentPitch;

        /// <summary>
        /// The current yaw Input. Can be used to verify which direction is the character rotating to. (Read Only)
        /// </summary>
        //public float CurrentYawTarget => camController.CurrentYaw;

        /// <summary>
        /// Defines the current weight the character is carrying on.
        /// </summary>
        public float Weight
        {
            get => m_Weight;
            set => m_Weight = m_WeightAffectSpeed ? Mathf.Clamp(value, 0, m_MaxWeight) : 0;
        }

        /// <summary>
        /// The current radius of the character.
        /// </summary>
        public float Radius => m_Capsule.radius;

        /// <summary>
        /// The current state of the character. (Read Only)
        /// </summary>
        public global::ObsoleteMotionState State { get; private set; } = global::ObsoleteMotionState.Idle;

        /// <summary>
        /// The current velocity of the character. (Read Only)
        /// </summary>
        public Vector3 Velocity => m_RigidBody.velocity;

        /// <summary>
        /// Returns the remaining stamina percentage.
        /// </summary>
        public float StaminaPercent => m_StaminaAmount / m_MaxStaminaAmount;

        /// <summary>
        /// Defines whether the character is aiming or not.
        /// </summary>
        public bool IsAiming
        {
            get;
            set;
        }

        /// <summary>
        /// Defines whether the character has injured his legs or not.
        /// </summary>
        public bool LowerBodyDamaged
        {
            get;
            set;
        }

        /// <summary>
        /// Defines whether the character should start shaking (losing his stability) or not.
        /// </summary>
        public bool TremorTrauma
        {
            get;
            set;
        }

        /// <summary>
        /// Defines whether the character is sliding or not.
        /// </summary>
        public bool IsSliding
        {
            get;
            private set;
        }

        /// <summary>
        /// Defines whether the character is crouched or not.
        /// </summary>
        public bool IsCrouched
        {
            get;
            private set;
        }

        /// <summary>
        /// Defines whether the character is ready to jump an obstacle or not.
        /// </summary>
        public bool ReadyToVault
        {
            get;
            set;
        }

        /// <summary>
        /// Returns true if the character is facing an obstacle and is able to vault it, false otherwise.
        /// </summary>
        public bool CanVault
        {
            get;
            private set;
        }

        /// <summary>
        /// The current force applied to move the character. (Read Only)
        /// </summary>
        public float CurrentTargetForce
        {
            get
            {
                if (IsCrouched)
                {
                    return m_CrouchForce;
                }

                if (m_Running)
                {
                    return WalkingForce * (m_Stamina ? 1 + (m_StaminaAmount * (m_RunMultiplier - 1)) / m_MaxStaminaAmount : m_RunMultiplier);
                }

                return State == global::ObsoleteMotionState.Climbing ? m_ClimbingSpeed : WalkingForce;
            }
        }

        /// <summary>
        /// Returns the sliding force balanced by the weight factor.
        /// </summary>
        private float SlidingThrust => m_SlidingThrust * WeightFactor;

        /// <summary>
        /// Returns the walking force balanced by the weight factor.
        /// </summary>
        private float WalkingForce => m_WalkingForce * WeightFactor;

        /// <summary>
        /// Returns the force loss due the weight carried by the character.
        /// </summary>
        private float WeightFactor
        {
            get
            {
                if (LowerBodyDamaged)
                {
                    return Mathf.Clamp01(1 + m_MaxSpeedLoss - ((1 - m_MaxSpeedLoss) * m_MaxWeight + m_MaxWeight * m_MaxSpeedLoss) / m_MaxWeight);
                }

                if (!m_Stamina)
                    return 1;

                float factor = Mathf.Clamp01(1 + m_MaxSpeedLoss - ((1 - m_MaxSpeedLoss) * Weight + m_MaxWeight * m_MaxSpeedLoss) / m_MaxWeight);
                return factor;
            }
        }

        /// <summary>
        /// Returns the jump force balanced by the weight factor.
        /// </summary>
        private float JumpForce
        {
            get
            {
                if (!m_Stamina || !m_WeightAffectJump)
                    return m_JumpForce;

                return m_JumpForce * WeightFactor;
            }
        }

        #endregion

        private void mStart()
        {
            // Init components
            m_RigidBody = GetComponent<Rigidbody>();
            m_RigidBody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            m_RigidBody.constraints = RigidbodyConstraints.FreezeRotation;
            m_RigidBody.mass = m_CharacterMass / 8;

            m_Capsule = GetComponent<CapsuleCollider>();
            //camController.Init(transform, m_MainCamera.transform);

            ReadyToVault = true;
            m_StaminaAmount = m_MaxStaminaAmount;

            // Events
            JumpEvent += PlayJumpSound;
            LandingEvent += PlayLandingSound;

            // Controllers

            //Controllable = true;

            // Inputs Bindings
            //m_InputBindings = SettingsManager.Instance.GetActionMap("Movement");
            //m_InputBindings.Enable();

            //m_JumpAction = m_InputBindings.FindAction("Jump");
            //m_CrouchAction = m_InputBindings.FindAction("Duck");
            //m_MoveAction = m_InputBindings.FindAction("Move");
            //m_SprintingAction = m_InputBindings.FindAction("Sprinting");

            // Instead of calling these methods once per frame, it's more efficient to update them at a lower frequency.
            InvokeRepeating(nameof(UpdateState), 0, 0.05f);
            InvokeRepeating(nameof(CheckGroundStatus), 0, 0.05f);
            //InvokeRepeating(nameof(CheckObstaclesAhead), 0, 0.05f);

            // AudioSources
            Transform root = transform.root;
            //m_PlayerBreathSource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterGeneric", root);
            //m_PlayerFootstepsSource = AudioManager.Instance.RegisterSource("[AudioEmitter] CharacterFeet", root);
        }

        //private void Update()
        //{
        //    if (Controllable)
        //    {
        //        HandleInput();
        //        CheckObstaclesAhead();
        //    }

        //    if (Grounded || State == ObsoleteMotionState.Climbing)
        //    {
        //        LadderEvent?.Invoke(State == ObsoleteMotionState.Climbing);

        //        if (m_Footsteps && State != ObsoleteMotionState.Flying)
        //            FootStepCycle();

        //        if (m_Stamina)
        //        {
        //            // Updates stamina amount.
        //            m_StaminaAmount = Mathf.MoveTowards(m_StaminaAmount, m_Running && !IsCrouched && ModuleVelocity.sqrMagnitude > CurrentTargetForce * CurrentTargetForce * 0.1f
        //                ? 0 : m_MaxStaminaAmount, Time.deltaTime * (m_Running ? m_DecrementRatio : m_IncrementRatio));

        //            if (m_Fatigue)
        //            {
        //                if (m_StaminaAmount <= m_StaminaThreshold)
        //                {
        //                    // Only plays the breath sound if the current stamina amount is less or equal the threshold.
        //                    m_PlayerBreathSource.Play(m_BreathSound, m_MaximumBreathVolume);
        //                    m_PlayerBreathSource.CalculateVolumeByPercent(m_StaminaThreshold, m_StaminaAmount, m_MaximumBreathVolume);
        //                }
        //            }
        //        }

        //        if (IsSliding)
        //        {
        //            // Stand up if there is anything preventing the character to slide.
        //            if (State != ObsoleteMotionState.Running || Vector3.Distance(transform.position, m_SlidingStartPosition) > m_DesiredSlidingDistance
        //                || Vector3.Dot(transform.forward, m_SlidingStartDirection) <= 0 || m_GroundRelativeAngle > m_SlidingSlopeLimit)
        //            {
        //                IsSliding = false;
        //                m_PreviouslySliding = false;
        //                m_NextSlidingTime = Time.time + m_DelayToGetUp;
        //                m_DesiredSlidingDistance = m_SlidingDistance.width;
        //                m_PlayerFootstepsSource.Stop();
        //                m_Running = m_StandAfterSliding && m_Running;
        //                IsCrouched = !m_StandAfterSliding || PreventStandingInLowHeadroom(transform.position);

        //                GettingUpEvent?.Invoke();

        //                if (m_OverrideCameraPitchLimit)
        //                {
        //                    //camController.OverrideCameraPitchLimit(false, m_SlidingCameraPitch.width, m_SlidingCameraPitch.height);
        //                }
        //            }
        //            else
        //            {
        //                if (!m_PreviouslySliding)
        //                {
        //                    m_PreviouslySliding = true;

        //                    StartSlidingEvent?.Invoke();
        //                }

        //                ScaleCapsuleForCrouching(IsSliding, 0.9f, m_CrouchingSpeed * 2);

        //                if (m_OverrideCameraPitchLimit)
        //                {
        //                    //camController.OverrideCameraPitchLimit(IsSliding, m_SlidingCameraPitch.width, m_SlidingCameraPitch.height);
        //                }
        //            }
        //        }
        //        else
        //        {
        //            // Calculate the sliding distance based on how much the character was running.
        //            m_DesiredSlidingDistance = Mathf.Max(Mathf.MoveTowards(m_DesiredSlidingDistance, State == ObsoleteMotionState.Running ? m_SlidingDistance.height * WeightFactor
        //                : m_SlidingDistance.width, Time.deltaTime * (State == ObsoleteMotionState.Running ? 2 : 3)), m_SlidingDistance.width);

        //            ScaleCapsuleForCrouching(IsCrouched, m_CrouchingHeight, m_CrouchingSpeed);
        //        }
        //    }

        //    if (IsSliding && State != ObsoleteMotionState.Running)
        //    {
        //        IsSliding = false;
        //        m_PlayerFootstepsSource.Stop();

        //        m_Capsule.rows = m_CharacterHeight;
        //        m_Capsule.radius = m_CharacterShoulderWidth * 0.5f;
        //        m_Capsule.center = Vector3.zero;
        //        m_MainCamera.transform.localPosition = new Vector3(0, m_CharacterHeight * 0.4f, 0);

        //        if (m_OverrideCameraPitchLimit)
        //        {
        //            //camController.OverrideCameraPitchLimit(false, m_SlidingCameraPitch.width, m_SlidingCameraPitch.height);
        //        }
        //    }

        //    if (!Grounded && (m_PreviouslyGrounded || m_Climbing))
        //    {
        //        // Set falling start position.
        //        m_FallingStartPos = transform.position;
        //        m_PreviouslyGrounded = false;
        //    }

        //    if (Grounded && !m_PreviouslyGrounded)
        //    {
        //        // Calculates the fall distance.
        //        m_FallDistance = m_FallingStartPos.height - transform.position.height;

        //        if (m_FallDistance > m_HeightThreshold && !m_SlopedSurface)
        //        {
        //            LandingEvent?.Invoke(Mathf.Round(m_DamageMultiplier * -Physics.gravity.height * (m_FallDistance - m_HeightThreshold)));
        //        }
        //        else if (m_FallDistance >= m_StepOffset + m_CharacterHeight * 0.5f || m_PreviouslyJumping)
        //        {
        //            LandingEvent?.Invoke(0);
        //        }

        //        m_FallDistance = 0;
        //        m_PreviouslyGrounded = true;
        //        m_SlopedSurface = false;

        //        if (m_OverrideCameraPitchLimit)
        //        {
        //            //camController.OverrideCameraPitchLimit(false, m_SlidingCameraPitch.width, m_SlidingCameraPitch.height);
        //        }
        //    }
        //}

        /// <summary>
        /// Method used to verify the actions the player wants to execute.
        /// </summary>
        //private void HandleInput()
        //{
        //    if (!m_Jump && !IsCrouched && !LowerBodyDamaged && !m_Climbing && !IsSliding)
        //    {
        //        bool requestJump = m_JumpAction.triggered;

        //        // CheckAdd if there is any obstacle ahead and try to vault, otherwise just jump upwards
        //        if (requestJump && ReadyToVault && CanVault && GetInput().height > Mathf.Epsilon && (Grounded || m_AllowWallJumping))
        //        {
        //            ReadOnlyTransform t = transform;
        //            StartCoroutine(Vault(t.forward * (m_ObstaclePosition.z + m_Capsule.radius * 2) +
        //                                 t.up * (m_ObstaclePosition.height + (m_Capsule.rows * 0.5f + m_StepOffset) - t.position.height)));

        //            VaultEvent?.Invoke();
        //            requestJump = false;
        //            m_Jumping = false;
        //        }

        //        if (requestJump && !PreventStandingInLowHeadroom(transform.position) && m_AllowJump && Grounded)
        //        {
        //            if (PreJumpEvent != null && State == ObsoleteMotionState.IdleState)
        //                PreJumpEvent.Invoke();

        //            Invoke(nameof(PerformJump), State != ObsoleteMotionState.IdleState ? 0 : m_JumpDelay);
        //        }
        //    }

        //    if (Grounded || State == ObsoleteMotionState.Climbing)
        //    {
        //        if (m_AllowRun)
        //            CheckRunning();

        //        if (m_AllowCrouch)
        //        {
        //            if (SettingManager.m_Instance.GamePlay.CrouchStyle == ActionMode.Toggle)
        //            {
        //                if (m_CrouchAction.triggered && !IsCrouched && !m_Running && State != ObsoleteMotionState.Climbing && !IsSliding)
        //                {
        //                    IsCrouched = true;
        //                }
        //                else
        //                {
        //                    IsCrouched &= !m_Running && !IsSliding && State != ObsoleteMotionState.Climbing && !m_CrouchAction.triggered && !m_JumpAction.triggered || PreventStandingInLowHeadroom(transform.position);
        //                }
        //            }
        //            else
        //            {
        //                if (IsCrouched && PreventStandingInLowHeadroom(transform.position) && !m_Running && State != ObsoleteMotionState.Climbing && !IsSliding)
        //                {
        //                    IsCrouched = true;
        //                }
        //                else
        //                {
        //                    IsCrouched = m_CrouchAction.ReadValue<float>() > 0;
        //                }
        //            }
        //        }

        //        if (!m_Sliding)
        //            return;

        //        if (m_CrouchAction.triggered && State == ObsoleteMotionState.Running && !IsSliding && !LowerBodyDamaged && m_StaminaAmount > m_MaxStaminaAmount * 0.4f
        //            && m_NextSlidingTime < Time.time && m_GroundRelativeAngle < m_SlidingSlopeLimit)
        //        {
        //            IsSliding = true;

        //            ReadOnlyTransform t = transform;
        //            m_SlidingStartPosition = t.position;
        //            m_SlidingStartDirection = t.forward;
        //        }
        //    }
        //}

        private void PerformJump()
        {
            if (!m_Jumping)
                m_Jump = true;
        }

        /// <summary>
        /// Casts a ray forward trying to find any obstacle in front of the character, 
        /// if found validate its dimensions to evaluate whether the character can vault or not.
        /// </summary>
        private void CheckObstaclesAhead()
        {
            if (!ReadyToVault || !m_Vault)
            {
                CanVault = false;
                m_ObstaclePosition = Vector3.zero;
                return;
            }

            // ReSharper disable once InlineOutVariableDeclaration
            // C# 6 compatibility.
            RaycastHit hitInfo;

            float radius = m_Capsule.radius;
            Transform t = transform;
            Vector3 origin = t.position + Vector3.up * (m_Capsule.height * 0.05f);
            Vector3 direction = t.TransformDirection(Vector3.forward);

            if (UnityEngine.Physics.SphereCast(origin, radius, direction, out hitInfo, radius * 2, m_AffectedLayers, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.attachedRigidbody)
                    return;

                // Analyze the normal vector and the obstacle surface normal
                float vertical_angle = Vector3.Angle(t.up, hitInfo.normal);
                float horizontal_angle = Vector3.Angle(-t.forward, hitInfo.normal);

                CanVault = hitInfo.collider && horizontal_angle <= 30 && vertical_angle >= m_ObstacleSlope.x && vertical_angle <= m_ObstacleSlope.y && ClearPath() && !PreventStandingInLowHeadroom(t.position + t.forward * (2 * m_Capsule.radius));

                if (!CanVault)
                    return;

                Vector3 pos = t.position + new Vector3(0, m_CharacterHeight * 2, 0) + t.forward * (radius * 2);
                UnityEngine.Physics.Raycast(new Ray(pos, transform.TransformDirection(Vector3.down)), out RaycastHit obstacleInfo, m_CharacterHeight * 2, m_AffectedLayers, QueryTriggerInteraction.Ignore);
                m_ObstaclePosition = new Vector3(0, obstacleInfo.point.y > transform.position.y ? obstacleInfo.point.y + m_StepOffset : hitInfo.point.y + m_StepOffset, hitInfo.distance * 1.5f);
            }
            else
            {
                CanVault = false;
            }
        }

        /// <summary>
        /// Returns true if the target obstacle has valid dimensions, false otherwise.
        /// </summary>
        private bool ClearPath()
        {
            return false;
            //const float DistanceEpsilon = 0.01f;

            //float radius = m_Capsule.radius;
            //ReadOnlyTransform t = transform;
            //Vector3 up = t.up;
            //Vector3 verticalOffset = up * ((m_CharacterHeight - m_MaxObstacleHeight + m_StepOffset) * (!Grounded ? 1.5f : 1));
            //Vector3 forward = t.forward;
            //Vector3 forwardOffset = forward * DistanceEpsilon;
            //Vector3 position = t.position;

            //Vector3 bottom = position + verticalOffset + forwardOffset + forward * (radius * 2);
            //Vector3 top = position + verticalOffset + up * m_CharacterHeight + forwardOffset + forward * (radius * 2);

            //bool capsuleTest = Physics.CheckCapsule(bottom, top, radius - DistanceEpsilon, m_AffectedLayers, QueryTriggerInteraction.Ignore);
            //bool lineTest = Physics.Linecast(position + forwardOffset, bottom, m_AffectedLayers, QueryTriggerInteraction.Ignore);

            //return !capsuleTest && !lineTest;
        }

        /// <summary>
        /// Moves the character to the target position, vaulting the obstacle.
        /// </summary>
        /// <param name="targetPosition">The character destination position.</param>
        private IEnumerator Vault(Vector3 targetPosition)
        {
            Vector3 position = transform.position;
            Vector3 initialPos = position;
            Vector3 destination = position + targetPosition;

            //Controllable = false;
            m_Capsule.enabled = false;

            // Make the character move to the target position.
            for (float t = 0; t <= m_VaultDuration; t += Time.deltaTime)
            {
                transform.position = Vector3.Lerp(initialPos, destination, t / m_VaultDuration * m_VaultAnimationCurve.Evaluate(t / m_VaultDuration));
                yield return new WaitForFixedUpdate();
            }

            m_Capsule.enabled = true;
            //Controllable = true;
        }

        /// <summary>
        /// Prevent the character from standing up.
        /// </summary>
        private bool PreventStandingInLowHeadroom(Vector3 position)
        {
            Ray ray = new Ray(position + m_Capsule.center + new Vector3(0, m_Capsule.height * 0.25f), transform.TransformDirection(Vector3.up));

            return UnityEngine.Physics.SphereCast(ray, m_Capsule.radius * 0.75f, out _, IsCrouched ? m_CharacterHeight * 0.6f : m_JumpForce * 0.12f,
                UnityEngine.Physics.AllLayers, QueryTriggerInteraction.Ignore);
        }

        private void mFixedUpdate()
        {
            // Movement axis
            Vector2 input = GetInput();

            if (m_Climbing)
            {
                ApplyClimbingVelocityChange(input);
            }
            else
            {
                ApplyInputVelocityChange(input);
            }

            ApplyGravityAndJumping(input);
        }

        private void ApplyClimbingVelocityChange(Vector2 input)
        {
            if ((Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon))
            {
                // Calculates movement direction.
                Transform t = transform;
                Vector3 up = t.up;
                Vector3 cameraForward = m_MainCamera.transform.forward;

                bool onLadderTop = m_LadderTopEdge.y - t.position.y < m_Capsule.height;

                Vector3 desiredMove = (input.y * (onLadderTop ? up * Vector3.Dot(cameraForward + up, up) + cameraForward
                                           : cameraForward) + transform.right * input.x).normalized;

                desiredMove.x *= m_ClimbingSpeed;
                desiredMove.z *= m_ClimbingSpeed;
                desiredMove.y = desiredMove.y * m_ClimbingSpeed * Mathf.Abs(Mathf.Sin(Time.time * Mathf.Deg2Rad * (700 - (m_ClimbingSpeed * 100))));

                if (m_RigidBody.velocity.sqrMagnitude < (m_ClimbingSpeed * m_ClimbingSpeed))
                {
                    m_RigidBody.AddForce(desiredMove, ForceMode.Impulse);
                }
            }

            if (!m_Jumping && Mathf.Abs(input.x) < float.Epsilon && Mathf.Abs(input.y) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
            {
                m_RigidBody.Sleep();
            }
        }

        private void ApplyInputVelocityChange(Vector2 input)
        {

            //if (IsSliding)
            //{
            //    if (!(m_RigidBody.velocity.sqrMagnitude < (m_SlidingThrust * m_SlidingThrust)))
            //        return;

            //    Vector3 slidingDir = Vector3.ProjectOnPlane(transform.forward, m_GroundContactNormal);
            //    m_RigidBody.AddForce(slidingDir * Mathf.Lerp(SlidingThrust, m_CrouchForce, Vector3.Distance(transform.position, m_SlidingStartPosition) / m_DesiredSlidingDistance), ForceMode.Impulse);
            //}
            //else
            //{
            //    if (Mathf.Abs(Input.width) - Mathf.Epsilon > 0 || Mathf.Abs(Input.height) - Mathf.Epsilon > 0)
            //    {
            //        // Calculates movement direction.
            //        ReadOnlyTransform t = transform;
            //        Vector3 desiredMove = t.forward * Input.height + t.right * Input.width;
            //        desiredMove = (desiredMove.sqrMagnitude > 1) ? Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal).normalized : Vector3.ProjectOnPlane(desiredMove, m_GroundContactNormal);

            //        desiredMove.width *= (Grounded ? CurrentTargetForce : CurrentTargetForce * m_AirControlPercent);
            //        desiredMove.z *= (Grounded ? CurrentTargetForce : CurrentTargetForce * m_AirControlPercent);
            //        desiredMove.height = desiredMove.height * (Grounded ? CurrentTargetForce : CurrentTargetForce * m_AirControlPercent);

            //        if (m_RigidBody.velocity.sqrMagnitude < (CurrentTargetForce * CurrentTargetForce))
            //        {
            //            m_RigidBody.AddForce(desiredMove, ForceMode.Impulse);
            //        }
            //    }
            //}
        }

        private void ApplyGravityAndJumping(Vector2 input)
        {

            //if (Grounded || m_Climbing)
            //{
            //    m_RigidBody.drag = 5f;

            //    if (m_Jump)
            //    {
            //        m_RigidBody.drag = 0f;
            //        Vector3 velocity = m_RigidBody.velocity;
            //        velocity = new Vector3(velocity.width, 0f, velocity.z);
            //        m_RigidBody.velocity = velocity;

            //        ReadOnlyTransform t = transform;
            //        if (State == ObsoleteMotionState.Running)
            //        {
            //            m_RigidBody.AddForce(t.forward * (JumpForce * 2.5f) + t.up * (JumpForce * 10), ForceMode.Impulse);
            //        }
            //        else
            //        {
            //            m_RigidBody.AddForce(t.up * (JumpForce * 10), ForceMode.Impulse);
            //        }

            //        m_Jumping = true;

            //        if (!m_Climbing)
            //            JumpEvent?.Invoke();
            //    }

            //    if (!m_Jumping && Mathf.Abs(Input.width) < float.Epsilon && Mathf.Abs(Input.height) < float.Epsilon && m_RigidBody.velocity.magnitude < 1f)
            //    {
            //        m_RigidBody.Sleep();
            //    }
            //}
            //else
            //{
            //    m_RigidBody.drag = 0f;

            //    if (m_RigidBody.velocity.magnitude < Mathf.Abs(Physics.gravity.height * m_CharacterMass / 2))
            //    {
            //        // Special thanks to Martin Hoffmann for pointing out the gravity acceleration issue.
            //        m_RigidBody.AddForce(Physics.gravity * m_GravityIntensity, ForceMode.Acceleration);
            //    }

            //    if (m_PreviouslyGrounded && !m_Jumping)
            //    {
            //        StickToGroundHelper();
            //    }
            //}
            //m_Jump = false;
        }

        /// <summary>
        /// Defines the capsule rows based on the character state.
        /// </summary>
        /// <param name="isCrouched">Is the character crouched?</param>
        /// <param name="height">The capsule target rows.</param>
        /// <param name="crouchingSpeed">Defines how fast the character can crouch.</param>
        private void ScaleCapsuleForCrouching(bool isCrouched, float height, float crouchingSpeed)
        {
            if (isCrouched)
            {
                if (Mathf.Abs(m_Capsule.height - height) > Mathf.Epsilon)
                {
                    //m_PlayerFootstepsSource.ForcePlay(m_CrouchingDownSound, m_CrouchingVolume);
                    m_NextStep = m_CrouchingDownSound.length + Time.time;
                }

                m_Capsule.height = height;
                m_Capsule.radius = m_CharacterShoulderWidth * 0.5f;
                m_Capsule.center = new Vector3(0, -(m_CharacterHeight - height) / 2, 0);

                m_MainCamera.transform.localPosition = Vector3.MoveTowards(m_MainCamera.transform.localPosition, new Vector3(0, height - 1, 0),
                    Time.deltaTime * 5 * crouchingSpeed);
            }
            else
            {
                if (Mathf.Abs(m_Capsule.height - m_CharacterHeight) > Mathf.Epsilon)
                {
                    //m_PlayerFootstepsSource.ForcePlay(m_StandingUpSound, m_CrouchingVolume);
                    m_NextStep = m_StandingUpSound.length + Time.time;
                }

                m_Capsule.height = m_CharacterHeight;
                m_Capsule.radius = m_CharacterShoulderWidth * 0.5f;
                m_Capsule.center = Vector3.zero;

                m_MainCamera.transform.localPosition = Vector3.MoveTowards(m_MainCamera.transform.localPosition, new Vector3(0, m_CharacterHeight * 0.4f, 0),
                    Time.deltaTime * 5 * crouchingSpeed);
            }
        }

        /// <summary>
        /// Update the character state by analyzing its properties, like speed and player Input.
        /// </summary>
        private void UpdateState()
        {

            //bool sleeping = IsSliding ? ModuleVelocity.sqrMagnitude < m_CrouchForce * m_CrouchForce * 0.9f && !m_SlopedSurface
            //    : ModuleVelocity.sqrMagnitude < CurrentTargetForce * CurrentTargetForce * 0.1f;

            //bool idle = GetInput() == Vector2.zero || sleeping;
            //bool running = m_Running;

            //if (Grounded)
            //{
            //    if (!running && !IsCrouched && !idle && !IsSliding)
            //    {
            //        State = ObsoleteMotionState.Walking;
            //        return;
            //    }
            //    if ((running && !IsCrouched && !idle) || (IsSliding && !sleeping))
            //    {
            //        State = ObsoleteMotionState.Running;
            //        return;
            //    }
            //    if (IsCrouched && !idle && !IsSliding)
            //    {
            //        State = ObsoleteMotionState.Crouched;
            //        return;
            //    }
            //    State = ObsoleteMotionState.IdleState;
            //}
            //else if (m_Climbing)
            //{
            //    State = ObsoleteMotionState.Climbing;
            //}
            //else
            //{
            //    if (m_FallingStartPos.height - transform.position.height >= m_StepOffset + m_Capsule.rows)
            //        State = ObsoleteMotionState.Flying;
            //}
        }

        /// <summary>
        /// Positions the character to keep it in contact with the ground while standing in sloped surfaces.
        /// </summary>
        private void StickToGroundHelper()
        {
            if (!UnityEngine.Physics.SphereCast(transform.position, m_Capsule.radius * 0.9f, Vector3.down, out RaycastHit hitInfo,
                (1 - m_Capsule.radius) + 0.1f, UnityEngine.Physics.AllLayers, QueryTriggerInteraction.Ignore))
                return;

            if (Mathf.Abs(m_GroundRelativeAngle) >= m_SlopeLimit)
            {
                m_RigidBody.velocity = Vector3.ProjectOnPlane(m_RigidBody.velocity, hitInfo.normal);
            }
        }

        /// <summary>
        /// Returns the current values of the axes that control the character's movement.
        /// </summary>
        public Vector2 GetInput()
        {
            //if (!Controllable)
            //    return Vector2.zero;

            Vector2 input = m_MoveAction.ReadValue<Vector2>();

            return input;
        }

        /// <summary>
        /// Defines if the character is running by checking the player inputs.
        /// </summary>
        private void CheckRunning()
        {
            if (!LowerBodyDamaged && GetInput().y > 0 && !IsAiming && !m_Climbing)
            {
                if (SettingsManager.Instance.GamePlay.SprintStyle == ActionMode.Hold)
                {
                    m_Running = m_SprintingAction.ReadValue<float>() > 0;
                }
                else
                {
                    if (m_SprintingAction.triggered)
                    {
                        m_Running = !m_Running;
                    }
                }
            }
            else
            {
                m_Running = false;
            }
        }

        /// <summary>
        /// Simulates the footstep sounds according to the character's current speed.
        /// </summary>
        private void FootStepCycle()
        {
            if (IsSliding)
            {
                PlaySlidingSound();
            }
            else
            {
                if (!m_Surface)
                    return;

                switch (State)
                {
                    case global::ObsoleteMotionState.Idle:
                        {
                            //if (Mathf.Abs(CurrentYawTarget) > camController.setting.Sensitivity_H * 0.5f)
                            {
                                if (m_AutomaticallyCalculateIntervals)
                                    OnFootStrike(m_CrouchVolume, (6 - m_CrouchForce) * 0.2f);
                                else
                                    OnFootStrike(m_CrouchVolume, m_CrouchBaseInterval * (1 + (1 - WeightFactor)));
                            }
                            break;
                        }
                    case global::ObsoleteMotionState.Walking when m_AutomaticallyCalculateIntervals:
                        OnFootStrike(m_WalkingVolume, (11 - CurrentTargetForce) * 0.07f);
                        break;
                    case global::ObsoleteMotionState.Walking:
                        OnFootStrike(m_WalkingVolume, m_WalkingBaseInterval * (1 + (1 - WeightFactor)));
                        break;
                    case global::ObsoleteMotionState.Crouched when m_AutomaticallyCalculateIntervals:
                        OnFootStrike(m_CrouchVolume, (11 - CurrentTargetForce) * 0.075f);
                        break;
                    case global::ObsoleteMotionState.Crouched:
                        OnFootStrike(m_CrouchVolume, m_CrouchBaseInterval);
                        break;
                    case global::ObsoleteMotionState.Running when m_AutomaticallyCalculateIntervals:
                        OnFootStrike(m_RunningVolume, 0.15f + m_WalkingForce / CurrentTargetForce * 0.25f);
                        break;
                    case global::ObsoleteMotionState.Running:
                        OnFootStrike(m_RunningVolume, m_RunningBaseInterval * (1 + (1 - WeightFactor)) + m_WalkingForce / CurrentTargetForce * (m_WalkingBaseInterval - m_RunningBaseInterval));
                        break;
                    case global::ObsoleteMotionState.Climbing when m_AutomaticallyCalculateIntervals:
                        OnFootStrike(m_WalkingVolume, (11 - m_ClimbingSpeed) * 0.07f);
                        break;
                    case global::ObsoleteMotionState.Climbing:
                        OnFootStrike(m_WalkingVolume, m_ClimbingInterval * (1 + (1 - WeightFactor)));
                        break;
                    case global::ObsoleteMotionState.Flying:
                        break;
                }
            }
        }

        /// <summary>
        /// Play the character's sliding sound.
        /// </summary>
        private void PlaySlidingSound()
        {
            if (!m_Footsteps || !m_Surface)
                return;

            SurfaceType surfaceType = m_Surface.GetSurfaceType(m_GroundContactPoint, m_TriangleIndex);

            if (!surfaceType)
                return;

            AudioClip sliding = surfaceType.GetRandomSlidingSound();
            //m_PlayerFootstepsSource.Play(sliding, m_SlidingVolume);
        }

        /// <summary>
        /// Play the character's jump sound.
        /// </summary>
        private void PlayJumpSound()
        {
            if (!m_Footsteps || !m_Surface)
                return;

            SurfaceType surfaceType = m_Surface.GetSurfaceType(m_GroundContactPoint, m_TriangleIndex);
            if (!surfaceType)
                return;

            AudioClip jump = surfaceType.GetRandomJumpSound();

            //m_PlayerFootstepsSource.ForcePlay(jump, m_JumpVolume);
        }

        /// <summary>
        /// Play the character's landing sound based on the surface its standing on.
        /// </summary>
        private void PlayLandingSound(float fallDamage)
        {
            if (!m_Footsteps || !m_Surface)
                return;

            SurfaceType surfaceType = m_Surface.GetSurfaceType(m_GroundContactPoint, m_TriangleIndex);
            if (!surfaceType)
                return;

            AudioClip land = surfaceType.GetRandomLandingSound();
            //AudioManager.Instance.PlayClipAtPoint(land, m_GroundContactPoint, 3, 8, m_LandingVolume, 0);
        }

        private void OnFootStrike(float volume, float stepLength)
        {
            if (m_NextStep > Time.time)
                return;

            m_NextStep = stepLength + Time.time;
            SurfaceType surfaceType = m_Surface.GetSurfaceType(m_GroundContactPoint, m_TriangleIndex);

            if (State == global::ObsoleteMotionState.Climbing)
            {
                if (!(GetInput() == Vector2.zero || Velocity.sqrMagnitude < CurrentTargetForce * CurrentTargetForce * 0.05f))
                {
                    if (surfaceType)
                    {
                        AudioClip footStep = surfaceType.GetRandomWalkingFootsteps();
                        //m_PlayerFootstepsSource.Play(footStep, volume);
                    }
                }
            }
            else
            {
                if (surfaceType)
                {
                    AudioClip footStep;
                    if (State == global::ObsoleteMotionState.Running)
                    {
                        footStep = surfaceType.GetRandomSprintingFootsteps();
                        //m_PlayerFootstepsSource.Play(footStep, volume);

                        // Better sound fidelity
                    //    AudioManager.Instance.PlayClipAtPoint(footStep, m_GroundContactPoint, 3, 8, volume, 0);
                    }
                    else
                    {
                        footStep = surfaceType.GetRandomWalkingFootsteps();
                        //m_PlayerFootstepsSource.Play(footStep, volume);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the angle between the character foot and the ground normal.
        /// </summary>
        /// <param name="ignoreEnvironment">Should ignore the environment around the character?</param>
        /// <returns>The current ground relative angle.</returns>
        private float CalculateGroundRelativeAngle(bool ignoreEnvironment)
        {
            Vector3 normal = Vector3.up;

            if (UnityEngine.Physics.Raycast(transform.position, Vector3.down, out RaycastHit hitInfo, m_Capsule.height + m_StepOffset, UnityEngine.Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                normal = hitInfo.normal;
            }

            if (ignoreEnvironment)
                return Vector3.Angle(normal, Vector3.up);

            Vector3 position = transform.position;
            Vector3 footPos = new Vector3(position.x, position.y - (m_CharacterHeight * 0.5f - m_StepOffset), position.z);
            if (UnityEngine.Physics.Raycast(footPos, transform.TransformDirection(Vector3.forward), m_CharacterHeight * 2, UnityEngine.Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {
                return Vector3.Angle(normal, Vector3.up);
            }

            return -Vector3.Angle(normal, Vector3.up);

        }

        /// <summary>
        /// Defines if the character is grounded by casting a ray downwards to _checkCache whether the character 
        /// is touching a collider bellow its feet.
        /// </summary>
        private void CheckGroundStatus()
        {

            //m_PreviouslyGrounded = m_Climbing ? m_PreviouslyGrounded : Grounded;
            //m_PreviouslyJumping = m_Jumping;
            //m_SlopedSurface = Mathf.Abs(m_GroundRelativeAngle) > m_SlopeLimit;
            //float offset = (1 - m_Capsule.radius) + (m_SlopedSurface ? 0.05f : m_StepOffset);

            //if (Physics.SphereCast(transform.position, m_Capsule.radius * 0.9f, -transform.up, out RaycastHit hitInfo, offset, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            //{
            //    m_GroundContactNormal = hitInfo.normal;
            //    m_GroundContactPoint = hitInfo.point;
            //    m_GroundRelativeAngle = CalculateGroundRelativeAngle(false);
            //    //Grounded = Mathf.Abs(m_GroundRelativeAngle) < m_SlopeLimit;

            //    m_TriangleIndex = hitInfo.triangleIndex;
            //    m_Surface = hitInfo.collider.GetSurface();
            //}
            //else
            //{
            //    //Grounded = false;
            //    IsCrouched = false;
            //    m_GroundContactNormal = Vector3.up;
            //}
            //if (!m_PreviouslyGrounded && Grounded && m_Jumping)
            //{
            //    m_Jumping = false;
            //}
        }

        public IEnumerator AdrenalineShot(float duration)
        {
            if (!m_Stamina)
                yield break;

            m_Stamina = false;
            StartCoroutine(RegenStaminaProgressively());
            yield return new WaitForSeconds(duration);
            m_Stamina = true;
        }

        private IEnumerator RegenStaminaProgressively(float duration = 1)
        {
            for (float t = 0; t <= duration; t += Time.deltaTime)
            {
                m_StaminaAmount = Mathf.Lerp(m_StaminaAmount, m_MaxStaminaAmount, t / duration);
                yield return new WaitForFixedUpdate();
            }
        }

        public void ClimbingLadder(bool climbing, Vector3 topEdge, SurfaceIdentifier surfaceIdentifier)
        {
            if (m_Ladder)
            {
                m_LadderTopEdge = topEdge;
                m_Climbing = climbing;

                if (State == global::ObsoleteMotionState.Climbing)
                    m_Surface = surfaceIdentifier;
            }
            else
            {
                m_Climbing = false;
            }
        }
    }

}
