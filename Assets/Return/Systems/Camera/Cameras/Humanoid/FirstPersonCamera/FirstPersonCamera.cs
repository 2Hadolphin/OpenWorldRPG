using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;
using Return.Preference;
using Return.Preference.GamePlay;
using Return.Agents;
using Cinemachine;

namespace Return.Cameras
{
    [DefaultExecutionOrder(8000)]
    public partial class FirstPersonCamera : HumanoidCameraBase, IAimController, ICameraPerfect
    {
        [Title("FirstPersonCameraReference")]
        [PropertyTooltip("Camera root position pivot(movement).")]
        [SerializeField]
        private Transform m_Root;

        /// <summary>
        /// Movement root.
        /// </summary>
        public Transform Root { get => m_Root; set => m_Root = value; }

        UserInputHandle InputHandle;

        #region Routine

        protected override void Awake()
        {
            base.Awake();

            if (!YawTransform)
                YawTransform = transform;

            if (!AimYawTransform)
                AimYawTransform = YawTransform;

            if (!PitchTransform)
                PitchTransform = transform;

            FirstPersonBodyCamera.enabled = false;

            if (!FirstPersonBodyCameraTransform)
                FirstPersonBodyCameraTransform = FirstPersonBodyCamera.transform;


            Assert.AreNotEqual(YawTransform, PitchTransform);

            if (InputHandle == null)
            {
                InputHandle = new();
                InputHandle.RegisterInput();
                InputHandle.SetHandler(this);
            }

            //InputHandle.enabled = true;
        }

        protected override void Start()
        {
            if (setting == null)
                setting = LoadDefaultPreset<CameraSetting>("DefaultFirstPersonCameraSetting");

            InitPreference();

            m_firstPersonHandleCamera.enabled = false;
        }


        protected override void OnEnable()
        {
            base.OnEnable();
        }

        public override void Activate()
        {
            base.Activate();

            //if (AimYawTransform)
            //    yaw = PitchTransform.localEulerAngles.height;

            if (PitchTransform)
                pitch = PitchTransform.localEulerAngles.x;

            if (MotionSystem != null)
                Init_CameraPerfect();

            Assert.IsFalse(InputHandle == null);

            InputHandle.enabled = true;

            ConstCache.captureMouseCursor = true;

            foreach (var cam in StackOverlap)
                cam.enabled = true;
        }

        public override void Deactivate()
        {
            if (InputHandle)
                InputHandle.enabled = false;

            if (StackOverlap.NotNull())
                foreach (var cam in StackOverlap)
                    cam.enabled = false;

            base.Deactivate();
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            var layer = Layers.without(Layers.FirstPerson_Handle);

            if (Physics.Raycast(mainCameraTransform.position, mainCameraTransform.forward, out var hit, 200, layer))
                Gizmos.DrawLine(mainCameraTransform.position, hit.point);

            Gizmos.color = Color.yellow;

            if (Physics.Raycast(HandleCameraTransform.position, HandleCameraTransform.forward, out var hitfpview, 200, layer))
                Gizmos.DrawLine(HandleCameraTransform.position, hitfpview.point);

            if (!_Beacon)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawRay(_Beacon.position, Quaternion.Euler(0, Clamp_Rotation_Max.y, 0) * _Beacon.forward * 2);
            Gizmos.DrawRay(_Beacon.position, Quaternion.Euler(0, Clamp_Rotation_Min.y, 0) * _Beacon.forward * 2);
            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * 2);
        }


        #endregion

        #region Setup

        public void LoadSetting(CamMatchData data)
        {
            //CurrentSetting.Offset_pos = data.GUIDs.Offset;
            //Clamp_P_Max = data.GUIDs.Clamp_Max;
            //Clamp_P_Min = data.GUIDs.Clamp_Min;

            //CurrentSetting.Sensitivity_H = data.Rotation.Sensitivity.width;
            //CurrentSetting.Sensitivity_V = data.Rotation.Sensitivity.height;

            //CurrentSetting.Offset_rot = Quaternion.Euler(data.Rotation.Offset);
            Clamp_Rotation_Max = data.Rotation.Clamp_Max;
            Clamp_Rotation_Min = data.Rotation.Clamp_Min;
        }

        #endregion

        #region Cam hierarchy

        [SerializeField]
        private Transform m_HeightTransform;

        /// <summary>
        /// Y-Axis (Left-Right)
        /// </summary>
        public Transform HeightTransform { get => m_HeightTransform; protected set => m_HeightTransform = value; }



        #endregion

        #region Overlap Body Camera

        //[SerializeField]
        //bool m_useBodyCamera;
        //public bool UseBodyCamera { get => m_useBodyCamera; set => m_useBodyCamera = value; }
        public bool UseBodyCamera { get ; protected set ; }

        [SerializeField]
        private Camera m_firstPersonBodyCamera;
        public Camera FirstPersonBodyCamera { get => m_firstPersonBodyCamera; set => m_firstPersonBodyCamera = value; }
        public Transform FirstPersonBodyCameraTransform { get; protected set; }

        public ICoordinate BodyCameraPosition { get; protected set; }

        /// <summary>
        /// **Unfinish procedural hand camera clamp.
        /// </summary>
        /// <param name="animator"></param>
        [Obsolete]
        public virtual void SetBodyCamera(Animator animator)
        {
            SetBodyCamera(new FirsPersonBodyCameraCache(animator));
        }

        public virtual void SetBodyCamera(ICoordinate coordinate)
        {
            UseBodyCamera = coordinate != null;
            FirstPersonBodyCamera.enabled = UseBodyCamera;

            if (UseBodyCamera)
                BodyCameraPosition = coordinate;// new Coordinate_Offset(pivot, offset, Quaternion.identity);
            else
                BodyCameraPosition = null;
        }

        protected virtual void UpdateBodyCamera()
        {
            if (!UseBodyCamera)
                return;

            var rotation = BodyCameraPosition.rotation;
            //rotation = Quaternion.Euler(pitch, 0, 0) * rotation;
            rotation=Quaternion.AngleAxis(Mathf.Clamp(pitch,-30,90), rotation * Vector3.right)*rotation;

            FirstPersonBodyCameraTransform.SetPositionAndRotation(BodyCameraPosition.position, rotation);
        }

        #endregion

        #region Overlap Item Camera

        /// <summary>
        /// m_items graph camera.
        /// </summary>
        [SerializeField]
        private Camera m_firstPersonHandleCamera;

        /// <summary>
        /// Hand graphics camera.
        /// </summary>
        public Camera HandleCamera => m_firstPersonHandleCamera;


        /// <summary>
        /// First person item camera transform.
        /// </summary>
        [SerializeField]
        private Transform m_HandleCameraTransform;

        /// <summary>
        /// First person item camera transform.
        /// </summary>
        public Transform HandleCameraTransform => m_HandleCameraTransform;

        /// <summary>
        /// Calculate hand camera focus point.
        /// </summary>
        public bool CalculateOverlapCamera { get; set; } =true;

        /// <summary>
        /// Character camera slot animation transform.
        /// </summary>
        [ShowInInspector, ReadOnly]
        private Transform m_HandleCameraSlotTransform;

        /// <summary>
        /// Character camera slot animation transform.
        /// </summary>
        public Transform FirstPersonHandleTransform
        {
            get => m_HandleCameraSlotTransform;
            set
            {
                m_firstPersonHandleCamera.enabled = value.NotNull();
                m_HandleCameraSlotTransform = value;
            }
        }

        #endregion

  
        public int SensorDensity = 5;
        public float SensorWidth = 1.3f;

        #region Define

        public enum MotionType { FPS, Director }

        #endregion

        #region FirstPersonFeature Overlap

        public List<Camera> StackOverlap = new();

        #endregion


        #region Cache Reference

        protected float Sensitivity_H=-1f;
        protected float Sensitivity_V=1f;

        protected bool m_enableSmooth;
        protected float m_MouseSmooth;

        protected bool m_enableAcceleration;
        protected float m_MouseAcceleration;

        protected virtual void InitPreference()
        {
            var gamePlay=SettingsManager.Instance.GamePlay;

            //ControlSetting.OnUpdate.performed -= LoadPreference;
            GamePlaySetting.OnUpdate.performed += LoadPreference;

            LoadPreference(gamePlay);

            m_SmoothingBuffer = new Vector2[setting.m_MouseSmoothingBufferSize];
        }

        protected virtual void LoadPreference(GamePlaySetting setting)
        {
            Sensitivity_H = setting.horizontalMouseSensitivity;
            Sensitivity_V = setting.verticalMouseSensitivity;

            m_enableSmooth = setting.enableMouseSmoothing;
            m_MouseSmooth = setting.mouseSmoothing;

            m_enableAcceleration = setting.enableMouseAcceleration;
            m_MouseAcceleration = setting.mouseAcceleration;

        }

        #endregion

        #region Windows

        //public Vector3 Deviation { get { return Overwrite_rot; } }

        /// <summary>
        /// Trace target.
        /// </summary>
        Transform m_HostTarget;

        /// <summary>
        /// Tracing target(player root).
        /// </summary>
        public override Transform HostObject 
        {
            get => m_HostTarget;
        }

        /// <summary>
        /// Set character target for first person view. 
        /// </summary>
        public override void SetTarget(Transform tf)
        {
            m_HostTarget = tf;

            var agent = tf.GetComponentInParent<IAgent>();
            SetTarget(agent);
        }

        public virtual void SetTarget(IAgent agent)
        {
            if (agent != null && agent.Resolver.TryGetModule(out MotionSystem))
            {
                MotionSystem.OnAfterMotion -= OnAfterControllerMove;
                MotionSystem.OnAfterMotion += OnAfterControllerMove;
            }
            else
            {
                Debug.LogError(" Missing : " + agent);
                enabled = false;
            }
        }

        #endregion

        #region Control
        //protected bool Tracing;
        //protected bool Freeze;
        public bool LockAxis_X = true;
        public bool LockAxis_Y = false;
        /// <summary>
        /// Flow value which over clamping
        /// </summary>
        public bool FlowAll = false;
        //public bool Flow_Sight = false;

        #endregion

        #region State


        /// <summary>
        /// ???? lerp?
        /// </summary>
        private float m_PendingYaw = 0f;

        // limit
        private float m_PitchLimitMin = -89f;
        // limit
        private float m_PitchLimitMax = 89f;

        // additive pitch
        private float m_PendingPitch = 0f;

        #endregion


        #region NeoState  
        [SerializeField, Range(0f, 1f), Tooltip("The time taken to turn the character to the aim-yaw direction (if Aim Yaw ReadOnlyTransform is set). 0 = call LerpYawToAim() manually, 1 = instant.")]
        private float m_SteeringRate = 0.5f;

        private bool m_DisconnectAimFromYaw = false;
        private float m_ConstraintsMatchMult = 0f;
        private bool m_YawConstrained = false;
        private Vector3 m_YawConstraint = Vector3.zero;
        private float m_YawLimit = 0f;
        private bool m_HeadingConstrained = false;
        private Vector3 m_HeadingConstraint = Vector3.zero;
        private float m_HeadingLimit = 0f;
        private Quaternion m_YawLocalRotation = Quaternion.identity;

        #region Offset

        [SerializeField, Tooltip("The transform to calculate the Input relative to. Use this to factor tilt into the yaw and pitch Input")]
        public Transform m_RelativeTo = null;


        #endregion

        #endregion


        // Use for Final caculate
        #region FinalSet
        [ShowInInspector,BoxGroup("State")]
        protected Vector3 Final_pos;
        [ShowInInspector, BoxGroup("State")]
        protected Quaternion Final_rot;
        #endregion

        #region Clamp
        [ShowInInspector, BoxGroup("Constraint")]
        protected Vector3 Clamp_Rotation_Min=new (-180,-85,-25);
        [ShowInInspector, BoxGroup("Constraint")]
        protected Vector3 Clamp_Rotation_Max = new(180, 85, 25);

        protected Vector3 Clamp_Position_Min;
        protected Vector3 Clamp_Position_Max;
        #endregion

        #region Overflow
        [ShowInInspector, BoxGroup("Overflow")]
        protected Vector3 Overflow_pos;
        [ShowInInspector, BoxGroup("Overflow")]
        protected Vector3 Overflow_rot;

        protected bool Overflow_enable_pos;
        protected bool Overflow_enable_rot;

        public event Action<Vector3> Overflow_Pass_pos;
        public event Action<Vector3> Overflow_Pass_rot;
        public event Action<Vector3> ViewPoint;
        #endregion


        ///     Axis-left as negative, Axis--right as positive.
        public virtual void SetCamAxisClamp(Vector3 min, Vector3 max)
        {
            Clamp_Rotation_Max = max;
            Clamp_Rotation_Min = min;

            if (min.y.Equals(0) && max.y.Equals(0))
            {
                LockAxis_Y = true;
            }
            else
                LockAxis_Y = false;
        }




        #region IAimController  **weapon sway

        public float aimYawDiff
        {
            get
            {
                if (m_DisconnectAimFromYaw)
                    return m_YawLocalRotation.eulerAngles.y;
                else
                    return 0f;
            }
        }

        [Obsolete]
        public float steeringRate
        {
            get { return m_SteeringRate; }
            set { m_SteeringRate = Mathf.Clamp01(value); }
        }

        [SerializeField]
        private float m_TurnRateMultiplier = 1f;

        public float turnRateMultiplier
        {
            get { return m_TurnRateMultiplier; }
            set { m_TurnRateMultiplier = Mathf.Clamp01(value); }
        }


        #region Yaw

        /// <summary>
        /// Yaw angle.
        /// </summary>
        [ShowInInspector]
        private float m_yaw = 0f;

        /// <summary>
        /// Angle of yaw transform.
        /// </summary>
        public virtual float yaw
        {
            get => m_yaw;
            set => m_yaw = value.WrapAngle();
        }

        public Quaternion yawLocalRotation
        {
            get => Quaternion.Euler(0, yaw, 0);
            //{
            //    if (m_DisconnectAimFromYaw)
            //        return YawTransform.localRotation;
            //    else
            //        return m_YawLocalRotation;
            //}
        }

        #endregion

        #region Pitch

        /// <summary>
        /// Pitch angle.
        /// </summary>
        [ShowInInspector]
        private float m_pitch = 0f;

        /// <summary>
        /// Angle of pitch transform.
        /// </summary>
        public float pitch
        {
            get => m_pitch;
            set
            {
                m_pitch = value.WrapAngle();
            }
        }

        /// <summary>
        /// Local rotation of pitch transform.
        /// </summary>
        public Quaternion pitchLocalRotation
        {
            get => Quaternion.Euler(pitch, 0f, 0f);
        }

        //public float pitch
        //{
        //    get
        //    {
        //        return -PitchTransform.localEulerAngles.width.WrapAngle();
        //        //return -(Mathf.Repeat(PitchTransform.localRotation.eulerAngles.width + 180f, 360f) - 180f);
        //    }
        //}
        #endregion

        public Quaternion rotation
        {
            get { return PitchTransform.rotation; }
            set { PitchTransform.rotation = value; }
        }

        public Vector3 aimHeading
        {
            get { return AimYawTransform.forward; }
        }

        public Vector3 heading
        {
            get { return YawTransform.forward; }
        }

        public Vector3 yawUp
        {
            get { return YawTransform.up; }
        }

        public Vector3 forward
        {
            get { return PitchTransform.forward; }
        }

        #endregion

        [Obsolete]
        public float constraintsSmoothing
        {
            get { return setting.m_ConstraintsDamping; }
            set
            {
                setting.m_ConstraintsDamping = Mathf.Clamp01(value);
                CalculateSmoothingMultiplier();
            }
        }


        [Obsolete]
        void CalculateSmoothingMultiplier()
        {
            float lerp = 1f - setting.m_ConstraintsDamping;
            lerp *= lerp;
            m_ConstraintsMatchMult = Mathf.Lerp(CameraSetting.k_MinConstraintsMatchMult, CameraSetting.k_MaxConstraintsMatchMult, lerp);
        }




        #region Control

        void Performe(Vector2 input)
        {
            //Debug.Log($"Move camera : {input}.");

            if (m_enableAcceleration)
                input = GetAcceleratedMouseInput(input, m_MouseAcceleration);

            if (m_enableSmooth)
                input = GetSmoothedMouseInput(input, m_MouseSmooth);


             AddRotation(input.x * Sensitivity_H, input.y * Sensitivity_V);

            return;
            HandleMouseInput(input);
        }

        Vector2 GetAcceleratedMouseInput(Vector2 input, float strength)
        {
            float speed = input.magnitude / ConstCache.deltaTime;

            float speedMultiplier = Mathf.Lerp(setting.m_MouseAccelSpeedMultiplyMin, setting.m_MouseAccelSpeedMultiplyMax, strength);

            float multiplier = 1f + (speed * speedMultiplier);

            // Clamp the multiplier?
            if (setting.m_MouseAccelerationMax > 1f && multiplier > setting.m_MouseAccelerationMax)
                multiplier = setting.m_MouseAccelerationMax;

            return input * multiplier;
        }

        private Vector2[] m_SmoothingBuffer = null;
        private int m_SmoothingStartIndex = 0;
        private int m_SmoothingCount = 0;

        Vector2 GetSmoothedMouseInput(Vector2 input, float strength)
        {
            // Increment the index
            ++m_SmoothingStartIndex;
            if (m_SmoothingStartIndex == m_SmoothingBuffer.Length)
                m_SmoothingStartIndex = 0;

            // Add Input to front of buffer (with frame time to make framerate independent)
            m_SmoothingBuffer[m_SmoothingStartIndex] = input / ConstCache.deltaTime;
            ++m_SmoothingCount;

            // CheckAdd if first entry
            if (m_SmoothingCount == 1)
                return input;

            // Get the weight multiplier
            float multiplier = Mathf.Lerp(setting.m_MouseSmoothingMultiplierMin, setting.m_MouseSmoothingMultiplierMax, strength);

            // Get smoothed value
            Vector2 accumulated = Vector2.zero;
            float currentMultiplier = 1f;
            float weight = 0f;
            int index = m_SmoothingStartIndex;
            for (int i = 0; i < m_SmoothingCount; ++i)
            {
                accumulated += m_SmoothingBuffer[index] * currentMultiplier;
                weight += currentMultiplier;

                currentMultiplier *= multiplier;

                --index;
                if (index < 0)
                    index += m_SmoothingBuffer.Length;
            }
            var result= accumulated *ConstCache.deltaTime / weight;
            Debug.Log("Smooth : "+result);
            return result;
        }

        /// <summary>
        /// ????????
        /// </summary>
        protected virtual void HandleMouseInput(Vector2 input)
        {
            #region Clamp

            float flow_v = input.y * setting.Sensitivity_V * ConstCache.deltaTime;

            if (LockAxis_X)
                Overwrite_rot.x = 0;//to Zero?
            else
            {
                Overwrite_rot.x -= flow_v;

                //Debug.Log(Overwrite_rot.width+" ** "+flow_v+" ** "+ CurrentSetting.Sensitivity_V);

                if (FlowAll)
                    Overwrite_rot.x = Overwrite_rot.x.Clamp(Clamp_Rotation_Min.x, Clamp_Rotation_Max.x);
                else
                    Overwrite_rot.x = Overwrite_rot.x.Clamp_flow(Clamp_Rotation_Min.x, Clamp_Rotation_Max.x, out flow_v);
            }

            float flow_h = input.x * setting.Sensitivity_H * ConstCache.deltaTime;

            if (LockAxis_Y)
                Overwrite_rot.y = 0;//?
            else
            {
                Overwrite_rot.y += flow_h;

                if (FlowAll)
                    Overwrite_rot.y = Overwrite_rot.y.Clamp(Clamp_Rotation_Min.y, Clamp_Rotation_Max.y);
                else
                    Overwrite_rot.y = Overwrite_rot.y.Clamp_flow(Clamp_Rotation_Min.y, Clamp_Rotation_Max.y, out flow_h);
            }


            #endregion


            #region Overflow
            Overflow_rot = new Vector3(flow_v, flow_h);
            Overflow_Pass_rot?.Invoke(Overflow_rot);
            #endregion

        }

        public virtual void InteractionCallback(Vector3 rot)
        {
            Overwrite_rot -= rot;
        }


        #endregion


        #region Routine

  
        public float OffsetHeight = 1.6f;
        public LayerMask GroundedMask;

        protected override void Update()
        {
            #region Main Camera
            if (Root && HostObject)
            {
                var t = ConstCache.deltaTime;
                var point = Root.position;

                if (HeightTransform)
                {
                    var up = Root.up;
                    var offset = up.Multiply(OffsetHeight);
                    var lerp=Vector3.Project(point + offset - HeightTransform.position, up);
                    HeightTransform.localPosition = offset+Vector3.Lerp(Vector3.zero, lerp, t);
                }

                var pos = Vector3.Lerp(point, HostObject.position, t * 30f);
                Root.position = pos;
            }

            // update rows
            if (HeightTransform)
            {

                //var point = RootTransform.position;
                //var radius = SensorWidth * 0.5f;

                //if (Physics.SphereCast(point + RootTransform.up.Multiply(OffsetHeight), radius, -RootTransform.up,out var hit,OffsetHeight, GroundedMask))
                //{
                //    //if (hit.distance > OffsetHeight- radius)
                //    //{
                //    //    // miss grounded
                //    //    Debug.Log("MissGrounded");
                //    //    return;
                //    //}
                //    var offset= RootTransform.up.Multiply(hit.distance + radius);
                //    HeightTransform.position = point + offset;
                //    Debug.DrawRay(point, offset, Color.red);
                //}
            }

            UpdateYaw();
            UpdatePitch();

            if (m_DisconnectAimFromYaw)
            {
                if (m_SteeringRate < 0.0001f)
                {
                    // Lerp of 0 means handle heading constraints only
                    LerpYawToAim(0f);
                }
                else
                {
                    if (m_SteeringRate >= 0.999f)
                    {
                        // Lerp of 1 is instant, but uses heading constraints
                        LerpYawToAim(1f);
                    }
                    else
                    {
                        // Lerp over time, using heading constraints
                        float lerp = Mathf.Lerp(0.0025f, 0.25f, m_SteeringRate);
                        LerpYawToAim(lerp);
                    }
                }
            }

            #endregion
        }

        /// <summary>
        /// Calculate hand camera position and rotation.
        /// </summary>
        public class FocusHandler
        {
            /// <summary>
            /// Yaw rotation and pitch axis.
            /// </summary>
            public Transform CharacterRoot;
            
            /// <summary>
            /// Origin point.
            /// </summary>
            public Transform Neck;

            public Transform rightHand;
            public Transform leftHand;

            public Transform leftShoulder;
            public Transform rightShoulder;

            /// <summary>
            /// Weight to focus on hands
            /// </summary>
            public float FocusRatio;

            public virtual void CalculatePoint(out PR pr)
            {
                pr = PR.Default;

                // calculate rows offset of overlap camera
                // hand position relate to neck

                var pos = (rightShoulder.position + leftShoulder.position).Multiply(0.5f);

                var rightPos = rightHand.position;
                var leftPos = leftHand.position;




                //var rot =Quaternion.LookRotation();

                // clamp rotation in main camera
            }
        }

        // fov
        protected override void LateUpdate()
        {
            #region Overlap

            #region Body
            UpdateBodyCamera();
            #endregion

            #region Hand

            if (FirstPersonHandleTransform.NotNull())
            {

                if (CalculateOverlapCamera) // math map to main camera
                {
                    var mainRot = mainCameraTransform.rotation;


                    // yaw
                    var yaw = this.yaw;
                    var pitch = this.pitch;

                }
                else // animation
                {

                }


                HandleCameraTransform.Copy(FirstPersonHandleTransform);
            }
            //else
            //{
            //    // calculate hand volume



            //    HandleCameraTransform.position = default;
            //}

            #endregion

            #endregion

            //base.LateUpdate();


            //if (YawTransform)
            //    YawTransform.rotation = YawCache;

            //if (PitchTransform)
            //    PitchTransform.rotation = PitchCache;

            return;

            try
            {
                //Vector3 pos = _Beacon.localPosition + CurrentSetting.Offset_pos + _Beacon.InverseTransformDirection(Final_pos);
                //pos = Vector3.Lerp(Yaw.position, pos, CurrentSetting.SmoothLerp_Position);
                //Yaw.localPosition = pos;

                //rot = Quaternion.Euler(Vector3.Lerp(_tf.eulerAngles, rot.eulerAngles, CurrentSetting.SmoothLerp_Rotation));
                //rot = Quaternion.Lerp(_tf.rotation, rot, CurrentSetting.SmoothLerp_Rotation);

                //pos.width = Mathf.LerpAngle(_tf.eulerAngles.width, pos.width, CurrentSetting.SmoothLerp_Rotation);
                //pos.height = Mathf.LerpAngle(_tf.eulerAngles.height, pos.height, CurrentSetting.SmoothLerp_Rotation);

                Final_rot = Quaternion.Euler(Overwrite_rot);// * CurrentSetting.Offset_rot;

                //var rot = _Beacon.localRotation * Final_rot * CurrentSetting.Offset_rot;
                var rot = /*Yaw.localRotation **/ Final_rot;// * CurrentSetting.Offset_rot;

                rot= Quaternion.Lerp(YawTransform.localRotation, rot, setting.SmoothLerp_Rotation);

                //Debug.Log(rot.eulerAngles+" ** "+ CurrentSetting.SmoothLerp_Rotation);

                rot.eulerAngles -= new Vector3(0, 0, rot.eulerAngles.z);

                YawTransform.localRotation = rot;

                //ViewPoint?.Invoke(pos + Yaw.forward * 10);
            }
            catch (Exception e)
            {
                //go scma get new view target
                Debug.LogError("Missing TraceTaget"+ e);
            }
        }

        protected virtual void UpdateYaw()
        {
            if (m_YawConstrained)
            {
                Vector3 target = Vector3.ProjectOnPlane(m_YawConstraint, AimYawTransform.up).normalized;
                if (target != Vector3.zero)
                {
                    // Get the signed yaw angle from the constraint target
                    float angle = Vector3.SignedAngle(target, AimYawTransform.forward, AimYawTransform.up);

                    // Get the min and max turn
                    float minTurn = -m_YawLimit - angle;
                    float maxTurn = m_YawLimit - angle;

                    // CheckAdd if outside bounds
                    bool outsideBounds = false;
                    if (minTurn > 0f)
                    {
                        // Get damped rotation towards constraints
                        float y = minTurn;
                        if (minTurn > setting.m_ConstraintsTolerance)
                            y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                        // Set pending yaw if above reaches constraints faster
                        if (m_PendingYaw < y)
                            m_PendingYaw = y;

                        // Prevent overshoot
                        if (m_PendingYaw > maxTurn)
                            m_PendingYaw = maxTurn;

                        // Falloff
                        //m_YawConstraintsFalloff

                        outsideBounds = true;
                    }

                    if (maxTurn < 0f)
                    {
                        // Get damped rotation towards constraints
                        float y = maxTurn;
                        if (maxTurn < -setting.m_ConstraintsTolerance)
                            y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                        // Set pending yaw if above reaches constraints faster
                        if (m_PendingYaw > y)
                            m_PendingYaw = y;

                        // Prevent overshoot
                        if (m_PendingYaw < minTurn)
                            m_PendingYaw = minTurn;

                        outsideBounds = true;
                    }

                    if (!outsideBounds)
                    {
                        // Apply falloff
                        if (setting.m_YawConstraintsFalloff > 0.0001f)
                        {
                            if (m_PendingYaw >= 0f)
                                m_PendingYaw *= Mathf.Clamp01(maxTurn / setting.m_YawConstraintsFalloff);
                            else
                                m_PendingYaw *= Mathf.Clamp01(-minTurn / setting.m_YawConstraintsFalloff);
                        }

                        // Clamp the rotation
                        m_PendingYaw = Mathf.Clamp(m_PendingYaw, minTurn, maxTurn);
                    }
                }

                Debug.Log(m_PendingYaw);
            }

            // Apply yaw rotation
            m_YawLocalRotation *= Quaternion.Euler(0f, m_PendingYaw, 0f);
            AimYawTransform.localRotation = m_YawLocalRotation;

            // Reset pending yaw
            m_PendingYaw = 0f;
        }

        protected virtual void UpdatePitch()
        {
            // CheckAdd if outside bounds already
            bool outsideBounds = false;

            var pitch = this.pitch;

            if (pitch > m_PitchLimitMax)
            {
                // Get damped rotation towards constraints
                float p = (m_PitchLimitMax - pitch) * Mathf.Clamp01(ConstCache.deltaTime * m_ConstraintsMatchMult);

                // Set pending pitch if above reaches constraints faster
                if (m_PendingPitch > p)
                    m_PendingPitch = p;

                // Assign & prevent overshoot
                pitch += m_PendingPitch;
                if (pitch < m_PitchLimitMin)
                    pitch = m_PitchLimitMin;

                outsideBounds = true;
            }
            
            if (pitch < m_PitchLimitMin)
            {
                // Get damped rotation towards constraints
                float p = (m_PitchLimitMin - pitch) * Mathf.Clamp01(ConstCache.deltaTime * m_ConstraintsMatchMult);

                // Set pending pitch if above reaches constraints faster
                if (m_PendingPitch < p)
                    m_PendingPitch = p;

                // Assign & prevent overshoot
                pitch += m_PendingPitch;

                if (pitch > m_PitchLimitMax)
                    pitch = m_PitchLimitMax;

                outsideBounds = true;
            }

            // Clamp the rotation
            if (!outsideBounds)
                pitch = Mathf.Clamp(pitch + m_PendingPitch, m_PitchLimitMin, m_PitchLimitMax);

            // Apply the pitch
            this.pitch = pitch;
            PitchTransform.localRotation = pitchLocalRotation;

            // Reset pending pitch
            m_PendingPitch = 0f;
        }


        #endregion


        #region ICameraController

        public void AddYaw(float y)
        {
            if (enabled)
                m_PendingYaw += y * 1f;// m_TurnRateMultiplier;
        }

        public void ResetYawLocal()
        {
            //if (isValid)
            {
                if (!m_DisconnectAimFromYaw)
                    m_YawLocalRotation = Quaternion.identity;
                YawTransform.localRotation = Quaternion.identity;
            }
        }

        public void ResetYawLocal(float offset)
        {
            //if (isValid)
            {
                var rotation = Quaternion.Euler(0f, offset, 0f);
                if (!m_DisconnectAimFromYaw)
                    m_YawLocalRotation = Quaternion.identity;
                YawTransform.localRotation = rotation;
            }
        }

        void LerpYawToAim(float amount)
        {
            if (!m_DisconnectAimFromYaw)
                return;

            if (m_HeadingConstrained)
            {
                Vector3 target = Vector3.ProjectOnPlane(m_HeadingConstraint, YawTransform.up).normalized;
                if (target != Vector3.zero)
                {
                    // Get the signed yaw angle from the constraint target
                    float angle = Vector3.SignedAngle(target, YawTransform.forward, YawTransform.up);

                    // Get the min and max turn
                    float minTurn = -m_HeadingLimit - angle;
                    float maxTurn = m_HeadingLimit - angle;

                    // Get pending heading
                    float pending = 0f;
                    if (amount > 0f)
                    {
                        pending = m_YawLocalRotation.eulerAngles.y;
                        pending = Mathf.Repeat(pending + 180f, 360f) - 180f;
                        pending = Mathf.Lerp(0f, pending, amount);
                    }

                    // CheckAdd if outside bounds
                    bool outsideBounds = false;
                    if (minTurn > 0f)
                    {
                        // Get damped rotation towards constraints
                        float y = minTurn;
                        if (minTurn > setting.m_ConstraintsTolerance)
                            y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                        // Set pending yaw if above reaches constraints faster
                        if (pending < y)
                            pending = y;

                        // Prevent overshoot
                        if (pending > maxTurn)
                            pending = maxTurn;

                        outsideBounds = true;
                    }
                    if (maxTurn < 0f)
                    {
                        // Get damped rotation towards constraints
                        float y = maxTurn;
                        if (maxTurn < -setting.m_ConstraintsTolerance)
                            y *= Mathf.Clamp01(Time.deltaTime * m_ConstraintsMatchMult);

                        // Set pending yaw if above reaches constraints faster
                        if (pending > y)
                            pending = y;

                        // Prevent overshoot
                        if (pending < minTurn)
                            pending = minTurn;

                        outsideBounds = true;
                    }

                    if (!outsideBounds)
                    {
                        // Apply falloff
                        if (setting.m_YawConstraintsFalloff > 0.0001f)
                        {
                            if (pending >= 0f)
                                pending *= Mathf.Clamp01(maxTurn / setting.m_YawConstraintsFalloff);
                            else
                                pending *= Mathf.Clamp01(-minTurn / setting.m_YawConstraintsFalloff);
                        }

                        // Clamp the rotation
                        pending = Mathf.Clamp(pending, minTurn, maxTurn);
                    }

                    // Apply heading rotation
                    var rotation = Quaternion.Euler(0f, pending, 0f);
                    YawTransform.localRotation *= rotation;
                    m_YawLocalRotation *= Quaternion.Inverse(rotation);
                    AimYawTransform.localRotation = m_YawLocalRotation;
                }
            }
            else
            {
                if (amount > 0f)
                {
                    if (amount >= 1f)
                    {
                        YawTransform.localRotation *= m_YawLocalRotation;
                        m_YawLocalRotation = Quaternion.identity;
                        AimYawTransform.localRotation = m_YawLocalRotation;
                    }
                    else
                    {
                        var lerped = Quaternion.Lerp(Quaternion.identity, m_YawLocalRotation, amount);
                        YawTransform.localRotation *= lerped;

                        m_YawLocalRotation *= Quaternion.Inverse(lerped);
                        AimYawTransform.localRotation = m_YawLocalRotation;
                    }
                }
            }
        }

        public void AddPitch(float p)
        {
            if (enabled)
                m_PendingPitch += p * 1f;//m_TurnRateMultiplier;
        }

        public void ResetPitchLocal()
        {
            //if (isValid)
            {
                pitch = Mathf.Clamp(0f, m_PitchLimitMin, m_PitchLimitMax);
                PitchTransform.localRotation = pitchLocalRotation;
            }
        }

        public void AddRotation(float y, float p)
        {
            //Debug.Log(string.Format("Yaw : {0} Pitch : {1}",height>0?"Right":"Left",p>0?"Up":"Down"));
            AddYaw(y);
            AddPitch(p);
        }

        //public void AddRotationInput(Vector2 input, Transform relativeTo)
        //{
        //    if (enabled)
        //    {
        //        // Get the corrected rotation
        //        Quaternion inputRotation = relativeTo.localRotation * Quaternion.Euler(input.height, input.width, 0f);
        //        Vector3 euler = inputRotation.eulerAngles;

        //        // Get the modified pitch & yaw (wrapped)
        //        float modifiedYaw = euler.height;
        //        if (modifiedYaw > 180f)
        //            modifiedYaw -= 360f;
        //        float modifiedPitch = euler.width;
        //        if (modifiedPitch > 180f)
        //            modifiedPitch -= 360f;

        //        // Get the vertical amount of the aimer (tilt has less effect closer to the vertical)
        //        float vertical = Mathf.Abs(Vector3.Dot(PitchTransform.forward, YawTransform.up));

        //        // Lerp between modified rotation and standard as it gets closer to vertical
        //        AddYaw(Mathf.Lerp(modifiedYaw, input.width, vertical));
        //        AddPitch(Mathf.Lerp(modifiedPitch, input.height, vertical));
        //    }
        //}

        public void SetYawConstraints(Vector3 center, float range)
        {
            if (range >= 360f)
            {
                ResetYawConstraints();
                return;
            }

            // Clamp the yaw limit
            m_YawLimit = Mathf.Clamp(range * 0.5f, 0f, 180f);

            m_YawConstraint = center;

            m_YawConstrained = true;
        }

        public void SetHeadingConstraints(Vector3 center, float range)
        {
            // Assert steering is enabled
            if (m_DisconnectAimFromYaw)
            {
                if (range >= 360f)
                {
                    ResetHeadingConstraints();
                    return;
                }

                // Clamp the heading limit
                m_HeadingLimit = Mathf.Clamp(range * 0.5f, 0f, 180f);

                m_HeadingConstraint = center;

                m_HeadingConstrained = true;
            }
            else
                Debug.LogError("Aim controller must have separate yaw and aim yaw transforms to constrain heading");
        }

        /// <summary>
        /// ???
        /// </summary>
        public void SetPitchConstraints(float min, float max)
        {
            m_PitchLimitMin = Mathf.Clamp(-max, -setting.Constraint.Rotation.Clamp_X.y, setting.Constraint.Rotation.Clamp_X.y);
            m_PitchLimitMax = Mathf.Clamp(-min, -setting.Constraint.Rotation.Clamp_X.y, setting.Constraint.Rotation.Clamp_X.y);
        }

        public void ResetYawConstraints()
        {
            m_YawConstrained = false;
        }

        public void ResetHeadingConstraints()
        {
            m_HeadingConstrained = false;
        }

        public void ResetPitchConstraints()
        {
            m_PitchLimitMin = setting.Constraint.Rotation.Clamp_X.x;
            m_PitchLimitMax = setting.Constraint.Rotation.Clamp_X.y;
        }

        #endregion



        #region Camera Perfect

        public Transform m_Head;

        /// <summary>
        /// Load head bone for camera rows
        /// </summary>
        protected virtual void Init_CameraPerfect()
        {
            m_Head = MotionSystem.Animator.GetBoneTransform(HumanBodyBones.Head);
            Assert.IsFalse(m_Head == null);
        }

        protected override void OnAfterControllerMove(PR pr)
        {
            //if (m_Head.NotNull())
            //{
            //    var dir = m_Head.position - HeightTransform.position;

            //    var additiveHeight = Vector3.Dot(dir, HeightTransform.up);

            //    var rows = HeightTransform.localPosition;

            //    rows.height = Mathf.Lerp(rows.height, rows.height + additiveHeight, ConstCache.deltaTime);

            //    HeightTransform.localPosition = rows;
            //}
        }



        #endregion



        #region ??

        [Obsolete]
        public static T LoadDefaultPreset<T>(string name = default) where T : UnityEngine.Object
        {
            if (string.IsNullOrEmpty(name))
                name = nameof(T);

            var target = Resources.Load<T>(name);
            Assert.IsNotNull(target);
            return target;
        }


        #endregion
    
        public class FirsPersonBodyCameraCache:ICoordinate
        {
            //Vector3 Offset => new (0, 0.03f, 0.3f);
            Vector3 Offset => new(0, 0.6f, 0.27f);

            public FirsPersonBodyCameraCache(Animator animator)
            {
                Root = animator.transform;
                Hips = animator.GetBoneTransform(HumanBodyBones.Hips);
                //var offset = animator.GetSkeleton(HumanBodyBones.Hips).position;
            }

            public Transform Hips;
            public Transform Root;

            public Space OutputPositionSpace { get ; set ; }
            public Space OutputRotationSpace { get ; set; }

            public Vector3 position => Hips.position+ Root.rotation* Offset;

            public Quaternion rotation => Root.rotation;

            public Vector3 forward { get; set; }

            public Vector3 up { get; set; }
        }
    }



    /// <summary>
    /// Callback interface for perfect camera adjust
    /// </summary>
    [Obsolete]
    public interface ICameraPerfect
    {
        



    }

}