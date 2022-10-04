using System;

namespace Return.Timers
{
    public partial class Timer
    {
        //public class PocketWatch : StopWatch, IDeltaTime
        //{
        //    public float PassTime;

        //    public void AddDeltaTime(float deltaTime)
        //    {
        //        PassTime += deltaTime;
        //    }

        //    public void Dispose()
        //    {
        //        DeltaTimer.Remove(this);
        //    }

        //    public override float GetPassTime()
        //    {
        //        return PassTime;
        //    }
        //}


        /// <summary>
        /// Clock 
        /// </summary>
        public class PocketWatch : NullCheck, IDisposable
        {
            public PocketWatch(float time, AdjustUpdateType updateType = AdjustUpdateType.Update)
            {
                Time = time;
                TickType = updateType;
            }

            public bool Qualify = true;
            public bool AutoReset;
            public AdjustUpdateType TickType;

            public event Action OnTimeUp;

            public virtual float Time { get; protected set; }

            public virtual float RestTime { get; protected set; }

            public bool Timeup { get; protected set; }

            internal virtual void Clock(float ping)
            {
                if (Timeup)
                    return;

                RestTime += ping;
                Timeup = RestTime >= Time;

                if (Timeup)
                    OnTimeUp?.Invoke();

                if (AutoReset)
                {
                    RestTime = 0;
                    Timeup = false;
                }

            }

            public virtual float Ratio => RestTime / Time;

            public virtual bool Check()
            {
                var timeUp = Timeup;
                if (timeUp)
                {
                    Reset();
                }

                return timeUp;
            }

            public virtual void Reset()
            {
                Timeup = false;
                RestTime = 0;
            }

            public void Dispose()
            {
                Qualify = false;
            }

        }

    }
}
