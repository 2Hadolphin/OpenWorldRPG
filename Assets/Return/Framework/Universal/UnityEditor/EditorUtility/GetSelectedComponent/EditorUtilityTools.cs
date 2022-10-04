using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

namespace Return
{
    public static partial class EditorUtilityTools
    {
        public static bool GetSelectedComponent<T>(out T obj) where T : UnityEngine.Component
        {
            var selected = Selection.activeGameObject;

            if (selected == null)
            {
                obj = default;
                return false;
            }
            
            return selected.TryGetComponent(out obj);
        }

        public static string GetGUID(UnityEngine.Object obj)
        {
            var path = AssetDatabase.GetAssetPath(obj);
            var guid = AssetDatabase.GUIDFromAssetPath(path);

            return guid.ToString();
        }

        public static bool IsSubAsset(UnityEngine.Object obj)
        {
            return AssetDatabase.IsSubAsset(obj);
        }
    }
}
