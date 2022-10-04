using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
namespace Return.Items
{
    public abstract class Container<T> : MonoItemModule
    {
        /// <summary>
        /// Max amount.
        /// </summary>
        public virtual int Capacity { get; set; }

        /// <summary>
        /// Current amount.
        /// </summary>
        public virtual int Amount { get; }

        /// <summary>
        /// Maximum capacity.
        /// </summary>
        public virtual float MaxVolume { get; set; }
        public virtual float Volume { get; }

        /// <summary>
        /// Maximum carrying weight
        /// </summary>
        public virtual float MaxWeight { get; set; }
        public virtual float Weight { get; }

        public abstract bool Search(Func<T, bool> find,out int sn);
        public abstract T Withdraw(int index);

        /// <summary>
        /// Store item and return index.
        /// </summary>
        /// <param name="object"></param>
        /// <returns></returns>
        public abstract int Store(T @object);

        public abstract void Clear();


    }

}