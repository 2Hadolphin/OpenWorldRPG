using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Return.Humanoid.IK;

namespace Return
{
    public class IKSchedule : ISchedule
    {
        public IKSchedule(ICoordinate start, ICoordinate stop, float duringTime, bool canInterrupt = true, bool Interlocking = false)
        {
            Start = start;
            Stop = stop;
            DuringTime = duringTime;
            CanInterrupt = canInterrupt;
            ApplyInterlocking = Interlocking;
        }
        public ICoordinate Start { get; protected set; }
        public ICoordinate Stop { get; protected set; }
        public readonly float DuringTime;
        public readonly bool CanInterrupt;
        public readonly bool ApplyInterlocking;
        public bool Processing { get; set; }
        public float TimeLog { get; set; }

        public event Action Begin;
        public event Action Interrupt;
        public event Action Finish;

        public void Init(ILimbIKGoalData bodyIKGoalData)
        {
            Processing = true;

            var coordinate = new Coordinate_Offset(bodyIKGoalData.GoalSpaceTransform, bodyIKGoalData.GoalPosition, bodyIKGoalData.GoalRotation);

            if (Start == null)
                Start = coordinate;

            if (Stop == null)
                Stop = coordinate;

            Begin?.Invoke();
        }

        public void Dispose()
        {
            Finish?.Invoke();
        }

        float ISchedule.DuringTime => DuringTime;
        bool ISchedule.CanInterrupt => CanInterrupt;
        bool ISchedule.ApplyInterlocking => ApplyInterlocking;
    }
    public interface ISchedule : IDisposable
    {
        ICoordinate Start { get; }
        ICoordinate Stop { get; }
        float DuringTime { get; }
        float TimeLog { get; set; }

        event Action Begin;
        event Action Interrupt;
        event Action Finish;
        bool CanInterrupt { get; }
        bool ApplyInterlocking { get; }
        bool Processing { get; }
        void Init(ILimbIKGoalData bodyIKGoalData);
    }

}
