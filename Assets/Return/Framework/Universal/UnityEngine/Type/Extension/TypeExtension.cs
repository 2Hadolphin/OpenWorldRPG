using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace Return
{
    public static class TypeExtension
    {
        /// <summary>
        /// Create item via type.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        /// <param name="go">Target to attach(Component).</param>
        /// <returns>Return true if obj been created.</returns>
        public static bool Instance(this Type type,out object obj,Func<GameObject> go)
        {
            try
            {
                if (type.IsSubclassOf(typeof(Component)))
                    obj = go().InstanceIfNull(type);
                else if (type.IsSubclassOf(typeof(ScriptableObject)))
                    obj = ScriptableObject.CreateInstance(type);
                else if (type.IsSubclassOf(typeof(UnityEngine.Object)))
                    obj = Activator.CreateInstance(type);
                else
                    obj = Activator.CreateInstance(type);

                Debug.Log(obj.IsNull());
            }
            catch (Exception e)
            {
                throw e;
            }

            return obj.NotNull();
        }

      
    }
}
