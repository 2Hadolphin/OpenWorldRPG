using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoSaveGames.Serialization
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public abstract class NeoSerializedScene : MonoBehaviour
    {
        [SerializeField, Tooltip("Prefabs available for serialization in this scene.")]
        private NeoSerializedGameObject[] m_Prefabs = new NeoSerializedGameObject[0];

        [SerializeField, Tooltip("Assets available for serialization in this scene.")]
        private ScriptableObject[] m_Assets = new ScriptableObject[0];

        [SerializeField]//, HideInInspector]
        private NeoSerializedSceneObjectContainer m_SceneObjects = null;

        private static List<NeoSerializedScene> s_ActiveScenes = new List<NeoSerializedScene>();

        private Scene m_Scene = new Scene();
        private int m_HashedPath = 0;

        public Scene scene
        {
            get
            {
                if (!m_Scene.IsValid())
                    m_Scene = gameObject.scene;
                return m_Scene;
            }
        }

        public int hashedPath
        {
            get
            {
                if (m_HashedPath == 0)
                    m_HashedPath = NeoSerializationUtilities.StringToHash(scene.path);
                return m_HashedPath;
            }
        }

        public abstract bool isMainScene
        {
            get;
        }

        public NeoSerializedGameObject[] registeredPrefabs
        {
            get { return m_Prefabs; }
        }

        public ScriptableObject[] registeredAssets
        {
            get { return m_Assets; }
        }

        public NeoSerializedSceneObjectContainer sceneObjects
        {
            get { return m_SceneObjects; }
        }

        protected virtual void Awake()
        {
            if (Application.isPlaying)
            {
                s_ActiveScenes.Add(this);

                // Assign child container
                if (m_SceneObjects == null || !m_SceneObjects.isValid)
                    m_SceneObjects = new NeoSerializedSceneObjectContainer(this);
                m_SceneObjects.Awake();

                // Notify the save manager that the scene is loaded
                SaveGameManager.NotifySceneLoaded(this);
            }
        }
        
        protected virtual void Start()
        {
            //if (Application.isPlaying)
            //    SaveGameManager.RegisterScene(this);
        }

        protected virtual void OnDestroy()
        {
            if (Application.isPlaying)
            {
                s_ActiveScenes.Remove(this);
                m_SceneObjects.OnDestroy();
                SaveGameManager.UnregisterScene(this);
            }
        }

        public static NeoSerializedScene GetByBuildIndex(int buildIndex)
        {
            for (int i = 0; i < s_ActiveScenes.Count; ++i)
            {
                if (s_ActiveScenes[i].scene.buildIndex == buildIndex)
                    return s_ActiveScenes[i];
            }
            return null;
        }

        public static NeoSerializedScene GetByName(string name)
        {
            for (int i = 0; i < s_ActiveScenes.Count; ++i)
            {
                if (s_ActiveScenes[i].scene.name == name)
                    return s_ActiveScenes[i];
            }
            return null;
        }
        
        public static NeoSerializedScene GetByPath(string path)
        {
            for (int i = 0; i < s_ActiveScenes.Count; ++i)
            {
                if (s_ActiveScenes[i].scene.path == path)
                    return s_ActiveScenes[i];
            }
            return null;
        }

        public static NeoSerializedScene GetByPathHash(int hash)
        {
            for (int i = 0; i < s_ActiveScenes.Count; ++i)
            {
                if (s_ActiveScenes[i].hashedPath == hash)
                    return s_ActiveScenes[i];
            }
            return null;
        }

#if UNITY_EDITOR

        public void RegisterPrefab(NeoSerializedGameObject prefab)
        {
            if (Array.IndexOf(m_Prefabs, prefab) == -1)
            {
                var so = new UnityEditor.SerializedObject(this);
                var prop = so.FindProperty("m_Prefabs");
                ++prop.arraySize;
                prop = prop.GetArrayElementAtIndex(prop.arraySize - 1);
                prop.objectReferenceValue = prefab;
                so.ApplyModifiedProperties();
            }
        }

#endif

#region VALIDATION

#if UNITY_EDITOR

        private static List<GameObject> s_GatheredGameObjects = new List<GameObject>();
        private static List<NeoSerializedGameObject> s_GatheredNsgos = new List<NeoSerializedGameObject>();

        protected void OnEnable()
        {
            if (!Application.isPlaying && gameObject.scene.IsValid())
            {
                UnityEditor.EditorApplication.hierarchyChanged += OnHierarchyChanged;
            }
        }

        protected void OnDisable()
        {
            if (!Application.isPlaying && gameObject.scene.IsValid())
            {
                UnityEditor.EditorApplication.hierarchyChanged -= OnHierarchyChanged;
            }
        }

        void OnHierarchyChanged()
        {
            // Gather scene objects
            scene.GetRootGameObjects(s_GatheredGameObjects);
            for (int i = 0; i < s_GatheredGameObjects.Count; ++i)
            {
                var nsgo = s_GatheredGameObjects[i].GetComponent<NeoSerializedGameObject>();
                if (nsgo != null)
                    s_GatheredNsgos.Add(nsgo);
            }

            // Validate container
            if (m_SceneObjects.Validate(s_GatheredNsgos))
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(scene);

            s_GatheredGameObjects.Clear();
            s_GatheredNsgos.Clear();
        }

#endif

        public virtual void OnValidate()
        {
            if (m_SceneObjects == null || m_SceneObjects.serializedScene == null)
                m_SceneObjects = new NeoSerializedSceneObjectContainer(this);

            ValidatePrefabs();
            ValidateAssets();
        }

        void ValidatePrefabs()
        {
            int valid = 0;
            for (int i = 0; i < m_Prefabs.Length; ++i)
            {
                if (m_Prefabs[i] != null)
                    ++valid;
            }
            if (valid != m_Prefabs.Length)
            {
                var rebuilt = new NeoSerializedGameObject[valid];
                int itr = 0;
                for (int i = 0; i < m_Prefabs.Length; ++i)
                {
                    if (m_Prefabs[i] != null)
                    {
                        rebuilt[itr] = m_Prefabs[i];
                        ++itr;
                    }
                }
                m_Prefabs = rebuilt;
            }
        }

        void ValidateAssets()
        {
            int valid = 0;
            for (int i = 0; i < m_Assets.Length; ++i)
            {
                if (m_Assets[i] != null)
                    ++valid;
            }
            if (valid != m_Assets.Length)
            {
                var rebuilt = new ScriptableObject[valid];
                int itr = 0;
                for (int i = 0; i < m_Assets.Length; ++i)
                {
                    if (m_Assets[i] != null)
                    {
                        rebuilt[itr] = m_Assets[i];
                        ++itr;
                    }
                }
                m_Assets = rebuilt;
            }
        }

#endregion

#region INSTANTIATION

        public T InstantiatePrefab<T>(T prototype) where T : Component
        {
            return NeoSerializedObjectFactory.Instantiate(prototype, m_SceneObjects);
        }

        public T InstantiatePrefab<T>(T prototype, Vector3 position, Quaternion rotation) where T : Component
        {
            return NeoSerializedObjectFactory.Instantiate(prototype, m_SceneObjects, position, rotation);
        }

        public T InstantiatePrefab<T>(int prefabID, int serializationKey) where T : Component
        {
            var result = NeoSerializedObjectFactory.Instantiate(prefabID, serializationKey, m_SceneObjects);
            if (result != null)
                return result.GetComponent<T>();
            else
                return null;
        }
		
#endregion

#region SERIALIZATION

        public void WriteData(INeoSerializer writer)
        {
            WriteData(writer, SaveMode.Default);
        }

        public void WriteData(INeoSerializer writer, SaveMode saveMode)
        {
            if (!scene.IsValid())
                return;

            PreSaveScene();

            m_SceneObjects.WriteGameObjects(writer, saveMode);
            WriteProperties(writer);

            PostSaveScene();
        }

        public void ReadData(INeoDeserializer reader)
        {
            PreLoadScene();

            m_SceneObjects.ReadGameObjectHierarchy(reader);
            m_SceneObjects.ReadGameObjectProperties(reader);
            ReadProperties(reader);

            PostLoadScene();
        }

        protected virtual void PreSaveScene() { }
        protected virtual void PostSaveScene() { }
        protected virtual void PreLoadScene() { }
        protected virtual void PostLoadScene() { }

        protected virtual void WriteProperties(INeoSerializer writer) { }
        protected virtual void ReadProperties(INeoDeserializer reader) { }

#endregion
    }
}
