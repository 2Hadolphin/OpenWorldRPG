using System.Collections.Generic;
using UnityEngine;
using Unity.Collections;
using UnityEngine.Assertions;
using Unity.Jobs;
using Sirenix.OdinInspector;
using System.Linq;
using UnityEngine.Profiling;
using System.Collections;
using System;

namespace Return.Items.Weapons
{
    public class RaycastSystem : SingletonMonoManager<RaycastSystem>,IComparer<RaycastHit>
    {
        public override bool crossScene => false;

        public override void LoadInitilization()
        {

        }


        #region Routine

        int IComparer<RaycastHit>.Compare(RaycastHit x, RaycastHit y)
        {
            return x.distance > y.distance ? 1 : -1;
        }

        static readonly HashSet<uint> _checkCache = new(20);
        public static HashSet<uint> checkCache
        {
            get
            {
                _checkCache.Clear();
                return _checkCache;
            }
        }

        static readonly List<float> _orderCache=new (20);
        public static List<float> orderCache
        {
            get
            {
                _orderCache.Clear();
                return _orderCache;
            }
        }

        static readonly List<RaycastHit> orderHits=new(20);

        private void FixedUpdate()
        {
            #region MainThread

            if (NormalMissions.Count>0)
                using (var missions = NormalMissions.CacheLoop())
                {
                    foreach (ITrajectoryMission mission in missions)
                    {
                        if (!mission.ReadyToRaycast(out var castCount))
                            continue;

    #if UNITY_EDITOR
                        if (castCount > 20)
                        {
                            Debug.LogError(string.Format("{0} require to many raycast objs, using batch instead.", mission));
                            continue;
                        }
    #endif

                        if (hits.Length < castCount)
                        {
                            Array.Resize(ref hits, castCount);
                            Array.Resize(ref orders, castCount);
                        }

                        var num = Physics.RaycastNonAlloc(
                            mission.GetRay(),
                            hits,
                            mission.MaxDistance,
                            mission.Layer,
                            mission.triggerInteraction
                            );
   
                        orderHits.Clear();
                        orderHits.AddRange(hits.Take(num));
                        orderHits.Sort(this);

                        if (mission.FinishTrajectory(orderHits))//(hits,num))
                        {
                            NormalMissions.Remove(mission);

                            // release mission to pool 
                            if (mission is IDisposable disposable)
                                disposable.Dispose();
                        }
                    }
                }

            #endregion


            #region Batch

            if (curDelay < MaxWaitCount)
                curDelay++;
            else
                FinishJob();

            if (PrepareJob())
                StartJob();

            #endregion
        }

        private void Update()
        {
            Debug.DrawRay(transform.position, transform.forward * 5, Color.red);
        }

        #endregion

        #region Main Thread

        [ShowInInspector, Sirenix.OdinInspector.ReadOnly]
        static readonly List<ITrajectoryMission> NormalMissions=new ();

        static RaycastHit[] hits = new RaycastHit[20];
        static int[] orders = new int[20];

        public static void AddMission(ITrajectoryMission mission)
        {
            NormalMissions.Add(mission);
        }

        #endregion

        #region Batch Raycast

        NativeArray<RaycastCommand> Commands;
        NativeArray<RaycastHit> Results;

        [ShowInInspector]
        readonly List<ITrajectoryBatchMission> Missions=new();
        readonly List<bool> ValidMission=new();

        int ValidMissionNums;
        int RequireHitNums;

        [ShowInInspector]
        protected List<ITrajectoryBatchMission> MissionQueue=new(20);

        protected virtual void AddBatchJobQueue(IEnumerable<ITrajectoryBatchMission> jobs)
        {
            if (MissionQueue == null)
                MissionQueue = new(jobs);
            else
                MissionQueue.AddRange(jobs);

            //Debug.Log(MissionQueue.Count);
        }

        public static void StartJobQueue(params ITrajectoryBatchMission[] jobs)
        {
            //Instance.AddBatchJobQueue(
            //    jobs.Where(width=>width is ITrajectoryBatchMission).
            //    Select(width=>width as ITrajectoryBatchMission));

            Instance.AddBatchJobQueue(jobs);
        }

        [SerializeField]
        uint MaxWaitCount = 5;
        uint curDelay=0;


        JobHandle Handle;
        public bool WaitForResult;

        /// <summary>
        /// Return true while has m_duringTransit to do.
        /// </summary>
        /// <returns></returns>
        protected virtual bool PrepareJob()
        {
            if (WaitForResult)
                return false;

            if (MissionQueue != null && MissionQueue.Count > 0)
            {
                //Debug.Log(Missions.Count);
                Missions.AddRange(MissionQueue);
                //Debug.Log(Missions.Count);
                MissionQueue.Clear();
            }

            var length = Missions.Count;

            if (length == 0)
                return false;

            var validMissionNums = 0;
            var requireHits = 0;

            if (ValidMission.Count != length)
                ValidMission.Capacity = length;

            for (int i = 0; i < length; i++)
            {
                var mission = Missions[i];

                var missionValid = mission.ReadyToRaycast(out var hitNums);

                ValidMission[i] = missionValid;

                if (!missionValid)
                {
                    Debug.LogError(mission + " invalid.");
                    continue;
                }

                validMissionNums++;
                requireHits += hitNums;



            }

            {
                var valid = validMissionNums > 0;

                Assert.IsTrue(requireHits > 0 == valid);

                if (valid)
                {
                    ValidMissionNums = validMissionNums;
                    RequireHitNums = requireHits;
                }
                //Debug.Log(valid);
                return valid;
            }
        }



        protected virtual void StartJob()
        {
            Commands = new(ValidMissionNums, Allocator.TempJob);
            Results = new(RequireHitNums, Allocator.TempJob);

            //Debug.Log(Results.Length +" ** "+ RequireHitNums);

            var length = Missions.Count;
            var validSN = 0;

            for (int i = 0; i < length; i++)
            {
                if (!ValidMission[i])
                    continue;

                Assert.IsTrue(validSN < ValidMissionNums);

                var mission = Missions[i];
                Commands[validSN] = mission.GetRaycastCommand();
                validSN++;
            }

            //Debug.Log(ValidMissionNums);

            Handle = RaycastCommand.ScheduleBatch(Commands, Results, ValidMissionNums);

            WaitForResult = true;
            //Handle.Complete();


            //for (int i = 0; i < validSN; i++)
            //{
            //    var result = Results[i];

            //    if (result.collider == null)
            //        continue;

            //    Debug.Log(string.Format("Hit {0} at {1} meter", result.collider,result.distance));
            //}
        }


        //[Button]
        protected virtual void FinishJob()
        {
            curDelay = 0;

            if (!WaitForResult)
                return;

            Assert.IsFalse(ValidMissionNums <= 0);

            if (!Handle.IsCompleted)
                return;

            Handle.Complete();


            var length = Missions.Count;
            //var resultSN=0;

            var results = Results.GetEnumerator();

            //Debug.Log(Results.Length);

            //using(var missions = Missions.CacheLoop())
            //{
            //    foreach (var mission in missions)
            //    {

            //    }
            //}

            for (int i = 0; i < length; i++)
            {
                //Debug.LogError(ValidMission[i]);

                if (!ValidMission[i])
                    continue;

                var mission = Missions[i];
                //var requireHits=mission.GetMaxHitResultNums;

                //if (requireHits <= 0)
                //    continue;

                //var array = new RaycastHit[requireHits];

                //for (int k = resultSN; k < resultSN+requireHits; k++)
                //{
                //    array[i] = Results[k];
                //}

                //resultSN += requireHits;


                if (mission.FinishTrajectory(results))
                {
                    //Debug.Log(Missions.Count);
                    Missions.RemoveAt(i);
                    //Debug.Log(Missions.Count);
                    ValidMission.RemoveAt(i);
                    ValidMissionNums--;
                    length--;
                    i--;
                }
            }

            if (Commands.IsCreated)
                Commands.Dispose(Handle);

            if (Results.IsCreated)
                Results.Dispose(Handle);

            WaitForResult = false;
        }

        #endregion



        #region Debug

#if UNITY_EDITOR

        [BoxGroup("Test")]
        public bool DebugDrawRay=true;

        private void OnGUI()
        {
            if (!DebugDrawRay)
                return;

            foreach (var hit in hits)
            {
                if (!hit.collider)
                    continue;

                UnityEditor.Handles.Label(hit.point,hit.collider.name);
            }
        }

        List<RaycastHit> debugHits=new();

        //[BoxGroup("Test")]
        //public List<Transform> StartPoints; 

        [BoxGroup("Test")]
        public LayerMask Layer=Physics.AllLayers;

        [BoxGroup("Test")]
        [SerializeField]
        int MaxCastCount=4;
        
        [BoxGroup("Test")]
        [Button]
        void TestCast()
        {
            //Missions = new(StartPoints.Select(width =>
            //new FirearmsTrajectoryBatchMission(width.position)
            //{
            //    Speed=10000,
            //    RaycastDirection=-width.up,
            //    RaycastMask = Layer,
            //    MaxDistance=1000
            //}));

            var ray = new Ray(transform.position, transform.forward);

            var mission = FirearmsBallisticTrajectoryMission.Create(ray.origin, ray.direction);

            mission.Speed = 600;
            mission.RaycastMask = Layer;
            mission.MaxDistance = 900;
            mission.MaxRaycastNumber = MaxCastCount;

            AddMission(mission);

            //var mission = new FirearmsBallisticTrajectoryMission(ray)
            //{
            //    Speed = 600,
            //    RaycastMask = Layer,
            //    MaxDistance = 1000,
            //    MaxRaycastNumber=4,
            //};

            //MissionQueue.Add(mission);

            //PrepareJob();

            //Profiler.BeginSample("CastJobTest");

            //StartJob();

            //Profiler.EndSample();

            //FinishJob();
        }


        //[BoxGroup("Test")]
        //[SerializeField]
        //GameObject ob;

        //[BoxGroup("Test")]
        //[Button]
        //void TestID()
        //{
        //    Debug.Log(ob.GetInstanceID());


        //    for (int i = 0; i < 10; i++)
        //    {
        //        var obj = Instantiate(ob);

        //        Debug.Log(obj.GetInstanceID());

        //        obj.hideFlags = HideFlags.DontSave;
        //    }
        //}

#endif
        #endregion



    }
}