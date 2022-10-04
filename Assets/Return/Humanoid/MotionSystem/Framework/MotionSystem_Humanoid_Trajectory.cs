using UnityEngine;
using Unity.Collections;
using Unity.Jobs;
using System.Collections.Generic;
using MxM;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Sirenix.OdinInspector;
using Return.Timers;

namespace Return.Motions
{
    public partial class MotionSystem_Humanoid /*MxM-Trajectory*/:IMxMTrajectory, IMxMTrajectoryGenerator
    {
        public IMxMTrajectoryGenerator trajectoryGenerator;

        public Vector3 InputVector { get; set; }

        //[SerializeField]
        //protected AbstractValue<Vector3> m_StrafeDirection { get; set; }
        //public Parameters<Vector3> StrafeDirection => m_StrafeDirection;

        public Vector3 StrafeDirection
        {
            get; set;
            //get => Parameters.GetVector3(Preset.ViewPortPlanarDirection);
            //set => Parameters.SetVector3(Preset.ViewPortPlanarDirection,value);
        }


        //MxMTrajectoryGenerator trajectoryGenerator;
        //public MxMTrajectoryGenerator TrajectoryGenerator => trajectoryGenerator;

        //[TabGroup(Controller)]
        //[ShowInInspector]
        //public bool AlignViewMotion
        //{
        //    get => _AlignViewMotion;
        //    set
        //    {
        //        if (!_AlignViewMotion.SetAs(value))
        //            return;

        //        trajectoryGenerator.Strafing = value;


        //        if (value)
        //        {
        //            // mxm.BlendInLayer(BlendLayers.LayerId,0.5f);
        //            mxm.AddFavourTag("Strafe");
        //            //mxm.SetCalibrationData("Strafe");
        //            mxm.SetFavourCurrentPose(true, 0.95f);
        //            //m_locomotionSpeedRamp.ResetFromSprint();
        //            mxm.AngularErrorWarpRate = 360f;
        //            mxm.AngularErrorWarpThreshold = 270f;
        //            mxm.AngularErrorWarpMethod = EAngularErrorWarpMethod.TrajectoryFacing;
        //            trajectoryGenerator.TrajectoryMode = ETrajectoryMoveMode.Strafe;
        //            //trajectoryGenerator.InputProfile = m_strafeLocomotion;
        //            mxm.PastTrajectoryMode = EPastTrajectoryMode.CopyFromCurrentPose;
        //        }
        //        else
        //        {
        //            mxm.RemoveFavourTag("Strafe");
        //            mxm.SetFavourCurrentPose(false, 1.0f);
        //            //mxm.SetCalibrationData(0);
        //            mxm.AngularErrorWarpRate = 60.0f;
        //            mxm.AngularErrorWarpThreshold = 90f;
        //            mxm.AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
        //            trajectoryGenerator.TrajectoryMode = ETrajectoryMoveMode.Normal;
        //            //trajectoryGenerator.InputProfile = m_generalLocomotion;
        //            mxm.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
        //        }

        //    }
        //}


        bool _AlignViewMotion;

        [TabGroup(Controller)]
        [ShowInInspector]
        public bool AlignViewMotion
        {
            get => _AlignViewMotion;
            set
            {
                if (!_AlignViewMotion.SetAs(value))
                    return;

                //trajectoryGenerator.Strafing = value;

                if (value)
                {
                    // mxm.BlendInLayer(BlendLayers.LayerId,0.5f);
                    mxm.AddFavourTag("Strafe");
                    //mxm.SetCalibrationData("Strafe");
                    mxm.SetFavourCurrentPose(true, 0.95f);
                    //m_locomotionSpeedRamp.ResetFromSprint();
                    mxm.AngularErrorWarpRate = 360f;
                    mxm.AngularErrorWarpThreshold = 270f;
                    mxm.AngularErrorWarpMethod = EAngularErrorWarpMethod.TrajectoryFacing;
                    //trajectoryGenerator.TrajectoryMode = ETrajectoryMoveMode.Strafe;
                    //trajectoryGenerator.InputProfile = m_strafeLocomotion;
                    mxm.PastTrajectoryMode = EPastTrajectoryMode.CopyFromCurrentPose;
                }
                else
                {
                    mxm.RemoveFavourTag("Strafe");
                    mxm.SetFavourCurrentPose(false, 1.0f);
                    //mxm.SetCalibrationData(0);
                    mxm.AngularErrorWarpRate = 60.0f;
                    mxm.AngularErrorWarpThreshold = 90f;
                    mxm.AngularErrorWarpMethod = EAngularErrorWarpMethod.CurrentHeading;
                    //trajectoryGenerator.TrajectoryMode = ETrajectoryMoveMode.Normal;
                    //trajectoryGenerator.InputProfile = m_generalLocomotion;
                    mxm.PastTrajectoryMode = EPastTrajectoryMode.ActualHistory;
                }

            }
        }

        public MxMBlendSpaceLayers BlendLayers;


        [TabGroup(Controller)]
        public int SyncFrequency = 5;

        protected virtual void Register_Trajectory()
        {
            //DrawTrajectory = true;

            mxm.SetCurrentTrajectoryGenerator(this);

            //InstanceIfNull(ref trajectoryGenerator);
            trajectoryGenerator = this;


            //Parameters.RegisterSetter(Preset.PositionBias);
            //Parameters.RegisterSetter(Preset.RotationBias);


            //m_navAgent = GetComponentInChildren<NavMeshAgent>();
            //m_path = new Vector3[5];


            //m_lastDesiredOrientation = transform.rotation.eulerAngles.height;

            //if (trajectoryGeneratorModule != null)
            //{
            //    SetTrajectoryModule(trajectoryGeneratorModule);
            //}


            //if (isMine)
            //    PostTrajectoryGenerator();

        }

#if UNITY_EDITOR

        protected virtual void TrajectoryGUI()
        {
            if (mxm)
            {
                var module = mxm.CurrentAnimData.TagNames;
                var tags = mxm.RequiredTags.GetFlagsOrder();

                foreach (var tag in tags)
                {
                    GUILayout.Label(module[tag]);
                }
            }
        }

#endif

        //===========================================================================================
        /**
        *  @brief Monobehaviour FixedTick which updates / records the past trajectory every physics
        *  update if the Animator component is set to AnimatePhysics update mode.
        *         
        *********************************************************************************************/
        public void FixedUpdate_Trajectory()
        {
            return;
            //trajectoryGenerator.InputVector=Parameters.GetVector3(Preset.ModuleVelocity);
            //return;
            if (Animator.updateMode == AnimatorUpdateMode.AnimatePhysics)
            {
                //MotionUpdate = Parameters.GetBool(Preset.MotionUpdate);

                UpdatePastTrajectory();
            }
        }

        protected bool DrawTrajectory { get; set; }

        //===========================================================================================
        /**
        *  @brief Monobehaviour Update function which is called every frame that the object is active. 
        *  
        *  This updates / records the past trajectory, provided that the Animator component isn't 
        *  running in 'Animate Physics'
        *         
        *********************************************************************************************/
        public void Update_TrajectoryInput()
        {
            if (DrawTrajectory)
            {
                Debug.DrawRay(Transform.position + new Vector3(0, 0.8f), StrafeDirection.normalized, Color.red);
                Debug.DrawRay(Transform.position + new Vector3(0, 0.9f), MotorVelocity.normalized, Color.cyan);
            }

            if (!isMine)
                return;

            {
                var moduleVector = MotorVelocity.normalized; //MotionModule.ModuleVector;
                    //Parameters.GetVector3(Preset.ModuleVector);

                var viewPortPlanarDiecrtion = AlignViewMotion ? StrafeDirection:StrafeDirection;
                    //Parameters.GetVector3(Preset.ViewPortPlanarDirection) :
                    //Parameters.GetVector3(Preset.ViewPortPlanarDirection);

                //Debug.Log(moduleVector);

                PushTrajectoryGenerator(moduleVector, viewPortPlanarDiecrtion);
            }

            UpdatePastTrajectory();

            return;


            {
                var moduleVector = MotorVelocity.normalized;//MotionModule.ModuleVector;
                    //Parameters.GetVector3(Preset.ModuleVector);

                var viewPortPlanarDiecrtion = AlignViewMotion ? StrafeDirection : StrafeDirection;
                //Parameters.GetVector3(Preset.ViewPortPlanarDirection) :
                //    Parameters.GetVector3(Preset.ViewPortPlanarDirection);


                PushTrajectoryGenerator(moduleVector, viewPortPlanarDiecrtion);
                return;

            }

            // m_trajectory
            //if (mAnimator.updateMode != AnimatorUpdateMode.AnimatePhysics)
            //{
            //    UpdatePastTrajectory();
            //}
        }


        #region IMxMTrajectory

        //===========================================================================================
        /**
        *  @brief Returns a list of trajectory points for the current goal. If it has not yet been
        *  extracted, the current goal will be first extracted and then returned. 
        *  
        *  Extraction only occurs once per frame and is cached for performance.
        *  
        *  @return TrajectoryPoint[] - a list of trajectory points representing the goal
        *         
        *********************************************************************************************/
        public TrajectoryPoint[] GetCurrentGoal()
        {
            if (!m_extractedThisFrame)
                ExtractGoal();

            return p_goal;
        }


        //===========================================================================================
        /**
        *  @brief IMxMTrajectory implemented function which returns the transform of the game object
        *  
        *  @return ReadOnlyTransform - the transform component of the game object
        *         
        *********************************************************************************************/
        public Transform GetTransform()
        {
            return Transform;
        }


        //===========================================================================================
        /**
        *  @brief In order to generate and record an appropriate trajectory the Trajectory Generator
        *  needs to know what the MxMAnimator requires from it with it's trajectory configuration. This
        *  function is used to do this. It passes an array of floats specificying the trajectory times 
        *  that the Trajectory generator needs to account for. 
        *  
        *  The functoin is called automatically from the MxMAnimator and the array of prediction times
        *  (past and future) comes directly from the trajectory configuration defined in the pre-process
        *  stage.
        *  
        *  @param [float[]] a_predictionTimes -an array of prediction times (past and future) that this
        *  trajectory generator must account for.
        *         
        *********************************************************************************************/
        public void SetGoalRequirements(float[] a_predictionTimes)
        {
            if (a_predictionTimes == null)
            {
                Debug.LogError("MxM Error: Attempting to set goal requirements on IMxMTrajectory with null prediction times.");
                return;
            }

            int predictionLength = a_predictionTimes.Length;

            if (predictionLength == 0)
            {
                Debug.LogError("MxM Error: Attempting to set goal requirements on IMxMTrajectory with no prediction times set.");
                return;
            }

            //From the known prediction times we can determine a time horizon and a time step which is essential for trajectory prediction.
            p_timeHorizon = a_predictionTimes[predictionLength - 1];
            p_sampleRate = Mathf.Max(p_sampleRate, 0.001f);
            p_timeStep = p_timeHorizon / p_sampleRate;

            if (p_predictionTimes == null)
            {
                p_predictionTimes = new List<float>(a_predictionTimes);
            }
            else
            {
                p_predictionTimes.Clear();

                for (int i = 0; i < predictionLength; ++i)
                {
                    p_predictionTimes.Add(a_predictionTimes[i]);
                }
            }

            p_goal = new TrajectoryPoint[predictionLength];
            int pastPredictionCount = 0;

            for (int i = 0; i < predictionLength; ++i)
            {
                if (i == 0 & a_predictionTimes[i] < 0f)
                    p_maxRecordTime = Mathf.Abs(a_predictionTimes[i]);

                if (a_predictionTimes[i] < 0f)
                    ++pastPredictionCount;
            }

            p_trajectoryIterations = Mathf.FloorToInt(p_timeHorizon * p_sampleRate);
            //Debug.LogError(p_trajectoryIterations);
            InitializeNativeData();

            //RegisterHandler past recording
            if (p_recordingFrequency <= Mathf.Epsilon)
                p_recordingFrequency = 1f / 30f;

            p_recordedPastPositions.Clear();
            p_recordedPastFacingAngles.Clear();
            p_recordedPastTimes.Clear();
            p_recordingTimer = Time.time;
            p_recordedPastPositions.Add(transform.position);
            p_recordedPastFacingAngles.Add(transform.rotation.eulerAngles.y);
            p_recordedPastTimes.Add(p_recordingTimer);

            Setup(a_predictionTimes);
        }




        //===========================================================================================
        /**
        *  @brief Usually a goal is extrated from the Trajectory Generator. However, it is also possible
        *  to set the goal inversly from the MxMAnimator, with this function. 
        *  
        *  Since the actual recorded and predicted trajectory points are stored in a more granular manner
        *  than the MxMAnimator deals with, these granular trajectories need to be interpolated from
        *  the passed trjaectory goal.
        *  
        *  This function is usually called automatically by the MxMAnimator following an action event 
        *  dependent on the 'PostEventTrajectoryHandling' property (see MxMAnimator.cs).
        *  
        *  This function is an implementation of IMxMTrajectory
        *  
        *  @param [TrajectoryPoint[]] a_trajectory - a list of trajectory points representing a trajectory
        *         
        *********************************************************************************************/
        public virtual void SetGoal(TrajectoryPoint[] a_trajectory)
        {
            int point = 0;
            ref readonly TrajectoryPoint startPoint = ref a_trajectory[point];
            ref readonly TrajectoryPoint nextPoint = ref a_trajectory[point + 1];

            float startTime = p_predictionTimes[point];
            float nextTime = p_predictionTimes[point + 1];

            float curTime = startTime;
            float timeDif = nextTime - startTime;

            bool useControllerPosition = false;

            if (nextTime > 0)
                useControllerPosition = true;

            //Copy over past recordings
            for (int i = 0; i < p_recordedPastPositions.Count; ++i)
            {
                float lerp = curTime / timeDif;

                //Todo: CheckAdd that this is in the right space
                if (useControllerPosition)
                {
                    p_recordedPastPositions[i] = Vector3.Lerp(startPoint.Position, Vector3.zero, lerp);
                    p_recordedPastFacingAngles[i] = Mathf.LerpAngle(startPoint.FacingAngle, 0f, lerp);
                }
                else
                {
                    p_recordedPastPositions[i] = Vector3.Lerp(startPoint.Position, nextPoint.Position, lerp);
                    p_recordedPastFacingAngles[i] = Mathf.LerpAngle(startPoint.FacingAngle, nextPoint.FacingAngle, lerp);
                }

                curTime += p_recordingFrequency;

                if (curTime > nextTime)
                {
                    ++point;
                    if (point + 1 < p_predictionTimes.Count)
                    {
                        startPoint = ref a_trajectory[point];
                        nextPoint = ref a_trajectory[point + 1];

                        startTime = p_predictionTimes[point];
                        nextTime = p_predictionTimes[point + 1];

                        timeDif = nextTime - startTime;

                        if (nextTime > 0f)
                        {
                            useControllerPosition = true;
                            timeDif = -startTime;
                        }

                        if (startTime > 0f)
                        {
                            break;
                        }
                    }
                    else
                    {
                        break;
                    }
                }

                if (curTime > 0f)
                    break;
            }
            
            curTime = 0f;
            useControllerPosition = true;
            timeDif = nextTime;


            //Copy over future predictions
            //for (int i = 0; i < p_trajPositions.Count; ++i)
            for (int i = 0; i < p_trajPositions.Length; ++i)
            {
                float lerp = curTime / timeDif;

                if (useControllerPosition)
                {
                    p_trajPositions[i] = Vector3.Lerp(Vector3.zero, nextPoint.Position, lerp);
                    p_trajFacingAngles[i] = Mathf.LerpAngle(0f, nextPoint.FacingAngle, lerp);
                }
                else
                {
                    p_trajPositions[i] = Vector3.Lerp(startPoint.Position, nextPoint.Position, lerp);
                    p_trajFacingAngles[i] = Mathf.LerpAngle(startPoint.FacingAngle, nextPoint.FacingAngle, lerp);
                }

                curTime += p_timeStep;

                if (curTime > nextTime)
                {
                    ++point;

                    if (point + 1 < p_predictionTimes.Count)
                    {
                        startPoint = ref a_trajectory[point];
                        nextPoint = ref a_trajectory[point + 1];

                        startTime = p_predictionTimes[point];
                        nextTime = p_predictionTimes[point + 1];

                        timeDif = nextTime - startTime;

                        useControllerPosition = false;

                    }
                    else
                    {
                        break;
                    }
                }
            }
        }



        //===========================================================================================
        /**
        *  @brief 
        *  
        *          *  This function is usually called automatically by the MxMAnimator following an action event 
        *  dependent on the 'PostEventTrajectoryHandling' property (see MxMAnimator.cs).
        *         
        *********************************************************************************************/
        public virtual void CopyGoalFromPose(ref PoseData a_poseData)
        {
            p_trajectoryGenerateJobHandle.Complete();

            if (a_poseData.Trajectory.Length != p_goal.Length)
                return;

            SetGoal(a_poseData.Trajectory);
        }

        //===========================================================================================
        /**
        *  @brief Pauses the trajectory generator
        *         
        *********************************************************************************************/
        public virtual void Pause()
        {
#if UNITY_EDITOR
            if (InputManager.IsAppShuttingDown)
                return;
#endif

            IsPaused = true;

            if (enabled)
            {
                --s_trajectoryGeneratorCount;
            }
        }

        //===========================================================================================
        /**
        *  @brief Unpauses the trajectory generator
        *         
        *********************************************************************************************/
        public virtual void UnPause()
        {
            IsPaused = false;

            if (enabled)
                ++s_trajectoryGeneratorCount;
        }


        //===========================================================================================
        /**
        *  @brief This function resets the 'Motion' on the trajectory generator. Essentially it sets
        *  all past and future predicted points to zero.  It is also an implementation of IMxMTrajectory
        *  
        *  This function is normally called automatically from the MxMAnimator after an event dependent
        *  on the method used 'PostEventTrajectoryHandling' (See MxMAnimator.cs). However, it can also
        *  be used to stomp all trajectory for whatever reason (e.g. when teleporting the character, 
        *  it may be useful to stomp trajectory so they don't do a running stop after teleportation)
        *  
        *********************************************************************************************/
        /// <summary>
        /// Rest trajectory.
        /// </summary>
        /// <param name="a_rotation"></param>
        public virtual void ResetMotion(float a_rotation = 0f)
        {
            if (!p_trajFacingAngles.IsCreated || !p_trajPositions.IsCreated)
                return;

            Vector3 zeroVector = Vector3.zero;
            m_extractedThisFrame = false;

            p_trajectoryGenerateJobHandle.Complete();
            for (int i = 0; i < p_trajFacingAngles.Length; ++i)
            {
                p_trajFacingAngles[i] = a_rotation;
                p_trajPositions[i] = zeroVector;
            }

            zeroVector = transform.position;

            for (int i = 0; i < p_recordedPastFacingAngles.Count; ++i)
            {
                p_recordedPastFacingAngles[i] = a_rotation;
                p_recordedPastPositions[i] = zeroVector;
            }
        }


        //===========================================================================================
        /**
        *  @brief Checks if the trajectory generator has movement Input or not. The movement Input is
        *  usually cached because it is processed during the update and may need to be fetched at a 
        *  later point.
        *  ??? 2 phase
        *  @return bool - true if there is movement Input, false if there is not.
        *         
        *********************************************************************************************/
        public virtual bool HasMovementInput()
        {
            //Debug.Log(string.Format("Trajectory - {0} {1}",MotionUpdate,Time.frameCount));
            return true;
        }

        //===========================================================================================
        /**
        *  @brief Returns the enabled value of the trajectory generator. This is an implementation of
        *  IMxMTrajectory so that the MxMAnimator can access the enabled status from the interface 
        *  alone.
        *  
        *  @return bool - whether or not the trajectory generator is enabled
        *         
        *********************************************************************************************/
        public bool IsEnabled()
        {
            return enabled;
        }
        #endregion


        //Serialized
        [Header("Trajectory History")]
        [HideInInspector]
        public float p_recordingFrequency = 0f; //Time between recording past trajectory points.
        [HideInInspector]
        public float p_sampleRate = 20f; //Number of samples or steps in calculating the future trajectory (Can be more granular than MxM trajectory configuration)
       [HideInInspector]
        public bool p_flattenTrajectory = false; //If true, the trajectory will be flattened with no 'Y' component value

        //Tracking
        //[ShowInInspector]
        protected float p_timeHorizon;  //The time horizon is how far in the future to predict the trajectory. This is set by the MxMAnimator
        //[ShowInInspector]
        protected float p_timeStep;     //The amount of time that must pass between future trajectory prediction samples
        //[ShowInInspector]
        protected int p_trajectoryIterations; //The number of iterations to achieve p_sampleRate within p_timeHorizon
        //[ShowInInspector]
        protected float p_curFacingAngle; //The current facing angle of the trajectory
        //[ShowInInspector]
        private bool m_extractedThisFrame; //True if an MxMAnimator extracted a trajectory goal this frame

        //Trajectory
        protected TrajectoryPoint[] p_goal; //The goal, or rather the last extracted trajectory
        protected NativeArray<float3> p_trajPositions; //A native array containing all future predicted trajectory positions of length p_trajectoryIterations
        protected NativeArray<float> p_trajFacingAngles; //A native array containing all future predicted trajectory facing angles of length p_trajectoryIterations
        protected List<float> p_predictionTimes; //The a list of prediction times, past and future. These are the prediction times set in the trajectory config
                                                 //[ShowInInspector]
                                                 //public float[] Angles;
        [HideInInspector]
        public float m_lastDesiredOrientation = 0f;
        [HideInInspector]
        public float TurnRate;

        //Past
        protected float p_recordingTimer; //A time to ensure past recording frequency is adhered to
        protected float p_maxRecordTime; //The maximum amount of time to record past trajectory for
        protected List<Vector3> p_recordedPastPositions = new List<Vector3>(); //A list of recorded trajectory positions in the character's past (World Space)
        protected List<float> p_recordedPastFacingAngles = new List<float>(); //A list of recorded trajectory facing angles in the character's past (World Space)
        protected List<float> p_recordedPastTimes = new List<float>(); //A list of recorded past times that correlates to p_recordedPastPositions and p_recordedPastFacingAngles

        //Job handling
        protected JobHandle p_trajectoryGenerateJobHandle; //A m_duringTransit handle to the trajectory generator m_duringTransit
        protected static int s_curTrajectoryGeneratorId; //A static counter for trajectory generator updates to ensure that the jobs only get batch scheduled after s_curTrajectoryGeneratorId == s_trajectoryGeneratorCount
        protected static int s_trajectoryGeneratorCount; //The number of trajectory generators that are active


        //Properties
        /// <summary>
        /// Used to pause and un-pause the trajectory generator
        /// </summary>
        public bool IsPaused 
        {
            get;set;
            //get => Parameters.GetTrigger(Preset.EnableMotion).Result;

            //private set
            //{
            //    Parameters.SetTrigger(Preset.EnableMotion, value);
            //}
        }

        /**
        *  @brief This is the core function of a motion matching controller. It is responsible for 
        *  updating the trajectory prediction for movement. This movement model can differ between 
        *  controllers but this controller takes a deceivingly simplistic approach. 
        *  
        *  The predicted trajectory output from this function is in evenly spaced points and it is not
        *  necessarily the trajectory passed to the MxMAnimator. Rather, the high resolution trajectory
        *  calculated here will be sampled using the ExtractMotion function before passing data to the
        *  MxMAnimator.
        *         
        *********************************************************************************************/
        protected virtual void _UpdatePrediction()
        {
            //Desired linear velocity is calculated based on motion module -user Input
            var desiredLinearVelocity = CalculateDesiredLinearVelocity();

            //Calculate the desired linear displacement over a single iteration
            var desiredLinearDisplacement = desiredLinearVelocity.Multiply(1f / p_sampleRate);

            //float desiredOrientation = 0f;

            //if (false)//m_trajectoryMode != ETrajectoryMoveMode.Normal || (!false && true/*m_faceDirectionOnIdle*/))
            //{
            //    if (false/*if is agent*/ || ViewPortITransform == null)
            //    {
            //        desiredOrientation = Vector3.SignedAngle(Vector3.forward, StrafeDirection, Vector3.up);
            //    }
            //    else
            //    {
            //        //If we are strafing we want to make the desired orienation relative to the camera
            //        var camForward = Parameters.GetVector3(Preset.ViewPortPlanarDirection);
            //        desiredOrientation = Vector3.SignedAngle(Vector3.forward, camForward, Vector3.up);
            //    }
            //}
            //else if (desiredLinearDisplacement.sqrMagnitude > 0.0001f)
            //{
            //    desiredOrientation = Mathf.Atan2(desiredLinearDisplacement.width,
            //        desiredLinearDisplacement.z) * Mathf.Rad2Deg;
            //}
            //else
            //{
            //    if (m_resetDirectionOnNoInput)
            //    {
            //        desiredOrientation = ReadOnlyTransform.rotation.eulerAngles.height;
            //    }
            //    else
            //    {
            //        desiredOrientation = m_lastDesiredOrientation;
            //    }
            //}

            float desiredOrientation = 0f;
            //if (m_trajectoryMode != ETrajectoryMoveMode.Normal
            //    || (!m_hasInputThisFrame && m_faceDirectionOnIdle))
            if (desiredLinearVelocity.sqrMagnitude < 0.07)
            {
                if (AlignViewMotion)//(m_controlMode > ETrajectoryControlMode.UserInput || m_camTransform == null)
                {
                    desiredOrientation = Vector3.SignedAngle(Vector3.forward, StrafeDirection, Vector3.up);
                }
                else
                {
                    //If we are strafing we want to make the desired orienation relative to the camera
                    //StrafeDirection= Parameters.GetVector3(Preset.ViewPortPlanarDirection);
                    desiredOrientation = Vector3.SignedAngle(Vector3.forward, StrafeDirection, Vector3.up);
                }
            }
            else if (desiredLinearDisplacement.sqrMagnitude > 0.0001f)
            {
                desiredOrientation = Mathf.Atan2(desiredLinearDisplacement.x,
                    desiredLinearDisplacement.z) * Mathf.Rad2Deg;
            }
            else
            {
                if (AlignViewMotion)
                {
                    desiredOrientation = Transform.rotation.eulerAngles.y;
                }
                else//??
                {
                    desiredOrientation = m_lastDesiredOrientation;
                }
            }

            m_lastDesiredOrientation = desiredOrientation;

            //var _desiredOrientation = Parameters.GetQuaternion(Preset.ModuleRotation).eulerAngles.height;
            //m_lastDesiredOrientation = _desiredOrientation;

            var _posBiasMultiplier = 3f;//Parameters.GetFloat(Preset.PositionBias);
            var _rotBiasMultiplier = 3f;// Parameters.GetFloat(Preset.RotationBias);

            var deltaTime = ConstCache.deltaTime;
            TurnRate = 10 * m_simulationSpeedScale;// * Time.deltaTime;
            //Debug.Log( " Liner : " + desiredLinearDisplacement + " \nPrediction : "+ _desiredOrientation );
            var trajectoryGenerateJob = new TrajectoryGeneratorJob()
            {
                TrajectoryPositions = p_trajPositions,
                TrajectoryRotations = p_trajFacingAngles,
                NewTrajectoryPositions = m_newTrajPositions,
                DesiredLinearDisplacement = desiredLinearDisplacement,
                DesiredOrientation = desiredOrientation,
                MoveRate = m_posBias * deltaTime, // Mathf.Max(0.1f,_posBiasMultiplier * m_simulationSpeedScale * deltaTime),
                TurnRate = TurnRate//10//m_dirBias*deltaTime,// Mathf.Max(0.1f, _rotBiasMultiplier * m_simulationSpeedScale * deltaTime)
            };
            if (trajectoryGenerateJob.MoveRate == default)
                Debug.LogError(nameof(trajectoryGenerateJob.MoveRate) + trajectoryGenerateJob.MoveRate);
            //Debug.Log(trajectoryGenerateJob.MoveRate+"**"+trajectoryGenerateJob.TurnRate);

            p_trajectoryGenerateJobHandle = trajectoryGenerateJob.Schedule();
        }
        [HideInInspector]
        public float DesireAngle;

        protected virtual void UpdatePrediction()
        {
            //Desired linear velocity is calculated based on motion module -user Input
            var desiredLinearVelocity = CalculateDesiredLinearVelocity();


            //Calculate the desired linear displacement over a single iteration
            var desiredLinearDisplacement = desiredLinearVelocity.Multiply(1f / p_sampleRate);

            var desiredOrientation = Vector3.SignedAngle(Vector3.forward, /*MotionModule.ModuleRotation*/MotorRotation* Vector3.forward,Motor.CharacterUp);
            m_lastDesiredOrientation = desiredOrientation;

            var _posBiasMultiplier = 10f; //Parameters.GetFloat(Preset.PositionBias); //10
            var _rotBiasMultiplier = 10f;//Parameters.GetFloat(Preset.RotationBias); //10

            var deltaTime = ConstCache.deltaTime;

            var trajectoryGenerateJob = new TrajectoryGeneratorJob()
            {
                TrajectoryPositions = p_trajPositions,
                TrajectoryRotations = p_trajFacingAngles,
                NewTrajectoryPositions = m_newTrajPositions,
                DesiredLinearDisplacement = desiredLinearDisplacement,
                DesiredOrientation = desiredOrientation,
                MoveRate = Mathf.Max(0.1f,_posBiasMultiplier ), 
                TurnRate = Mathf.Max(0.1f,_rotBiasMultiplier*deltaTime )
            };

            p_trajectoryGenerateJobHandle = trajectoryGenerateJob.Schedule();
        }

        protected void Setup(float[] a_predictionTimes) //RegisterHandler function to be implemented for any custom logic based on the prediction times. It is called at the end of SetGoalRequirements.
        {

        }

        float FacingAngle
        {
            get 
            {
                return Transform.rotation.eulerAngles.y;
            }

            set
            {

            }
        }

        //===========================================================================================
        /**
        *  @brief Updates the trajectory logic
        *         
        *********************************************************************************************/
        protected void UpdatePastTrajectory()
        {
            if (!IsPaused)
            {
                m_extractedThisFrame = false;

                RecordPastTrajectory();
     

                p_curFacingAngle = FacingAngle;// (ReadOnlyTransform.rotation*Quaternion.Inverse(Parameters.GetQuaternion(Preset.ViewPortPlanarRotation))).eulerAngles.height;//ReadOnlyTransform.rotation.eulerAngles.height;

                p_trajectoryGenerateJobHandle.Complete(); //Complete the m_duringTransit before it needs to be used again


                UpdatePrediction();

              

                ++s_curTrajectoryGeneratorId;
                if (s_curTrajectoryGeneratorId == s_trajectoryGeneratorCount)
                {
                    s_curTrajectoryGeneratorId = 0;
                    JobHandle.ScheduleBatchedJobs();
                }
            }
        }

        //===========================================================================================
        /**
        *  @brief Records the past trajectory if the time passed meets the recording frequency requirements
        *         
        *********************************************************************************************/
        protected void RecordPastTrajectory()
        {
            if (Time.time - p_recordingTimer >= p_recordingFrequency)
            {
                p_recordedPastPositions.Insert(0, transform.position);
                p_recordedPastFacingAngles.Insert(0, p_curFacingAngle);

                p_recordingTimer = Time.time;
                p_recordedPastTimes.Insert(0, p_recordingTimer);

                float totalRecordTime = p_recordedPastTimes[0] - p_recordedPastTimes[p_recordedPastTimes.Count - 1];

                if (totalRecordTime > p_maxRecordTime + p_recordingFrequency)
                {
                    p_recordedPastPositions.RemoveAt(p_recordedPastPositions.Count - 1);
                    p_recordedPastFacingAngles.RemoveAt(p_recordedPastFacingAngles.Count - 1);
                    p_recordedPastTimes.RemoveAt(p_recordedPastTimes.Count - 1);
                }
            }
        }

       

        //===========================================================================================
        /**
        *  @brief This function is called by the MxMAnimator when it requires a goal to run an 
        *  animation search. Essentially it extracts a course set of trajectory points from the granular
        *  recorded past and predicted future. The goal extracted is based on the trajectory configuration
        *  specified in the Pre-Processing stage
        *  
        *  This function is an implementation of IMxMTrajectory
        *         
        *********************************************************************************************/
        public void ExtractGoal()
        {
            p_trajectoryGenerateJobHandle.Complete();
            //Angles = p_trajFacingAngles.ToArray();
            var transformPosition = Transform.position;
            //Extract data from the trajectory for the goal times
            for (int i = 0; i < p_goal.Length; ++i)
            {
                float timeDelay = p_predictionTimes[i];

                if (timeDelay < 0f) //Past trajectory search
                {
                    int curIndex = 1;
                    float lerp = 0f;
                    float curTime = Time.time;
                    for (int k = 1; k < p_recordedPastTimes.Count; ++k)
                    {
                        float time = p_recordedPastTimes[k];

                        if (time < curTime + timeDelay)
                        {
                            curIndex = k;


                            float timeError = (curTime + timeDelay) - time;
                            float deltaTime = p_recordedPastTimes[k - 1] - time;

                            lerp = timeError / deltaTime;
                            break;
                        }
                    }

                    if (curIndex < p_recordedPastTimes.Count)
                    {
                        Vector3 position = Vector3.Lerp(p_recordedPastPositions[curIndex], p_recordedPastPositions[curIndex - 1], lerp);
                        float facingAngle = Mathf.LerpAngle(p_recordedPastFacingAngles[curIndex], p_recordedPastFacingAngles[curIndex - 1], lerp);

                        if (p_flattenTrajectory)
                            position.y = transformPosition.y;

                        //p_goal[i] = new TrajectoryPoint(position - transformPosition, p_recordedPastFacingAngles[curIndex]);
                        p_goal[i] = new TrajectoryPoint(position - transformPosition, facingAngle);
                    }
                    else
                    {
                        p_goal[i] = new TrajectoryPoint();
                    }
                }
                else
                {
                    //int index = Mathf.RoundToInt(timeDelay / p_timeStep);
                    int index = Mathf.RoundToInt(timeDelay / p_timeHorizon * p_trajPositions.Length) - 1;

                    if (index >= p_trajPositions.Length)
                        index = p_trajPositions.Length - 1;

                    Vector3 position = p_trajPositions[index];

                    if (p_flattenTrajectory)
                        position.y = 0f;

                    p_goal[i] = new TrajectoryPoint(position, p_trajFacingAngles[index]);
                }
            }

            m_extractedThisFrame = true;


        }

        //===========================================================================================
        /**
        *  @brief Extracts motion from the trajectory at a specific time in the Trajectory. More 
        *  accurately, this return a tuple with the movement vector and angular delta from the 
        *  character's current transform to the trajectory point specified in the passed time
        *  
        *  @param [float] a_time - the time in the future to extract motion from
        *  
        *  @return Vector3 moveDelta - (Tuple) the delta position between the character and the extracted trajectory point
        *  @return float angleDelta - (Tuple) the delta angle between the character and the extracted trajectory point
        *         
        *********************************************************************************************/
        public (Vector3 moveDelta, float angleDelta) ExtractMotion(float a_time)
        {
            p_trajectoryGenerateJobHandle.Complete();

            a_time = Mathf.Clamp(a_time, 0f, p_predictionTimes[p_predictionTimes.Count - 1]);

            int startIndex = Mathf.FloorToInt(a_time / p_timeStep);
            int endIndex = Mathf.CeilToInt(a_time / p_timeStep);

            float lerp = (a_time - (startIndex * p_timeStep)) / p_timeStep;

            return (Vector3.Lerp(p_trajPositions[startIndex], p_trajPositions[endIndex], lerp),
                Mathf.LerpAngle(p_trajFacingAngles[startIndex], p_trajFacingAngles[endIndex], lerp));
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour OnDestroy function which ensures all native data has been disposed to
        *  avoid memory leaks.
        *         
        *********************************************************************************************/
        protected virtual void OnDestroy_Trajectory()
        {
            DisposeNativeData();
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour OnDisable function which is called everytime this component is disabled.
        *  It disposes any native allocated data to avoid memory leaks and removes a counter from the 
        *  trajectory generator count so that jobs will be batched appropriately
        *         
        *********************************************************************************************/
        protected virtual void OnDisable_Trajectory()
        {
            ResetMotion();

            if (!IsPaused)
            {
                --s_trajectoryGeneratorCount;
            }
            DisposeNativeData();
        }

        //===========================================================================================
        /**
        *  @brief Monobehaviour OnEnable function which is called everytime this component is enabled.
        *  It initializes all native data and adds a count to trajectory generators to ensure that jobs
        *  from all trajectory generators will be batched
        *         
        *********************************************************************************************/
        protected virtual void OnEnable_Trajectory()
        {
            if (isMine)
            {
                var timer = Timer.GetTimer(SyncFrequency);

                timer.Bell -= PostTrajectoryGenerator;
                timer.Bell += PostTrajectoryGenerator;
            }

            InitializeNativeData();

            if (!IsPaused)
            {
                ++s_trajectoryGeneratorCount;
            }

            //mxMAnimator.SetCurrentTrajectoryGenerator(this);
        }

        //===========================================================================================
        /**
        *  @brief Initializes native arrays for use with the MxMTrajectoryGenerator. 
        *         
        *********************************************************************************************/
        protected virtual void InitializeNativeData()
        {
            DisposeNativeData();

            p_trajPositions = new NativeArray<float3>(p_trajectoryIterations,
                Allocator.Persistent, NativeArrayOptions.ClearMemory);

            p_trajFacingAngles = new NativeArray<float>(p_trajectoryIterations,
                Allocator.Persistent, NativeArrayOptions.ClearMemory);


            //**-------------------------override

            m_newTrajPositions = new NativeArray<float3>(p_trajectoryIterations,
                Allocator.Persistent, NativeArrayOptions.ClearMemory);

        }

        //===========================================================================================
        /**
        *  @brief This function disposes any native data that has been allocated to avoid memory leaks
        *         
        *********************************************************************************************/
        protected virtual void DisposeNativeData()
        {
            p_trajectoryGenerateJobHandle.Complete();

            if (p_trajFacingAngles.IsCreated)
                p_trajFacingAngles.Dispose();

            if (p_trajPositions.IsCreated)
                p_trajPositions.Dispose();

            //*************override
            if (m_newTrajPositions.IsCreated)
                m_newTrajPositions.Dispose();
        }




        #region Override

        [Header("Motion Settings")]
        [Range(0f, 5f)]
        [HideInInspector]
        public float m_simulationSpeedScale = 1f;
        [HideInInspector] public float m_maxSpeed = 4f; //The maximum speed of the trajectory (can be modified at runtime)
        [HideInInspector] public float m_posBias = 15f; //The positional responsivity of the trajectory (can be modified at runtime)
        [HideInInspector] public float m_dirBias = 20f; //The rotational responsivity of the trajectory (can be modified at runtime)
        //[SerializeField] 
        //private ETrajectoryMoveMode m_trajectoryMode = ETrajectoryMoveMode.Normal; //If the trajectory should behave like strafing or not.
        //                                                                                            //[SerializeField] private ETrajectoryControlMode m_controlMode = ETrajectoryControlMode.UserInput; // how the trajectory is to be controlled
        public ReadOnlyTransform ViewPortITransform; //The camera transform used to make Input relative to the camera.

        //public Vector3 InputVector { get; set; } //The raw Input vector
        //public Vector2 InputVector2D { get { return new Vector2(InputVector.width, InputVector.z); } set { InputVector = new Vector3(value.width, 0f, value.height); } }
        //public Vector3 LinearInputVector { get; set; } //The transformed Input vector relative to camera

        //public float MaxSpeed { get { return m_maxSpeed; } set { m_maxSpeed = value; } } //The maximum speed of the trajectory generator
        //public float PositionBias { get { return m_posBias; } set { m_posBias = value; } } //The positional responsiveness of the trajectory generator
        //public float DirectionBias { get { return m_dirBias; } set { m_dirBias = value; } } //The rotational responsiveness of the trajectory generator

        [HideInInspector]
        [Header("Inputs")]// change name
        public bool m_resetDirectionOnNoInput = true;


        //[SerializeField] 
        //private MxMInputProfile m_mxmInputProfile = null; //A reference to the Inputs profile asset used to shape the trajectory
        
        //?? preset
        [SerializeField] [HideInInspector]
        private TrajectoryGeneratorModule trajectoryGeneratorModule = null; //The trajectory generator module to use

        //private bool m_hasInputThisFrame ; //A bool to cache whether there has been movement Input in the current frame.
        private NativeArray<float3> m_newTrajPositions; //A native array buffer for storing the new trajectory points calculated for the current frame.


        /** AI */
        //private NavMeshAgent m_navAgent;
        //private Vector3[] m_path;

        //[Header("AI")]
        //[SerializeField] private float m_stoppingDistance = 1f;
        //[SerializeField] private bool m_applyRootSpeedToNavAgent = true;
        //[SerializeField] private bool m_faceDirectionOnIdle = false;
        //public bool FaceDirectionOnIdle { get { return m_faceDirectionOnIdle; } set { m_faceDirectionOnIdle = value; } }
        //public float StoppingDistance { get { return m_stoppingDistance; } set { m_stoppingDistance = value; } }
        //public bool ApplyRootSpeedToNavAgent { get { return m_applyRootSpeedToNavAgent; } set { m_applyRootSpeedToNavAgent = value; } }

        //public bool Strafing
        //{
        //    get { return m_trajectoryMode == ETrajectoryMoveMode.Strafe ? true : false; }
        //    set
        //    {
        //        if (value)
        //            m_trajectoryMode = ETrajectoryMoveMode.Strafe;
        //        else
        //            m_trajectoryMode = ETrajectoryMoveMode.Normal;
        //    }
        //}

        //public bool Climbing
        //{
        //    get { return m_trajectoryMode == ETrajectoryMoveMode.Climb ? true : false; }
        //    set
        //    {
        //        if (value)
        //            m_trajectoryMode = ETrajectoryMoveMode.Climb;
        //        else
        //            m_trajectoryMode = ETrajectoryMoveMode.Normal;
        //    }
        //}


 

        //===========================================================================================
        /**
        *  @brief Monobehaviour Late Update function which is called every frame that the object is 
        *  active but only after the animation update. This function projects root motion onto the 
        *  navmesh agent's speed to help prevent foot sliding but also ensure that the character 
        *  still stays perfectly on the navmesh path.
        *         
        *********************************************************************************************/
        //public void _TrajectoryLateUpdate()
        //{
        //    Vector3 v = m_navAgent.velocity * Time.deltaTime;
        //    Vector3 animatorDelta = p_animator.deltaPosition;

        //    float rootSpeed = 0f;
        //    if (Vector3.Angle(animatorDelta, v) < 180f)
        //    {
        //        float vMag = v.magnitude;

        //        Vector3 projectedDelta = (Vector3.Dot(animatorDelta, v) / (vMag * vMag)) * v;
        //        rootSpeed = projectedDelta.magnitude / Time.deltaTime;
        //    }

        //    if (!float.IsNaN(rootSpeed))
        //    {
        //        m_navAgent.speed = Mathf.Max(rootSpeed, 0.5f);
        //    }

        //}

        //===========================================================================================
        
        #region AI
        //===========================================================================================
        /**
        *  @brief This update function is used for the ComplexAI control option. This kind of prediction
        * takes the AI path and tries to place trajectory points along it whilst sill doing it in a
        * smooth manner.
        *         
        *********************************************************************************************/

        //void UpdatePrediction_ComplexAI()
        //{
        //    if (m_path == null)
        //        m_path = new Vector3[6];

        //    Vector3 charPosition = transform.position;

        //    m_newTrajPositions[0] = float3.zero;
        //    p_trajFacingAngles[0] = 0f;
        //    int iterations = m_newTrajPositions.Length;

        //    if (m_navAgent.remainingDistance < m_stoppingDistance)
        //    {
        //        for (int i = 1; i < iterations; ++i)
        //        {
        //            float percentage = (float)i / (float)(iterations - 1);

        //            m_newTrajPositions[i] = math.lerp(p_trajPositions[i], float3.zero,
        //                1f - math.exp(-m_posBias * percentage * Time.deltaTime));
        //        }

        //        m_hasInputThisFrame = false;
        //    }
        //    else
        //    {
        //        int pathPointCount = m_navAgent.path.GetCornersNonAlloc(m_path);

        //        int pathIndex = 1;
        //        float pathCumDisplacement = 0f;

        //        float3 from;
        //        float3 to;
        //        //float largestFacingAngle = 0f;
        //        float desiredOrientation = 0f;
        //        for (int i = 1; i < iterations; ++i)
        //        {
        //            float percentage = (float)i / (float)(iterations - 1);
        //            float desiredDisplacement = m_maxSpeed * percentage;

        //            //find the desired point along the path.
        //            float3 lastPoint = float3.zero;
        //            float3 desiredPos = float3.zero;
        //            desiredOrientation = 0f;
        //            to = m_path[pathIndex - 1] - charPosition;
        //            for (int k = pathIndex; k < pathPointCount; ++k)
        //            {
        //                from = to;
        //                to = m_path[k] - charPosition;

        //                float displacement = math.length(to - from);

        //                if (pathCumDisplacement + displacement > desiredDisplacement)
        //                {
        //                    float lerp = (desiredDisplacement - pathCumDisplacement) / displacement;
        //                    desiredPos = math.lerp(from, to, lerp);

        //                    break;
        //                }

        //                if (k == pathPointCount - 1)
        //                {

        //                    desiredPos = to;
        //                    desiredOrientation = transform.rotation.eulerAngles.height;
        //                    break;
        //                }

        //                pathCumDisplacement += displacement;
        //                ++pathIndex;
        //            }

        //            float3 lastPosition = p_trajPositions[i - 1];

        //            float3 adjustedTrajectoryDisplacement = math.lerp(p_trajPositions[i] - lastPosition, desiredPos - lastPosition,
        //                1f - math.exp(-m_posBias * percentage * Time.deltaTime));

        //            m_newTrajPositions[i] = m_newTrajPositions[i - 1] + adjustedTrajectoryDisplacement;
        //        }

        //        if (Strafing || (m_faceDirectionOnIdle && !m_hasInputThisFrame))
        //            desiredOrientation = (Mathf.Atan2(StrafeDirection.width, StrafeDirection.z) * Mathf.Rad2Deg);

        //        //Rotation iteration
        //        to = m_newTrajPositions[0];
        //        for (int i = 1; i < m_newTrajPositions.Length; ++i)
        //        {
        //            float percentage = (float)i / (float)(iterations - 1);

        //            from = to;
        //            to = m_newTrajPositions[i];
        //            float3 next = to + (to - from);

        //            if (i < m_newTrajPositions.Length - 1)
        //                next = m_newTrajPositions[i + 1];

        //            if (!Strafing)
        //            {
        //                //var displacementVector = Vector3.Lerp(to - from, next - to, 0.5f);
        //                //var displacementVector = to - from;
        //                var displacementVector = next - to;
        //                desiredOrientation = Vector3.SignedAngle(Vector3.forward, displacementVector, Vector3.up);
        //            }

        //            if (Vector3.SqrMagnitude(to - from) > 0.05f)
        //            {
        //                // float facingAngle = Mathf.LerpAngle(p_trajFacingAngles[i], desiredOrientation,
        //                //     1f - math.exp(-m_dirBias * percentage * Time.deltaTime));

        //                float facingAngle = math.degrees(Mathf.LerpAngle(math.radians(p_trajFacingAngles[i]),
        //                    math.radians(desiredOrientation), 1f - math.exp(-m_posBias * percentage)));

        //                p_trajFacingAngles[i] = facingAngle;

        //            }
        //        }

        //        m_hasInputThisFrame = true;
        //    }

        //    for (int i = 0; i < iterations; ++i)
        //    {
        //        p_trajPositions[i] = m_newTrajPositions[i];
        //    }
        //}

        #endregion
        //===========================================================================================
        /**
        *  @brief This function calculates the desired linear velocity based on Input. This is the
        *  Input multiplied by the maximum speed and modified by an Input profile.
        *  
        *  In order to ensure that the generator never produces a trajectory that there is no animation
        *  for, it needs to be extended or shortened based on an Input profile. The Input profile 
        *  basically remaps ranges of Input magnitude to a viable Input magnitude. The Input vector
        *  is then modified by this value as well as the responsiveness depending on the Input profile.
        *  
        *  @return Vector3 - the desired linear velocity of the character.
        *         
        *********************************************************************************************/
        private Vector3 CalculateDesiredLinearVelocity()
        {
            //Debug.Log(MotionUpdate);

            //if (!MotionUpdate)
            //    return Vector3.zero;

            var moduleV = MotorVelocity.normalized;//MotionModule.ModuleVelocity;

            //if(MotionUpdate)
            //    Debug.Log(moduleV);
            return moduleV;

            //Get the movement Input
            //if (m_trajectoryMode == ETrajectoryMoveMode.Climb)
            //{

            //    if (!m_customInput)
            //        InputVector = new Vector3(Inputs.GetAxis(k_horizontalAxis), Inputs.GetAxis(k_verticalAxis), 0f);
            //    else
            //        InputVector = new Vector3(InputVector.width, InputVector.z, InputVector.height);

            //    InputVector = new Vector3(InputVector.width, InputVector.z, InputVector.height);

            //}
            //else
            //{
            //    if (!m_customInput)
            //        InputVector = new Vector3(Inputs.GetAxis(k_horizontalAxis), 0f, Inputs.GetAxis(k_verticalAxis));
            //}


            //Normalize the Input so it is no greater than magnitude of 1
            //if (InputVector.sqrMagnitude > 1f)
            //{
            //    InputVector = InputVector.normalized;
            //}

            ////If we are using an Input profile then run it and get our Input scale and bias multipliers
            //if (m_mxmInputProfile != null)
            //{
            //    var inputData = m_mxmInputProfile.GetInputScale(InputVector);
            //    InputVector *= inputData.scale;
            //    mxMAnimator.LongErrorWarpScale = 1f / inputData.scale;

            //    m_posBiasMultiplier = inputData.posBias;
            //    m_dirBiasMultiplier = inputData.dirBias;
            //}
            //else
            //{
            //    m_posBiasMultiplier = 1f;
            //    m_dirBiasMultiplier = 1f;
            //}

            //if (InputVector.sqrMagnitude > 0.001f)
            //{
            //    MotionUpdate = true;

            //    if (ViewPortITransform == null)
            //    {
            //        return InputVector * m_maxSpeed;
            //    }
            //    else
            //    {
            //        //Project the camera forward vector onto a ground plane
            //        Vector3 forward = Vector3.ProjectOnPlane(ViewPortITransform.forward, Vector3.up);

            //        //Rotate our Input vector relative to the camera
            //        LinearInputVector = Quaternion.FromToRotation(Vector3.forward, forward) * InputVector;

            //        //Return our desired velocity by multiplying our Input by our max speed
            //        return LinearInputVector * m_maxSpeed;
            //    }
            //}
            //else
            //{
            //    MotionUpdate = false;
            //}

            ////No Input so just return zero
            //return Vector3.zero;
        }

        //===========================================================================================
        /**
        *  @brief This function calculates the desired linear velocity based on AI Inputs.
        *  
        *  @return Vector3 - the desired linear velocity of the character.
        *         
        *********************************************************************************************/
        //private Vector3 CalculateDesiredLinearVelocity_AI()
        //{
        //    m_posBiasMultiplier = 1f;
        //    m_dirBiasMultiplier = 1f;

        //    float destSqr = (m_navAgent.destination - transform.position).sqrMagnitude;

        //    if (destSqr < m_stoppingDistance)
        //    {
        //        InputVector = Vector3.zero;
        //        m_hasInputThisFrame = false;
        //        return Vector3.zero;
        //    }

        //    InputVector = (m_navAgent.steeringTarget - transform.position);

        //    if (InputVector.sqrMagnitude > 0.001f)
        //    {
        //        InputVector = InputVector.normalized;
        //        m_hasInputThisFrame = true;
        //        float maxSpeed = Mathf.Min(Mathf.Sqrt(destSqr), m_maxSpeed);

        //        return InputVector * maxSpeed;
        //    }
        //    else
        //    {
        //        m_hasInputThisFrame = false;
        //    }

        //    //No Input so just return zero
        //    return Vector3.zero;
        //}

        //===========================================================================================
        /**
        *  @brief This function calculates the relative input vector transfromed both by the camera
        *  and then the character (in that order). This allows movement (or trajectory) input to be 
        *  dependent on camera angle. Since the trajectory is going to be transformed into the character
        *  space by the MxMAnimator, it needs to be inversely transformed by the character transform as well
        *  
        *  @return Vector3 - the relative input vector that will be used to generate a trajectory.
        *         
        *********************************************************************************************/
        public Vector3 GetRelativeInputVector()
        {
            throw new System.NotImplementedException();

            var inputVector = MotorVelocity.normalized;//MotionModule.ModuleVector;
                //Parameters.GetVector3(Preset.ModuleVector);

            if (ViewPortITransform == null)
            {
                return inputVector;
            }
            else
            {
                var planarDir = StrafeDirection; //Parameters.GetVector3(Preset.ViewPortPlanarDirection);
                Vector3 linearInput = Quaternion.FromToRotation(Vector3.forward, planarDir) * inputVector;

                return Transform.InverseTransformVector(linearInput);
            }
        }







        //===========================================================================================
        /**
        *  @brief Allows developers to manually set the Input vector on the Trajectory Generator for
        *  custom Input.
        *         
        *********************************************************************************************/
        //public void SetInput(Vector2 a_input)
        //{
        //    InputVector = new Vector3(a_input.width, 0f, a_input.height);
        //}

        //===========================================================================================
        /**
        *  @brief Changes all the settings on the trajectory generator to match that of the passed
        *  module
        *
        * @param [TrajectoryGeneratorModule] a_trajGenModule - The module to use.
        *         
        *********************************************************************************************/
        public void SetTrajectoryModule(TrajectoryGeneratorModule a_trajGenModule)
        {
            if (a_trajGenModule == null)
                return;

            m_maxSpeed = a_trajGenModule.MaxSpeed;
            m_posBias = a_trajGenModule.PosBias;
            m_dirBias = a_trajGenModule.DirBias;

            //m_trajectoryMode = a_trajGenModule.TrajectoryMode;
            p_flattenTrajectory = a_trajGenModule.FlattenTrajectory;
            //m_customInput = a_trajGenModule.CustomInput;
            m_resetDirectionOnNoInput = a_trajGenModule.ResetDirectionOnNoInput;
            //m_stoppingDistance = a_trajGenModule.StoppingDistance;
            //m_applyRootSpeedToNavAgent = a_trajGenModule.ApplyRootSpeedToNavAgent;
            //m_faceDirectionOnIdle = a_trajGenModule.FaceDirectionOnIdle;
            //m_camTransform = a_trajGenModule.CamTransform;
            //m_mxmInputProfile = a_trajGenModule.InputProfile;

        }

        #endregion

    }
}
