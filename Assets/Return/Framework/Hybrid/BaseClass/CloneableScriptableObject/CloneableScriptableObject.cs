using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using UnityEngine.Serialization;
using Newtonsoft.Json;

namespace Return
{
    [Serializable]
    public class CloneableScriptableObject : SerializedScriptableObject
    {
        [ReadOnly]
        [ShowInInspector]
        [PropertyOrder(-100)]
        [HideLabel]
        public HideFlags m_Flag => this.hideFlags;
        
        public T UnityJsonCloneSelf<T>(HideFlags hideFlags = HideFlags.DontSave) where T : CloneableScriptableObject
        {
            var newSO = CreateInstance<T>();
            newSO.hideFlags = hideFlags;

            //var data = JsonConvert.SerializeObject((T)this, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            //JsonConvert.PopulateObject(data, newSO);

            var data = JsonUtility.ToJson((T)this);
            JsonUtility.FromJsonOverwrite(data, newSO);
            return newSO;
        }

        public ScriptableObject UnityJsonClone(HideFlags hideFlags = HideFlags.DontSave)
        {
            var type = GetType();
            var newSO = CreateInstance(type);
            newSO.hideFlags = hideFlags;

            //var data = JsonConvert.SerializeObject((T)this, new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore });
            //JsonConvert.PopulateObject(data, newSO);

            var data = JsonUtility.ToJson(this);
            JsonUtility.FromJsonOverwrite(data, newSO);
            return newSO;
        }

        //protected virtual T _Clone<T>(T @object)  where T:ScriptableObject
        //{
        //    var data = JsonConvert.SerializeObject(@object);

        //    var newSO = CreateInstance<T>();

        //    JsonConvert.PopulateObject(data, newSO);
        //    return newSO;
        //}
    }


}