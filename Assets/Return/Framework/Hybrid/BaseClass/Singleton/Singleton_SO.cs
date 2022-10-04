using UnityEngine;
using Sirenix.OdinInspector;

public abstract class Singleton_SO : SerializedScriptableObject, ISerializationCallbackReceiver
{
    /// <summary>
    /// Manually invoke or IStart push.
    /// </summary>
    public virtual void Initialize() { }



    public void Destroy()
    {
#if UNITY_EDITOR
        DestroyImmediate(this,true);
#else
        Destroy(this);
#endif
    }

    //public virtual void OnAfterDeserialize()
    //{
    //}

    //public virtual void OnBeforeSerialize()
    //{

    //}
}


