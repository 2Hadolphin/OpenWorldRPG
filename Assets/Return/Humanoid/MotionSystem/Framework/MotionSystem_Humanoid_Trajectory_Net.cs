using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TNet;
using Unity.Collections;
using Unity.Jobs;
using MxM;
using UnityEngine.Assertions;
using Unity.Mathematics;
using Sirenix.OdinInspector;
using MxMGameplay;
using Return.Cameras;
using Return.Timers;
using Return.Framework.Parameter;
using System;

namespace Return.Motions
{
    public partial class MotionSystem_Humanoid /*TNet*/
    {
        [Inject]
        public virtual void Inject_TNet(ITNO tno)
        {
            Assert.IsFalse(tno.IsNull());

            this.tno = tno;
            isMine = new Parameter<bool>(() => tno.isMine);
        }
        
        /// <summary>
        /// TNetwork handle. 
        /// </summary>
        public ITNO tno { get; protected set; }

        /// <summary>
        /// Is network control belongs to this device.  **Net sync
        /// </summary>
        public Parameters<bool> isMine { get; protected set; }


        public int SyncRate = 5;

        //[System.NonSerialized]
        //public PocketWatch SyncTimer_Position;

        [System.NonSerialized]
        IAlarm SyncCache;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(UnityEditor.EditorApplication.isPlaying)
            {
                //if (SyncTimer_Position)
                //    SyncTimer_Position.DisposePlayingPerformer();
                Stop_Trajectory_Net();
                SetTrajectorySyncTimer();
            }    
        }
#endif

        protected virtual void Register_Net()
        {
            //m_tno = Agent.tno;
            //IsMine = Agent.isMine;

            //if(Agent.Authorize(out m_tno))
            //{
            //    IsMine = new AbstractValue<bool>(() => tno.isMine);
            //}
            //else
            //{
            //    IsMine = new AbstractValue<bool>(() => false);
            //}
        }

        /// <summary>
        /// Sync trajectory
        /// </summary>
        protected virtual void OnEnable_Net()
        {
            if (!isMine)
                return;

            SetTrajectorySyncTimer();
        }

        void SetTrajectorySyncTimer()
        {
            //Assert.IsNull(SyncTimer_Position);

            SyncCache = Timer.GetTimer(SyncRate);
            SyncCache.Bell += OnSyncPosition;


            //SyncTimer_Position = Timer.Countdown(1f / SyncRate, true);
            //SyncTimer_Position.OnTimeUp += OnSyncPosition;
        }

        protected virtual void Stop_Trajectory_Net()
        {

            if (SyncCache.NotNull())
            {
                SyncCache.Bell -= OnSyncPosition;
            }

            //Assert.IsNotNull(SyncTimer_Position);
            //if (SyncTimer_Position)
            //{
            //    SyncTimer_Position.DisposePlayingPerformer();
            //    SyncTimer_Position = null;
            //}    
        }

        protected void OnSyncPosition()
        {
            //Debug.Log("SyncNet");
#if UNITY_EDITOR
            if(tno.IsNull())
            {
                //SyncMotion(ReadOnlyTransform.GetWorldPR());
                return;
            }
#endif
            if (tno.canSend)
                tno.SendQuickly(nameof(SyncMotion), Target.OthersSaved, Transform.GetWorldPR());
        }

        #region Motor

        [RFC]
        protected void SyncMotion(PR pr)
        {
            if (Motor.NotNull())
                Motor.SetPosition(pr,false);
            else
                Transform.SetWorldPR(pr);

            net = pr;
        }



        #endregion

        int lastPostIndex;

        PR net;

        //public static implicit operator NetMotionSystem (m_MotionSystem system)
        //{
        //    return system.Net;
        //}

        void GizmosNetTarget()
        {
            DebugDrawUtil.DrawCoordinate(net,net);
        }

        /// <summary>
        /// Sync trajectory.
        /// </summary>
        protected virtual void PostTrajectoryGenerator()
        {
#if UNITY_EDITOR
            if(tno == null)
            {
                var motionVector = trajectoryGenerator.InputVector;
                var viewPortPlanarDirection = trajectoryGenerator.StrafeDirection;

                PushTrajectoryGenerator(motionVector, viewPortPlanarDirection);
                return;
            }
#endif

            if (isMine)
            {
                var motionVector = trajectoryGenerator.InputVector;
                var viewPortPlanarDirection = trajectoryGenerator.StrafeDirection;

                if (motionVector == default)
                    tno.Send(nameof(PushTrajectoryGenerator), Target.OthersSaved, motionVector, viewPortPlanarDirection);
                else
                    tno.SendQuickly(nameof(PushTrajectoryGenerator), Target.OthersSaved, motionVector, viewPortPlanarDirection);
            }
        }

        [RFC]
        protected virtual void PushTrajectoryGenerator(Vector3 motionVector = default, Vector3 viewPortPlanarDirection = default)
        {
            trajectoryGenerator.InputVector = motionVector;
            trajectoryGenerator.StrafeDirection = viewPortPlanarDirection;
        }

        //void UpdateNetGoal(TrajectoryPoint lastPoint)
        //{
        //    SyncTrajectory(lastPoint.GUIDs, ReadOnlyTransform.up.Multiply(lastPoint.FacingAngle));
        //}


        [RFC]
        [Obsolete]
        public void SyncTrajectory(Vector3 lastPos, Vector3 lastViewPortPlanarDirection)
        {
            if (tno.isMine)
                tno.SendQuickly(nameof(SyncTrajectory), Target.OthersSaved, lastPos, lastViewPortPlanarDirection);
            else
            {
                var vector = lastPos - Transform.position;
                if (vector.sqrMagnitude < 0.0025f)
                    vector = Vector3.zero;

                //trajectoryGenerator.LinearInputVector = vector;
                
                trajectoryGenerator.InputVector = vector;
                trajectoryGenerator.StrafeDirection = lastViewPortPlanarDirection;
            }
        }


    }
}

