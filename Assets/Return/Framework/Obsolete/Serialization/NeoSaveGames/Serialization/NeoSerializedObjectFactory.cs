using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoSaveGames.Serialization
{
    public static class NeoSerializedObjectFactory
    {
        private static Dictionary<int, RegisteredPrefab> s_RegisteredPrefabs = new Dictionary<int, RegisteredPrefab>();
        private static Dictionary<int, RegisteredAsset> s_RegisteredAssets = new Dictionary<int, RegisteredAsset>();

        private class RegisteredPrefab
        {
            public int references;
            public NeoSerializedGameObject prefab;

            public void Increment()
            {
                ++references;
            }

            public bool Decrement()
            {
                --references;
                return references > 0;
            }

            public RegisteredPrefab(NeoSerializedGameObject p)
            {
                prefab = p;
                references = 1;
            }
        }

        private class RegisteredAsset
        {
            public int references;
            public INeoSerializableAsset asset;

            public void Increment()
            {
                ++references;
            }

            public bool Decrement()
            {
                --references;
                return references > 0;
            }

            public RegisteredAsset(INeoSerializableAsset a)
            {
                asset = a;
                references = 1;
            }
        }

        public static NeoSerializedGameObject GetPrefab(int prefabID)
        {
            RegisteredPrefab result;
            if (s_RegisteredPrefabs.TryGetValue(prefabID, out result))
                return result.prefab;
            else
                return null;
        }

        public static bool IsPrefabRegistered(int prefabID)
        {
            return s_RegisteredPrefabs.ContainsKey(prefabID);
        }

        public static INeoSerializableAsset GetAsset(int assetID)
        {
            RegisteredAsset result;
            if (s_RegisteredAssets.TryGetValue(assetID, out result))
                return result.asset;
            else
                return null;
        }

        public static bool IsAssetRegistered(int assetID)
        {
            return s_RegisteredAssets.ContainsKey(assetID);
        }

        #region REGISTRATION

        public static void RegisterPrefab(NeoSerializedGameObject prefab)
        {
            if (prefab != null)
            {
                RegisteredPrefab registeredPrefab;
                if (s_RegisteredPrefabs.TryGetValue(prefab.prefabStrongID, out registeredPrefab))
                {
                    //Debug.Log("Incremented prefab references: " + prefab.prefabStrongID);
                    registeredPrefab.Increment();
                }
                else
                {
                    s_RegisteredPrefabs.Add(prefab.prefabStrongID, new RegisteredPrefab(prefab));
                    //Debug.Log("Registered prefab: " + prefab.prefabStrongID);
                }
            }
        }

        public static void RegisterPrefabs(NeoSerializedGameObject[] prefabs)
        {
            for (int i = 0; i < prefabs.Length; ++i)
                RegisterPrefab(prefabs[i]);
        }

        public static void UnregisterPrefab(NeoSerializedGameObject prefab)
        {
            if (prefab != null)
            {
                RegisteredPrefab registeredPrefab;
                if (s_RegisteredPrefabs.TryGetValue(prefab.prefabStrongID, out registeredPrefab))
                {
                    //Debug.Log("Unregistering prefab: " + prefab.prefabStrongID);
                    if (registeredPrefab.references == 1)
                        s_RegisteredPrefabs.Remove(prefab.prefabStrongID);
                    else
                        registeredPrefab.Decrement();
                }
            }
        }

        public static void UnregisterPrefabs(NeoSerializedGameObject[] prefabs)
        {
            for (int i = 0; i < prefabs.Length; ++i)
                UnregisterPrefab(prefabs[i]);
        }

        public static void RegisterAsset(INeoSerializableAsset asset)
        {
            if (asset != null)
            {
                RegisteredAsset registeredAsset;
                if (s_RegisteredAssets.TryGetValue(asset.GetInstanceID(), out registeredAsset))
                    registeredAsset.Increment();
                else
                    s_RegisteredAssets.Add(asset.GetInstanceID(), new RegisteredAsset(asset));
            }
        }

        public static void RegisterAssets(INeoSerializableAsset[] assets)
        {
            for (int i = 0; i < assets.Length; ++i)
                RegisterAsset(assets[i]);
        }

        public static void UnregisterAsset(INeoSerializableAsset asset)
        {
            if (asset != null)
            {
                RegisteredAsset registeredAsset;
                if (s_RegisteredAssets.TryGetValue(asset.GetInstanceID(), out registeredAsset))
                {
                    if (registeredAsset.references == 1)
                        s_RegisteredAssets.Remove(asset.GetInstanceID());
                    else
                        registeredAsset.Decrement();
                }
            }
        }

        public static void UnregisterAssets(INeoSerializableAsset[] assets)
        {
            for (int i = 0; i < assets.Length; ++i)
                UnregisterAsset(assets[i]);
        }

        #endregion

        #region INSTANTIATION

        static List<int> s_ErrorObjects = new List<int>();
        static List<int> s_ErrorIDs = new List<int>();

        static void LogInstantiationError(GameObject prefab, string message)
        {
            int id = prefab.GetInstanceID();
            if (!s_ErrorObjects.Contains(id))
            {
                Debug.LogWarning(message, prefab);
                s_ErrorObjects.Add(id);
            }
        }

        static bool CheckAndConnectPrefabInstance<T>(T instance, T prefab, INeoSerializedGameObjectContainer container) where T : Component
        {
            // Get NeoSerializedGameObject for prefab/object
            var nsgo = instance.GetComponent<NeoSerializedGameObject>();
            if (nsgo == null)
            {
                LogInstantiationError(prefab.gameObject, "Using NeoSerializedObjectFactory to instantiate a prefab without a NeoSerializedGameObject component: " + prefab.name);
                return false;
            }
            
            if (!s_RegisteredPrefabs.ContainsKey(nsgo.prefabStrongID))
            {
                LogInstantiationError(prefab.gameObject, "Using NeoSerializedObjectFactory to instantiate a prefab that isn't registered for serialization/deserialization: " + prefab.name);
                return false;
            }

            // RegisterHandler object
            container.RegisterObject(nsgo);
            nsgo.wasRuntimeInstantiated = true;

            return true;
        }
        
        public static T Instantiate<T>(T prototype, INeoSerializedGameObjectContainer container) where T : Component
        {
            var result = Object.Instantiate(prototype, container.rootTransform);
            CheckAndConnectPrefabInstance(result, prototype, container);
            return result;
        }

        public static T Instantiate<T>(T prototype, INeoSerializedGameObjectContainer container, Vector3 position, Quaternion rotation) where T : Component
        {
            var result = Object.Instantiate(prototype, position, rotation, container.rootTransform);
            CheckAndConnectPrefabInstance(result, prototype, container);
            return result;
        }
        public static NeoSerializedGameObject Instantiate(int prefabID, int serializedKey, INeoSerializedGameObjectContainer container)
        {
            RegisteredPrefab registeredPrefab;
            if (s_RegisteredPrefabs.TryGetValue(prefabID, out registeredPrefab))
            {
                //Debug.Log(string.Format("Instantiating Prefab: {0}, id: {1}", registeredPrefab.prefab.name, serializedKey));
                var result = Object.Instantiate(registeredPrefab.prefab, container.rootTransform);
                result.serializationKey = serializedKey;
                result.wasRuntimeInstantiated = true;
                container.RegisterObject(result);
                return result;
            }
            else
            {
                if (!s_ErrorIDs.Contains(prefabID))
                {
                    Debug.LogWarning("Attempting to instantiate prefab from ID that hasn't been registered. ID: " + prefabID);
                    s_ErrorIDs.Add(prefabID);
                }
                return null;
            }
        }

        #endregion
    }
}