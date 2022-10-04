using UnityEngine;
using UnityEngine.AddressableAssets;
namespace Return
{
    public abstract class BaseAssetReferenceT<T>: AssetReferenceT<T> where T:UnityEngine.Object
    {
        protected BaseAssetReferenceT(string guid) : base(guid)
        {
            Debug.Log(guid);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Typeless override of parent editorAsset. Used by the editor to represent the main asset referenced.
        /// </summary>
        public new Object editorAsset
        {
            get
            {
                if (CachedAsset != null || string.IsNullOrEmpty(AssetGUID))
                    return CachedAsset;

                var prop = typeof(AssetReference).GetProperty("editorAsset");
                return prop.GetValue(this, null) as Object;
            }
        }
#endif
    }

}
