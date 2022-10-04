using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

namespace Return
{
    /// <summary>
    /// Never inherit this
    /// </summary>
    [DefaultExecutionOrder(ExecuteOrderList.Framework)]
    public abstract class SingletonMonoBehaviour : BaseComponent
    {
        protected abstract void Awake();
    }

    public abstract class SingletonMono<T> : SingletonMonoBehaviour where T : SingletonMono<T>
    {
        public virtual bool crossScene => true;

#if UNITY_EDITOR

        static bool Init;

        public static bool HasInit
        {
            get
            {
                if (Init)
                    return Init;
                else
                    return Instance;
            }
        }
#endif

        public static T Instance
        {
            get
            {
                if (!_Instance)
                {

                    //_Instance = new GameObject(nameof(T)).AddComponent<T>();
                    _Instance = new GameObject(typeof(T).ToString(),typeof(T)).GetComponent<T>();

                    if (_Instance.crossScene)
                    {
                        DontDestroyOnLoad(_Instance);
                        _Instance.gameObject.hideFlags = HideFlags.DontSave;
                    }
                }

                return _Instance;
            }
        }

        [ShowInInspector]
        [ReadOnly]
        static T _Instance;

        protected override void Awake()
        {
            if (_Instance != null && _Instance != this)
            {
                DestroyImmediate(this);
                return;
            }

            if (!_Instance)
                _Instance = (T)this;


            Assert.IsNotNull(_Instance);
            DontDestroyOnLoad(_Instance.transform.root);
        }
    }

    /// <summary>
    /// Preload manager before scene load. **ConstCache **SettingManager **Steam/TNet3
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class SingletonMonoManager<T> : SingletonMono<T> where T: SingletonMonoManager<T>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void RuntimeInitializeOnLoad()
        {
            Instance.LoadInitilization();
            Debug.Log(Instance);
        }

        /// <summary>
        /// Load after awake must be as singleton.
        /// </summary>
        public abstract void LoadInitilization();
      
        protected static void LogSystem(object instance)
        {
            Debug.Log(string.Format("Initilized .. {0}",instance.GetType()));
        }
    }
}