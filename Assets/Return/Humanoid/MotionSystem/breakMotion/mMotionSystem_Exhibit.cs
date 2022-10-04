//using UnityEngine;
//using UnityEngine.InputSystem;

//using System.Collections;
//using Return.Definition;
//using System;
//using Return.CentreModule;
//using System.ComponentModel;

//using Unity.Collections;
//using Unity.Jobs;
//using Return.Humanoid.Motion;
//using Return.Cams;

//namespace Return.Humanoid
//{
//    public  partial class mMotionSystem_Exhibit: BaseMotionSystem, ICam_Match_FP, Interface_RigState
//    {

//        #region Reference
//        protected CharacterController controller;
//        protected mPlayerInput Input;
//        protected testCamera _camera;
//        protected Camera_FirstPerson CamDir;
//        protected Camera_ThirdPerson CamDir_TP;
//        protected ReadOnlyTransform camTF;
//        protected ReadOnlyTransform _tf;
//        protected Anim_Humanoid ctc;
//        [SerializeField]
//        protected CapsuleCollider ColliderBounds = null;

//        #endregion

//        #region AbstractValue

//        #region Environment
//        [SerializeField]
//        protected float gravity = -9.8f;
//        // interface == ground material
//        #endregion

//        #region Physic
//        [SerializeField]
//        protected LayerMask GroundedMask = default;

//        #endregion



//        #region Character Setting
//        [Header("Character Behavior")]
//        /// <summary>
//        /// Character moving speed  Meter/Second. 
//        /// </summary>
//        protected float baseSpeed;
//        protected float addedSpeed;
//        protected float SpeedMultiple;
//        [SerializeField]
//        protected float _MovSpeed = 1.3f;
//        [SerializeField]
//        [Distance(0, 7)]
//        protected float _Speed_Walk = 2.5f;
//        [SerializeField]
//        [Distance(0, 12)]
//        protected float _Speed_Rush = 10;
//        /// <summary>
//        /// AbstractValue multiply by basic moving speed while character rush. 
//        /// </summary>
//        [SerializeField]
//        protected float RushPara = 1;//wait init
//        /// <summary>
//        /// Basic jump rows when character jump. 
//        /// </summary>
//        [SerializeField]
//        [Distance(0, 1.75f)]
//        protected float JumpHight = 0.67f;
//        /// <summary>
//        /// Time character take back to IK StartType while Input width&height both zero. 
//        /// </summary>
//        [SerializeField]
//        protected float CooldownTime = 0.3f;

//        public float LandHeight = 0.35f;

//        [SerializeField]
//        protected float TurnDegree;
//        public AnimationCurve TurnAroundMap;
//        #endregion



//        public CameraSpace ViewType = CameraSpace.ThirdPerson;
//        [SerializeField]
//        protected CamMatchData _camset;
//        CamMatchData ICam_Match_FP.CameraSetting { get { return _camset; } set { _camset = value; } }
//        /// <summary>
//        /// Time during Align Cam with Character
//        /// </summary>
//        public float ForceAlignCamTime = 0.3f;

//        [Header("Routine Setting")]
//        [SerializeField]
//        [Distance(0, float.MaxValue)]
//        protected float updateRateTime = 0.2f;
//        #endregion



//        #region StateCatch
//        [SerializeField]
//        protected bool FreeCam;
//        protected float FwdDotMovdir;
//        protected Vector2 inputMov;
//        protected Vector3 MovDir_Local = Vector3.zero;
//        protected Vector3 MovDir_Global = Vector3.zero;
//        protected Vector3 MovVelocity_Global = Vector3.zero;
//        protected float MovSpeed = 0;
//        [SerializeField]
//        protected bool controlable = false;//while controlable as grounded as attach to wall 
//        public bool DuringInteract;

//        [System.ComponentModel.ReadOnly(true)]
//        protected Vector3 LocalAcceleration;

//        protected Vector3 MovPos = Vector3.zero;
//        protected Vector3 FinalMovPos = Vector3.zero;
//        protected Vector3 InertiaDir = Vector3.zero;
//        protected int JumpCount = 0; //Skill double Jump


//        [SerializeField]
//        protected bool Stepping = false;


//        [SerializeField]
//        protected bool Grounded;
//        protected bool Control_Grounded_Collider
//        {
//            set
//            {
//                if (Grounded.Equals(!value))
//                {
//                    Control_Grounded = value;
//                }
//            }
//        }
//        protected bool Grounded_Cast;
//        protected bool Grounded_Foot;

//        #region ObsoleteMotionState
//        [SerializeField]
//        [System.ComponentModel.ReadOnly(true)]
//        protected bool
//            s_Locomotion,
//            s_Retreat,
//            s_Sprint,
//            s_Squat,
//            s_Climb,
//            s_Jump,
//            s_Fall,
//            s_Rebalance,
//            s_Lie = false,
//            s_Operate;

//        #endregion



//        protected bool MotionEdited = false;
//        protected virtual void EditMotion(Motion_Main motion, bool Enable)
//        {
//            /*
//            if (Motion.HasFlag(motion) ^ Enable)
//                Motion ^= motion;
//            */
//            if (Enable)
//            {
//                if (!Motion.HasFlag(motion))
//                {
//                    Motion = motion;
//                }
//                else
//                {
//                    return;
//                }

//            }
//            else//disable
//            {
//                switch (motion)
//                {
//                    case Motion_Main.Null:
//                        break;
//                    case Motion_Main.Locomotion:
//                        break;
//                    case Motion_Main.Climb:
//                        break;
//                    case Motion_Main.Lie:
//                        break;
//                    case Motion_Main.Jump:
//                        break;
//                    case Motion_Main.Fall:
//                        Motion = Motion_Main.Rebalance;

//                        break;

//                    case Motion_Main.Operate:
//                        break;
//                }
//            }


//            MotionEdited = true;

//        }

//        [SerializeField]
//        protected Motion_Main Motion;
//        public event Action<Motion_Main> Event_Motion_Main;

//        [SerializeField]
//        protected Motion_Foot motion_Foot;
//        public event Action<Motion_Foot> Event_Motion_Foot;
//        protected virtual void EditMotion(Motion_Foot motion, bool Enable)
//        {
//            if (motion_Foot.HasFlag(motion) ^ Enable)
//                motion_Foot ^= motion;

//            if (motion_Foot.HasFlag(Motion_Foot.Walk) && !s_Squat)
//                motion = Motion_Foot.Walk;
//            else if (s_Squat)
//                motion = Motion_Foot.Squat;
//            else
//                motion = Motion_Foot.Null;

//            Event_Motion_Foot?.Invoke(motion);
//        }
//        protected bool Control_Squat
//        {
//            set
//            {
//                if (!s_Squat.Equals(value))
//                {
//                    s_Squat = value;
//                    Event_Squat?.Invoke(value);
//                    EditMotion(Motion_Foot.Squat, value);
//                }
//            }
//        }
//        protected bool Control_Sprint
//        {
//            set
//            {
//                if (!s_Sprint.Equals(value))
//                {
//                    s_Sprint = value;
//                    Event_Sprint?.Invoke(value);
//                }
//            }
//        }
//        protected bool Control_Move
//        {
//            set
//            {
//                if (!s_Locomotion.Equals(value))
//                {
//                    s_Locomotion = value;
//                    Event_Moving?.Invoke(value);
//                    EditMotion(Motion_Main.Locomotion, value);
//                    EditMotion(Motion_Foot.Walk, value);
//                }
//            }
//        }
//        protected bool Control_Jump
//        {
//            set
//            {
//                if (!s_Jump.Equals(value))
//                {
//                    if (s_Locomotion)//jump prepare
//                    {
//                        Event_Jumping?.Invoke(value);
//                        EditMotion(Motion_Main.Jump, value);
//                    }
//                    else
//                    {
//                        Event_Jumping?.Invoke(value);
//                        EditMotion(Motion_Main.Jump, value);
//                    }

//                    if (value && true/* jump while anim call back*/)
//                    {
//                        //Jump_Up();
//                    }
//                    s_Jump = true;
//                }
//            }
//        }
//        protected bool Control_Fall
//        {
//            set
//            {
//                if (!s_Fall.Equals(value))
//                {
//                    s_Fall = value;
//                    Event_Falling?.Invoke(value);
//                    EditMotion(Motion_Main.Fall, value);
//                }

//            }
//        }
//        protected bool Control_Rebalance
//        {
//            set
//            {
//                if (!s_Rebalance.Equals(value))
//                {
//                    s_Rebalance = value;

//                    if (value)
//                    {
//                        SpeedMultiple *= 0.7f;
//                    }
//                    else
//                    {
//                        SpeedMultiple /= 0.7f;
//                    }

//                    EditMotion(Motion_Main.Rebalance, value);
//                }
//            }
//        }
//        protected bool Control_Grounded
//        {
//            set
//            {
//                Grounded = value;
//                Event_Grounded?.Invoke(value);
//                if (value)
//                    EditMotion(Motion_Main.Locomotion, value);
//            }
//        }


//        #region performed
//        public event Action<Vector3> Direction_Loacl, Direction_Global, Velocity_Local;
//        public event Action<float> Speed;
//        public event Action<float> Event_RotateDegree;
//        public event Action<float> Event_Preflutter;
//        // why spread
//        public event Action<bool> Event_Grounded, Event_Moving, Event_Squat, Event_Sprint, Event_Jumping, Event_Falling;


//        protected event Action<Vector3> Interaction;


//        /// <summary>
//        /// Using to update and post Animation state.
//        /// </summary>
//        #endregion


//        #region Routine


//        //Control
//        [SerializeField]
//        protected bool Bool_Routine_Movement;
//        protected bool Control_Movement
//        {
//            get { return Bool_Routine_Movement; }
//            set
//            {
//                if (value)
//                {
//                    if (!Bool_Routine_Movement)
//                        StartCoroutine(MovementRoutine_TP());
//                }
//                else
//                {
//                    if (Bool_Routine_Movement)
//                        Bool_Routine_Movement = false;
//                }
//            }
//        }

//        protected bool Bool_Routine_RigSensor;
//        protected bool Control_RigSensor
//        {
//            get { return Bool_Routine_RigSensor; }
//            set
//            {
//                if (value)
//                {
//                    if (!Bool_Routine_RigSensor)
//                        StartCoroutine(RigSensor());
//                }
//                else
//                {
//                    Bool_Routine_RigSensor = false;
//                }
//            }
//        }

//        protected bool Control_UsualPhysic
//        {
//            set
//            {
//                if (value)
//                {

//                }
//                else
//                {

//                }
//            }
//        }

//        protected bool Bool_Routine_AlignDirection;
//        protected bool Control_AlignDirection
//        {
//            set
//            {
//                if (value)
//                {
//                    if (!Bool_Routine_AlignDirection)
//                        StartCoroutine(AlignDirection_Cam());

//                }
//                else
//                {
//                    if (Bool_Routine_AlignDirection)
//                        Bool_Routine_AlignDirection = false;
//                }
//            }
//        }

//        #endregion

//        #region OnSensorUpdate

//        protected Coroutine Jumper;

//        #endregion


//        protected Coroutine HasAnimateStep;


//        #endregion


//        #region PublicCatch
//        public ReadOnlyTransform root { get { if (_tf == null) return null; else return _tf; } }
//        public CharacterController getController { get { return controller; } }
//        public Camera Cam { get { return _camera.Cam; } }

//        public bool GetState(Motion_Main motion_Main)
//        {
//            switch (motion_Main)
//            {
//                case Motion_Main.Null:
//                    if (Motion.Equals(motion_Main))
//                        return true;
//                    break;

//                case Motion_Main.Locomotion:
//                    return s_Locomotion;
//                case Motion_Main.Climb:
//                    return s_Climb;
//                case Motion_Main.Lie:
//                    return s_Lie;
//                case Motion_Main.Jump:
//                    return s_Jump;
//                case Motion_Main.Fall:
//                    return s_Fall;
//                case Motion_Main.Operate:
//                    return s_Operate;
//            }

//            return false;
//        }


//        protected void MotionEditedCheck()
//        {
//            if (MotionEdited)
//                MotionEdited = false;
//            else
//                return;

//            if (Motion.HasFlag(Motion_Main.Null))
//            {
//                if (s_Jump)
//                {
//                    Motion = Motion_Main.Jump;
//                    s_Jump = false;
//                }
//                else if (s_Fall)
//                {
//                    Motion = Motion_Main.Fall;
//                }
//                else if (s_Climb)
//                {
//                    Motion = Motion_Main.Climb;
//                }
//                else if (s_Lie)
//                {
//                    Motion = Motion_Main.Lie;
//                }
//                else if (s_Operate)
//                {
//                    Motion = Motion_Main.Operate;
//                }
//                else if (s_Locomotion)
//                {
//                    Motion = Motion_Main.Locomotion;
//                }
//                else
//                    Motion = Motion_Main.Null;
//            }

//            Event_Motion_Main?.Invoke(Motion);
//        }
//        #endregion




//        #region Mono
//        private void OnDisable()
//        {
//            Controlable = false;

//            StopAllCoroutines();
//        }
//        #endregion

//        public  void Init(HumanoidAgent_Exhibit agent_)
//        {


//            #region Set Reference

//            _tf = transform.parent;

//            controller = _tf.CharacterRoot.GetComponent<CharacterController>();

//            Input = InputManager.Inputs;


//            //ctc = Agent.I.Animate;


//            //_camera = Agent.O.Camera;

//            if (ViewType.Equals(CameraSpace.FirstPerson))
//                CamDir = _camera as Camera_FirstPerson;

//            if (ViewType.Equals(CameraSpace.ThirdPerson))
//                CamDir_TP = _camera as Camera_ThirdPerson;

//            print(ConstCache.PrimeCamDirector);

//            //_camera.SetHandler(Input.Humanoid);

//            CamDir?.LoadSetting(_camset);

//            camTF = Cam.transform;


//            #endregion


//            #region Set Preset

//            LocalAcceleration.height = -2.0f;
//            controller.detectCollisions = false;


//            #endregion
   
//            LockCamera = false;

//            #region ChildSystem

//            #endregion

//            #region SetCoroutine

//            /*
//            if (c_rotating == null)
//                c_rotating = StartCoroutine(RotateEnumerator());
//                */

//            #endregion
//            SetActivate(ActiveState.Control);
//            Input.Enable();
//            Cursor.lockState = CursorLockMode.Locked;


//            Input.Humanoid.Movement.performed += CheckMovement;//debug
//            //??
//        }
//        [Flags]
//        public enum ActiveState { Preset = 0, Physic = 1, Control = 2 }
//        public virtual void SetActivate(ActiveState state)
//        {
//            int lvl = (int)state;



//            if (state.HasFlag(ActiveState.Control))
//            {

//                Control_RigSensor = true;
//                Control_Movement = true;
//                // Control_UsualPhysic = true;

//                // root_rotator.Init(Vector3.up);
//                // Event_FloatX += root_rotator.LogValue;
//                //CamDir.Overflow_Pass_pos += Overflow_pos;


//                Controlable = true;
//            }
//            else
//            {
//                Control_Movement = false;

//                Control_RigSensor = false;

//            }


//            //GDR.data.MonoModule.controllerCentre.CursorPost += ControllerCentre_CursorActivate;
//            //Controlable = false;
//        }

//        internal void SetMove(bool v)
//        {
//            throw new NotImplementedException();
//        }

//        public virtual void Overflow_pos(Vector3 pos)
//        {

//        }
//        protected bool Turing = false;
//        public virtual void Overflow_rot_Free(Vector3 rot)
//        {
//            if (Turing)
//            {
//                _tf.Rotate(_tf.up, rot.height);
//                return;
//            }


//            if (rot.height > 0)
//            {
//                if (TurnDegree < 0)
//                    TurnDegree *= -1;
//            }
//            else if (rot.height < 0)
//            {
//                if (TurnDegree > 0)
//                    TurnDegree *= -1;
//            }
//            else
//            {
//                //print("InClamp");
//                return;
//            }
//            //ReOffset-> Cam View
//            //start turn routine

//            //AnimPost?.Invoke(this, new AnimArg_Humanoid_Locomotion(AnimArg_Humanoid_Locomotion.PostType.Turn, true));
//        }

//        Quaternion TurnCatch;
//        protected virtual void TurnCallback(float p)
//        {
//            var last = Quaternion.Inverse(TurnCatch);

//            p = TurnAroundMap.Evaluate(p);
//            TurnCatch = Quaternion.Lerp(Quaternion.identity, Quaternion.Euler(0, TurnDegree, 0), p);
//            last *= TurnCatch;
//            //print(last.eulerAngles.height);
//            _tf.localRotation *= last;

//            var callback = new Vector3(last.eulerAngles.width.WrapAngle(), last.eulerAngles.height.WrapAngle());
//            CamDir.InteractionCallback(callback);
//            ctc.ikManager.Head_IK.Callback(callback.height);

//            Event_RotateDegree?.Invoke(Mathf.Sign(TurnDegree));
//        }

//        public virtual void Overflow_rot_Lock(Vector3 rot)
//        {
//            rot.width = 0;
//            _tf.rotation *= Quaternion.Euler(rot);
//        }

//        private void ControllerCentre_CursorActivate(bool LockCam)
//        {
//            if (FreeCam)
//            {
//                //Event_Vector3.Invoke(Vector3.zero);

//            }
//            else
//            {
//                CamDir.SetCamAxisClamp(
//                    new Vector3(0, _camset.Rotation.Clamp_Y.width),
//                    new Vector3(0, _camset.Rotation.Clamp_Y.height)
//                    );
//            }


//        }




//        #region mPlayerInput


//        protected void Movement(InputAction.CallbackContext ctx)
//        {
//            inputMov = ctx.ReadValue<Vector2>();
//        }
//        protected void Sprint_Start(InputAction.CallbackContext ctx)
//        {
//            Control_Sprint = true;
//            RushPara = 2;
//        }
//        protected void Sprint_Stop(InputAction.CallbackContext ctx)
//        {
//            Control_Sprint = false;
//            RushPara = 1;
//        }
//        protected void Jump(InputAction.CallbackContext ctx)
//        {

//            if (Grounded && !s_Jump)
//            {
//                Control_Jump = true;
//            }
//            else if (JumpCount > 0 && !s_Jump)
//            {
//                JumpCount -= 1;
//            }
//            else
//                return;
//        }
//        protected void Squat_Start(InputAction.CallbackContext ctx)
//        {

//        }
//        protected void Squat_Stop(InputAction.CallbackContext ctx)
//        {

//        }

//        protected virtual void CheckMovement(InputAction.CallbackContext ctx)
//        {
//            if (!s_Locomotion)
//            {
//                Stepping = true;
//                //StartCoroutine(SetStep());
//                Control_Movement = true;
//                Control_RigSensor = true;

//                //AlignMoventDircetion();
//            }

//            Input.Humanoid.Movement.performed -= CheckMovement;
//        }



//        #region Rountine
//        /*
//        /// <summary>
//        ///  Only Detect Flat Mesh 
//        /// </summary>
//        /// <returns></returns>
//        protected IEnumerator GravityRoutine()
//        {
//            Ray ray = new Ray();
//            RaycastHit hit;

//            for (; ; )
//            {
//                if (DuringInteract) // Climb wall/tree/lader etc
//                    continue;

//                var fixedTime = ConstCache.fixeddeltaTime;
//                ray.origin = _tf.position;
//                ray.direction = Vector3.down;

//                if (Physics.OnSensorUpdate(ray, out hit, float.MaxValue, GroundedMask, QueryTriggerInteraction.Ignore))//form spine bone or root tf?
//                {
//                    //analyze collider is static or interactable as rigable and get material

//                    if (hit.distance > 0.2f)//float.Epsilon
//                        Grounded_Cast = false;
//                    else if (!Grounded_Cast)
//                        Grounded_Cast = true;
//                }
//                else
//                {
//                    ray.origin += Vector3.up * 1.5f;
//                    if (Physics.OnSensorUpdate(ray, out hit, float.MaxValue, GroundedMask, QueryTriggerInteraction.Ignore))
//                    {
//                        _tf.localPosition += new Vector3(0, hit.distance + 0.05f);
//                        LocalAcceleration.height = Physics.gravity.height;
//                    }
//                    else
//                    {
//                        Falling = true;
//                        //StartCoroutine(OffGroundSensor());
//                        if (Grounded_Cast)
//                            Grounded_Cast = false;
//                    }


//                    //logError
//                }

//                if (!Grounded_Cast)
//                    LocalAcceleration.height += gravity * fixedTime;

//                if (!Grounded_Cast.Equals(lastGrounded))
//                {
//                    lastGrounded = Grounded_Cast;
//                    Event_Grounded?.Invoke(lastGrounded);
//                }

//                yield return ConstCache.WaitForFixedUpdate;
//            }
//        }
//        */
//        /*
//        protected IEnumerator SetStep()
//        {
//            //test
//            float stepTime = refs.I.IK.IK_Foot.StepTime;

//            //StartCoroutine(Routine_Step(stepTime));
//            //cal mov distance-overlap space

//            float Stepdistance = _MovSpeed * stepTime;
//            Vector3 movDir = CalMovDir();
//            Vector3 capsuleBase = _tf.position + movDir * Stepdistance;
//            Vector3 capsuleTop = capsuleBase + controller.rows * _tf.up;

//            if (Physics.CapsuleCast(capsuleBase, capsuleTop, controller.radius, movDir, out var hit, Stepdistance, GroundedMask))
//            {
//                Stepdistance = hit.distance;
//            }

//            // refs.I.IK.IK_Foot.TakeStep(Stepdistance, MovSpeed, new Callback(StepEnd_StartWalk));
//            StepEnd_StartWalk();
//            yield break;
//        }

//        protected virtual void StepEnd_StartWalk()
//        {
//            Stepping = false;

//            if (State_Squat)
//            {
//                ctc.SetFootState(Anim_Humanoid.ACList_Foot.Squat);
//            }
//            else if (State_Lie)
//            {
//                ctc.SetFootState(Anim_Humanoid.ACList_Foot.Lie);
//            }
//            else if (State_Climb)
//            {
//                ctc.SetFootState(Anim_Humanoid.ACList_Foot.Climb);
//            }
//            else
//                ctc.SetFootState(Anim_Humanoid.ACList_Foot.Walk);

//        }
//        */
//        /// <summary>
//        /// Using this during Moving "After Stepping End"
//        /// </summary>
//        /// <returns></returns>

//        protected IEnumerator MovementRoutine_TP()
//        {
//            Bool_Routine_Movement = true;

//            while (Bool_Routine_Movement)
//            {
//                if (Grounded)
//                {
//                    CalMov_Axis();

//                    if (FreeCam)
//                        CalMov_Free_TP();
//                    else
//                        CalMov_Lock_TP();

//                    FinalMov(MovPos);
//                }
//                else
//                    FinalMov(InertiaDir);

//                yield return null;
//            }

//            yield break;
//        }
//        #endregion

//        protected virtual void Cal_Final(Vector3 movPos)
//        {
//            movPos *= _MovSpeed * RushPara;
//            movPos += _tf.TransformVector(LocalAcceleration);
//            controller.Move(movPos.Multiply(ConstCache.deltaTime));

//            var v = _tf.InverseTransformVector(movPos);

//            //ctc.ApplyMotion(new Vector2(v.width, v.z));

//            controller.Move(v.Multiply(ConstCache.deltaTime));
//        }

//        public void MotionMov(Vector3 v)
//        {
//            //controller.Move(v.Multiply(ConstCache.deltaTime));
//            var s = 1f / Vector3.Dot(MovVelocity_Global, v);
//            //ctc.ApplyMotionSpeed();

//        }
//        public void MotionRot(Vector3 v)
//        {
//            _tf.Rotate(v.Multiply(ConstCache.deltaTime));
//        }

//        protected virtual void FinalMov(Vector3 movPos)
//        {
//            FinalMovPos = (movPos * (_MovSpeed * RushPara) + LocalAcceleration) * ConstCache.deltaTime;

//            if (controller.Move(FinalMovPos).HasFlag(CollisionFlags.Below))
//            {
//                if (!Grounded)
//                {
//                    LocalAcceleration.height = Physics.gravity.height;
//                    Control_Grounded_Collider = true;
//                }
//            }
//            else
//            {
//                if (Grounded)
//                {
//                    Control_Grounded_Collider = false;
//                }
//            }

//        }

//        #region Mission
//        /// <summary>
//        /// NEED GO protected with event call
//        /// </summary>
//        public virtual void Jump_Up()
//        {
//            LocalAcceleration.height = Mathf.Sqrt(JumpHight * 2 * -gravity);
//            //LocalAcceleration.height = Mathf.Sqrt((1 + JumpHight) * -2 * gravity);
//            InertiaDir = MovPos;
//            s_Jump = true;
//        }


//        protected IEnumerator AlignDirection_Cam()
//        {
//            Bool_Routine_AlignDirection = true;


//            while (Bool_Routine_AlignDirection)
//            {
//                if (Grounded)
//                    _tf.LookAt(_tf.position + Vector3.Lerp(_tf.forward, cam_Forward, ConstCache.deltaTime * 5), _tf.up);

//                yield return null;
//            }

//            Bool_Routine_AlignDirection = false;
//            yield break;
//        }
//        protected IEnumerator AlignDirection_Mov()
//        {
//            Bool_Routine_AlignDirection = true;
//            s_Retreat = true;
//            while (s_Retreat && Bool_Routine_AlignDirection)
//            {
//                if (Grounded)
//                    _tf.LookAt(_tf.position + Vector3.Lerp(_tf.forward, -MovPos, ConstCache.deltaTime * FwdDotMovdir), _tf.up);
//                yield return null;
//            }

//            s_Retreat = false;
//            Bool_Routine_AlignDirection = false;
//        }
//        #endregion




//        #endregion

//        #region DebugGUI
//        private void OnGUI()
//        {
//            System.Text.StringBuilder sb = new System.Text.StringBuilder();
//            sb.Append(Motion.ToString());
//            sb.Append("\n");
//            sb.Append("Grounded : " + Grounded.ToString());
//            sb.Append("\n");
//            sb.Append("RigSensor : " + Bool_Routine_RigSensor.ToString());
//            sb.Append("\n");
//            sb.Append("s_Locomotion : " + s_Locomotion.ToString());
//            sb.Append("\n");
//            sb.Append("s_Jump : " + s_Jump.ToString());
//            sb.Append("\n");
//            sb.Append("RigSpeed : " + LocalAcceleration.height.ToString());


//            GUILayout.Box(sb.ToString());
//        }
//        #endregion

//        #region Movement



//        /*
//        protected virtual void CalMov_Free_FP()
//        {
//            cam_Forward = Vector3.ProjectOnPlane(camTF.forward, Vector3.up).normalized;

//            MovPos = camTF.right * inputMov.width + cam_Forward * inputMov.height;
//        }
//        protected virtual void CalMov_Lock_FP()
//        {
//            MovPos = _tf.right * inputMov.width + _tf.forward * inputMov.height;
//        }
//        */

//        protected Vector3 cam_Forward;
//        protected Vector3 cam_Right;
//        protected virtual void CalMov_Axis()
//        {
//            cam_Forward = Vector3.ProjectOnPlane(camTF.forward, Vector3.up).normalized;
//            cam_Right = Vector3.ProjectOnPlane(camTF.right, Vector3.up).normalized;
//        }
//        protected virtual void CalMov_Free_TP()
//        {
//            MovPos = cam_Right * inputMov.width + cam_Forward * inputMov.height;
//        }
//        protected virtual void CalMov_Lock_TP()
//        {
//            MovPos = cam_Right * inputMov.width + cam_Forward * inputMov.height;
//        }




//        /// <summary>
//        /// move character by time and Return Forecast Finish Point
//        /// </summary>
//        /// <param name="lastTime"></param>
//        /// <returns></returns>


//        protected Vector3 CalMovDir()
//        {
//            if (FreeCam)
//                return camTF.right * inputMov.width + camTF.forward * inputMov.height;
//            else
//                return _tf.right * inputMov.width + _tf.forward * inputMov.height;
//        }
//        protected Vector2 CalMovDir2D()
//        {
//            return CalMovDir().Z2Y();
//        }


//        #endregion

//        #region Rotate


//        protected void overflowRotY(float rot)
//        {
//            Event_RotateDegree?.Invoke(rot);
//        }




//        #endregion

//        #region PhysicConstraint



//        #endregion

//        #region Controll Windows
//        public bool LockCamera
//        {
//            get
//            {
//                return !FreeCam;
//            }

//            set
//            {
//                if (FreeCam == !value)
//                    return;
//                else
//                    FreeCam = !value;

//                print("ChangeCameraMode to  " + (value == true ? "Lock" : "Free") + ViewType);
//                if (ViewType.Equals(CameraSpace.ThirdPerson))
//                {
//                    if (value)
//                    {
//                        Control_AlignDirection = true;
//                    }
//                    else
//                    {
//                        Control_AlignDirection = false;
//                    }
//                }
//                else if (ViewType.Equals(CameraSpace.FirstPerson))
//                {

//                    if (value)//Lock
//                    {
//                        //unlock
//                        CamDir.Overflow_Pass_rot -= Overflow_rot_Free;

//                        //set new
//                        CamDir.LockAxis_Y = true;
//                        CamDir.FlowAll = true;
//                        CamDir.SetCamAxisClamp(new Vector3(_camset.Rotation.Clamp_X.width, 0), new Vector3(_camset.Rotation.Clamp_X.height, 0));
//                        CamDir.Overflow_Pass_rot += Overflow_rot_Lock;

//                    }
//                    else//Free
//                    {
//                        //unlock
//                        CamDir.Overflow_Pass_rot -= Overflow_rot_Lock;

//                        //set new
//                        CamDir.LockAxis_Y = false;
//                        CamDir.FlowAll = false;
//                        CamDir.SetCamAxisClamp(_camset.Rotation.Clamp_Min, _camset.Rotation.Clamp_Max);
//                        CamDir.Overflow_Pass_rot += Overflow_rot_Free;
//                    }
//                }

//            }
//        }



//        protected bool IsCamAlign
//        {
//            get
//            {
//                float degree = CamDir.Deviation.height;
//                if (degree < 3 && degree > -3)
//                    return true;
//                return false;
//            }
//        }
//        public bool Controlable
//        {
//            get
//            {
//                return controlable;
//            }
//            set
//            {
//                if (Input == null)
//                    return;
//                if (value)
//                {
//                    Input.Enable();
//                    Input.Humanoid.Movement.performed += Movement;
//                    Input.Humanoid.Rush.performed += Sprint_Start;
//                    Input.Humanoid.Rush.canceled += Sprint_Stop;
//                    Input.Humanoid.Jump.performed += Jump;
//                    Input.Humanoid.Squat.performed += Squat_Start;
//                    Input.Humanoid.Squat.canceled += Squat_Stop;
//                }
//                else
//                {
//                    /*
//                    Input.Player.Movement.performed -= Movement;
//                    Input.Player.Rush.performed -= Rush;
//                    Input.Player.Rush.canceled -= RushEnd;
//                    Input.Player.Jump.performed -= Jump;
//                    */
//                    Input.Disable();
//                }
//            }
//        }

//        #region OpenTake
//        Vector3 Interface_RigState.VelocityDirection_Global { get { return MovDir_Global; } }

//        Vector3 Interface_RigState.VelocityDirection_Local { get { return MovDir_Local; } }

//        float Interface_RigState.ModuleVelocity { get { return MovSpeed; } }

//        bool Interface_RigState.IsMoving { get { return s_Locomotion; } }

//        Vector3 Interface_RigState.NormalizeVelocity_Local { get { return MovDir_Local; } }



//        #endregion

//        public void AutoRotate(float yrot, float xrot)  //??
//        {
//            //Rotate(yrot, xrot);
//            //print(xrot);
//        }

//        public void QuickTeleport(Vector3 newPos)
//        {
//            controller.Move(newPos);
//        }

//        IEnumerator Interface_RigState.ForecastPoint_Frame(float DuringTime, Injector.m_Value<Vector3> post)
//        {
//            for (; DuringTime > 0; DuringTime -= ConstCache.deltaTime)
//            {
//                post?.Invoke(_tf.position + MovDir_Global * (MovSpeed * DuringTime));
//                yield return null;
//            }

//            post?.Invoke(_tf.position);
//            yield break;
//        }

//        IEnumerator Interface_RigState.ForecastPoint_Physic(float DuringTime, Injector.m_Value<Vector3> post)
//        {
//            for (; DuringTime > 0; DuringTime -= ConstCache.fixeddeltaTime)
//            {
//                post?.Invoke(_tf.position + MovDir_Global * (MovSpeed * DuringTime));
//                yield return ConstCache.WaitForFixedUpdate;
//            }
//            post?.Invoke(_tf.position);
//            yield break;
//        }

//        IEnumerator Interface_RigState.ForecastPoint_Time(float DuringTime, float Rate, Injector.m_Value<Vector3> post)
//        {
//            var t = new WaitForSeconds(Rate);
//            for (; DuringTime > 0; DuringTime -= Rate)
//            {
//                post?.Invoke(_tf.position + MovDir_Global * (MovSpeed * DuringTime));
//                yield return t;
//            }
//            post?.Invoke(_tf.position);
//            yield break;
//        }

//        Vector3 Interface_RigState.ForecastPoint(float Time)
//        {
//            return _tf.position + MovDir_Global * Time;
//        }

//        #endregion




//        #region Debug


//        #endregion


//        protected IEnumerator RigSensor()
//        {

//            Bool_Routine_RigSensor = true;
//            int framecut = 0;
//            while (Bool_Routine_RigSensor)// execute while moving
//            {
//                var v = MovPos;

//                #region cooldown

//                if (v.Equals(Vector3.zero))
//                {
//                    if (s_Locomotion && framecut < 8)
//                    {
//                        if (framecut < 0)
//                            framecut = 0;
//                        else
//                            framecut++;
//                    }
//                    else if (s_Locomotion)
//                    {
//                        Control_Move = false;
//                        LockCamera = false;
//                        framecut = 0;
//                    }
//                }
//                else if (Grounded && !s_Locomotion)//move
//                {
//                    Control_Move = true;
//                    LockCamera = true;
//                }

//                #endregion


//                v = controller.velocity;

//                #region Col_RigState

//                if (!Grounded)
//                    LocalAcceleration.height += Physics.gravity.height * ConstCache.deltaTime;

//                if (s_Jump)//jumping up
//                {
//                    if (v.height < float.Epsilon&&LocalAcceleration.height<float.Epsilon) //start droping
//                    {
//                        Control_Jump = false;
//                        Control_Fall = true;
//                    }
//                }
//                else if (s_Fall)//falling down
//                {
//                    var max = Physics.gravity.height * 3f;
                    
//                    if (Physics.OnSensorUpdate(_tf.position, v, out var hit,max.Clamp(0.3f,3f), GroundedMask)) // prepare to land
//                    {
//                        if (hit.distance < LandHeight)
//                        {
//                            Control_Fall = false;
//                            Control_Rebalance = true;
//                        }

//                    }
//                }

//                #endregion

//                #region Preset performed
//                var norDir = v.normalized;
//                if (!MovDir_Global.Equals(norDir))//has change
//                {
//                    MovVelocity_Global = v;

//                    MovDir_Global = norDir;
//                    Direction_Global?.Invoke(MovDir_Global);
//                    MovDir_Local = _tf.InverseTransformDirection(norDir);
//                    Direction_Loacl?.Invoke(MovDir_Local);


//                    //yield return null;
//                    MovSpeed = v.magnitude;
//                    Speed?.Invoke(MovSpeed);
//                    Velocity_Local?.Invoke(_tf.InverseTransformVector(v));


//                    #region Align StartDirection
//                    if (MovSpeed > _Speed_Walk)
//                    {
//                        FwdDotMovdir = Vector3.Dot(_tf.forward, MovDir_Global) * -5f;
//                        if (FwdDotMovdir > 0)
//                        {
//                            if (!Bool_Routine_AlignDirection)
//                                StartCoroutine(AlignDirection_Mov());
//                        }
//                        else
//                            s_Retreat = false;
//                    }
//                    else if (s_Retreat)
//                        s_Retreat = false;
//                    #endregion
//                }

//                #endregion

//                yield return null;

//                MotionEditedCheck();
//             }

//            Control_Movement = false;
//            LockCamera = false;

//            Input.Humanoid.Movement.performed += CheckMovement;

//            yield break;
//        }


//    }
//}