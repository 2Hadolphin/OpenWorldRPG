using UnityEngine;
using Return;
using Return.Humanoid;
using Sirenix.OdinInspector;
using Return.Cameras;
using MxM;
using UnityEngine.Assertions;
using KinematicCharacterController;
using Return.Inputs;

namespace Return.Motions
{
    public class Turnround : MotionModule_Humanoid, IKCCAngularVelocityModule, IMxMExtension
    {
        #region Preset
        public override MotionModulePreset_Humanoid GetData => pref;
        public ControlModule_Turnround_UserInput pref { get; protected set; }

        public override void LoadData(MotionModulePreset data)
        {
            pref = data as ControlModule_Turnround_UserInput;
        }
        #endregion

        [ShowInInspector, KinematicCharacterController.ReadOnly]
        protected ReadOnlyTransform CamTransform;
        protected Transform Root;

        protected ICamWrapper camWrapper;

        #region Resolver

        [Inject]
        IMxMHandler IMxMHandler;

        [Inject]
        ICharaterMotionHandler MotionHandler;

        public override void SetHandler(IMotionSystem motionSystem)
        {
            base.SetHandler(motionSystem);
        }

        #endregion



        #region IKCCAngularVelocityModule

        public virtual void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            currentRotation = ModuleRotation;

            //Debug.Log($"FrameCount {Time.frameCount} Rotate {ModuleRotation.eulerAngles}");
        }

        #endregion


        #region Routine

        protected override void Register()
        {
            base.Register();

            Root = Motor.Transform;   
        }

        protected override void Activate()
        {
            base.Activate();

            ModuleRotation = Root.rotation;

            // add mxm extension
            IMxMHandler.mxm.SetExtension(this);

            MotionHandler.SetMotionModule(this);

            if (IMotions.isMine)
            {
                CameraManager.SubscribeCamera<FirstPersonCamera>(CameraStateChange);
            }
        }

        protected override void Deactivate()
        {
            // remove mxm extension
            IMxMHandler.mxm.RemoveExtension(this);

            if (IMotions.isMine)
            {
                CameraManager.OnCameraStateChange -= CameraStateChange;
            }

            base.Deactivate();
        }

        #endregion

        #region Parameter


        public float Sensitivity_H;
        public float Sensitivity_V;

        protected Quaternion ViewPortPlanarRotation;

        #endregion

        #region Catch


        protected Vector2 InputRot;
        protected float Rotate_Vertical;
        protected float Rotate_Horizontal;
        protected Vector3 Offset;

        #endregion


        protected virtual void CameraStateChange(CustomCamera cam, UTag type, bool enable)
        {
            if (enable)
            {
                camWrapper = cam;
                cam.SetTarget(Motor.Transform);
            }

            if (cam)
            {
                CamTransform = new WrapTransform(cam.mainCameraTransform);
            }
            else
            {
                CamTransform = new WrapTransform(Root);
            }


            Motions.ViewPortITransform = CamTransform;
        }

        // *Delta time<1 
        public float LerpParameter = 10;

        public override void SubscribeRountine(bool enable)
        {
            if (IMotions.isMine)
            {
                IMotions.UpdatePost -= Tick;

                if (enable)
                    IMotions.UpdatePost += Tick;
            }

        }

        protected virtual void Tick(float deltaTime)
        {
            if (ValidModuleMotion())
            {
                var grounded = Motions.GroundingStatus;

                if (!grounded.FoundAnyGround)
                    return;

                var up = Motor.CharacterUp;
                var camQuat = camWrapper.GetCamCorrectedRotation;

                var camfwd = camQuat * Vector3.forward;



                //var camdir = RootTransform.InverseTransformDirection(Vector3.ProjectOnPlane(CamTransform.forward, up)).normalized;

                // Calculate camera direction and rotation on the character plane
                var cameraPlanarDirection = Vector3.ProjectOnPlane(camfwd, up).normalized;

                Debug.DrawRay(Root.position + new Vector3(0, 0.5f), cameraPlanarDirection, Color.white);

                if (cameraPlanarDirection.sqrMagnitude == 0f)
                    cameraPlanarDirection = Vector3.ProjectOnPlane(camQuat * Vector3.up, up).normalized;

                var cameraPlanarRotation = Quaternion.LookRotation(cameraPlanarDirection, up);



                //Parameters.SetQuaternion(pref.ViewPortRotation, camQuat);
                //Parameters.SetVector3(pref.ViewPortDirection, camfwd);
                ViewPortPlanarRotation = cameraPlanarRotation;
                //Parameters.SetVector3(pref.ViewPortPlanarDirection, cameraPlanarDirection);

                //Debug.DrawRay(Root.position + Vector3.up, cameraPlanarDirection, Color.green);

                var motionSpeed = IMotions.MotionVelocity; //Parameters.GetVector3(pref.ModuleVelocity);

                Quaternion targetRotation;

                //if (motionSpeed.sqrMagnitude < 0.0005f)
                //{
                //    targetRotation = Parameters.GetQuaternion(pref.ModuleRotation);
                //    //Debug.LogError(Quaternion.Angle(targetRotation, RootTransform.rotation));
                //}
                //else

                //if (Vector3.Angle(camfwd, RootTransform.forward) < 3f)
                //{
                //    targetRotation = Quaternion.identity;
                //}
                //else
                //{
                //    targetRotation = cameraPlanarRotation;
                //}

                targetRotation = Quaternion.Lerp(Root.rotation, cameraPlanarRotation, ConstCache.deltaTime * RotationLerp);

                ModuleRotation = targetRotation;


                //Parameters.SetQuaternion(pref.ModuleRotation, targetRotation);
                //Parameters.SetVector3(pref.ModuleDirection, targetRotation * Vector3.forward);
            }
        }

        public float RotationLerp = 50f;

        #region Mxm

        public bool IsEnabled { get { return enabled; } }
        public bool DoUpdatePhase1 { get { return true; } }
        public bool DoUpdatePhase2 { get { return false; } }
        public bool DoUpdatePost { get { return false; } }

        private IMxMTrajectoryGenerator m_trajectoryGenerator;
        //[System.Obsolete]
        //private MxMTrajectoryGenerator m_trajectoryGenerator;

        [SerializeField]
        private float m_blendTime = 0.5f;
        [ShowInInspector]
        private float m_minAngleToTriggerTIP;

        private MxMEventDefinition m_eventDef;

        private float m_mxmBlendTime = 0.25f;

        private int CurrentTurnProfileId { get; set; }
        private int CurrentTurnEventHandle { get; set; }


        public void Initialize()
        {
            CurrentTurnProfileId = -1;

            m_mxmBlendTime = IMxMHandler.mxm.BlendTime;
            m_trajectoryGenerator = Motions.trajectoryGenerator;
            //Motions.CharacterRoot.InstanceIfNull(ref m_trajectoryGenerator);

            //RegisterHandler Event Definition
            m_eventDef = ScriptableObject.CreateInstance<MxMEventDefinition>();
            m_eventDef.Priority = -1;
            m_eventDef.ContactCountToMatch = 0;
            m_eventDef.ContactCountToWarp = 1;
            m_eventDef.EventType = EMxMEventType.Standard;
            m_eventDef.ExitWithMotion = true;
            m_eventDef.MatchPose = true;
            m_eventDef.MatchTrajectory = false;
            m_eventDef.PostEventTrajectoryMode = EPostEventTrajectoryMode.Reset;
            m_eventDef.MatchPosition = false;
            m_eventDef.MatchTiming = false;
            m_eventDef.MatchRotation = false;
            m_eventDef.RotationWarpType = EEventWarpType.Linear;
            m_eventDef.RotationWeight = 10f;
            m_eventDef.hideFlags = HideFlags.DontSave;
            //Go through each TIP profile, determine the minimum TIP trigger
            //And fetch Event UnityHandles for each profile
            m_minAngleToTriggerTIP = float.MaxValue;
            for (int i = 0; i < pref.m_turnInPlaceProfiles.Length; ++i)
            {
                ref TurnInPlaceProfile profile = ref pref.m_turnInPlaceProfiles[i];

                if (profile.TriggerRange.x < m_minAngleToTriggerTIP)
                    m_minAngleToTriggerTIP = profile.TriggerRange.x;

                profile.FetchEventHandles(IMotions.mxm);
            }
        }

        public void UpdatePhase1()
        {
            if (Motions.AlignViewMotion)
                return;

            m_trajectoryGenerator.ExtractGoal();

            if (CurrentTurnProfileId > -1) //A turn-in-place event is already playing
            {
                var moduleVelocity = IMotions.MotionVector; //Parameters.GetVector3(pref.ModuleVector);
                //Cancel the turn-in-place immediately if the player wants to move
                if (moduleVelocity.sqrMagnitude > 0.0001f)
                {
                    IMotions.mxm.ForceExitEvent();
                    CurrentTurnProfileId = -1;
                    IMotions.mxm.BlendTime = m_mxmBlendTime;
                }
                else
                {
                    //Reset the current turn profile to -1 if the turn in place has finished playing
                    if (!IMotions.mxm.CheckEventPlaying(CurrentTurnEventHandle))
                    {
                        CurrentTurnProfileId = -1;
                        IMotions.mxm.BlendTime = m_mxmBlendTime;
                    }

                    //TIP can begin from the The FollowThrough stage of another turn-in-place event
                    if (IMotions.mxm.CurrentEventState == EEventState.FollowThrough)
                        DetectAndTurn();
                }
            }
            else //TIP can begin from the IdleState state of MxM
            {
                if (IMotions.mxm.IsIdle)
                    DetectAndTurn();
            }
        }

        private void DetectAndTurn()
        {
            //We can only start turning if we are beyond the min turn threshold
            float rawAngle = ViewPortPlanarRotation.eulerAngles.y - Root.rotation.eulerAngles.y;
            Quaternion rotation = Quaternion.AngleAxis(rawAngle, Motor.CharacterUp);
            rawAngle = rotation.eulerAngles.y;

            rawAngle = rawAngle.WrapAngle();

            bool left = rawAngle < 0.0f;
            float angle = Mathf.Abs(rawAngle);

            if (angle < m_minAngleToTriggerTIP)
                return; //We don't search for TIP if it doesn't meet minimum requirements

            //We can now search for the appropriate TIP profile to use
            for (int i = 0; i < pref.m_turnInPlaceProfiles.Length; ++i)
            {
                ref TurnInPlaceProfile profile = ref pref.m_turnInPlaceProfiles[i];

                if (angle > profile.TriggerRange.x
                    && angle < profile.TriggerRange.y)
                {
                    //A turn profile match was found. Trigger it appropriately
                    CurrentTurnProfileId = i;

                    CurrentTurnEventHandle = left ? profile.LeftEventHandle : profile.RightEventHandle;

                    m_eventDef.Id = CurrentTurnEventHandle;
                    //if (profile.WarpTime)
                    //    m_eventDef.TimingWarpType = EEventWarpType.Linear;
                    //else
                    //    m_eventDef.TimingWarpType = EEventWarpType.None;

                    //m_eventDef.DesiredDelay = profile.DesiredTime;

                    if (profile.Warp)
                        m_eventDef.RotationWarpType = EEventWarpType.Linear;
                    else
                        m_eventDef.RotationWarpType = EEventWarpType.None;

                    IMotions.mxm.BlendTime = m_blendTime;
                    var tf = Motor.Transform;
                    m_eventDef.ClearContacts();
                    m_eventDef.AddEventContact(tf.position, tf.rotation.eulerAngles.y + rawAngle);
                    IMotions.mxm.BeginEvent(m_eventDef);

                    break;
                }
            }
        }

        public void Terminate() { }

        public void UpdatePhase2() { }

        public void UpdatePost() { }



        #endregion

    }
}