using System.Collections;
using UnityEngine;
using System;
using Return.Database;

namespace Return.Framework.Pools
{
    /// <summary>
    /// Basic class of pool.
    /// </summary>
    public abstract class DynamicPool : DataEntity,IPool
    {
        [SerializeField]
        protected int m_RequirePoolNumber;
        public int RequirePoolNumber { get => m_RequirePoolNumber; protected set => m_RequirePoolNumber = value; }

        /// <summary>
        /// Type of item storage in this pool.
        /// </summary>
        public abstract Type GetPoolType { get; }

        public abstract void Archive(object obj);

        /// <summary>
        /// Add pool requirement
        /// </summary>
        public abstract void Prewarm(int count);

        /// <summary>
        /// Cancel pool requirement.
        /// </summary>
        public abstract void RubPool(int count);



        public virtual void DisposePool()
        {
#if UNITY_EDITOR
            if (UnityEditor.AssetDatabase.IsMainAsset(this))
                return;
            else if (!UnityEditor.EditorApplication.isPlaying)
                DestroyImmediate(this);
            else
#endif
                Destroy(this);
        }

  
    }

    public enum DestroyType
    {
        CurrentGameObject,
        CurrentComponent,
        ParentGameObject,
        RootGameObject,
    }

    /// <summary>
    /// Interface to execute pool archive and unarchive. 
    /// </summary>
    public interface IPool
    {
        void Archive(object obj);
    }

    /// <summary>
    /// Item can been archive.
    /// </summary>
    public interface IArchivable
    {
        void Archive();
        void Unarchive();
    }


    /// <summary>
    /// Interface for poolable item to cache pool reference.
    /// </summary>
    public interface IPoolContain
    {
        /// <summary>
        /// Pool this item belong to.
        /// </summary>
        IPool Pool { get; set; }

        /// <summary>
        /// Storage this item to pool
        /// </summary>
        void Archive(IPool pool=null);
    }

    public interface IInstancePool
    {
        void SetStorage(object key, int capacity);

        void Storage<T>(object key, T item);

        T GetItem<T>(T key) where T : class;
    }

    public interface IPoolable
    {

    }

}
