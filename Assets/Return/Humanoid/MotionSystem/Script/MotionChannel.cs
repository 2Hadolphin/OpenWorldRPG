using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Unity.Collections;

using Unity.Jobs;
using Unity.Collections.LowLevel.Unsafe;

namespace Return.Humanoid
{


    [Serializable]
    public class MotionChannel : MonoBehaviour,IDisposable,IMotionChannel
    {

        event Action IMotionChannel.Update { add => UpdateChannel += value; remove => UpdateChannel -= value; }
        event Action<LocomotionPoint[]> IMotionChannel.ChannelPost { add => UpdatePost += value; remove => UpdatePost -= value; }
        int IMotionChannel.Key_Current => Key_Current;
        LocomotionPoint[] IMotionChannel.ChannelPoints => ChannelData;

        public bool[] HasModify { get => Modify; }

        private void Update()
        {

        }
        private void LateUpdate()
        {
            
        }

        private void OnEnable()
        {
            
            Activate(Capcity);
        }
        private void OnDisable()
        {
            Dispose();
        }

        private void OnDestroy()
        {
      
        }
        public void Activate(int capcity)
        {
            Channel = new NativeArray<LocomotionPoint>(new LocomotionPoint[capcity], Allocator.Persistent);
            ChannelData = new LocomotionPoint[capcity];
            Modify = new bool[capcity];

            Key_Predict = capcity - 1;
            Key_Current = capcity / 2;

            Capcity = capcity;
            enabled = true;
        }

        [NativeDisableContainerSafetyRestriction]
        [NativeDisableParallelForRestriction]
        protected NativeArray<LocomotionPoint> Channel;
        protected int Capcity=40;
        public int Key_Predict;
        /// <summary>
        /// Freeze Key_Current from m_duringTransit edit
        /// </summary>
        public int Key_Current;
        protected bool[] Modify;

        public JobHandle Handle_Predict;
        public JobHandle Handle_MergeRecord;

        protected LocomotionPoint[] ChannelData;
        public LocomotionPoint[] GetChannelData { get => ChannelData; }

        protected Bounds[] Space;
        public Bounds[] Bounds { get => Space; }
        public Action UpdateChannel { get; private set; }
        public Action<LocomotionPoint[]> UpdatePost { get; private set; }
        public void Extract()
        {
            if (!Handle_Predict.IsCompleted || !Handle_MergeRecord.IsCompleted)
                return;

            Handle_Predict.Complete();
            Handle_MergeRecord.Complete();
            ExtractPredict();
            ExtractRecord();
            UpdateChannel?.Invoke();

            while (KeyIn.Count > 0)
            {
                var rig = KeyIn.Dequeue();
                KeyIn_(rig.PR.Position, rig.PR.Rotation, rig.Velocity, rig.Acceleration);
            }


        }
        protected void ExtractPredict()
        {
            for (int i = Key_Current; i <= Key_Predict; i++)
            {
                var newPoint= Channel[i];
                if (Channel[i].Equals(ChannelData[i]))//?
                {
                    Modify[i] = false;
                    continue;
                }

                ChannelData[i] = newPoint;
                Modify[i] = true;
                Debug.DrawRay(newPoint.Position, Vector3.up);
            }
        }
        protected void ExtractRecord()
        {
            for (int i = 0; i < Key_Current; i++)
            {
                var newPoint = Channel[i];
                if (Channel[i].Equals(ChannelData[i]))
                {
                    Modify[i] = false;
                    continue;
                }

                ChannelData[i] = newPoint;
                Modify[i] = true;
                Debug.DrawRay(newPoint.Position / newPoint.BundleNumber, Vector3.up);
            }
        }


        public Queue<Return.RigState> KeyIn=new Queue<Return.RigState>();



        protected void KeyIn_(Vector3 position, Quaternion rotation, Vector3 velocity, Vector3 acceleration)
        {
            var point = new LocomotionPoint()
            {
                Position = position,
                BodyDirection = rotation,
                Velocity = velocity,
                process = LocomotionPoint.Process.KeyIn,
                BundleNumber = 1,
            };

            Handle_Predict.Complete();
            Handle_MergeRecord.Complete();


            Channel[Key_Current] = point;


            //start predict m_duringTransit
            if (Handle_Predict.IsCompleted)
            {

                //ExtractPredict();

                var predictNums = Key_Predict - Key_Current;
                var predictJob = new PredictJob()
                {
                    Points = Channel.GetSubArray(Key_Current + 1, predictNums),
                    CurrentPoint = position,
                    InertiaPoint = position + point.Velocity,
                    PredictPoint = position + acceleration,
                    length = predictNums,
                };

                Handle_Predict = predictJob.Schedule(predictNums, 3);
            }




            //start merge record m_duringTransit
            if (Handle_MergeRecord.IsCompleted)
            {
                //ExtractRecord();

                var mergeNums = Key_Current;
                var mergeJob = new MergeJob()
                {
                    Points = Channel.GetSubArray(0, mergeNums + 1),
                    Pin = Key_Current,
                };

                Handle_MergeRecord = mergeJob.Schedule(Handle_Predict);
            }

        }

        public struct PredictJob : IJobParallelFor
        {
            public NativeArray<LocomotionPoint> Points;
            public Vector3 CurrentPoint;
            public Vector3 InertiaPoint;
            public Vector3 PredictPoint;
            public int length;
            public void Execute(int index)
            {
                var p = (float)index / length;

                var point = new LocomotionPoint()
                {
                    BundleNumber = 1,
                    Position = Bezier.EvaluateQuadratic(CurrentPoint, InertiaPoint, PredictPoint, p),
                    Velocity = Vector3.Lerp(InertiaPoint - CurrentPoint, PredictPoint - CurrentPoint, p),
                    process = LocomotionPoint.Process.KeyIn,
                };

                Points[index] = point;
            }
        }
        public struct MergeJob : IJob
        {
            public NativeArray<LocomotionPoint> Points;
            public int Pin;

            public void Execute()
            {
                var length = Points.Length - 1;

                var sn = 1;

                bool carry = true;
                LocomotionPoint carryPoint = Points[length];

                var i = Pin-1;
                var nums = length;

                bool CheckRound = false;

            SecondRound:



                for (; i >0; i--)
                {
                    var point = Points[i];

                    if (point.BundleNumber > sn)
                    {
                        if (carry)
                            Points[i] = carryPoint;
                        else
                            carry = true;

                        carryPoint = point;
                    }
                    else if (carry)
                    {
                        Points[i] += carryPoint;
                        carry = false;
                    }
                    sn++;
                }


                return;
                i = 0;
                nums = Pin;

                if (!CheckRound)
                {
                    CheckRound = true;
                    goto SecondRound;
                }

            }
        }

        public void Dispose()
        {
            
            Handle_Predict.Complete();
            Handle_MergeRecord.Complete();

            Channel.Clean();
        }
    }

    public interface IMotionChannel
    {
        public event Action Update;
        public event Action<LocomotionPoint[]> ChannelPost;
        public LocomotionPoint[] ChannelPoints { get; }
        public int Key_Current { get; }

    }
}