using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.Assertions;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Return
{
    /// <summary>
    /// Addressable asset reference.
    /// </summary>
    [HideLabel]
    [Serializable]
    public class AddressableReference<T> : PresetReference<T> where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        public AddressableReference(T obj)
        {
            //if(!Reference.ValidateAsset(obj))
                if (UnityEditor.AssetDatabase.IsMainAsset(obj) || UnityEditor.AssetDatabase.IsSubAsset(obj))
                    AddressableEditorUtility.SetAddressableGroup(obj, "CharacterMesh");

            Reference = new AssetReferenceT<T>(EditorUtilityTools.GetGUID(obj));
        }

        [OnInspectorGUI]
        void DrawReference()
        {
            T asset=null;

            if(Reference.NotNull())
            {
                if (string.IsNullOrEmpty(Reference.SubObjectName))
                    asset = Reference.editorAsset;
                else
                    asset = Reference.GetEditorAsset<T>();
            }    


            var newAsset =UnityEditor.EditorGUILayout.ObjectField("Reference",asset,typeof(T),false);

            if (newAsset == null)
            {
                Reference.ReleaseAsset();
                return;
            }

            if (newAsset != asset)
            {
                Reference = new AssetReferenceT<T>(EditorUtilityTools.GetGUID(newAsset));

                //if (EditorUtilityTools.IsSubAsset(newAsset))
                //    Reference.SetEditorSubObject(newAsset);
                //else
                //    Reference.SetEditorAsset(newAsset);

                //Reference.LogAssetReference();

                Reference.LoadAssetAsync<T>().Completed+=(x)=>Debug.Log("Editor load : "+x.Result);

                Debug.Log("Set " + newAsset);
            }
        }

#endif

        [SerializeField]
        [HideInInspector]
        public AssetReferenceT<T> Reference;

        [NonSerialized]
        static readonly Dictionary<AsyncOperationHandle<T>, Action<object>> CallbackCache = new(50);


        public override bool LoadAssetT(Action<T> callback, bool returnNativeObj = false)
        {
#if UNITY_EDITOR
            AddressableEditorUtility.LogAssetReference(Reference);
#endif
            var valid = Reference.IsValid() && Reference.IsDone;

            if (valid)
            {
                Debug.Log("Reference is done");
                callback.Invoke(Reference.Asset as T);
            }
            else
            {
                valid = AddressableExtension.LoadNativeObject<T>(Reference, out var handle);

                if (valid)
                {
                    Debug.Log("Subscribe handle");
                    if(CallbackCache.TryGetValue(handle,out var finish))
                    {
                        finish += new ActionWrapper<T>(callback);
                    }
                    else
                    {
                        CallbackCache.Add(handle, typeof(T).IsSubclassOf(typeof(UnityEngine.Object)) ? new ActionWrapper<T>(callback) : (Action<object>)callback);
                        handle.Completed += OnAddressableLoadingCompleted;
                    }
                }
            }

            return valid;
        }


        public override async UniTask<T> LoadAsset(bool returnNativeObj = true, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.FixedUpdate, CancellationToken cancellationToken = default)
        {
            var request = Reference.LoadAssetAsync<T>();


            await request.ToUniTask(progress, timing, cancellationToken);

            Assert.IsTrue(request.IsDone);

            return request.Result;
        }

        static void OnAddressableLoadingCompleted(AsyncOperationHandle<T> handle)
        {
            Assert.IsTrue(handle.Status.HasFlag(AsyncOperationStatus.Succeeded),handle.Status.ToString());
            Assert.IsTrue(CallbackCache.ContainsKey(handle));

            try
            {
                if(handle.Status.HasFlag(AsyncOperationStatus.Succeeded))
                    if (CallbackCache.TryGetValue(handle, out var callback))
                    {
                        //handle.Completed -= callback;
                        callback?.Invoke(handle.Result);
                    }

            }
            finally
            {
                CallbackCache.Remove(handle);
            }
        }
    }

}
