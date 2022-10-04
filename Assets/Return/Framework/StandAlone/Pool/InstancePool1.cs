using System.Collections.Generic;

namespace Return.Framework.Pools
{
    public class InstancePool: IInstancePool,IPool
    {
        public InstancePool(int capacity)
        {
            m_cacheObjects = new(capacity);
        }

        private Dictionary<object, Stack<object>> m_cacheObjects;

        protected Dictionary<object, Stack<object>> CacheObjects { get => m_cacheObjects; set => m_cacheObjects = value; }




        public virtual void SetStorage(object key,int capacity)
        {
            if (!CacheObjects.TryGetValue(key, out var storage))
            {
                storage = new Stack<object>(1);
                CacheObjects.Add(key, storage);
            }
        }

        public virtual void Storage<T>(object key,T item)
        {
            if(!CacheObjects.TryGetValue(key,out var storage))
            {
                storage = new Stack<object>(1);
                CacheObjects.Add(key, storage);
            }

            if (item is IArchivable archivable)
                archivable.Archive();

            storage.Push(item);
        }

        public virtual T GetItem<T>(T key) where T : class
        {
            if (CacheObjects.TryGetValue(key, out var storage))
                if (storage.TryPop(out var item))
                    return item as T;

            return null;
        }

        public void Archive(object obj)
        {
            
        }



    }
}
