using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Return;
using UnityEngine.Assertions;
using System.Threading.Tasks;
//using Zenject;
using Cysharp.Threading.Tasks;


namespace Return.Timers
{
    [DefaultExecutionOrder(ExecuteOrderList.Framework)]
    public partial class Timer : BaseComponent,IStart,ITick//,IInitializable,ITickable//: SingletonMono<Timer>
    {
        public static int FrameRate;
        public static float Time;
        public readonly static List<IDeltaTime> DeltaTimer = new();

        public StopWatch GetStopWatch() => new() { TimeStart = Time };

        //public PocketWatch GetPocketWatch()
        //{
        //    var pocketWatch = new PocketWatch(float.MaxValue) { TimeStart = Time };
        //    DeltaTimer.Add(pocketWatch);
        //    return pocketWatch;
        //}

        static Timer Instance;

        void IStart.Initialize()
        {
            Instance = this;
        }

        void ITick.Tick()
        {
            var length = DeltaTimer.Count;
            var deltaTime = UnityEngine.Time.deltaTime;
            for (int i = 0; i < length; i++)
            {
                DeltaTimer[i].AddDeltaTime(deltaTime);
            }
            FrameRate = (int)(1f / deltaTime);


            #region old
            var delta = ConstCache.deltaTime;

            var clocks = Targets.GetEnumerator();

            while (clocks.MoveNext())
            {
                clocks.Current.Clock(delta);
            }

            var frame = ConstCache.Frame;
            var frameCheck = NextFrame.GetEnumerator();
            while (frameCheck.MoveNext())
            {
                var c = frameCheck.Current;
                if (c.TargetFrame == frame)
                    c.Action();
            }
            #endregion
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="delegate"></param>
        /// <param name="performanceProtect">only invoke while frame rate higher than performanceFrameRate</param>
        public static async void CoroutineDelegate(ICoroutineDelegate @delegate, int executeTimes = 1, int performanceFrameRate = -1)
        {
            Assert.IsTrue(executeTimes >= 1);
            if (performanceFrameRate > 0)
                RunDelegate_Update_Performance(@delegate, executeTimes, performanceFrameRate);
            //Instance.StartCoroutine(m_CoroutineDelegatePerformance(@delegate, executeTimes, performanceFrameRate));
            else
                RunDelegate_Updatae(@delegate, executeTimes);
                //Instance.StartCoroutine(m_CoroutineDelegate(@delegate, executeTimes));
        }

        //static IEnumerator m_CoroutineDelegatePerformance(ICoroutineDelegate @delegate, int executeTimes, int frameRate)
        //{
        //    while (!@delegate.Finish())
        //    {
        //        if (FrameRate > frameRate)
        //            for (int i = 0; i < executeTimes; i++)
        //                @delegate.Execute();

        //        yield return null;
        //    }

        //    yield break;
        //}

        static async void RunDelegate_Update_Performance(ICoroutineDelegate @delegate, int executeTimes, int frameRate)
        {
            while (!@delegate.Finish())
            {
                if (FrameRate > frameRate)
                    for (int i = 0; i < executeTimes; i++)
                        @delegate.Execute();

                await Task.Yield();
            }
        }

        static async void  RunDelegate_Updatae(ICoroutineDelegate coroutineDelegate,int executeTimes)
        {
            while (!coroutineDelegate.Finish())
            {
                for (int i = 0; i < executeTimes; i++)
                    coroutineDelegate.Execute();

                await Task.Yield();
            }
        }

        static IEnumerator m_CoroutineDelegate(ICoroutineDelegate @delegate, int executeTimes)
        {
            while (!@delegate.Finish())
            {
                for (int i = 0; i < executeTimes; i++)
                    @delegate.Execute();

                yield return null;
            }

            yield break;
        }

        /// <summary>
        /// Cache timer clock with special frequency which larger than 1.
        /// </summary>
        protected static Dictionary<int, IAlarm> m_Timer = new();

        public static IAlarm GetTimer(int timesPerSecond = 2)
        {
            Assert.IsTrue(timesPerSecond >= 1);

            if (!m_Timer.TryGetValue(timesPerSecond, out var timer))
            {
                var newTimer = new TimerClock(1f / timesPerSecond);
                
                
                newTimer.StartTick();
                timer = newTimer;
            }

            return timer;
        }



        class clock : IEquatable<clock>
        {
            public clock(Action<float> action)
            {
                GetAction = action;
            }

            public event Action<clock> Delete;
            public readonly Action<float> GetAction;


            public float DuringTime;
            public float CurrentTime = 0;

            public void Clock(float deltaTime)
            {
                CurrentTime += deltaTime;
                if (CurrentTime > DuringTime)
                    if (GetAction == null)
                        Delete.Invoke(this);
                    else
                        GetAction?.Invoke(deltaTime);
            }

            public bool Equals(clock other)
            {
                return GetAction.Equals(other.GetAction);
            }
        }


        HashSet<clock> Targets = new();

        static public void SubscribeUpdate(Action<float> @update, float time)
        {
            var clock = new clock(@update);

            if (Instance.Targets.Add(clock))
                clock.Delete += Instance.DeleteClock;
        }

        static public void UnsubscribeUpdate(Action<float> @update)
        {
            var clock = new clock(@update);
            Instance.DeleteClock(clock);
        }

        void DeleteClock(clock clock)
        {
            if (Targets.Remove(clock))
                clock.Delete -= DeleteClock;
        }



        public static PocketWatch Countdown(float time, bool loop = false)
        {
            if (time < 0)
                time = 0;

            var clock = new PocketWatch(time) { AutoReset = loop };
            Countdown(clock).ToUniTask();
            //Instance.StartCoroutine(Countdown(clock));
            return clock;
        }


        static IEnumerator Countdown(PocketWatch clock)
        {
            do
            {
                switch (clock.TickType)
                {
                    case AdjustUpdateType.Update:
                        yield return null;
                        clock.Clock(ConstCache.deltaTime);
                        break;
                    case AdjustUpdateType.LateUpdate:
                        yield return new WaitForEndOfFrame();
                        clock.Clock(ConstCache.deltaTime);
                        break;
                    case AdjustUpdateType.FixedUpdate:
                        yield return new WaitForFixedUpdate();
                        clock.Clock(ConstCache.fixeddeltaTime);
                        break;

                    default:
                        var wait = new WaitForSeconds(clock.Time);
                        yield return wait;
                        clock.Clock(clock.Time);
                        break;
                }
            }
            while (clock.Qualify);

            yield break;
        }

        #region NextFrame

        HashSet<Callback> NextFrame = new HashSet<Callback>();

        struct Callback
        {
            public Action Action;
            public int TargetFrame;

            public override int GetHashCode()
            {
                return Action.GetHashCode();
            }
        }
        public static void InvokeNextFrame(Action callback, int framecount = 1)
        {
            Instance.NextFrame.Add(
                new()
                {
                    Action = callback,
                    TargetFrame = ConstCache.Frame + framecount
                });
        }

        #endregion


        #region GUI

        //UpdateEvent GUI=new(new EventFactory());


        #endregion



    }

    //public class UpdateEvent : IDisposable
    //{
    //    IDisposablePublisher<float> tickPublisher;

    //    // Subscriber is used from outside so public property
    //    public ISubscriber<float> OnTick { get; }

    //    public UpdateEvent(EventFactory eventFactory)
    //    {
    //        // CreateEvent can deconstruct by tuple and set together
    //        (tickPublisher, OnTick) = eventFactory.CreateEvent<float>();

    //        // also create async event(IAsyncSubscriber) by `CreateAsyncEvent`
    //        // eventFactory.CreateAsyncEvent
    //    }

    //    int count;

    //    void Tick()
    //    {
    //        tickPublisher.Publish(count++);
    //    }

    //    public void Dispose()
    //    {
    //        // You can unsubscribe all from Publisher.
    //        tickPublisher.Dispose();
    //    }
    //}
}

