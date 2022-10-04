using System;
using UnityEngine;

namespace Return.Framework.Pools
{
    /// <summary>
    /// Universal pool.
    /// </summary>
    public abstract class GenericPool<T> : DynamicPool, IPool<T> where T : class
    {
        public override Type GetPoolType => typeof(T);

        public abstract T Request(bool forceGet = false);
        public virtual T Request(bool activate, bool forceGet)
        {
            var item = Request(forceGet);

            if (item == null)
                Activate(item);

            if (item is IPoolContain poolContain)
                poolContain.Pool = this;

            return item;
        }


        public abstract void Return(T item);


        public IFactory<T> Factory;

        protected virtual T Create()
        {
            if (Factory == null)
            {
                if (GetPoolType.Instance(out var newItem, null))
                    return newItem as T;
                else
                    throw new NotImplementedException($"Pool generate item failure with {typeof(T)}.");
            }
            else
                return Factory.Create();
        }

        /// <summary>
        /// Things to do before stock. 
        /// </summary>
        protected abstract void Purchase(T item);

        protected abstract void Activate(T item);

        public override void Archive(object obj)
        {
            if (obj is not T item)
            {
                Debug.LogException(new InvalidCastException($"{obj} is not a valid item for this pool {this}.\n object type : {obj.GetType()} \n pool type : {GetPoolType}"));
                return;
            }


            Return(item);
        }
    }
}
