using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif
#endif

namespace NeoSaveGames.Serialization
{
    [DisallowMultipleComponent]
    [HelpURL("https://docs.neofps.com/manual/savegamesref-mb-neoserializedgameobject.html")]
    public class NeoSerializedGameObject : MonoBehaviour
    {
        [SerializeField, Tooltip("Save and reload the object's name. This is only required if the name would change at runtime")]
        private bool m_SaveName = false;

        [Header("Transform")]
        [SerializeField, Tooltip("If and how to serialize the object position")]
        private TransformSerialization m_Position = TransformSerialization.LocalSpace;
        [SerializeField, Tooltip("If and how to serialize the object rotation")]
        private TransformSerialization m_Rotation = TransformSerialization.LocalSpace;
        [SerializeField, Tooltip("Should the object local scale be serialized")]
        private bool m_LocalScale = false;

        [SerializeField, Tooltip("How to filter out child objects. If set to exclude, the objects in the list below will not be serialized. If set to include, only the objects below will be serialized")]
        private NeoSerializationFilter m_FilterChildObjects = NeoSerializationFilter.Exclude;
        [SerializeField]
        private NeoSerializedGameObject[] m_ChildObjects = new NeoSerializedGameObject[0];

        [SerializeField, Tooltip("How to filter out serialized components. If set to components, the objects in the list below will not be serialized. If set to include, only the components below will be serialized")]
        private NeoSerializationFilter m_FilterNeoComponents = NeoSerializationFilter.Exclude;
        [SerializeField]
        private MonoBehaviour[] m_NeoComponents = new MonoBehaviour[0];
        [SerializeField]
        private Component[] m_OtherComponents = new Component[0];

        [SerializeField]
        private Override[] m_Overrides = new Override[0];

        [SerializeField]//, HideInInspector]
        private NeoSerializedGameObjectChildContainer m_Children = null;

        [SerializeField, HideInInspector]
        private int m_PrefabStrongID = 0; // This is only required when registered for runtime instantiation. Could simplify

        private static readonly NeoSerializationKey k_NameKey = new NeoSerializationKey("name");
        private static readonly NeoSerializationKey k_ActiveKey = new NeoSerializationKey("active");
        private static readonly NeoSerializationKey k_EnabledKey = new NeoSerializationKey("enabled");
        private static readonly NeoSerializationKey k_PositionKey = new NeoSerializationKey("position");
        private static readonly NeoSerializationKey k_RotationKey = new NeoSerializationKey("rotation");
        private static readonly NeoSerializationKey k_ScaleKey = new NeoSerializationKey("scale");
        private static readonly NeoSerializationKey k_SettingsKey = new NeoSerializationKey("settings");

        private static List<INeoSerializableComponent> s_GatheredNeoComponents = new List<INeoSerializableComponent>(16);

        private int m_SerializationKey = 0;
        private bool m_WasRuntimeInstantiated = false;
        private bool m_SaveSettings = false;

        public event Action<NeoSerializedGameObject> onDestroyed;

        public enum TransformSerialization
        {
            LocalSpace,
            WorldSpace,
            Ignore
        }

        public int serializationKey
        {
            get { return m_SerializationKey; }
            set
            {
                if (m_SerializationKey == 0)
                    m_SerializationKey = value;
                else
                {
                    if (m_SerializationKey != value)
                        Debug.LogError("Cannot change an objects serialization key once it has been set", gameObject);
                    m_SerializationKey = value;
                }
            }
        }

        public int prefabStrongID
        {
            get { return m_PrefabStrongID; }
        }

        public bool wasRuntimeInstantiated
        {
            get { return m_WasRuntimeInstantiated; }
            set { m_WasRuntimeInstantiated = value; }
        }

        public bool saveName
        {
            get { return m_SaveName; }
            set
            {
                m_SaveName = value;
                m_SaveSettings = true;
            }
        }

        public NeoSerializationFilter filterChildObjects
        {
            get { return m_FilterChildObjects; }
            set
            {
                m_FilterChildObjects = value;
                m_SaveSettings = true;
            }
        }

        public NeoSerializationFilter filterNeoComponents
        {
            get { return m_FilterNeoComponents; }
            set
            {
                m_FilterNeoComponents = value;
                m_SaveSettings = true;
            }
        }

        public NeoSerializedScene serializedScene
        {
            get { return NeoSerializedScene.GetByPath(gameObject.scene.path); }
        }

        public NeoSerializedGameObjectChildContainer serializedChildren
        {
            get { return m_Children; }
        }

        public bool willBeSerialized
        {
            get
            {
                if (transform.parent == null)
                    return true;

                if (m_WasRuntimeInstantiated)
                {
                    var parentNsgo = transform.parent.GetComponent<NeoSerializedGameObject>();
                    if (parentNsgo == null || !parentNsgo.willBeSerialized)
                        return false;
                }

                var parent = GetParent();
                if (parent == null)
                    return false;

                if (!parent.willBeSerialized)
                    return false;

                return parent.WillSerializeChildObject(this);
            }
        }

        bool WillSerializeChildObject(NeoSerializedGameObject child)
        {
            bool filtered = Array.IndexOf(m_ChildObjects, child) != -1;
            if (m_FilterChildObjects == NeoSerializationFilter.Include)
                return filtered;
            else
                return !filtered;
        }

        [Serializable]
        public class Override
        {
            [SerializeField, Tooltip("The save mode this override applies to")]
            private int m_SaveMode = 1;

            [Header("Transform")]
            [SerializeField, Tooltip("If and how to serialize the object position")]
            private OverrideTransformSerialization m_Position = OverrideTransformSerialization.UseDefault;
            [SerializeField, Tooltip("If and how to serialize the object rotation")]
            private OverrideTransformSerialization m_Rotation = OverrideTransformSerialization.UseDefault;
            [SerializeField, Tooltip("Should the object local scale be serialized")]
            private OverrideScaleSerialization m_LocalScale = OverrideScaleSerialization.UseDefault;

            [SerializeField, Tooltip("How to filter out child objects. If set to exclude, the objects in the list below will not be serialized. If set to include, only the objects below will be serialized")]
            private OverrideNeoSerializationFilter m_FilterChildObjects = OverrideNeoSerializationFilter.UseDefault;
            [SerializeField]
            private NeoSerializedGameObject[] m_ChildObjects = new NeoSerializedGameObject[0];

            [SerializeField, Tooltip("How to filter out serialized components. If set to components, the objects in the list below will not be serialized. If set to include, only the components below will be serialized")]
            private OverrideNeoSerializationFilter m_FilterNeoComponents = OverrideNeoSerializationFilter.UseDefault;
            [SerializeField]
            private MonoBehaviour[] m_NeoComponents = new MonoBehaviour[0];

            [SerializeField, Tooltip("Override the other components list instead of default")]
            private bool m_OverrideOtherComponents = false;
            [SerializeField]
            private Component[] m_OtherComponents = new Component[0];

#if UNITY_EDITOR
            // Inspector foldout expansion tracking
            [HideInInspector] public bool expandOverride = true;
            [HideInInspector] public bool expandChildObjects = false;
            [HideInInspector] public bool expandNeoComponents = false;
            [HideInInspector] public bool expandOtherComponents = false;
#endif

            // Accessors
            public SaveMode saveMode { get { return m_SaveMode; } }
            public OverrideTransformSerialization serializePosition { get { return m_Position; } }
            public OverrideTransformSerialization serializeRotation { get { return m_Rotation; } }
            public OverrideScaleSerialization serializeLocalScale { get { return m_LocalScale; } }
            public OverrideNeoSerializationFilter filterChildObjects { get { return m_FilterChildObjects; } }
            public NeoSerializedGameObject[] childObjects { get { return m_ChildObjects; } }
            public OverrideNeoSerializationFilter filterNeoComponents { get { return m_FilterNeoComponents; } }
            public MonoBehaviour[] neoComponents { get { return m_NeoComponents; } }
            public bool overrideOtherComponents {  get { return m_OverrideOtherComponents; } }
            public Component[] otherComponents { get { return m_OtherComponents; } }

            public enum OverrideTransformSerialization
            {
                UseDefault,
                LocalSpace,
                WorldSpace,
                Ignore
            }

            public enum OverrideScaleSerialization
            {
                UseDefault,
                True,
                False
            }

            public enum OverrideNeoSerializationFilter
            {
                UseDefault,
                Exclude,
                Include
            }

            public Override(SaveMode mode)
            {
                m_SaveMode = mode;
            }

            public void ApplyLimiter(INeoSerializedGameObjectLimiter limiter)
            {
                if (limiter.restrictChildObjects)
                {
                    m_FilterChildObjects = OverrideNeoSerializationFilter.UseDefault;
                    if (m_ChildObjects.Length > 0)
                        m_ChildObjects = new NeoSerializedGameObject[0];
                }

                if (limiter.restrictNeoComponents)
                {
                    m_FilterNeoComponents = OverrideNeoSerializationFilter.UseDefault;
                    if (m_NeoComponents.Length > 0)
                        m_NeoComponents = new MonoBehaviour[0];
                }

                if (limiter.restrictOtherComponents)
                {
                    m_OverrideOtherComponents = false;
                }
            }
        }

        protected void Awake()
        {
            // Assign child container
            if (m_Children == null || !m_Children.isValid)
                m_Children = new NeoSerializedGameObjectChildContainer(this);
            m_Children.Awake();
        }

        protected void OnDestroy()
        {
            // Destroy child NeoSerializedGameObjectContainer
            // Prevents children from unregistering and bloating the destroyed objects list
            if (m_Children != null)
                m_Children.OnDestroy();
            if (onDestroyed != null)
                onDestroyed(this);
        }

        public void OnValidate()
        {

            // Assign child container
            if (m_Children == null || !m_Children.isValid)
                m_Children = new NeoSerializedGameObjectChildContainer(this);
            
            // Check for restrictions
            var limiters = GetComponents<INeoSerializedGameObjectLimiter>();
            for (int i = 0; i < limiters.Length; ++i)
            {
                if (limiters[i].restrictChildObjects)
                {
                    m_FilterChildObjects = NeoSerializationFilter.Include;
                    if (m_ChildObjects.Length > 0)
                        m_ChildObjects = new NeoSerializedGameObject[0];
                }

                if (limiters[i].restrictNeoComponents)
                {
                    m_FilterNeoComponents = NeoSerializationFilter.Include;
                    if (m_NeoComponents.Length > 0)
                        m_NeoComponents = new MonoBehaviour[0];
                }

                if (limiters[i].restrictOtherComponents)
                {
                    if (m_OtherComponents.Length > 0)
                        m_OtherComponents = new Component[0];
                }

                for (int j = 0; j < m_Overrides.Length; ++j)
                    m_Overrides[j].ApplyLimiter(limiters[i]);
            }

#if UNITY_EDITOR
            if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
            {
                // Delay validation since PrefabUtility throws SendMessage warnings in OnValidate
                if (!m_ValidationPending)
                {
                    m_ValidationPending = true;
                    UnityEditor.EditorApplication.update += DeferredOnValidate;
                }
            }
#endif
        }

#if UNITY_EDITOR

        private NeoSerializedGameObject m_OriginalPrefab = null;
        private bool m_ValidationPending = false;
        void DeferredOnValidate()
        {
            // Remove delayed validation callback
            m_ValidationPending = false;
            UnityEditor.EditorApplication.update -= DeferredOnValidate;

            // Null check for when changing play mode, etc (MonoBehaviour pointers are safe pointers)
            if (this != null)
            {
                // Object is prefab but in a scene (could be staging scene for editing prefabs)
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (stage != null && stage.IsPartOfPrefabContents(gameObject))
                {
                    // Object is prefab but in the prefab staging scene (editing the prefab)
                    // Check if prefab is stored
                    if (m_OriginalPrefab == null)
                    {
#if UNITY_2020_1_OR_NEWER
                        m_OriginalPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath).GetComponent<NeoSerializedGameObject>();
#else
                        m_OriginalPrefab = UnityEditor.AssetDatabase.LoadAssetAtPath<GameObject>(stage.prefabAssetPath).GetComponent<NeoSerializedGameObject>();
#endif
                        //m_OriginalPrefab.CheckPrefabID();
                        //m_OriginalPrefab.CheckSerializationKey();
                    }
                }
                else
                {
                    if (!UnityEditor.PrefabUtility.IsPartOfPrefabAsset(gameObject))
                    {
                        // Object is in a scene and not a prefab - check key only
                        CheckSerializationKey();
                    }
                    else
                    {
                        if (!UnityEditor.PrefabUtility.IsPartOfPrefabInstance(gameObject))
                        {
                            // Object is prefab but not in a scene (project view only)
                            CheckPrefabID();
                            CheckSerializationKey();
                        }
                    }
                }
            }
        }

        private void OnBeforeTransformParentChanged()
        {
            // Double check this. It already caused an error in OnDestroy
            if (!Application.isPlaying)
                m_Children = null;
        }
#endif

                        public NeoSerializedGameObject GetParent()
        {
            var itr = transform.parent;
            while (itr != null)
            {
                var nsgo = itr.GetComponent<NeoSerializedGameObject>();
                if (nsgo != null)
                    return nsgo;
                else
                    itr = itr.parent;
            }
            return null;
        }

        public void SetParent(NeoSerializedGameObject target)
        {
            if (target != null)
            {
                // Unregister from current
                var current = GetParent();
                if (current != null)
                {
                    if (current == target)
                        return;
                    current.serializedChildren.UnregisterObject(this);
                }

                // Set transform parent
                transform.SetParent(target.transform);

                // RegisterHandler with new parent
                target.serializedChildren.RegisterObject(this);
            }
            else
                Debug.LogError("Cannot set NeoSerializedGameObject parent to null");
        }

#region WRITING

        public void WriteGameObject(INeoSerializer writer, SaveMode saveMode)
        {
            // Write name
            if (m_SaveName)
                writer.WriteValue(k_NameKey, name);

            // Write active state
            writer.WriteValue(k_ActiveKey, gameObject.activeSelf);

            // Save settings (doesn't apply to overrides)
            if (m_SaveSettings)
                writer.WriteValue(k_SettingsKey, new Vector3Int(m_SaveName ? 1 : 0, (int)m_FilterChildObjects, (int)m_FilterNeoComponents));

            // Check for overrides
            Override over = null;
            if (saveMode != SaveMode.Default)
            {
                for (int i = 0; i < m_Overrides.Length; ++i)
                {
                    if (m_Overrides[i].saveMode == saveMode)
                    {
                        over = m_Overrides[i];
                        break;
                    }
                }
            }

            // Get settings
            TransformSerialization serializePosition = m_Position;
            TransformSerialization serializeRotation = m_Rotation;
            bool serializeScale = m_LocalScale;
            var filterChildren = m_FilterChildObjects;
            var childObjects = m_ChildObjects;
            var filterNeoComponents = m_FilterNeoComponents;
            var neoComponents = m_NeoComponents;
            var otherComponents = m_OtherComponents;
            if (over != null)
            {
                switch (over.serializePosition)
                {
                    case Override.OverrideTransformSerialization.LocalSpace:
                        serializePosition = TransformSerialization.LocalSpace;
                        break;
                    case Override.OverrideTransformSerialization.WorldSpace:
                        serializePosition = TransformSerialization.WorldSpace;
                        break;
                    case Override.OverrideTransformSerialization.Ignore:
                        serializePosition = TransformSerialization.Ignore;
                        break;
                }
                switch (over.serializeRotation)
                {
                    case Override.OverrideTransformSerialization.LocalSpace:
                        serializeRotation = TransformSerialization.LocalSpace;
                        break;
                    case Override.OverrideTransformSerialization.WorldSpace:
                        serializeRotation = TransformSerialization.WorldSpace;
                        break;
                    case Override.OverrideTransformSerialization.Ignore:
                        serializeRotation = TransformSerialization.Ignore;
                        break;
                }
                switch (over.serializeLocalScale)
                {
                    case Override.OverrideScaleSerialization.True:
                        serializeScale = true;
                        break;
                    case Override.OverrideScaleSerialization.False:
                        serializeScale = false;
                        break;
                }
                switch (over.filterChildObjects)
                {
                    case Override.OverrideNeoSerializationFilter.Exclude:
                        filterChildren = NeoSerializationFilter.Exclude;
                        childObjects = over.childObjects;
                        break;
                    case Override.OverrideNeoSerializationFilter.Include:
                        filterChildren = NeoSerializationFilter.Include;
                        childObjects = over.childObjects;
                        break;
                }
                switch (over.filterNeoComponents)
                {
                    case Override.OverrideNeoSerializationFilter.Exclude:
                        filterNeoComponents = NeoSerializationFilter.Exclude;
                        neoComponents = over.neoComponents;
                        break;
                    case Override.OverrideNeoSerializationFilter.Include:
                        filterNeoComponents = NeoSerializationFilter.Include;
                        neoComponents = over.neoComponents;
                        break;
                }
                if (over.overrideOtherComponents)
                    otherComponents = over.otherComponents;
            }

            // Write transform
            switch (serializePosition)
            {
                case TransformSerialization.LocalSpace:
                    writer.WriteValue(k_PositionKey, transform.localPosition);
                    break;
                case TransformSerialization.WorldSpace:
                    writer.WriteValue(k_PositionKey, transform.position);
                    break;
            }
            switch (serializeRotation)
            {
                case TransformSerialization.LocalSpace:
                    writer.WriteValue(k_RotationKey, transform.localRotation);
                    break;
                case TransformSerialization.WorldSpace:
                    writer.WriteValue(k_RotationKey, transform.rotation);
                    break;
            }
            if (serializeScale)
                writer.WriteValue(k_ScaleKey, transform.localScale);

            // Write neo-serialized components
            if (filterNeoComponents == NeoSerializationFilter.Include)
            {
                // Write components in m_NeoComponents array only
                for (int i = 0; i < neoComponents.Length; ++i)
                    WriteNeoComponent(writer, neoComponents[i], saveMode);
            }
            else
            {
                GetComponents(s_GatheredNeoComponents);
                for (int i = 0; i < s_GatheredNeoComponents.Count; ++i)
                {
                    // Skip components in m_NeoComponents
                    if (Array.IndexOf(neoComponents, s_GatheredNeoComponents[i]) != -1)
                        continue;

                    WriteNeoComponent(writer, s_GatheredNeoComponents[i], saveMode);
                }
                s_GatheredNeoComponents.Clear();
            }

            // Write other components (always manually included)
            for (int i = 0; i < otherComponents.Length; ++i)
                WriteOtherComponent(writer, otherComponents[i]);

            // Write child objects
            m_Children.WriteGameObjects(writer, filterChildren, childObjects, saveMode);
        }

        void WriteNeoComponent(INeoSerializer writer, MonoBehaviour component, SaveMode saveMode)
        {
            var c = component as INeoSerializableComponent;
            if (c != null)
            {
                writer.PushContext(SerializationContext.ComponentNeoSerialized, NeoSerializationUtilities.GetPersistentComponentID(component));
                writer.WriteValue(k_EnabledKey, component.enabled);
                c.WriteProperties(writer, this, saveMode);
                writer.PopContext(SerializationContext.ComponentNeoSerialized);
            }
        }

        void WriteNeoComponent(INeoSerializer writer, INeoSerializableComponent component, SaveMode saveMode)
        {
            var c = component as MonoBehaviour;
            if (c != null)
            {
                writer.PushContext(SerializationContext.ComponentNeoSerialized, NeoSerializationUtilities.GetPersistentComponentID(c));
                writer.WriteValue(k_EnabledKey, c.enabled);
                component.WriteProperties(writer, this, saveMode);
                writer.PopContext(SerializationContext.ComponentNeoSerialized);
            }
        }

        void WriteOtherComponent(INeoSerializer writer, Component component)
        {
            if (component != null)
            {
                writer.PushContext(SerializationContext.ComponentNeoFormatted, NeoSerializationUtilities.GetPersistentComponentID(component));
                var formatter = NeoSerializationFormatters.GetFormatter(component);
                if (formatter != null)
                    formatter.WriteProperties(writer, component, this);
                writer.PopContext(SerializationContext.ComponentNeoFormatted);
            }
        }

#endregion

#region READING

        public void ReadGameObjectHierarchy(INeoDeserializer reader)
        {
            // Rebuild the hierarchies separate to reading properties
            // Required to Resolver references
            m_Children.ReadGameObjectHierarchy(reader);
        }

        public void ReadGameObjectProperties(INeoDeserializer reader)
        {
            string n;
            if (reader.TryReadValue(k_NameKey, out n, string.Empty))
            {
                name = n;
            }

            Vector3Int settings;
            if (reader.TryReadValue(k_SettingsKey, out settings, Vector3Int.zero))
            {
                m_SaveName = settings.x == 1;
                m_FilterChildObjects = (NeoSerializationFilter)settings.y;
                m_FilterNeoComponents = (NeoSerializationFilter)settings.z;
                m_SaveSettings = true;
            }

            // Read active state
                bool active;
            reader.TryReadValue(k_ActiveKey, out active, gameObject.activeSelf);
            gameObject.SetActive(active);

            // Read transform
            Vector3 position;
            if (reader.TryReadValue(k_PositionKey, out position, Vector3.zero))
            {
                switch (m_Position)
                {
                    case TransformSerialization.LocalSpace:
                        transform.localPosition = position;
                        break;
                    case TransformSerialization.WorldSpace:
                        transform.position = position;
                        break;
                }
            }
            Quaternion rotation;
            if (reader.TryReadValue(k_RotationKey, out rotation, Quaternion.identity))
            {
                switch (m_Rotation)
                {
                    case TransformSerialization.LocalSpace:
                        transform.localRotation = rotation;
                        break;
                    case TransformSerialization.WorldSpace:
                        transform.rotation = rotation;
                        break;
                }
            }
            if (m_LocalScale)
            {
                Vector3 scale;
                if (reader.TryReadValue(k_ScaleKey, out scale, Vector3.one))
                    transform.localScale = scale;
            }

            // Read neo-serialized components
            if (m_FilterNeoComponents == NeoSerializationFilter.Include)
            {
                // Write components in m_NeoComponents array only
                for (int i = 0; i < m_NeoComponents.Length; ++i)
                    ReadNeoComponent(reader, m_NeoComponents[i]);
            }
            else
            {
                GetComponents(s_GatheredNeoComponents);
                for (int i = 0; i < s_GatheredNeoComponents.Count; ++i)
                {
                    // Skip components in m_NeoComponents
                    if (Array.IndexOf(m_NeoComponents, s_GatheredNeoComponents[i]) != -1 ||
                        s_GatheredNeoComponents[i] == null)
                        continue;

                    ReadNeoComponent(reader, s_GatheredNeoComponents[i]);
                }
                s_GatheredNeoComponents.Clear();
            }

            // Read other components (always manually included)
            for (int i = 0; i < m_OtherComponents.Length; ++i)
                ReadOtherComponent(reader, m_OtherComponents[i]);

            // Read child objects
            m_Children.ReadGameObjectProperties(reader);
        }

        void ReadNeoComponent(INeoDeserializer reader, MonoBehaviour component)
        {
            var c = component as INeoSerializableComponent;
            if (c != null)
            {
                int id = NeoSerializationUtilities.GetPersistentComponentID(component);
                if (reader.PushContext(SerializationContext.ComponentNeoSerialized, id))
                {
                    try
                    {
                        // Get enabled state of the component
                        // Do this first incase reading properties causes coroutines to be started, etc
                        bool isEnabled = true;
                        if (reader.TryReadValue(k_EnabledKey, out isEnabled, true))
                            component.enabled = isEnabled;

                        c.ReadProperties(reader, this);
                    }
                    //catch (Exception e)
                    //{
                    //    Debug.LogError("Reading component failed due to error: " + e.Message, component.gameObject);
                    //}
                    finally
                    {
                        reader.PopContext(SerializationContext.ComponentNeoSerialized, id);
                    }
                }
            }
        }

        void ReadNeoComponent(INeoDeserializer reader, INeoSerializableComponent component)
        {
            var c = component as MonoBehaviour;
            if (c != null)
            {
                int id = NeoSerializationUtilities.GetPersistentComponentID(c);
                if (reader.PushContext(SerializationContext.ComponentNeoSerialized, id))
                {
                    try
                    {
                        // Get enabled state of the component
                        // Do this first incase reading properties causes coroutines to be started, etc
                        bool isEnabled = true;
                        if (reader.TryReadValue(k_EnabledKey, out isEnabled, true))
                            c.enabled = isEnabled;

                        component.ReadProperties(reader, this);
                    }
                    //catch (Exception e)
                    //{
                    //    Debug.LogError(string.Format("Reading component ({0}) failed due to error: {1}", component.GetType(), e.Message), c.gameObject);
                    //}
                    finally
                    {
                        reader.PopContext(SerializationContext.ComponentNeoSerialized, id);
                    }
                }
            }
        }

        void ReadOtherComponent(INeoDeserializer reader, Component component)
        {
            // Require formatters system before re-enabling this feature
            if (component != null)
            {
                int id = NeoSerializationUtilities.GetPersistentComponentID(component);
                if (reader.PushContext(SerializationContext.ComponentNeoFormatted, id))
                {
                    try
                    {
                        var formatter = NeoSerializationFormatters.GetFormatter(component);
                        formatter.ReadProperties(reader, component, this);
                    }
                    //catch (Exception e)
                    //{
                    //    Debug.LogError("Reading component failed due to error: " + e.Message, component.gameObject);
                    //}
                    finally
                    {
                        reader.PopContext(SerializationContext.ComponentNeoFormatted, id);
                    }

                }
            }
        }

#endregion

#region INSTANTIATION

        public T InstantiatePrefab<T>(T prototype) where T : Component
        {
            return NeoSerializedObjectFactory.Instantiate(prototype, m_Children);
        }

        public T InstantiatePrefab<T>(T prototype, Vector3 position, Quaternion rotation) where T : Component
        {
            return NeoSerializedObjectFactory.Instantiate(prototype, m_Children, position, rotation);
        }

        public T InstantiatePrefab<T>(int prefabID, int serializationKey) where T : Component
        {
            var result = NeoSerializedObjectFactory.Instantiate(prefabID, serializationKey, m_Children);
            if (result != null)
                return result.GetComponent<T>();
            else
                return null;
        }

#endregion
        
#region EDITOR SPECIFIC (CONDITIONAL)
#if UNITY_EDITOR

        // Inspector foldout expansion tracking
        [HideInInspector] public bool expandChildObjects = false;
        [HideInInspector] public bool expandNeoComponents = false;
        [HideInInspector] public bool expandOtherComponents = false;

        // Guid tracking (if it doesn't match the asset file's then update and use to generate a new prefabID)
        [HideInInspector] public string prefabGuid = string.Empty;

        private static List<NeoSerializedGameObject> s_GatheredNsgos = new List<NeoSerializedGameObject>();
        
        void CheckSerializationKey()
        {
            var parent = GetParent();
            if (parent == null)
            {
                if (gameObject.scene.IsValid())
                {
                    var nss = NeoSerializedScene.GetByBuildIndex(gameObject.scene.buildIndex);
                    if (nss != null)
                        nss.sceneObjects.Validate(this);
                }
            }
            else
            {
                parent.serializedChildren.Validate(this);
            }
        }

        public void CheckPrefabID()
        {
            string guid = UnityEditor.AssetDatabase.AssetPathToGUID(UnityEditor.AssetDatabase.GetAssetPath(gameObject));
            if (!string.IsNullOrEmpty(guid) && prefabGuid != guid)
            {
                prefabGuid = guid;
                m_PrefabStrongID = NeoSerializationUtilities.StringToHash(guid);
            }
        }

        [UnityEditor.InitializeOnLoadMethod]
        static void AddPrefabHierarchyChecks()
        {
            PrefabStage.prefabStageOpened += ValidatePrefab;
        }

        private static void ValidatePrefab(PrefabStage stage)
        {
            var root = stage.prefabContentsRoot;
            var nsgo = root.GetComponent<NeoSerializedGameObject>();
            if (nsgo != null)
                nsgo.CheckPrefabHierarchy();
        }

        public bool CheckPrefabHierarchy()
        {
            bool result = false;

            GetComponentsInChildren(true, s_GatheredNsgos);

            // Filter for invalid objects
            for (int i = s_GatheredNsgos.Count - 1; i >= 0; --i)
            {
                if (s_GatheredNsgos[i].GetParent() != this)
                    s_GatheredNsgos.RemoveAt(i);
            }

            // Assign child container if required and validate
            if (m_Children == null || !m_Children.isValid)
            {
                m_Children = new NeoSerializedGameObjectChildContainer(this);
                result = true;
            }
            if (m_Children.Validate(s_GatheredNsgos))
            {
                UnityEditor.EditorUtility.SetDirty(this);
                result = true;
            }

            s_GatheredNsgos.Clear();

            return result;
        }

#endif
#endregion
    }
}
