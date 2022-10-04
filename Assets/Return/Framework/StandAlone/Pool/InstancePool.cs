using UnityEngine;
using System;

namespace Return.Framework.Pools
{
    /// <summary>
    /// Pool to cache custom targets of type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class BundlePool<T> : GenericPool<T> where T : Component
    {
        public override Type GetPoolType => typeof(T);


        protected override T Create()
        {
            if (Factory == null)
                return new GameObject(name + nameof(T)).AddComponent<T>();
            else
                return Factory.Create();
        }

        /// <summary>
        /// Things to do before stock. 
        /// </summary>
        protected override void Purchase(T item)
        {
            item.gameObject.SetActive(false);
            var tf = item.gameObject.transform;
            tf.parent = null;
            tf.position = new(0, -2000);
        }

        protected override void Activate(T item)
        {
            item.gameObject.SetActive(true);
        }

    }
}
