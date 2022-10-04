using System;
using System.Collections;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Linq;
using Return.Framework.Pools;
using System.Collections.Generic;

namespace Return
{

    public class ConstPool
    {
        private static InstancePool ps_pool;

        public ConstPool()
        {
            ps_pool = new(100);
        }

        public static T GetItem<T>(T key) where T : class
        {
            return ps_pool.GetItem(key);
        }

        public static void SetStorage(object key, int capacity=1)
        {
            ps_pool.SetStorage(key, capacity);
        }

        public static void Storage<T>(object key, T item)
        {
            ps_pool.Storage(key, item);
        }

        public static bool TryGetStorage<T>(object key,out T item) where T:class
        {
            item = ps_pool.GetItem(key) as T;
            return item.IsNull();
        }
    }



    public static class GCPool
    {
        static Dictionary<object, DictionaryList<Object, Object>> cacheObjects=new(200);


        /// <summary>
        /// 
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="prefab"></param>
        /// <param name="instance"></param>
        public static void Register(object owner,Object prefab, Object instance)
        {
            if(!cacheObjects.TryGetValue(owner, out var items))
            {
                items = new();
                cacheObjects.Add(owner, items);
            }

            items.Add(prefab, instance);
        }





    }
}
