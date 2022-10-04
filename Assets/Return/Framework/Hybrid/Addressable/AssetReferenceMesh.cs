using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
namespace Return
{

    [System.Serializable]
    public class AssetReferenceMesh : BaseAssetReferenceT<Mesh>
    {
        public AssetReferenceMesh(string guid) : base(guid) { }
    }

}
