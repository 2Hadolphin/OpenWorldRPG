using Return.Framework.Pools;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

namespace Return
{
    public class StackPool<T> : BundlePool<T> where T : Component
    {
        protected Stack<T> Pool=new();

        public DestroyType DestroyPath;

        public override void Prewarm(int count)
        {
            RequirePoolNumber += count;

            if (Pool == null)
                Pool = new(count);

            // add
            for (int i = 0; i < count; i++)
            {
                var item = Create();
                Purchase(item);
                Pool.Push(item);
            }
        }


        public override void RubPool(int count)
        {
            RequirePoolNumber -= count;

            Assert.IsNotNull(Pool);

            for (int i = 0; i < count; i++)
            {
                var decrape = Pool.Pop();
                DestoryItem(decrape);
            }
        }

        public override T Request(bool forceGet=false)
        {
            if (Pool.TryPop(out var item))
                return item;
            
            if(forceGet)
                return Create();

            Debug.LogWarning(this + " pool out of stock.");
            return null;
        }

        public override void Return(T item)
        {
            Purchase(item);
            Pool.Push(item);
        }

        public virtual void DestoryItem(T item)
        {
            UnityEngine.Object target;

            switch (DestroyPath)
            {
                case DestroyType.CurrentGameObject:
                    target = item.gameObject;
                    break;
                case DestroyType.CurrentComponent:
                    target = item;
                    break;
                case DestroyType.ParentGameObject:
                    target = item.transform.parent.gameObject;
                    break;
                case DestroyType.RootGameObject:
                    target = item.transform; //get root
                    break;

                default:
                    Debug.LogError(new KeyNotFoundException(DestroyPath.ToString()));
                    goto case DestroyType.CurrentGameObject;
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlaying)
                DestroyImmediate(target);
            else
#endif
            Destroy(target);
        }
    }

}
