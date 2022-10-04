using Return.Humanoid;
using System;
using UnityEngine;
using Return.Humanoid.IK;
using Return.Creature;

namespace Return
{
    [Serializable]
    public class Stroke : NullCheck, IEquatable<Stroke>
    {
        public Stroke(LimbIKWrapper _wrapper)
        {
            IK = _wrapper;
        }

        public object sender;
        public LimbIKWrapper IK;
        public TwoBoneIKDataArg Data { get; protected set; }

        public Limb Limb => IK.Limb;
        public bool DuringTheTrip { get; protected set; }
        public bool CanInterrupt;
        public bool AutoHandleHintPosition;

        public bool Equals(Stroke other)
        {
            return sender.Equals(other);
        }

        public virtual void StartStroke(TwoBoneIKDataArg data)
        {
            Data = data;
            DuringTheTrip = true;
            data.Evaluate(0);
            Solve(0);
        }

        public virtual void Solve(float deltaTime)
        {
            if (null == Data)
                return;

            Debug.Log(Data.GoalUpdate);
            if (!Data.Evaluate(deltaTime))
            {
                DuringTheTrip = false;
                Data = null;
                Debug.Log("EndStroke");
                return;
            }

            if (Data.GoalUpdate)
            {
                IK.weight = Data.GoalWeight;
                IK.targetPositionWeight = Data.GoalPositionWeight;
                IK.targetRotationWeight = Data.GoalRotationWeight;
                switch (Data.GoalSpace)
                {
                    case Space.World:
                        IK.GoalPosition = Data.GoalPR;
                        IK.GoalRotation = Data.GoalPR;
                        break;
                    case Space.Self:
                        IK.GoalLocalPosition = Data.GoalPR;
                        IK.GoalLocalRotation = Data.GoalPR;
                        break;
                }
            }

            if (Data.HintUpdate)
            {
                IK.hintWeight = Data.HintWeight;
                switch (Data.HintSpace)
                {
                    case Space.World:
                        IK.HintPosition = Data.HintPosition;
                        break;
                    case Space.Self:
                        IK.HintLocalPosition = Data.HintPosition;
                        break;
                }
            }
        }


        /// <summary>
        /// Return true if has verify mission
        /// </summary>
        public bool Valid()
        {
            return DuringTheTrip;
        }

        public bool Ready2Use()
        {
            return !DuringTheTrip || CanInterrupt;
        }
    }
}