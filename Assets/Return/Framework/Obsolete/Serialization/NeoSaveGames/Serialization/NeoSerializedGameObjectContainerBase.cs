using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoSaveGames.Serialization
{
    [Serializable]
    public abstract class NeoSerializedGameObjectContainerBase : INeoSerializedGameObjectContainer
    {
        [SerializeField]
        private List<int> m_Keys = new List<int>();
        [SerializeField]
        private List<NeoSerializedGameObject> m_Values = new List<NeoSerializedGameObject>();

        private static readonly NeoSerializationKey k_DestroyedKey = new NeoSerializationKey("destroyedObjects");
        private static readonly NeoSerializationKey k_RuntimeKey = new NeoSerializationKey("runtimeObjects");

        private Dictionary<int, NeoSerializedGameObject> m_TrackedObjects = null;
        private List<int> m_DestroyedObjects = null;

        public abstract Transform rootTransform
        {
            get;
        }

        public abstract bool isValid
        {
            get;
        }

        protected bool isBuildingHierarchy
        {
            get;
            private set;
        }

        public NeoSerializedGameObject this[int key]
        {
            get
            {
                NeoSerializedGameObject result;
                m_TrackedObjects.TryGetValue(key, out result);
                return result;
            }
        }

        public void Awake()
        {
            if (m_TrackedObjects == null && isValid)
                RebuildDictionary();
        }

        public void OnDestroy()
        {
            m_TrackedObjects = null;
            m_DestroyedObjects = null;
        }

        public bool Contains(NeoSerializedGameObject nsgo)
        {
            return m_Values.Contains(nsgo);
        }

        public int GetSerializationKeyForObject(NeoSerializedGameObject nsgo)
        {
            if (Application.isPlaying)
            {
                foreach (var pair in m_TrackedObjects)
                {
                    if (pair.Value == nsgo)
                        return pair.Key;
                }
            }
            else
            {
                for (int i = 0; i < m_Values.Count; ++i)
                {
                    if (m_Values[i] == nsgo)
                        return m_Keys[i];
                }
            }
            return 0;
        }

        void RebuildDictionary()
        {
            int count = Math.Min(m_Keys.Count, m_Values.Count);
            m_TrackedObjects = new Dictionary<int, NeoSerializedGameObject>(count);
            for (int i = 0; i != count; i++)
            {
                if (m_Values[i] != null)
                {
                    m_TrackedObjects.Add(m_Keys[i], m_Values[i]);
                    m_Values[i].serializationKey = m_Keys[i];
                    m_Values[i].onDestroyed += UnregisterObject;
                }
            }
            m_DestroyedObjects = new List<int>();

            if (Application.isPlaying)
            {
                m_Keys = null;
                m_Values = null;
            }
        }

        protected int GenerateKey()
        {
            int key = 0;
            while (key == 0 || m_TrackedObjects.ContainsKey(key))
                key = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            return key;
        }

        public virtual void RegisterObject(NeoSerializedGameObject nsgo)
        {
            if (m_TrackedObjects == null)
                RebuildDictionary();

            // Check if child object
            if (nsgo.transform.parent != rootTransform)
            {
                Debug.LogError("Attempting to register object that is not a child of the container object.", nsgo.gameObject);
                return;
            }
            
            // Use existing serialization key (if instantiated on load) or generate a new one
            int key = nsgo.serializationKey;
            if (key == 0)
                key = GenerateKey();
            else
            {
                if (m_TrackedObjects.ContainsKey(key))
                {
                    Debug.LogError(string.Format("Attempting to register object with key ({0}) that is already registered. This object will not be tracked: {1}", key, nsgo.name), nsgo.gameObject);
                    return;
                }
            }
            m_TrackedObjects.Add(key, nsgo);
            nsgo.serializationKey = key;
            
            if (Application.isPlaying)
                nsgo.onDestroyed += UnregisterObject;
        }

        public virtual void UnregisterObject(NeoSerializedGameObject nsgo)
        {
            if (m_TrackedObjects == null)
                return;

            NeoSerializedGameObject found = null;
            if (m_TrackedObjects.TryGetValue(nsgo.serializationKey, out found) && found == nsgo)
            {
                m_TrackedObjects.Remove(nsgo.serializationKey);
                nsgo.onDestroyed -= UnregisterObject;
            }

            // Add to destroyed list
            if (!nsgo.wasRuntimeInstantiated)
                m_DestroyedObjects.Add(nsgo.serializationKey);
        }

        public NeoSerializedGameObject GetChildObject(int serializationKey)
        {
            NeoSerializedGameObject result;
            if (m_TrackedObjects.TryGetValue(serializationKey, out result))
                return result;
            else
                return null;
        }

        public NeoSerializedGameObject CreateChildObject(string name)
        {
            return CreateChildObject(name, Vector3.zero, Quaternion.identity, 0);
        }

        public NeoSerializedGameObject CreateChildObject(string name, int serializationKey)
        {
            return CreateChildObject(name, Vector3.zero, Quaternion.identity, serializationKey);
        }

        public NeoSerializedGameObject CreateChildObject(string name, Vector3 localPosition, Quaternion localRotation)
        {
            return CreateChildObject(name, localPosition, localRotation, 0);
        }

        public NeoSerializedGameObject CreateChildObject(string name, Vector3 localPosition, Quaternion localRotation, int serializationKey)
        {
            // Create object and add NeoSerializedGameObject
            var go = new GameObject(name);
            var result = go.AddComponent<NeoSerializedGameObject>();
            //result.serializeName = true;

            // GUIDs, etc
            var t = result.transform;
            t.SetParent(rootTransform);
            t.localPosition = localPosition;
            t.localRotation = localRotation;
            t.localScale = Vector3.one;

            // RegisterHandler with object
            result.serializationKey = serializationKey;
            RegisterObject(result);
            result.wasRuntimeInstantiated = true;

            return result;
        }

        public void WriteGameObjects(INeoSerializer writer, SaveMode saveMode)
        {
            WriteGameObjects(writer, NeoSerializationFilter.Exclude, null, saveMode);
        }

        public void WriteGameObjects(INeoSerializer writer, NeoSerializationFilter filter, NeoSerializedGameObject[] objects, SaveMode saveMode)
        {
            // Check if awake
            Awake();

            // Write list of destroyed objects (non-runtime initialised)
            if (m_DestroyedObjects.Count > 0)
                writer.WriteValues(k_DestroyedKey, m_DestroyedObjects);

            if (m_TrackedObjects.Count > 0)
            {
                // Runtime initialised prefabs & objects
                var runtime = new List<Vector2Int>(m_TrackedObjects.Count);
                var nsgos = new List<NeoSerializedGameObject>(m_TrackedObjects.Count);

                // Populate
                foreach (var nsgo in m_TrackedObjects.Values)
                {
                    bool serializeObject = false;
                    if (filter == NeoSerializationFilter.Include)
                    {
                        // Add if in objects list
                        if (objects != null && Array.IndexOf(objects, nsgo) != -1)
                            serializeObject = true;
                    }
                    else
                    {
                        // Add if not in objects list
                        if (objects == null || Array.IndexOf(objects, nsgo) == -1)
                            serializeObject = true;
                    }

                    if (serializeObject)
                    {
                        nsgos.Add(nsgo);
                        if (nsgo.wasRuntimeInstantiated)
                        {
                            //Debug.Log(string.Format("Writing runtime object: {0}, key: {1}", nsgo.name, nsgo.serializationKey));
                            runtime.Add(new Vector2Int(nsgo.prefabStrongID, nsgo.serializationKey));
                        }
                    }
                }

                // write list of runtime initialised prefabs
                if (runtime.Count > 0)
                    writer.WriteValues(k_RuntimeKey, runtime);

                // Write all tracked gameobjects
                for (int i = 0; i < nsgos.Count; ++i)
                {
                    if (nsgos[i] != null)
                    {
                        writer.PushContext(SerializationContext.GameObject, nsgos[i].serializationKey);
                        nsgos[i].WriteGameObject(writer, saveMode);
                        writer.PopContext(SerializationContext.GameObject);
                    }
                }
            }
        }

        public void ReadGameObjectHierarchy(INeoDeserializer reader)
        {
            // Check if awake
            Awake();

            isBuildingHierarchy = true;

            // Should check non-dynamic children are already registered??

            // Destroy any non-runtime instantiated objects that were removed
            int[] destroyed;
            if (reader.TryReadValues(k_DestroyedKey, out destroyed, null))
            {
                for (int i = 0; i < destroyed.Length; ++i)
                {
                    NeoSerializedGameObject destroy;
                    if (m_TrackedObjects.TryGetValue(destroyed[i], out destroy))
                        UnityEngine.Object.Destroy(destroy.gameObject);
                }
            }

            // Instantiate runtime objects
            Vector2Int[] runtime;
            if (reader.TryReadValues(k_RuntimeKey, out runtime, null))
            {
                for (int i = 0; i < runtime.Length; ++i)
                {
                    if (runtime[i].x == 0)
                        CreateChildObject("Pending", runtime[i].y);
                    else
                    {
                        //try
                        //{
                            NeoSerializedObjectFactory.Instantiate(runtime[i].x, runtime[i].y, this);
                        //}
                        //catch (Exception e)
                        //{
                        //    Debug.LogError("Failed to instantiate runtime object due to error: " + e.Message);
                        //}
                    }
                }
            }

            // Read all tracked objects
            foreach (var nsgo in m_TrackedObjects.Values)
            {
                if (reader.PushContext(SerializationContext.GameObject, nsgo.serializationKey))
                {
                    nsgo.ReadGameObjectHierarchy(reader);
                    reader.PopContext(SerializationContext.GameObject, nsgo.serializationKey);
                }
            }

            isBuildingHierarchy = false;
        }

        public void ReadGameObjectProperties(INeoDeserializer reader)
        {
            // Read all tracked objects
            foreach (var nsgo in m_TrackedObjects.Values)
            {
                if (reader.PushContext(SerializationContext.GameObject, nsgo.serializationKey))
                {
                    nsgo.ReadGameObjectProperties(reader);
                    reader.PopContext(SerializationContext.GameObject, nsgo.serializationKey);
                }
            }
        }

#if UNITY_EDITOR

        private static List<NeoSerializedGameObject> s_StoredObjects = new List<NeoSerializedGameObject>();
        
        public void Validate(NeoSerializedGameObject nsgo)
        {
            if (!isValid || Application.isPlaying)
                return;

            // Check if object is registered
            for (int i = 0; i < m_Keys.Count && i < m_Values.Count; ++i)
            {
                if (m_Values[i] == nsgo)
                    return;
            }

            // Add if not
            m_Values.Add(nsgo);
            m_Keys.Add(EditorGenerateKey());
        }

        public bool Validate(List<NeoSerializedGameObject> gathered)
        {
            if (!isValid || Application.isPlaying)
                return false;

            bool result = false;

            // Check keys lengths
            if (m_Keys.Count != m_Values.Count)
            {
                // Trim keys array
                if (m_Keys.Count > m_Values.Count)
                    m_Keys.RemoveRange(m_Values.Count, m_Keys.Count - m_Values.Count);
                else
                {
                    // Extend keys array
                    for (int i = m_Keys.Count; i < m_Values.Count; ++i)
                        m_Keys.Add(EditorGenerateKey());
                    result = true;
                }
            }

            // Check values validity
            for (int i = m_Values.Count - 1; i >= 0; --i)
            {
                if (m_Values[i] == null || m_Keys[i] == 0)
                {
                    m_Values.RemoveAt(i);
                    m_Keys.RemoveAt(i);
                    result = true;
                }
            }

            // Record values to check which are still valid
            if (s_StoredObjects == null)
                s_StoredObjects = new List<NeoSerializedGameObject>(m_Values.Count);
            for (int i = 0; i < m_Values.Count; ++i)
                s_StoredObjects.Add(m_Values[i]);

            // Gather scene objects
            for (int i = 0; i < gathered.Count; ++i)
            {
                // If already known, remove from remaining list
                if (m_Values.Contains(gathered[i]))
                    s_StoredObjects.Remove(gathered[i]);
                else
                {
                    // Else add to container
                    m_Values.Add(gathered[i]);
                    m_Keys.Add(EditorGenerateKey());
                    result = true;
                }
            }

            // Remove remaining objects that weren't found
            // They must have moved in the hierarchy
            for (int i = 0; i < s_StoredObjects.Count; ++i)
            {
                if (!Application.isPlaying)
                    Debug.LogWarning(string.Format("A NeoSerializedGameObject was registered with the wrong parent: {0}. If you insert a new NSGO between 2 others in the hierarchy, any old game saves will skip loading the old child object.", s_StoredObjects[i].name));

                int index = m_Values.IndexOf(s_StoredObjects[i]);
                if (index != -1)
                {
                    m_Values.RemoveAt(index);
                    m_Keys.RemoveAt(index);
                    result = true;
                }
            }

            // Check child object hierarchy
            foreach (var nsgo in m_Values)
                result |= nsgo.CheckPrefabHierarchy();

            s_StoredObjects.Clear();

            return result;
        }
        
        protected int EditorGenerateKey()
        {
            int result = 0;
            while (result == 0 || m_Keys.Contains(result))
                result = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
            return result;
        }

#endif
    }
}