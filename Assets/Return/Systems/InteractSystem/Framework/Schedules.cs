using UnityEngine;
using System;
using Return.Items;
using Return.Equipment.Slot;

namespace Return.Agents
{
    /// <summary>
    /// Control stroke of hand slot.
    /// </summary>
    [Serializable]
    public class Schedules
    {
        public Schedules(Slot _slot, Symmetry side)
        {
            Slot = _slot;
            Side = side;
        }

        public readonly Slot Slot;
        public readonly Symmetry Side;

        public mQueue<Plan> Plans = new(3);

        /// <summary>
        /// Show case => deactivate item
        /// </summary>
        public IPickup Item => Plans.Peek()?.Item;


        /// <summary>
        /// Is this handle support others and free.
        /// </summary>
        public bool IsSupport;

        /// <summary>
        /// CheckAdd whether handle is holding object.
        /// </summary>
        public bool IsEmpty => IsSupport ? true : Item == null;

        /// <summary>
        /// Return true if has plan to do.
        /// </summary>
        public bool HasSchedule => Plans.Count > 0;

        /// <summary>
        /// Empty & non schedule
        /// </summary>
        public bool IsFree { get => IsEmpty && !HasSchedule; }

        /// <summary>
        /// CheckAdd handle is nothing to do.
        /// </summary>
        public bool IsFinalFree
        {
            get
            {
                if (Plans.Count == 0)
                    return IsEmpty;

                var stroke = Plans.Last.StrokeType;

                if (IsEmpty)
                {
                    if (stroke == StrokeType.Equip || stroke == StrokeType.Pickup)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (stroke == StrokeType.Drop || stroke == StrokeType.Store)
                        return true;
                    else
                        return false;
                }

            }
        }

        public bool Interruptible = true;
        public int AdaptHash;

        /// <summary>
        /// return true if schedule is ready to execute 
        /// </summary>
        public bool Slove(float delta, out float ratio, out Plan dependent)
        {
            if (HasSchedule)
            {
                var plan = Plans.Peek();
                var log = plan.TimeLog;
                var time = plan.Time;
                plan.TimeLog = Mathf.MoveTowards(log, time, delta);
                ratio = log / time;
                plan.TimeLog = log;

                dependent = plan.Dependent;
                if (log < time)
                    return false;
                else
                    return true;
            }

            ratio = default;
            dependent = null;
            return false;
        }
    }


}
