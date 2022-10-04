using UnityEngine;
using System;

namespace Return
{

    public abstract class VelocityWrapper : NullCheck, IControllerVelocityWrapper,IDisposable
    {
        public Vector3 Velocity;
        public Vector3 CurrentVelocity;
        public virtual bool ApplyGravity { get; set; } = false;
        public virtual bool Override { get; set; } = true;
        

        public abstract void Main(ref Vector3 currentVelocity, float deltaTime);

        public virtual bool Finish(ref Vector3 currentVelocity, float deltaTime)
        {
            return IsFinish;
        }

        public bool IsFinish;
        public virtual void Dispose()
        {
            IsFinish = true;
        }

        public class Acceleration : VelocityWrapper
        {
            public override void Main(ref Vector3 currentVelocity, float deltaTime)
            {
                currentVelocity += Velocity.Multiply(deltaTime);
                CurrentVelocity = currentVelocity;
            }
        }

        /// <summary>
        /// Without time scale, add in once
        /// </summary>
        public class Impulse : VelocityWrapper
        {
            public override void Main(ref Vector3 currentVelocity, float deltaTime)
            {
                currentVelocity += Velocity;
                CurrentVelocity = currentVelocity;
            }

            public override bool Finish(ref Vector3 currentVelocity, float deltaTime)
            {
                return true;
            }
        }

        public class VelocityChange : VelocityWrapper
        {
            public float Damp = 1f;
            public override void Main(ref Vector3 currentVelocity, float deltaTime)
            {
                currentVelocity = Vector3.Lerp(currentVelocity, Velocity, Damp);
            }
        }
    }



}