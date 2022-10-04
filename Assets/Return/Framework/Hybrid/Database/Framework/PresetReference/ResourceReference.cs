using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading;
using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Return.Database
{

    public class ResourceReference<T> : PresetReference<T> where T:UnityEngine.Object
    {
        [SerializeField]
        private string m_Path;

        public string Path { get => m_Path; set => m_Path = value; }

#if UNITY_EDITOR
        [OnValueChanged(nameof(LoadAsset))]
        [SerializeField]
        UnityEngine.Object m_asset;

        void LoadAsset()
        {
            if (m_asset.IsNull())
                return;

            var path = AssetDatabase.GetAssetPath(m_asset);
            var index = path.IndexOf("Resources");

            if (index <= 0)
                return;

            path = path.Substring(index);

            Path = path;
        }

#endif

        public override bool LoadAssetT(Action<T> callback, bool returnNativeObj = true)
        {
            var value = Resources.Load<T>(Path);
            
            var valid=value.NotNull();

            //if(valid && !returnNativeObj)
            // instatiate

            if (valid)
                callback?.Invoke(value);

            return value;
        }

        public override async UniTask<T> LoadAsset(bool returnNativeObj = true, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.FixedUpdate, CancellationToken cancellationToken = default)
        {
            var request = Resources.LoadAsync<T>(Path);

            await request.ToUniTask(progress,timing,cancellationToken);

            Assert.IsTrue(request.isDone);

            return request.asset as T;
        }

    }

}

