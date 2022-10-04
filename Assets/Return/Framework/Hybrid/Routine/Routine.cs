using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;
using System.Linq;
using System.Threading.Tasks;

namespace Return
{
    /// <summary>
    /// Class to invoke game execute phase.
    /// </summary>
    [HideMonoScript]
    [DefaultExecutionOrder(ExecuteOrderList.Framework)]
    public class Routine : BaseComponent //,IStart //IInitializable
    {
#if UNITY_EDITOR
        protected override string EditorLabel => "Apply custom update phase";
#endif

        #region Setup

        protected virtual void Awake()
        {
            if (transform.root == null)
                DontDestroyOnLoad(this.gameObject);

            Initialize();
        }

        public void Initialize()
        {
            InstanceIfNull<UpdateNormal>();
            InstanceIfNull<UpdateEarly>();
            InstanceIfNull<UpdateLast>();
        }

        #endregion


        #region Routines

        #region Start

        public static void AddStartable(IStart start)
        {
            if(!Starts.Contains(start))
                Starts.Enqueue(start);
        }

        readonly static Queue<IStart> Starts = new(100);

        protected void Update()
        {
            if (Starts.Count != 0)
            {
                var count = Starts.Count;

                while (count > 0 && Starts.TryDequeue(out var start))
                {
                    count--;

                    try
                    {
                        if (start is MonoBehaviour mono)
                        {
                            // check destroy
                            if (!mono)
                            {
                                Debug.LogError($"Startable object invalid {start}.");
                                continue;
                            }

                            // check active
                            if (!mono.isActiveAndEnabled)
                            {
                                Starts.Enqueue(start);
                                continue;
                            }
                        }

                        start?.Initialize();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                    }
                }
            }

        }

        #endregion

        #region FixedUpdate

        static readonly NotPepeatEvent m_OnFixedUpdateEarly = new();
        /// <summary>
        /// Fixed update when <see cref="ExecuteOrderList.Early"/>.
        /// </summary>
        public static UEvent OnFixedUpdateEarly => m_OnFixedUpdateEarly;

        static readonly NotPepeatEvent m_OnFixedUpdateLast = new();
        /// <summary>
        /// Fixed update when <see cref="ExecuteOrderList.Late"/>.
        /// </summary>
        public static UEvent OnFixedUpdateLast => m_OnFixedUpdateLast;

        static readonly NotPepeatEvent m_OnFixedUpdate = new();
        protected static UEvent OnFixedUpdate => m_OnFixedUpdate;


        readonly static List<IFixedTick> OrderFixedTicks_Early = new(100);
        readonly static List<IFixedTick> OrderFixedTicks_Last = new(100);

        static public void AddLateTickable(IFixedTick tick)
        {
            if (tick is IOrder orderable)
            {
                // insert order queue
                if (orderable.GetExecuteOrder() < 0)
                    PushTick(OrderFixedTicks_Early, orderable);
                else
                    PushTick(OrderFixedTicks_Last, orderable);
            }
            else
            {
                OnFixedUpdate.Subscribe(tick.FixedTick);
                //FixedTicks.Add(tick);
            }
        }

        static public void RemoveTick(IFixedTick tick)
        {
            if (tick is IOrder orderable)
            {
                // insert order queue
                if (orderable.GetExecuteOrder() < 0)
                    OrderFixedTicks_Early.Remove(tick);
                else
                    OrderFixedTicks_Last.Remove(tick);
            }
            else
            {
                OnFixedUpdate.Unsubscribe(tick.FixedTick);
            }
        }

        #endregion

        #region Update

        
        static readonly NotPepeatEvent m_OnUpdateEarly = new();
        /// <summary>
        /// Update when <see cref="ExecuteOrderList.Early"/>.
        /// </summary>
        public static UEvent OnUpdateEarly => m_OnUpdateEarly;

        static readonly NotPepeatEvent m_OnUpdateLast = new();
        /// <summary>
        /// Update when <see cref="ExecuteOrderList.Late"/>.
        /// </summary>
        public static UEvent OnUpdateLast => m_OnUpdateLast;

        static readonly NotPepeatEvent m_OnUpdate = new();
        public static UEvent OnUpdate => m_OnUpdate;

        //readonly static List<ITick> Ticks = new(200);
        readonly static List<ITick> OrderTicks_Early = new(100);
        readonly static List<ITick> OrderTicks_Last = new(100);

        static public void AddTickable(ITick tick)
        {
            if (tick is IOrder orderable)
            {
                // insert order queue
                if (orderable.GetExecuteOrder() < 0)
                    PushTick(OrderTicks_Early, orderable);
                else
                    PushTick(OrderTicks_Last, orderable);
            }
            else
            {
                OnUpdate.Subscribe(tick.Tick);
                //Ticks.Add(tick);
            }
        }

        static public void RemoveTick(ITick tick)
        {
            if (tick is IOrder orderable)
            {
                // insert order queue
                if (orderable.GetExecuteOrder() < 0)
                    OrderTicks_Early.Remove(tick);
                else
                    OrderTicks_Last.Remove(tick);
            }
            else
            {
                OnUpdate.Unsubscribe(tick.Tick);
            }
        }

        #endregion

        #region LateUpdate

        static readonly NotPepeatEvent m_OnLateUpdateEarly = new();
        /// <summary>
        /// Late update when <see cref="ExecuteOrderList.Early"/>.
        /// </summary>
        public static UEvent OnLateUpdateEarly => m_OnLateUpdateEarly;

        static readonly NotPepeatEvent m_OnLateUpdateLast = new();
        /// <summary>
        /// Late update when <see cref="ExecuteOrderList.Late"/>.
        /// </summary>
        public static UEvent OnLateUpdateLast => m_OnLateUpdateLast;

        static readonly NotPepeatEvent m_OnLateUpdate = new();
        public static UEvent OnLateUpdate => m_OnLateUpdate;

        readonly static List<ILateTick> OrderLateTicks_Early = new(100);
        readonly static List<ILateTick> OrderLateTicks_Last = new(100);

        static public void AddLateTickable(ILateTick tick)
        {
            if (tick is IOrder orderable)
            {
                // insert order queue
                if (orderable.GetExecuteOrder() < 0)
                    PushTick(OrderLateTicks_Early, orderable);
                else
                    PushTick(OrderLateTicks_Last, orderable);
            }
            else
            {
                OnLateUpdate.Subscribe(tick.LateTick);
            }
        }

        static public void RemoveTick(ILateTick tick)
        {
            if (tick is IOrder orderable)
            {
                // insert order queue
                if (orderable.GetExecuteOrder() < 0)
                    OrderLateTicks_Early.Remove(tick);
                else
                    OrderLateTicks_Last.Remove(tick);
            }
            else
            {
                OnLateUpdate.Unsubscribe(tick.LateTick);
            }
        }

        #endregion

        #region GUI

        static readonly NotPepeatEvent m_OnGUI = new();
        public static UEvent GUI => m_OnUpdate;

        private void OnGUI()
        {
            m_OnGUI.Invoke();
        }

        #endregion

        #region Gizmos

        static readonly NotPepeatEvent m_OnGizmos = new();
        public static UEvent Gizmos => m_OnGUI;

        private void OnDrawGizmos()
        {
            m_OnGizmos.Invoke();
        }


        #endregion

        #region Quit



        /// <summary>
        /// Game quitting.
        /// </summary>
        public static bool quitting { get; protected set; } = false;

        private void OnApplicationQuit()
        {
            quitting = true;
            Debug.Log(nameof(OnApplicationQuit));
        }

        #endregion

        #endregion



        #region Sort


        /// <summary>
        /// Add tickable to list and sort the queue.
        /// </summary>
        static public void PushTick(IList list, IOrder tick)
        {
            var length = list.Count;
            var index = length;

            for (int i = 0; i < length; i++)
            {
                if (list[i] is IOrder order && tick.CompareTo(order) > 0)
                {
                    index = i;
                    break;
                }
            }

            list.Insert(index, tick);
        }


        static void Tick(IList<ITick> list)
        {
            var length = list.Count;

            bool invalid;
            int i = 0;

            do
            {
                try
                {
                    for (; i < length; i++)
                    {
                        list[i].Tick();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    list.RemoveAt(i);
                    invalid = true;
                }
                finally
                {
                    invalid = false;
                }
            }
            while (invalid && i < length);
        }

        static void Tick(IList<IFixedTick> list)
        {
            var length = list.Count;

            bool invalid;
            int i = 0;

            do
            {
                try
                {
                    for (; i < length; i++)
                    {
                        list[i].FixedTick();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    list.RemoveAt(i);
                    invalid = true;
                }
                finally
                {
                    invalid = false;
                }
            }
            while (invalid && i < length);
        }

        static void Tick(IList<ILateTick> list)
        {
            var length = list.Count;

            bool invalid;
            int i = 0;

            do
            {
                try
                {
                    for (; i < length; i++)
                    {
                        list[i].LateTick();
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    list.RemoveAt(i);
                    invalid = true;
                }
                finally
                {
                    invalid = false;
                }
            }
            while (invalid && i < length);
        }


        #endregion

        #region Order Routine

        [DefaultExecutionOrder(0)]
        protected class UpdateNormal : BaseComponent
        {
            private void FixedUpdate()
            {
                m_OnFixedUpdate.Invoke();
            }

            private void Update()
            {
                m_OnUpdate.Invoke();
            }

            private void LateUpdate()
            {
                m_OnLateUpdate.Invoke();
            }
        }

        [DefaultExecutionOrder(ExecuteOrderList.Early)]
        protected class UpdateEarly : BaseComponent
        {
            private void FixedUpdate()
            {
                m_OnFixedUpdateEarly.Invoke();
            }

            private void Update()
            {
                m_OnUpdateEarly.Invoke();
            }

            private void LateUpdate()
            {
                m_OnLateUpdateEarly.Invoke();
            }
        }


        [DefaultExecutionOrder(ExecuteOrderList.Late)]
        protected class UpdateLast : BaseComponent
        {
            private void FixedUpdate()
            {
                m_OnFixedUpdateLast.Invoke();
            }

            private void Update()
            {
                m_OnUpdateLast.Invoke();
            }

            private void LateUpdate()
            {
                m_OnLateUpdateLast.Invoke();
            }    
        }

        #endregion

        #region Events

        public abstract class UEvent
        {
            protected abstract event Action performed;

            public virtual void Subscribe(Action action) => performed += action;

            public virtual void Unsubscribe(Action action) => performed -= action;
        }

        /// <summary>
        /// 
        /// </summary>
        private class NotPepeatEvent : UEvent
        {
            private Action m_Event;

            protected override event Action performed
            {
                add
                {
                    m_Event -= value;
                    m_Event += value;
                }
                remove => m_Event -= value;
            }


            public void Invoke()
            {
                m_Event?.Invoke();
            }
        }

        #endregion
    }


}
