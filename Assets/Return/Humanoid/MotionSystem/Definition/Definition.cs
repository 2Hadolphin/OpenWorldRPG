using UnityEngine;
using System;
using Unity.Collections;
using Unity.Jobs;

namespace Return.Humanoid
{
    [Serializable]
    public struct LocomotionPoint
    {
        public enum Process { Empty = 0, KeyIn = 1, Sensor = 2 }
        public Process process;
        public Vector3 Position;
        public Vector3 Velocity;
        public Quaternion BodyDirection; //Cam
        public Quaternion AngularVelocity;
        public int BundleNumber;

        public static LocomotionPoint operator +(LocomotionPoint a, LocomotionPoint b)
        {
            return new LocomotionPoint
            {
                process = Process.KeyIn,
                Position = a.Position += b.Position,
                BodyDirection = a.BodyDirection *= b.BodyDirection,
                BundleNumber = a.BundleNumber += b.BundleNumber,
                Velocity = a.Velocity += b.Velocity,
            };

        }


    }

    public struct LocomotionChannelJob : IJobParallelFor
    {
        public NativeArray<LocomotionPoint> LocomotionChannel;
        public Vector3 Position;
        public Quaternion Rotation;
        public float BodyRadius;
        public int MiddleIndex;

        public float Inertia;

        public void Execute(int index)
        {
            var point = LocomotionChannel[index];

            if (index > MiddleIndex)
            {

            }
            else if (index < MiddleIndex)
            {

            }
            else
            {

            }


        }





    }



    public enum Motion_Main
    {
        Null,
        /// <summary>
        /// Moving on foot
        /// </summary>
        Locomotion,
        /// <summary>
        /// Moving by limbs
        /// </summary>
        Climb,
        /// <summary>
        /// Body Grounded
        /// </summary>
        Lie,
        /// <summary>
        /// Off land
        /// </summary>
        Jump,
        Fall,
        /// <summary>
        /// Standup etc
        /// </summary>
        Rebalance,
        /// <summary>
        /// Driving
        /// </summary>
        Operate
    }
    [Flags]
    public enum Motion_Foot
    {
        Null = 0,
        Walk = 1,
        Squat = 2,
        Lie = 3,
        Swim = 4,
        Climb = 5
    }
    public struct RigState
    {
        public Vector3 LoaclSpeed;
        public bool Grounded;
        public bool Straf;
        public float RotateDegree;
        public float Preflutter;
    }
    public enum HumanoidRigState { Static, Inertia, Interact }




}