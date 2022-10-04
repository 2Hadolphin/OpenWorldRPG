#if UNITY_EDITOR
using System.Collections;
using UnityEngine;
using UnityEditor;
using System.Linq;

namespace Return.Editors
{
    public class RemoveMissingScript
    {
        [MenuItem("Assets/GameObject/Remove Missing Scripts", true)]
        static bool CheckGameObject()
        {
            return Selection.activeObject is GameObject;
        }

        [MenuItem("Assets/GameObject/Remove Missing Scripts")]
        public static void StripMissingScripts()
        {
            if (Selection.activeGameObject != null)
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(Selection.activeGameObject);
        }

        [MenuItem("GameObject/Remove All Missing Scripts")]
        public static void RemoveAllMissingScripts()
        {
            var objs = Resources.FindObjectsOfTypeAll<GameObject>();
            int count = objs.Sum(GameObjectUtility.RemoveMonoBehavioursWithMissingScript);
            Debug.Log($"Removed {count} missing scripts");
        }
    }
}
#endif