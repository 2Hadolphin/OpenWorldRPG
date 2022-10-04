using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Newtonsoft.Json;
using UnityEngine.Assertions;

namespace Return
{

    /// <summary>
    /// Instance scriptobject with autobuild
    /// </summary>
    public abstract class SingletonSO<T> : Singleton_SO where T : SingletonSO<T>
    {
        [ReadOnly]
        [ShowInInspector]
        private static T m_Instance;

        //[ShowInInspector]
        //[ReadOnly]
        public static T Instance { get=> m_Instance; private set=> m_Instance=value; }


        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        //static void RuntimeInitializeOnLoad()
        //{
        //    Debug.Log(RuntimeInitializeLoadType.BeforeSceneLoad + " Manager Initialize : " + Instance);
        //    LoadInitilization_BeforeSceneLoad();
        //}

        //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        //static void RuntimeInitializeAfterSceneLoad()
        //{
        //    Debug.Log(RuntimeInitializeLoadType.AfterSceneLoad + " Manager Initialize : " + Instance);
        //    LoadInitilization_AfterSceneLoad();
        //}

        ///// <summary>
        ///// Initilize instance.
        ///// </summary>
        //public virtual void LoadInitilization_BeforeSceneLoad() { }


        ///// <summary>
        ///// Initilize instance.
        ///// </summary>
        //public virtual void LoadInitilization_AfterSceneLoad() { }

        /// <summary>
        /// Custom serialize data
        /// </summary>
        public virtual void LoadData() { }

        /// <summary>
        /// Custom serialize data
        /// </summary>
        public virtual void SaveData() { }

        protected virtual void Awake() { }

        protected virtual void OnEnable()
        {
#if UNITY_EDITOR
            //if (!UnityEditor.EditorApplication.isPlaying)
            //    return;
#endif
            if (WhetherThisSingleton(this as T))
                LoadData();
            else
                Destroy();
        }

        /// <summary>
        /// Load Instance
        /// </summary>
        protected static T GetInstance(string managerName=null, bool allowTemporary = true)
        {
            if (!Instance)
            {
                if (string.IsNullOrEmpty(managerName))
                    managerName = nameof(T);

                // Load or create instance
                Instance = Resources.Load<T>(managerName);

                if (Instance == null)
                    Instance = FindObjectOfType<T>(true);

                if (Instance == null && allowTemporary)
                    Instance = CreateInstance<T>();

#if UNITY_EDITOR
                if (!UnityEditor.EditorApplication.isPlaying)
                {
                    Debug.LogError("Failure to load singleton when editor mode : "+Instance);
                }
                else
#endif
                // Initialise
                if (Instance != null)
                    Instance.Initialize();

            }

            return Instance;
        }

        /// <summary>
        /// ?????????
        /// </summary>
        protected static B GetBehaviourProxy<B>() where B : MonoBehaviour
        {
            // Get or create the manager object
            var go = GetSharedGameobject();

            // Add the component
            var result = go.AddComponent<B>();

            return result;
        }

        public virtual bool IsValid()
        {
            return true;
        }

 

        protected virtual void OnDestroy()
        {
            SaveData();
        }



        /// <summary>
        /// Must use this to setting singleton
        /// </summary>
        protected static bool WhetherThisSingleton(object singleton)
        {
#if UNITY_EDITOR
            if (Instance&&singleton!=Instance)
            {
                //Debug.Log(Instance);
                UnityEditor.EditorGUIUtility.PingObject(Instance);
            }
#endif

            if (singleton == Instance)
                return true;

            if (null != Instance)
                return false;


            // CheckAdd alive resources 
            T exist;
            exist = FindObjectOfType<T>();
            if (exist && exist != singleton)
            {
                Instance = exist;
                return false;
            }

            // CheckAdd data resources 
            exist = Resources.Load<T>(nameof(T));
            if (exist && exist != singleton)
            {
                Instance = exist;
                return false;
            }

#if UNITY_EDITOR

            // Create new resource
            //exist = mEditorFileIO.InstanceSO<T>();
            //if (exist)
            //{
            //    Instance = exist;
            //    return false;
            //}
#else
            //exist = ScriptableObject.CreateInstance<T>();
            //if (exist)
            //{
            //    Instance = exist;
            //    return false;
            //}
#endif
            Instance = singleton as T;

            //Debug.Log(singleton);
            return true;

        }


        #region Base

        private static GameObject ps_SharedGameobject = null;

        /// <summary>
        /// Get or create the manager object. ????
        /// </summary>
        protected static GameObject GetSharedGameobject()
        {
            // Create the object if it doesn't exist
            if (ps_SharedGameobject == null)
            {
                ps_SharedGameobject = new GameObject("Manager")
                {
                    layer = Layers.Ignore,
                    tag = Tags.Config
                };
                DontDestroyOnLoad(ps_SharedGameobject);
            }

            return ps_SharedGameobject;
        }

        public static void DisableRuntimeManagers()
        {
            var go = GetSharedGameobject();
            go.SetActive(false);
        }

        public static void EnableRuntimeManagers()
        {
            var go = GetSharedGameobject();
            go.SetActive(true);
        }

        #endregion
    }

    /// <summary>
    /// Load before scene loaded
    /// </summary>
    public abstract class SingletonSOManager<T> : SingletonSO<T> where T : SingletonSOManager<T>
    {

    }


}