using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Sirenix.OdinInspector;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Return.Database
{
    /// <summary>
    /// Reference to target asset.
    /// </summary>
    public abstract class AssetsPreset : PresetDatabase
    {
        public AssetsSourceType SourceType = AssetsSourceType.Internal;


    }


    /// <summary>
    /// Reference to target asset.
    /// </summary>
    public abstract class AssetsPreset<T> : AssetsPreset where T : UnityEngine.Object
    {

#if UNITY_EDITOR

        [ReadOnly]
        public string Path;

        /// <summary>
        /// Editor guid check
        /// </summary>
        public void LoadGUID()
        {
            if (Asset)
            {
                AssetDatabase.TryGetGUIDAndLocalFileIdentifier(Asset, out GUID, out long id);
                Debug.Log(id);
                Path = AssetDatabase.GUIDToAssetPath(GUID);
                Dirty();
            }
            //else
            //    throw new KeyNotFoundException(string.Format("Asset reference is invalid, can't found any asset via GUID : {0} ", GUID));
        }

        [PreviewField(Height = 65, Alignment = ObjectFieldAlignment.Center)]
        [PropertySpace(10, 10)]
        [AssetsOnly]
        [AssetsPresetValid(nameof(GUID), nameof(SourceType))]
        [ShowInInspector]
        [OnValueChanged(nameof(LoadGUID))]
#endif
        [SerializeField]
        public T Asset;


#if UNITY_EDITOR



        [PropertyOrder(10)]
        [Button(nameof(BundleAsset))]
        public virtual void BundleAsset()
        {
            return;
            var asset = Asset;
            AssetDatabase.RemoveObjectFromAsset(Asset);
            AssetDatabase.AddObjectToAsset(asset, AssetDatabase.GetAssetPath(this));
            Dirty();
            AssetDatabase.SaveAssets();
        }

        void SearchAsset()
        {
            if (string.IsNullOrEmpty(GUID))
                return;

            var path = AssetDatabase.GUIDToAssetPath(GUID);
            if (string.IsNullOrEmpty(path))
                throw new KeyNotFoundException(string.Format("GUID is invalid, can't found any asset via GUID : {0} ", GUID));

            Asset = AssetDatabase.LoadAssetAtPath<T>(path);
            Dirty();
        }

        [PropertyOrder(1)]
        [Button(nameof(Remove))]
        void Remove()
        {
            GUID = null;
            Asset = null;
            Path = null;
            Dirty();
        }

        [OnValueChanged(nameof(SearchAsset))]
#endif
        [PropertySpace(5, 5)]
        public string GUID;


#if UNITY_EDITOR

        protected virtual void OnDestroy()
        {
            if (Asset)
            {
                if (AssetDatabase.IsMainAsset(Asset))
                {
                    Assert.IsTrue(AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(Asset)));
                }
            }
        }
#endif



        public static implicit operator T(AssetsPreset<T> preset)
        {
            if (preset.Asset)
                return preset.Asset;

#if UNITY_EDITOR
            var path = UnityEditor.AssetDatabase.GUIDToAssetPath(preset.GUID);
            if (string.IsNullOrEmpty(path))
                throw new KeyNotFoundException(string.Format("Missing origin asset reference, can't found any asset via GUID : {0} ", preset.GUID));

            return UnityEditor.AssetDatabase.LoadAssetAtPath<T>(path);
#else
        return null;

#endif
        }
    }

}

