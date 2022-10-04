#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using NeoSaveGames.Serialization;

namespace NeoFPSEditor
{
    public class SaveInfoBaseEditor : Editor
    {
        ReorderableList m_PrefabList = null;
        ReorderableList m_AssetList = null;

        bool m_ExpandLists = false;

        protected virtual void OnEnable()
        {
            var prefabsProperty = serializedObject.FindProperty("m_Prefabs");
            var assetsProperty = serializedObject.FindProperty("m_Assets");

            m_PrefabList = new ReorderableList(
                serializedObject,
                prefabsProperty,
                true, false, false, true
            );
            m_PrefabList.drawElementCallback = DrawRegisteredPrefabEntry;
            m_PrefabList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Registered Prefabs");
            };
            m_PrefabList.onRemoveCallback = RemoveListEntryAtIndex;

            m_AssetList = new ReorderableList(
                serializedObject,
                assetsProperty,
                true, false, false, true
            );
            m_AssetList.drawElementCallback = DrawRegisteredAssetEntry;
            m_AssetList.drawHeaderCallback = (Rect rect) =>
            {
                EditorGUI.LabelField(rect, "Registered Assets");
            };
            m_AssetList.onRemoveCallback = RemoveListEntryAtIndex;

            m_ExpandLists = (prefabsProperty.arraySize + assetsProperty.arraySize) < 10;
        }

        void DrawRegisteredPrefabEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Shift down by 2 (compensate for single rows rows)
            rect.y += 1;
            rect.height -= 4;
            // Get the element
            var element = m_PrefabList.serializedProperty.GetArrayElementAtIndex(index);
            var prefab = element.objectReferenceValue as NeoSerializedGameObject;
            // Draw name
            var newValue = EditorGUI.ObjectField(rect, prefab, typeof(NeoSerializedGameObject), false);
            if (newValue == null)
                element.objectReferenceValue = null;
        }

        void DrawRegisteredAssetEntry(Rect rect, int index, bool isActive, bool isFocused)
        {
            // Shift down by 2 (compensate for single rows rows)
            rect.y += 1;
            rect.height -= 4;
            // Get the element
            var element = m_AssetList.serializedProperty.GetArrayElementAtIndex(index);
            var asset = element.objectReferenceValue as ScriptableObject;
            // Draw name
            var newValue = EditorGUI.ObjectField(rect, asset, typeof(ScriptableObject), false);
            if (newValue == null)
                element.objectReferenceValue = null;
        }

        void RemoveListEntryAtIndex(ReorderableList list)
        {
            var prop = list.serializedProperty;
            for (int i = prop.arraySize - 1; i > list.index; --i)
            {
                var lhs = prop.GetArrayElementAtIndex(i - 1);
                var rhs = prop.GetArrayElementAtIndex(i);
                lhs.objectReferenceValue = rhs.objectReferenceValue;
            }
            --prop.arraySize;
            prop.serializedObject.ApplyModifiedProperties();
        }
        
        protected void InspectRecreatableItems()
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Recreatable Items", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Prefab objects and ScriptableObject assets must be registered here in order to dynamically create them from a save game.",
                MessageType.Info
                );

            var prefab = EditorGUILayout.ObjectField("Add Prefab", null, typeof(NeoSerializedGameObject), false) as NeoSerializedGameObject;
            if (prefab != null)
                RegisterPrefab(prefab);

            var asset = EditorGUILayout.ObjectField("Add Asset", null, typeof(ScriptableObject), false) as ScriptableObject;
            if (asset != null)
            {
                SerializedArrayAdd(m_AssetList.serializedProperty, asset);
            }

            var folder = EditorGUILayout.ObjectField("Add Folder", null, typeof(DefaultAsset), false);
            if (folder != null)
            {
                // Get path
                string folderPath = AssetDatabase.GetAssetPath(folder);

                // Add prefabs from folder
                var guids = AssetDatabase.FindAssets("t:GameObject", new string[] { folderPath });
                for (int i = 0; i < guids.Length; ++i)
                {
                    var obj = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (obj != null)
                    {
                        var cast = obj.GetComponent<NeoSerializedGameObject>();
                        if (cast != null)
                            SerializedArrayAdd(m_PrefabList.serializedProperty, cast);
                    }
                }

                // Add assets from folder
                guids = AssetDatabase.FindAssets("t:ScriptableObject", new string[] { folderPath });
                for (int i = 0; i < guids.Length; ++i)
                {
                    asset = AssetDatabase.LoadAssetAtPath<ScriptableObject>(AssetDatabase.GUIDToAssetPath(guids[i]));
                    if (asset != null && asset is INeoSerializableAsset)
                        SerializedArrayAdd(m_AssetList.serializedProperty, asset);
                }
            }

            if (GUILayout.Button("Clear Prefabs"))
                SerializedArrayClear(m_PrefabList.serializedProperty);
            if (GUILayout.Button("Clear Assets"))
                SerializedArrayClear(m_AssetList.serializedProperty);

            m_ExpandLists = EditorGUILayout.Foldout(m_ExpandLists, "Registered Items", true);
            if (m_ExpandLists)
            {
                m_PrefabList.DoLayoutList();
                m_AssetList.DoLayoutList();
            }
        }

        public void RegisterPrefab(NeoSerializedGameObject prefab)
        {
            // Make sure it's the root-most prefab that's added
            var parent = prefab.transform.parent;
            while (parent != null)
            {
                var parentObject = parent.GetComponent<NeoSerializedGameObject>();
                if (parentObject != null)
                    prefab = parentObject;
                parent = prefab.transform.parent;
            }

            SerializedArrayAdd(m_PrefabList.serializedProperty, prefab);
        }

        void SerializedArrayAdd<T>(SerializedProperty arrayProp, T o) where T : Object
        {
            int count = arrayProp.arraySize;
            for (int i = 0; i < count; ++i)
            {
                var test = arrayProp.GetArrayElementAtIndex(i);
                if ((test.objectReferenceValue as T) == o)
                    return;
            }

            ++arrayProp.arraySize;
            var entry = arrayProp.GetArrayElementAtIndex(arrayProp.arraySize - 1);
            entry.objectReferenceValue = o;
        }

        void SerializedArrayClear(SerializedProperty arrayProp)
        {
            arrayProp.ClearArray();
            arrayProp.arraySize = 0;
        }
    }
}

#endif