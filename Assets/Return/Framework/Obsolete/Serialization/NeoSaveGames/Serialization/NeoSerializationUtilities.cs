using System;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace NeoSaveGames.Serialization
{
    /// <summary>
    /// A selection of useful methods and utilities for serialization / deserialization
    /// </summary>
    public static class NeoSerializationUtilities
    {
        private static List<Component> s_ComponentBuffer = new List<Component>(8);
        private static List<NeoSerializedGameObject> s_ReferenceChain = new List<NeoSerializedGameObject>(32);
        private static List<int> s_ReverseChain = new List<int>(32);
        private static TypeComparer s_TypeComparer = new TypeComparer();

        private class TypeComparer : IComparer<Component>
        {
            public int Compare(Component x, Component y)
            {
                if (x == null || y == null)
                    return 0;

                // "CompareTo()" method 
                return x.GetType().Name.CompareTo(y.GetType().Name);

            }
        }

        /// <summary>
        /// Convert a string to a unique integer ID
        /// </summary>
        /// <param name="key">The string to generate a hash for</param>
        /// <returns>A hash of the string</returns>
        public static int StringToHash(string key)
        {
            return Animator.StringToHash(key);
        }

        /// <summary>
        /// Get a consistent ID for a component. This is based off the type name and the index of the type on its GameObject.
        /// As long as the type name does not change and the component is not reshuffled on the GameObject (more components of the same type can be appended)
        /// then this method will return the same value.
        /// </summary>
        /// <param name="c">The component an ID is required for</param>
        /// <returns>An ID value to use for serialization</returns>
        public static int GetPersistentComponentID(Component c)
        {
            Type t = c.GetType();

            // Get number of component on object
            c.gameObject.GetComponents(t, s_ComponentBuffer);
            int i = 0;
            for (; i < s_ComponentBuffer.Count; ++i)
                if (s_ComponentBuffer[i] == c)
                    break;
            s_ComponentBuffer.Clear();

            return StringToHash(t.ToString() + i.ToString("D3"));
        }

        /// <summary>
        /// Write a serialized component reference.
        /// 
        /// References are either stored as an ID if it is on the sourceNeoSerializedGameObject. If the component is on a different
        /// NeoSerializedGameObject then it is stored as a chain of serialization keys for the hierarchy to the object the component is stored on.
        /// 
        /// The referenced component must be on an object that also contains a NeoSerializedGameObject component which is serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The component reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteComponentReference<T>(INeoSerializer writer, T value, NeoSerializedGameObject pathFrom, string key) where T : class
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key when writing component reference (cannot be null or empty)");
                return false;
            }
            else
                return WriteComponentReference(writer, value, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Write a Transform reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced Transform must be on a GameObject with a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The Transform reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteTransformReference(INeoSerializer writer, Transform value, NeoSerializedGameObject pathFrom, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key when writing component reference (cannot be null or empty)");
                return false;
            }
            else
                return WriteTransformReference(writer, value, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Write a GameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced GameObject must be have a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The GameObject reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteGameObjectReference(INeoSerializer writer, GameObject value, NeoSerializedGameObject pathFrom, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key when writing component reference (cannot be null or empty)");
                return false;
            }
            else
                return WriteGameObjectReference(writer, value, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Write a NeoSerializedGameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced NeoSerializedGameObject must be properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The NeoSerializedGameObject reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteNeoSerializedGameObjectReference(INeoSerializer writer, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom, string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                Debug.LogError("Invalid key when writing component reference (cannot be null or empty)");
                return false;
            }
            else
                return WriteNeoSerializedGameObjectReference(writer, value, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Write a Transform reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced Transform must be on a GameObject with a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The Transform reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteTransformReference(INeoSerializer writer, Transform value, NeoSerializedGameObject pathFrom, int hash)
        {
            if (value == null)
                return WriteObjectReference(writer, null, pathFrom, hash);
            else
            {
                var target = value.GetComponent<NeoSerializedGameObject>();
                if (target == null)
                {
                    Debug.LogError("Can only write transform references for transforms with a NeoSerializedGameObject behaviour on the same object");
                    return false;
                }
                else
                    return WriteObjectReference(writer, target, pathFrom, hash);
            }
        }

        /// <summary>
        /// Write a GameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced GameObject must be have a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The GameObject reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteGameObjectReference(INeoSerializer writer, GameObject value, NeoSerializedGameObject pathFrom, int hash)
        {
            if (value == null)
                return WriteObjectReference(writer, null, pathFrom, hash);
            else
            {
                var target = value.GetComponent<NeoSerializedGameObject>();
                if (target == null)
                {
                    Debug.LogError("Can only write GameObject references for transforms with a NeoSerializedGameObject behaviour on the same object");
                    return false;
                }
                else
                    return WriteObjectReference(writer, target, pathFrom, hash);
            }
        }

        /// <summary>
        /// Write a NeoSerializedGameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced NeoSerializedGameObject must be properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The NeoSerializedGameObject reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteNeoSerializedGameObjectReference(INeoSerializer writer, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom, int hash)
        {
            if (value == null)
                return WriteObjectReference(writer, null, pathFrom, hash);
            else
                return WriteObjectReference(writer, value, pathFrom, hash);
        }

        /// <summary>
        /// Write a serialized component reference.
        /// 
        /// References are either stored as an ID if it is on the sourceNeoSerializedGameObject. If the component is on a different
        /// NeoSerializedGameObject then it is stored as a chain of serialization keys for the hierarchy to the object the component is stored on.
        /// 
        /// The referenced component must be on an object that also contains a NeoSerializedGameObject component which is serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The component reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        public static bool WriteComponentReference<T>(INeoSerializer writer, T value, NeoSerializedGameObject pathFrom, int hash) where T : class
        {
            // Check basic parameters
            if (writer == null || pathFrom == null)
            {
                Debug.LogError("Invalid parameters when writing component reference");
                return false;
            }

            // Convert to component
            var cast = value as Component;
            if (value != null && cast == null)
            {
                Debug.LogError("AbstractValue is not a component");
                return false;
            }

            // Check if componenet reference is null
            if (value == null)
                writer.WriteValues(hash, (int[])null);
            else
            {
                // Check if component has an attached NeoSerializedGameObject
                var otherNsgo = cast.GetComponent<NeoSerializedGameObject>();
                if (otherNsgo == null)
                {
                    Debug.LogError("Can only write component references for components with a NeoSerializedGameObject behaviour on the same object: " + cast.name);
                    return false;
                }

                // Check if component is on a different game object to the referencing object
                // If so, check the other object is valid and then create a chain from the scene root to that object
                if (otherNsgo != pathFrom)
                {
                    // Get the serialized scene the object is in
                    var targetScene = NeoSerializedScene.GetByPath(cast.gameObject.scene.path);
                    if (targetScene == null)
                    {
                        Debug.LogError("Cannot write reference because the target object is not in a valid serialized scene.", cast.gameObject);
                        return false;
                    }

                    // Get the serialized scene the object is in
                    var sourceScene = NeoSerializedScene.GetByPath(pathFrom.gameObject.scene.path);
                    if (sourceScene == null)
                    {
                        Debug.LogError("Cannot write reference because the source object is not in a valid serialized scene.", pathFrom.gameObject);
                        return false;
                    }

                    // Check component is serialized
                    if (!otherNsgo.willBeSerialized)
                    {
                        Debug.LogError("Cannot write component reference because attached NeoSerializedGameObject is not serialized by a parent object or in the root of a serialized scene: " + otherNsgo.name, otherNsgo.gameObject);
                        return false;
                    }
                   
                    // Build reference chain (needs reversing)
                    do
                    {
                        s_ReferenceChain.Add(otherNsgo);
                        otherNsgo = otherNsgo.GetParent();
                    } while (otherNsgo != null);

                    // Start path chain with scene ID (0 if same scene as source)
                    if (sourceScene.hashedPath == targetScene.hashedPath)
                        s_ReverseChain.Add(0);
                    else
                        s_ReverseChain.Add(targetScene.hashedPath);

                    // Walk up from root object to target component's object
                    for (int i = s_ReferenceChain.Count - 1; i >= 0; --i)
                        s_ReverseChain.Add(s_ReferenceChain[i].serializationKey);
                }

                // Add the component ID
                s_ReverseChain.Add(GetPersistentComponentID(cast));

                // Write the chain
                writer.WriteValues(hash, s_ReverseChain);

                // Clean up
                s_ReferenceChain.Clear();
                s_ReverseChain.Clear();
            }

            return true;
        }

        /// <summary>
        /// Write a NeoSerializedGameObject reference.
        /// 
        /// References are either stored as an ID if it is on the sourceNeoSerializedGameObject. If the component is on a different
        /// NeoSerializedGameObject then it is stored as a chain of serialization keys for the hierarchy to the object the component is stored on.
        /// 
        /// The referenced component must be on an object that also contains a NeoSerializedGameObject component which is serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="writer">The serializer to use to write the properties</param>
        /// <param name="value">The component reference value to write</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `WriteProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>**true** if the write succeeded and **false** if there was a problem</returns>
        private static bool WriteObjectReference(INeoSerializer writer, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom, int hash)
        {
            // Check basic parameters
            if (writer == null || pathFrom == null)
            {
                Debug.LogError("Invalid parameters when writing component reference");
                return false;
            }

            // Check if reference is null
            if (value == null)
                writer.WriteValues(hash, (int[])null);
            else
            {
                // Check if value is on a different game object to the referencing object
                // If so, check the other object is valid and then create a chain from the scene root to that object
                if (value != pathFrom)
                {
                    // Get the serialized scene the object is in
                    var targetScene = NeoSerializedScene.GetByPath(value.gameObject.scene.path);
                    if (targetScene == null)
                    {
                        Debug.LogError("Cannot write reference because the target object is not in a valid serialized scene.", value.gameObject);
                        return false;
                    }

                    // Get the serialized scene the object is in
                    var sourceScene = NeoSerializedScene.GetByPath(pathFrom.gameObject.scene.path);
                    if (sourceScene == null)
                    {
                        Debug.LogError("Cannot write reference because the source object is not in a valid serialized scene.", pathFrom.gameObject);
                        return false;
                    }

                    // Check GameObject is serialized
                    if (!value.willBeSerialized)
                    {
                        Debug.LogError("Cannot write reference because attached NeoSerializedGameObject is not serialized by a parent object or in the root of a serialized scene.", value.gameObject);
                        return false;
                    }
                    
                    // Walk parent chain and add to list
                    do
                    {
                        s_ReferenceChain.Add(value);
                        value = value.GetParent();
                    }
                    while (value != null);

                    // Start path chain with scene ID (0 if same scene as source)
                    if (sourceScene.hashedPath == targetScene.hashedPath)
                        s_ReverseChain.Add(0);
                    else
                        s_ReverseChain.Add(targetScene.hashedPath);

                    // Walk up from root object to target object
                    for (int i = s_ReferenceChain.Count - 1; i >= 0; --i)
                        s_ReverseChain.Add(s_ReferenceChain[i].serializationKey);
                }

                // Write the chain
                writer.WriteValues(hash, s_ReverseChain);

                // Clean up
                s_ReferenceChain.Clear();
                s_ReverseChain.Clear();
            }

            return true;
        }

        /// <summary>
        /// Read a Transform reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced Transform must be on a GameObject with a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>The referenced Transform or null if not found</returns>
        public static bool TryReadTransformReference(INeoDeserializer reader, out Transform output, NeoSerializedGameObject pathFrom, string key)
        {
            return TryReadTransformReference(reader, out output, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Read a Transform reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced Transform must be on a GameObject with a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>The referenced Transform or null if not found</returns>
        public static bool TryReadTransformReference(INeoDeserializer reader, out Transform output, NeoSerializedGameObject pathFrom, int hash)
        {
            NeoSerializedGameObject result;
            if (TryReadNeoSerializedGameObjectReference(reader, out result, pathFrom, hash))
            {
                output = (result != null) ? result.transform : null;
                return true;
            }
            else
            {
                output = null;
                return false;
            }
        }

        /// <summary>
        /// Read a GameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced GameObject must be have a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>The referenced GameObject or null if not found</returns>
        public static bool TryReadGameObjectReference(INeoDeserializer reader, out GameObject output, NeoSerializedGameObject pathFrom, string key)
        {
            return TryReadGameObjectReference(reader, out output, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Read a GameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced GameObject must be have a NeoSerializedGameObject component which is properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>The referenced GameObject or null if not found</returns>
        public static bool TryReadGameObjectReference(INeoDeserializer reader, out GameObject output, NeoSerializedGameObject pathFrom, int hash)
        {
            NeoSerializedGameObject result;
            if (TryReadNeoSerializedGameObjectReference(reader, out result, pathFrom, hash))
            {
                output = (result != null) ? result.gameObject : null;
                return true;
            }
            else
            {
                output = null;
                return false;
            }
        }

        /// <summary>
        /// Read a NeoSerializedGameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced NeoSerializedGameObject must be properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>The referenced NeoSerializedGameObject or null if not found</returns>
        public static bool TryReadNeoSerializedGameObjectReference(INeoDeserializer reader, out NeoSerializedGameObject output, NeoSerializedGameObject pathFrom, string key)
        {
            return TryReadNeoSerializedGameObjectReference(reader, out output, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Read a NeoSerializedGameObject reference.
        /// 
        /// References are stored as a chain of serialization keys for the object hierarchy, from the scene root to the target object.
        /// 
        /// The referenced NeoSerializedGameObject must be properly serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>The referenced NeoSerializedGameObject or null if not found</returns>
        /// <returns></returns>
        public static bool TryReadNeoSerializedGameObjectReference(INeoDeserializer reader, out NeoSerializedGameObject output, NeoSerializedGameObject pathFrom, int hash)
        {
            output = null;

            int[] chain = null;
            if (reader.TryReadValues(hash, out chain, null))
            {
                // Check for null
                if (chain == null)
                    return false;

                // Check if on the same object
                if (chain.Length == 0)
                    return pathFrom;
                else
                {
                    // Get the target scene
                    NeoSerializedScene targetScene = null;
                    if (chain[0] == 0)
                    {
                        // Get from source object
                        if (pathFrom == null)
                            return false;
                        targetScene = NeoSerializedScene.GetByPath(pathFrom.gameObject.scene.path);
                    }
                    else
                        targetScene = NeoSerializedScene.GetByPathHash(chain[0]);

                    // Iterate through the chain
                    if (targetScene != null)
                    {
                        // Grab start of chain
                        var next = targetScene.sceneObjects[chain[1]];
                        if (next == null)
                            return false;

                        // Work along chain to target object
                        for (int i = 2; i < chain.Length; ++i)
                        {
                            // Get the next object in the chain
                            next = next.serializedChildren[chain[i]];
                            if (next == null)
                                return false;
                        }
                        output = next;
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
                return false;
        }

        /// <summary>
        /// Read a serialized component reference.
        /// 
        /// References are either stored as an ID if it is on the sourceNeoSerializedGameObject. If the component is on a different
        /// NeoSerializedGameObject then it is stored as a chain of serialization keys for the hierarchy to the object the component is stored on.
        /// 
        /// The referenced component must be on an object that also contains a NeoSerializedGameObject component which is serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <typeparam name="T">The component type required</typeparam>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="key">The key for the reference property</param>
        /// <returns>The referenced component or null if not found</returns>
        public static bool TryReadComponentReference<T>(INeoDeserializer reader, out T output, NeoSerializedGameObject pathFrom, string key) where T : class
        {
            return TryReadComponentReference(reader, out output, pathFrom, StringToHash(key));
        }

        /// <summary>
        /// Read a serialized component reference.
        /// 
        /// References are either stored as an ID if it is on the sourceNeoSerializedGameObject. If the component is on a different
        /// NeoSerializedGameObject then it is stored as a chain of serialization keys for the hierarchy to the object the component is stored on.
        /// 
        /// The referenced component must be on an object that also contains a NeoSerializedGameObject component which is serialized and in the same scene as the nsgo parameter.
        /// </summary>
        /// <typeparam name="T">The component type required</typeparam>
        /// <param name="reader">The deserializer to use to read the properties</param>
        /// <param name="pathFrom">The serialized game object to use as the source. If the reference property is in a serialized component, this should be the object passed to its `ReadProperties()` method</param>
        /// <param name="hash">The hash for the reference property</param>
        /// <returns>The referenced component or null if not found</returns>
        public static bool TryReadComponentReference<T>(INeoDeserializer reader, out T output, NeoSerializedGameObject pathFrom, int hash) where T : class
        {
            output = null;

            // Get the chain to the component
            int[] chain = null;
            if (!reader.TryReadValues(hash, out chain, null) | chain == null)
                return false;

            // Get the NeoSerializedGameObject the component is attached to
            NeoSerializedGameObject owner = null;
            switch (chain.Length)
            {
                case 0:
                    return false;
                case 1:
                    owner = pathFrom;
                    break;
                default:
                    {
                        // Get the target scene
                        NeoSerializedScene targetScene = null;
                        if (chain[0] == 0)
                        {
                            // Get from source object
                            if (pathFrom == null)
                                return false;
                            targetScene = NeoSerializedScene.GetByPath(pathFrom.gameObject.scene.path);
                        }
                        else
                        {
                            targetScene = NeoSerializedScene.GetByPathHash(chain[0]);
                        }

                        // Iterate through the chain
                        if (targetScene != null)
                        {
                            // Grab start of chain
                            var next = targetScene.sceneObjects[chain[1]];
                            if (next == null)
                                return false;

                            // Work along chain to containing object
                            for (int i = 2; i < chain.Length - 1; ++i)
                            {
                                // Get the next object in the chain
                                next = next.serializedChildren[chain[i]];
                                if (next == null)
                                    return false;
                            }
                            owner = next;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    break;
            }        
            
            // Get all components on object and sort by type
            owner.GetComponents(s_ComponentBuffer);
            s_ComponentBuffer.Sort(s_TypeComparer);

            // Iterate through and check hashes
            Type lastType = null;
            int count = 0;
            for(int i = 0; i < s_ComponentBuffer.Count; ++i)
            {
                // Get type and check count on object
                var t = s_ComponentBuffer[i].GetType();
                if (t != lastType)
                {
                    lastType = t;
                    count = 0;
                }
                else
                    ++count;

                // Get hash and check against target
                int currentHash = StringToHash(t.ToString() + count.ToString("D3"));
                if (currentHash == chain[chain.Length - 1])
                {
                    //Debug.Log("Found component reference, index: " + count);
                    output = s_ComponentBuffer[i] as T;
                    return true;
                }
            }

            // Hash wasn't found
            //Debug.Log("Component reference not found");
            return false;
        }
    }
}
