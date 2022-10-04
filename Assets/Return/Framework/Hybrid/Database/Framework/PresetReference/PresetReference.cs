using System.Collections;
using Sirenix.OdinInspector;
using System;
using System.Text;
using Cysharp.Threading.Tasks;
using System.Threading;

namespace Return
{
    /// <summary>
    /// Container to refer asset.
    /// </summary>
    [HideLabel]
    [Serializable]
    public abstract class PresetReference:NullCheck
    {
        public abstract bool LoadAsset<T>(Action<T> callback, bool returnNativeObj = true) where T:UnityEngine.Object;
    }

    /// <summary>
    /// 
    /// </summary>
    [HideLabel]
    [Serializable]
    public abstract class PresetReference<T>: PresetReference where T : UnityEngine.Object
    {
        public override bool LoadAsset<V>(Action<V> callback, bool returnNativeObj = true)
        {
            return LoadAssetT(new ActionWrapper<V>(callback), returnNativeObj);

            //var valid = typeof(TSerializable) == typeof(V);
            
            //if(valid)
            //    return LoadAssetT()
        }

        public abstract bool LoadAssetT(Action<T> callback,bool returnNativeObj=true);

        public abstract UniTask<T> LoadAsset(bool returnNativeObj = true, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.FixedUpdate, CancellationToken cancellationToken = default(CancellationToken));
    }

}
