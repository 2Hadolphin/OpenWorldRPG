using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Assertions;

using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Text;



namespace Return
{
    public static class AddressableExtension
    {
        static readonly Dictionary<AsyncOperationHandle<GameObject>, Action<object>> LoadingCache = new(100);

        public static bool TryCache<T>(this AssetReference assetReference,out T result) where T :UnityEngine.Object
        {
            var valid = assetReference.IsValid() && assetReference.IsDone;

            if (!valid)
            {
                result = default;
                return valid;
            }

            return assetReference.Asset.Parse(out result);
        }

        public static bool LoadNativeObject<T>(this AssetReference asset, out AsyncOperationHandle<T> handle)
        {
            var valid = asset.RuntimeKeyIsValid();



            if (valid /*&& !asset.OperationHandle.Status.HasFlag(AsyncOperationStatus.Succeeded)*/)
            {
                handle = asset.LoadAssetAsync<T>();
            }
            else
            {
#if UNITY_EDITOR
                Debug.LogError(asset + " AssetReference " + asset.IsValid());
                Debug.LogError(asset + " RuntimeKey " + asset.RuntimeKeyIsValid());
#endif
                handle = default;
            }

            return valid;
        }


        public static bool LoadNativeGameObject(this AssetReference asset, Action<GameObject> callback)
        {
            return LoadGameObject(asset, callback, false);
        }

        public static GameObject LoadInstantiateGameObject(this AssetReferenceGameObject asset)
        {
            if (asset.IsValid() && asset.IsDone)
                return GameObject.Instantiate((GameObject)asset.Asset);
            else
                return GameObject.Instantiate(asset.LoadAssetAsync().WaitForCompletion());
        }

        /// <summary>
        /// Asyn load addressable gameObject
        /// </summary>
        public static bool LoadInstanceGameObject(this AssetReference asset, Action<GameObject> callback)
        {
            return LoadGameObject(asset, callback, true);
        }

        private static bool LoadGameObject(this AssetReference asset,Action<GameObject> callback,bool instantiate)
        {
            Assert.IsNotNull(callback);

            var valid = /*asset.IsValid()&&*/asset.RuntimeKeyIsValid();

            if (valid)
            {
                var handle = asset.LoadAssetAsync<GameObject>();

                Assert.IsFalse(LoadingCache.ContainsKey(handle));

                LoadingCache.Add(handle, new ActionWrapper<GameObject>(callback));

                if (instantiate)
                    handle.Completed += LoadInastancGameObjectCallback;
                else
                    handle.Completed += LoadNativeGameObjectCallback;
            }
#if UNITY_EDITOR
            else
            {
                Debug.LogError(asset+" AssetReference " + asset.IsValid());
                Debug.LogError(asset+" RuntimeKey " + asset.RuntimeKeyIsValid());
            }
#endif

            return valid;
        }

        private static bool ValidResult(AsyncOperationHandle<GameObject> result,out Action<GameObject> callback,bool removeCache=true)
        {
            callback = default;

            if (LoadingCache.TryGetValue(result, out var cacheCallback) && cacheCallback.NotNull())
            {
                if (removeCache)
                    LoadingCache.Remove(result);

                if (!result.Status.HasFlag(AsyncOperationStatus.Succeeded))
                {
                    Debug.LogError("Loading addressable faiulre " + result.Status);
                    return false;
                }
                else
                {
                    callback = (Action<GameObject>)cacheCallback;
                    return true;
                }
            }

            Debug.LogError("Missing addressable handle func.");
            return false;
        }

        private static void LoadNativeGameObjectCallback(AsyncOperationHandle<GameObject> handle)
        {
            if (ValidResult(handle, out var callback))
                callback(handle.Result);
        }

        private static void LoadInastancGameObjectCallback(AsyncOperationHandle<GameObject> handle)
        {
            if (ValidResult(handle, out var callback))
                callback(GameObject.Instantiate(handle.Result));
        }
    }
}
