using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Jobs;
using Unity.Collections;
using Return;
using System;
using System.Linq;
using Return.Physical;

namespace Return.Items.Weapons
{
    public interface IBaseTrajectoryMission
    {
        Vector3 StartPoint { get; }
        Vector3 StartDirection { get; }


        /// <summary>
        /// CheckAdd mission is ready to do or not.
        /// </summary>
        /// <param name="validHitNums">Number of require hit count.</param>
        /// <returns>Return true if valid.</returns>
        bool ReadyToRaycast(out int validHitNums);
    }

    public interface ITrajectoryMission: IBaseTrajectoryMission
    {
        Ray GetRay();
        float MaxDistance { get; }
        LayerMask Layer { get; }
        QueryTriggerInteraction triggerInteraction { get; }
        //bool FinishTrajectory(RaycastHit[] hit/*, uint[] orders*/, int validNum);
        bool FinishTrajectory(List<RaycastHit> hit);

    }

    public interface ITrajectoryBatchMission: IBaseTrajectoryMission
    {
        RaycastCommand GetRaycastCommand();
        //bool FinishTrajectory(params RaycastHit[] hit);
        bool FinishTrajectory(IEnumerator<RaycastHit> hits);

    }

    public abstract class BaseTrajectoryMission: IBaseTrajectoryMission
    {
        protected virtual void Init(Vector3 origin,Vector3 direction)
        {
            ray = new(origin, direction);
            StartPoint = origin;
            StartDirection = direction;
        }

        protected Ray ray;

        public virtual Vector3 StartPoint { get; protected set; }
        public virtual Vector3 StartDirection { get; protected set; }

        public virtual float MaxDistance { get; set; }

        public virtual int GetMaxHitResultNums => MaxRaycastNumber;

        public int MaxRaycastNumber;


        /// <summary>
        /// Targetedable mask
        /// </summary>
        public LayerMask RaycastMask;

        /// <summary>
        /// Return remaining valid quantity of raycast hit.
        /// </summary>
        /// <param name="hitNums"> Equal to GetMaxHitResultNums - hitCounts </param>
        /// <returns></returns>
        public abstract bool ReadyToRaycast(out int hitNums);
    }


    public abstract class TrajectoryMission : BaseTrajectoryMission,ITrajectoryMission
    {
        public virtual Ray GetRay() => ray;

        public LayerMask Layer => RaycastMask;

        public virtual QueryTriggerInteraction triggerInteraction { get; set; } = QueryTriggerInteraction.Collide;

        //public abstract bool FinishTrajectory(RaycastHit[] hit/*, uint[] orders*/, int validNum);
        public abstract bool FinishTrajectory(List<RaycastHit> hit);
    }

    [System.Serializable]
    public abstract class BatchTrajectoryMission: BaseTrajectoryMission,ITrajectoryBatchMission
    {
        protected BatchTrajectoryMission() { }


        public abstract RaycastCommand GetRaycastCommand();

        //public abstract bool FinishTrajectory(params RaycastHit[] hit);

        /// <summary>
        /// Pass hit result and _checkCache mission valid.
        /// </summary>
        /// <param name="hits">OnSensorUpdate system hit result.</param>
        /// <returns>Is mission finish.</returns>
        public abstract bool FinishTrajectory(IEnumerator<RaycastHit> hits);
    }

    /// <summary>
    /// Non-ballistic => shot gun or explosive
    /// </summary>
    [Obsolete] //=> remake out raycast command to shot gun
    public class FirearmsTrajectoryBatchMission:BatchTrajectoryMission
    {
        public FirearmsTrajectoryBatchMission(Ray _ray)
        {
            Init(_ray.origin, ray.direction);
        }

        public Vector3 StartPosition;

        public float Speed;

        //public Vector3 CurrentPosition;
        //public Vector3 RaycastDirection;

        public float Drag;

        public int MaxRaycastNumber=1;

        float distance = 0;

        int hitCount;

 

        public override Vector3 StartPoint => StartPosition;

        public override Vector3 StartDirection => ray.direction;

        public override int GetMaxHitResultNums => MaxRaycastNumber;

        public uint iterate = 256;

        public override bool FinishTrajectory(IEnumerator<RaycastHit> hits)
        {
            iterate--;

            // cache maxDistance hit distance.
            float max = 0;

            // control recive hit result capacity
            var resultCount = GetMaxHitResultNums-hitCount;

            while (resultCount > 0 && hits.MoveNext())
            {
                resultCount--;
                var hit = hits.Current;

                

                if (hit.collider.IsNull())
                    continue;

                hitCount++;
                max = Mathf.Max(max, hit.distance);

                Debug.Log(hits.Current.collider);
            }

            if (max > 0)
            {
                distance += max;

            }

            var newPos = ray.GetPoint(Speed * ConstCache.deltaTime);
            Debug.DrawLine(ray.origin, newPos, Color.yellow, 15);

            ray.origin = newPos;

            return iterate == 0 || hitCount > GetMaxHitResultNums || distance > MaxDistance;
        }

        public override bool ReadyToRaycast(out int hitNums)
        {
            hitNums = MaxRaycastNumber-hitCount;

            return true;
        }

        public override RaycastCommand GetRaycastCommand()
        {
#if UNITY_EDITOR
          if(!UnityEditor.EditorApplication.isPlaying)
            return new RaycastCommand(StartPoint, StartDirection,MaxDistance, RaycastMask, MaxRaycastNumber);
          else
#endif
            return new RaycastCommand(ray.origin, ray.direction, Speed * ConstCache.deltaTime, RaycastMask, MaxRaycastNumber);

        }

    
    }

    public class FirearmsBallisticTrajectoryMission : TrajectoryMission,IDisposable
    {
        static readonly Stack<FirearmsBallisticTrajectoryMission> MissionPool = new(100);

        public static FirearmsBallisticTrajectoryMission Create(Vector3 origin,Vector3 direction)
        {
            if (!MissionPool.TryPop(out var mission))
                mission = new();

            mission.ResetConfig();
            mission.Init(origin, direction);

            return mission;
        }

        protected virtual void ResetConfig()
        {
            MaxRaycastNumber = 1;
            Speed = 719;
            iterate = 256;


            // state
            distance = 0;
            hitCount = 0;
        }

        public float Speed;

        public uint iterate;


        float distance;

        int hitCount;

        struct orderCache:IComparable<orderCache>
        {
            public int index;
            public float distance;

            public int CompareTo(orderCache other)
            {
                if (distance > other.distance)
                    return 1;
                else
                    return -1;
            }
        }
        public override bool FinishTrajectory(List<RaycastHit> hits)
        {
            iterate--;

            var count = hits.Count;

            var castDistance = Speed * ConstCache.fixeddeltaTime;

            if (count > 0)
            {
                foreach (var hit in hits)
                {
                    Debug.Log(hit.collider.name+hit.collider.GetInstanceID());
                    var surface=hit.collider.GetSurface();
                    if (surface.NotNull() && surface.AllowDecals(hit.triangleIndex))
                        Debug.DrawRay(hit.point, hit.normal, Color.red, 10);
                }

                distance += hits[count-1].distance;
                hitCount += count;
            }


            var newPos = ray.CastPoint(castDistance);
    
            Debug.DrawLine(ray.origin, newPos, Color.yellow, 15);

            ray.origin = newPos;

            return iterate == 0 || hitCount > GetMaxHitResultNums || distance > MaxDistance;
        }

        public delegate bool CheckBallisticFinish(); 

        public override bool ReadyToRaycast(out int hitNums)
        {
            hitNums = MaxRaycastNumber - hitCount;

            return hitNums>0;
        }

        public void Dispose()
        {
            MissionPool.Push(this);
        }
    }



}