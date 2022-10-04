#if UNITY_EDITOR

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;
using System.Text;

#if UNITY_2021_2_OR_NEWER
using UnityEditor.SceneManagement;
#else
using UnityEditor.Experimental.SceneManagement;
#endif

namespace NeoSaveGames.Serialization
{
    [CustomEditor(typeof(NeoSerializedGameObject), true)]
    public class NeoSerializedGameObjectEditor : Editor
    {
        private ReorderableList m_ChildObjectsList = null;
        private ReorderableList m_NeoComponentsList = null;
        private ReorderableList m_OtherComponentsList = null;
        private List<NeoSerializedGameObjectOverrideEditor> m_Overrides = new List<NeoSerializedGameObjectOverrideEditor>();

        private NeoSerializedGameObject m_SourcePrefab = null;
        private NeoSerializedGameObject m_RegisterablePrefab = null;
        private SaveGameManager m_SaveGameManager = null;
        private int m_SourcePrefabID = 0;
        //private bool m_EditingPrefab = false;

        protected void OnEnable()
        {
            InitialiseChildObjectList();
            InitialiseNeoComponentList();
            InitialiseOtherComponentList();
            ResetOverrides();
            CheckIDs();
            CheckForPrefabStage();
            GetSaveDetails();
        }

        void CheckForPrefabStage()
        {
            // CheckAdd if the selected object is part of a prefab being edited in the prefab staging scene
            var cast = target as NeoSerializedGameObject;
            if ((PrefabStageUtility.GetCurrentPrefabStage() != null && PrefabStageUtility.GetCurrentPrefabStage().IsPartOfPrefabContents(cast.gameObject)))
            {
#if UNITY_2020_1_OR_NEWER
                var original = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabStageUtility.GetCurrentPrefabStage().assetPath);
#else
                var original = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabStageUtility.GetCurrentPrefabStage().prefabAssetPath);
#endif
                m_SourcePrefab = original.GetComponent<NeoSerializedGameObject>();
                if (m_SourcePrefab != null)
                    m_SourcePrefabID = m_SourcePrefab.prefabStrongID;
                //m_EditingPrefab = true;
            }
            //else
            //{
            //    if (PrefabUtility.IsPartOfAnyPrefab(cast) && !PrefabUtility.IsPartOfPrefabInstance(cast))
            //        m_EditingPrefab = true;
            //}
        }

        void GetSaveDetails()
        {

            var guids = AssetDatabase.FindAssets("t:SaveGameManager");
            if (guids != null && guids.Length > 0)
                m_SaveGameManager = AssetDatabase.LoadAssetAtPath<SaveGameManager>(AssetDatabase.GUIDToAssetPath(guids[0]));

            var cast = target as NeoSerializedGameObject;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(cast.gameObject))
            {
#if UNITY_2020_1_OR_NEWER
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(stage.assetPath);
#else
                var obj = AssetDatabase.LoadAssetAtPath<GameObject>(stage.prefabAssetPath);
#endif
                if (obj != null)
                    m_RegisterablePrefab = obj.GetComponent<NeoSerializedGameObject>();
            }
            else
            {
                m_RegisterablePrefab = cast.transform.root.GetComponent<NeoSerializedGameObject>();
            }
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.UpdateIfRequiredOrScript();

            var cast = target as NeoSerializedGameObject;
            bool needsRegistering = true;

            List<string> info = new List<string>();
            List<string> errors = new List<string>();
            NeoSerializedScene sceneInfo = null;

            // CheckAdd if registered with save system
            bool registeredWithSceneSaveInfo = false;
            bool registeredWithSaveManager = false;
            if (m_SaveGameManager != null)
            {
                if (m_SaveGameManager.CheckIsPrefabRegistered(m_RegisterablePrefab))
                    registeredWithSaveManager = true;
            }
            
            // Print serialization key
            if (cast.gameObject.scene.IsValid() && PrefabStageUtility.GetCurrentPrefabStage() == null)
            {
                // Get the scene
                var objects = cast.gameObject.scene.GetRootGameObjects();
                foreach (var obj in objects)
                {
                    sceneInfo = obj.GetComponent<NeoSerializedScene>();
                    if (sceneInfo != null)
                        break;
                }

                if (sceneInfo == null)
                {
                    errors.Add("- Object WILL NOT be serialized");
                    errors.Add("- No SceneSaveInfo found in scene");
                }
                else
                {
                    // CheckAdd if registered with scene save info
                    if (Array.IndexOf(sceneInfo.registeredPrefabs, m_RegisterablePrefab) != -1)
                        registeredWithSceneSaveInfo = true;

                    // CheckAdd if will be serialized
                    if (cast.willBeSerialized)
                    {
                        info.Add("- Object WILL be serialized");

                        if (!registeredWithSaveManager && !registeredWithSceneSaveInfo)
                            info.Add("- Prefab is not registered for runtime instantiation");
                        else
                        {
                            if (registeredWithSaveManager)
                                info.Add("- Prefab is registered with SaveGameManager");
                            if (registeredWithSceneSaveInfo)
                                info.Add("- Prefab is registered with SceneSaveInfo for this scene");
                        }

                        needsRegistering = false;
                    }
                    else
                    {
                        errors.Add("- Object WILL NOT be serialized");

                        if (!registeredWithSaveManager && !registeredWithSceneSaveInfo)
                            info.Add("- Prefab is not registered for runtime instantiation");
                    }
                }

                if (Application.isPlaying)
                {
                    if (cast.serializationKey == 0)
                        info.Add("- Serialization Key: <Not Set>");
                    else
                        info.Add("- Serialization Key: " + cast.serializationKey);
                }
                else
                {
                    int key = 0;

                    var parent = cast.GetParent();
                    if (parent != null)
                    {
                        key = parent.serializedChildren.GetSerializationKeyForObject(cast);
                    }
                    else
                    {
                        if (cast.transform.parent == null)
                        {
                            if (sceneInfo != null)
                            {
                                key = sceneInfo.sceneObjects.GetSerializationKeyForObject(cast);
                                if (key == 0)
                                    errors.Add("- Serialization key not set by scene");
                            }
                        }
                        else
                        {
                            errors.Add("- No NeoSerializedGameObject on root object");
                        }
                    }

                    if (key == 0)
                        info.Add("- Serialization Key: <Not Set>");
                    else
                        info.Add("- Serialization Key: " + key);
                }
            }
            else
            {
                // Log save system info
                if (!registeredWithSaveManager && !registeredWithSceneSaveInfo)
                    info.Add("- Prefab is not registered for runtime instantiation");
                else
                {
                    if (registeredWithSaveManager)
                        info.Add("- Prefab is registered with SaveGameManager");
                    if (registeredWithSceneSaveInfo)
                        info.Add("- Prefab is registered with SceneSaveInfo for this scene");
                }
            }

            // CheckAdd if it's a prefab (this only works outside of prefab-stage when editing a prefab - Great system ya got there Unity!)
            if (PrefabUtility.IsPartOfAnyPrefab(cast))
            {
                var stage = PrefabStageUtility.GetCurrentPrefabStage();
                if (!cast.gameObject.scene.IsValid() || (stage != null && stage.IsPartOfPrefabContents(cast.gameObject)))
                    info.Add("- Prefab ID: " + cast.prefabStrongID);

                // CheckAdd if it's an instance of the prefab in a scene
                if (PrefabUtility.IsPartOfPrefabInstance(cast))
                {
                    if (PrefabUtility.IsDisconnectedFromPrefabAsset(cast))
                        errors.Add("- Source prefab is disconnected");
                    if (PrefabUtility.IsPrefabAssetMissing(cast))
                        errors.Add("- Source prefab is MISSING");
                }
            }
            else
            {
                if (m_SourcePrefab != null)
                    info.Add("- Prefab ID: " + m_SourcePrefabID);
            }

            // CheckAdd if runtime instantiated
            if (cast.wasRuntimeInstantiated)
                info.Add("- Object correctly instantiated at runtime");

            // Build the error string
            StringBuilder builder = new StringBuilder();
            for(int i = 0; i < errors.Count; ++i)
                builder.AppendLine(errors[i]);
            for (int i = 0; i < info.Count; ++i)
                builder.AppendLine(info[i]);

            if (builder.Length > 0)
            {
                if (builder[builder.Length - 1] == '\n')
                    builder.Remove(builder.Length - 1, 1);

                EditorGUILayout.HelpBox(builder.ToString(), errors.Count > 0 ? MessageType.Warning : MessageType.Info);
            }

            // RegisterHandler with save system
            if (m_RegisterablePrefab != null && needsRegistering && !Application.isPlaying)
            {
                GUI.enabled = !registeredWithSaveManager && m_SaveGameManager != null;
                if (GUILayout.Button("RegisterHandler Prefab With SaveGameManager"))
                {
                    var guids = AssetDatabase.FindAssets("t:SaveGameManager");
                    if (guids != null && guids.Length > 0)
                    {
                        Debug.Log("Adding to save game manager: " + m_RegisterablePrefab);
                        EditorGUIUtility.PingObject(m_RegisterablePrefab.gameObject);
                        m_SaveGameManager.RegisterPrefab(m_RegisterablePrefab);
                    }
                }

                GUI.enabled = !registeredWithSceneSaveInfo && sceneInfo != null;
                if (GUILayout.Button("RegisterHandler Prefab With SceneSaveInfo"))
                {
                    sceneInfo.RegisterPrefab(m_RegisterablePrefab);
                }

                GUI.enabled = true;
            }
            EditorGUILayout.Space();

            // For debugging:
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("prefabGuid"));
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_PrefabStrongID"));

            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_SaveName"), new GUIContent("Save Object Name"));            

            // Transform
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Position"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Rotation"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_LocalScale"));

            // CheckAdd for limiters
            var limiters = cast.GetComponents<INeoSerializedGameObjectLimiter>();

            // Serialized contents
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Serialized Contents", EditorStyles.boldLabel);
            OnInspectChildObjects(limiters);
            OnInspectNeoComponents(limiters);
            OnInspectOtherComponents(limiters);

            // Overrides
            OnInspectOverrides(limiters);

            // For debugging:
            //EditorGUILayout.Space();
            //EditorGUILayout.LabelField("TEMPORARY", EditorStyles.boldLabel);
            //EditorGUILayout.PropertyField(serializedObject.FindProperty("m_Children"), true);

            EditorGUILayout.Space();
            serializedObject.ApplyModifiedProperties();
        }

        #region IDS

        void CheckIDs()
        {
            var nsgo = target as NeoSerializedGameObject;
            if (ShouldCheckGameObject(nsgo))
            {
                string guid = AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(nsgo.gameObject));
                CheckGameObject(nsgo, guid);
            }
        }

        void CheckGameObject(NeoSerializedGameObject nsgo, string guid)
        {
            // CheckAdd object
            if (nsgo.prefabGuid != guid)
            {
                var so = new SerializedObject(nsgo);
                so.FindProperty("prefabGuid").stringValue = guid;
                so.FindProperty("m_PrefabStrongID").intValue = NeoSerializationUtilities.StringToHash(guid);
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            // CheckAdd children
            var children = nsgo.GetComponentsInChildren<NeoSerializedGameObject>(true);
            foreach(var child in children)
            {
                if (child != nsgo && ShouldCheckGameObject(child))
                    CheckGameObject(child, guid);
            }
        }

        bool ShouldCheckGameObject(NeoSerializedGameObject nsgo)
        {
            if (Application.isPlaying)
                return false;

            if (PrefabUtility.IsPartOfPrefabInstance(nsgo))
                return false;

            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(nsgo.gameObject))
                return false;

            return true;
        }

#endregion

#region CHILD OBJECTS

        void InitialiseChildObjectList()
        {
            m_ChildObjectsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_ChildObjects"),
                true, true, true, true
                );
            m_ChildObjectsList.drawHeaderCallback = DrawChildObjectListHeader;
            m_ChildObjectsList.drawElementCallback = DrawChildObjectListElement;
            m_ChildObjectsList.onAddDropdownCallback = OnChildObjectListAdd;
            m_ChildObjectsList.onRemoveCallback = OnListRemove;
        }

        void OnInspectChildObjects(INeoSerializedGameObjectLimiter[] limiters)
        {
            var foldoutProp = serializedObject.FindProperty("expandChildObjects");
            var filterProp = serializedObject.FindProperty("m_FilterChildObjects");
            var exceptionsProp = serializedObject.FindProperty("m_ChildObjects");
            
            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, GetFilteredFoldoutLabel(filterProp, exceptionsProp, "Child Objects"), true);
            if (foldoutProp.boolValue)
            {
                // CheckAdd if child objects are limited
                bool restricted = false;
                for (int i = 0; i < limiters.Length; ++i)
                {
                    if (limiters[i].restrictChildObjects)
                    {
                        restricted = true;
                        break;
                    }
                }

                if (restricted)
                {
                    // Show restricted message
                    EditorGUILayout.HelpBox("Child object settings restricted by an attached behaviour that implements INeoSerializedGameObjectLimiter", MessageType.Info);
                }
                else
                {
                    // Show properties
                    EditorGUILayout.PropertyField(filterProp);
                    m_ChildObjectsList.DoLayoutList();
                }
                
                EditorGUILayout.Space();
            }
        }

        void DrawChildObjectListHeader(Rect rect)
        {
            var filterProp = serializedObject.FindProperty("m_FilterChildObjects");
            if (filterProp.enumValueIndex == 0)
                EditorGUI.LabelField(rect, "Exclude Child Objects");
            else
                EditorGUI.LabelField(rect, "Include Child Objects");
        }

        void DrawChildObjectListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_ChildObjectsList.serializedProperty.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue == null)
                EditorGUI.LabelField(rect, "Invalid child object");
            else
            {
                // Draw object name
                rect.width -= 102;
                rect.height -= 2;
                rect.y += 2;
                EditorGUI.LabelField(rect, element.objectReferenceValue.name);
                
                // Draw ping button
                rect.x += rect.width;
                rect.width = 50;
                rect.y -= 2;
                rect.height -= 2;
                if (GUI.Button(rect, "show"))
                    EditorGUIUtility.PingObject(element.objectReferenceValue);

                // Draw select button
                rect.x += 52;
                if (GUI.Button(rect, "select"))
                    Selection.activeObject = element.objectReferenceValue;
            }
        }

        void OnChildObjectListAdd(Rect buttonRect, ReorderableList list)
        {
            // Create menu
            list.serializedProperty.serializedObject.Update();
            var menu = new GenericMenu();

            // Add all option
            menu.AddItem(new GUIContent("All"), false, OnChildObjectListAddAll);

            // Add individual child objects
            var targetObject = target as NeoSerializedGameObject;
            List<NeoSerializedGameObject> children = new List<NeoSerializedGameObject>(
                targetObject.GetComponentsInChildren<NeoSerializedGameObject>(true)
                );

            // Filter out invalid objects
            for (int i = children.Count - 1; i >= 0; --i)
            {
                if (children[i] == targetObject)
                {
                    children.RemoveAt(i);
                    continue;
                }

                if (!CompareNearestParent(children[i], targetObject))
                {
                    children.RemoveAt(i);
                    continue;
                }

                if (ListContains(list.serializedProperty, children[i]))
                    children.RemoveAt(i);
            }

            // Add list entries
            if (children.Count > 0)
            {
                menu.AddSeparator(string.Empty);
                for (int i = 0; i < children.Count; ++i)
                    menu.AddItem(new GUIContent(children[i].name), false, OnChildObjectListAddObject, children[i]);
            }

            // Show menu
            menu.ShowAsContext();
        }

        void OnChildObjectListAddAll()
        {
            // Get all child objects
            var targetObject = target as NeoSerializedGameObject;
            List<NeoSerializedGameObject> children = new List<NeoSerializedGameObject>(
                targetObject.GetComponentsInChildren<NeoSerializedGameObject>(true)
                );

            // Filter out all objects more than 1 NeoSerializedGameObject away in the hierarchy
            for (int i = children.Count - 1; i >= 0; --i)
            {
                if (children[i] == targetObject)
                {
                    children.RemoveAt(i);
                    continue;
                }

                if (!CompareNearestParent(children[i], targetObject))
                    children.RemoveAt(i);
            }

            // Apply filtered list to array
            var prop = serializedObject.FindProperty("m_ChildObjects");
            prop.arraySize = children.Count;
            for (int i = 0; i < children.Count; ++i)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = children[i];
            prop.serializedObject.ApplyModifiedProperties();
        }

        void OnChildObjectListAddObject(object o)
        {
            var prop = serializedObject.FindProperty("m_ChildObjects");
            int index = prop.arraySize++;
                prop.GetArrayElementAtIndex(index).objectReferenceValue = (NeoSerializedGameObject)o;
            prop.serializedObject.ApplyModifiedProperties();
        }

        bool CompareNearestParent (NeoSerializedGameObject current, NeoSerializedGameObject nearest)
        {
            var itr = current.transform.parent;
            while (itr != null)
            {
                var check = itr.GetComponent<NeoSerializedGameObject>();
                if (check == null)
                    itr = itr.parent;
                else
                    return check == nearest;
            }
            return false;
        }

#endregion

#region NEO COMPONENTS

        void InitialiseNeoComponentList()
        {
            m_NeoComponentsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_NeoComponents"),
                true, true, true, true
                );
            m_NeoComponentsList.drawHeaderCallback = DrawNeoComponentListHeader;
            m_NeoComponentsList.drawElementCallback = DrawNeoComponentListElement;
            m_NeoComponentsList.onAddDropdownCallback = OnNeoComponentListAdd;
            m_NeoComponentsList.onRemoveCallback = OnListRemove;
        }

        void OnInspectNeoComponents(INeoSerializedGameObjectLimiter[] limiters)
        {
            var foldoutProp = serializedObject.FindProperty("expandNeoComponents");
            var filterProp = serializedObject.FindProperty("m_FilterNeoComponents");
            var exceptionsProp = serializedObject.FindProperty("m_NeoComponents");

            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, GetFilteredFoldoutLabel(filterProp, exceptionsProp, "Neo-Serialized Components"), true);
            if (foldoutProp.boolValue)
            {
                // CheckAdd if child objects are limited
                bool restricted = false;
                for (int i = 0; i < limiters.Length; ++i)
                {
                    if (limiters[i].restrictNeoComponents)
                    {
                        restricted = true;
                        break;
                    }
                }

                if (restricted)
                {
                    // Show restricted message
                    EditorGUILayout.HelpBox("Neo-Serialized Component settings restricted by an attached behaviour that implements INeoSerializedGameObjectLimiter", MessageType.Info);
                }
                else
                {
                    // Show properties
                    EditorGUILayout.PropertyField(filterProp);
                    m_NeoComponentsList.DoLayoutList();
                }

                EditorGUILayout.Space();
            }
        }

        void DrawNeoComponentListHeader(Rect rect)
        {
            var filterProp = serializedObject.FindProperty("m_FilterNeoComponents");
            if (filterProp.enumValueIndex == 0)
                EditorGUI.LabelField(rect, "Exclude Neo Components");
            else
                EditorGUI.LabelField(rect, "Include Neo Components");
        }

        void DrawNeoComponentListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_NeoComponentsList.serializedProperty.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue as INeoSerializableComponent == null)
                EditorGUI.LabelField(rect, "Invalid Neo Component");
            else
            {
                // Draw object name
                rect.height -= 2;
                rect.y += 2;
                EditorGUI.LabelField(rect, GetComponentName(element.objectReferenceValue));
            }
        }

        void OnNeoComponentListAdd(Rect buttonRect, ReorderableList list)
        {
            // Create menu
            list.serializedProperty.serializedObject.Update();
            var menu = new GenericMenu();

            // Add all option
            menu.AddItem(new GUIContent("All"), false, OnNeoComponentListAddAll);

            // Add individual components
            var targetObject = target as NeoSerializedGameObject;
            var components = new List<INeoSerializableComponent>(targetObject.GetComponents<INeoSerializableComponent>());

            // Filter out invalid objects
            for (int i = components.Count - 1; i >= 0; --i)
            {
                if (ListContains(list.serializedProperty, components[i] as Component))
                    components.RemoveAt(i);
            }

            // Add list entries
            if (components.Count > 0)
            {
                menu.AddSeparator(string.Empty);
                for (int i = 0; i < components.Count; ++i)
                    menu.AddItem(new GUIContent(GetComponentName(components[i])), false, OnNeoComponentListAddComponent, components[i]);
            }

            // Show menu
            menu.ShowAsContext();
        }

        void OnNeoComponentListAddAll()
        {
            var targetObject = target as NeoSerializedGameObject;
            var components = targetObject.GetComponents<INeoSerializableComponent>();

            var prop = serializedObject.FindProperty("m_NeoComponents");
            prop.arraySize = components.Length;
            for (int i = 0; i < components.Length; ++i)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = components[i] as Component;
            prop.serializedObject.ApplyModifiedProperties();
        }

        void OnNeoComponentListAddComponent(object o)
        {
            if (o is INeoSerializableComponent)
            {
                var prop = serializedObject.FindProperty("m_NeoComponents");
                int index = prop.arraySize++;
                prop.GetArrayElementAtIndex(index).objectReferenceValue = (Component)o;
                prop.serializedObject.ApplyModifiedProperties();
            }
        }

#endregion

#region OTHER COMPONENTS

        void InitialiseOtherComponentList()
        {
            m_OtherComponentsList = new ReorderableList(
                serializedObject,
                serializedObject.FindProperty("m_OtherComponents"),
                true, true, true, true
                );
            m_OtherComponentsList.drawHeaderCallback = DrawOtherComponentListHeader;
            m_OtherComponentsList.drawElementCallback = DrawOtherComponentListElement;
            m_OtherComponentsList.onAddDropdownCallback = OnOtherComponentListAdd;
            m_OtherComponentsList.onRemoveCallback = OnListRemove;
        }

        string GetOtherComponentsString(SerializedProperty filterProp)
        {
            int count = filterProp.arraySize;
            if (count > 0)
                return string.Format("Other Components (Include: {0})", filterProp.arraySize);
            else
                return "Other Components (None)";
        }

        void OnInspectOtherComponents(INeoSerializedGameObjectLimiter[] limiters)
        {
            var foldoutProp = serializedObject.FindProperty("expandOtherComponents");
            var arrayProp = serializedObject.FindProperty("m_OtherComponents");

            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, GetOtherComponentsString(arrayProp), true);
            if (foldoutProp.boolValue)
            {
                // CheckAdd if child objects are limited
                bool restricted = false;
                for (int i = 0; i < limiters.Length; ++i)
                {
                    if (limiters[i].restrictOtherComponents)
                    {
                        restricted = true;
                        break;
                    }
                }

                if (restricted)
                {
                    // Show restricted message
                    EditorGUILayout.HelpBox("Formatted Component settings restricted by an attached behaviour that implements INeoSerializedGameObjectLimiter", MessageType.Info);
                }
                else
                {
                    // Show properties
                    m_OtherComponentsList.DoLayoutList();
                }

                EditorGUILayout.Space();
            }
        }

        void DrawOtherComponentListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Include Other Components");
        }

        void DrawOtherComponentListElement(Rect rect, int index, bool isActive, bool isFocused)
        {
            var element = m_OtherComponentsList.serializedProperty.GetArrayElementAtIndex(index);
            if (element.objectReferenceValue == null)
                EditorGUI.LabelField(rect, "Invalid component");
            else
            {
                // Draw object name
                rect.height -= 2;
                rect.y += 2;
                EditorGUI.LabelField(rect, GetComponentName(element.objectReferenceValue));
            }
        }

        void OnOtherComponentListAdd(Rect buttonRect, ReorderableList list)
        {
            // Create menu
            list.serializedProperty.serializedObject.Update();
            var menu = new GenericMenu();

            // Add all option
            menu.AddItem(new GUIContent("All"), false, OnOtherComponnentListAddAll);

            // Add individual components
            var targetObject = target as NeoSerializedGameObject;
            var components = new List<Component>(targetObject.GetComponents<Component>());

            // Filter out invalid objects
            for (int i = components.Count - 1; i >= 0; --i)
            {
                // CheckAdd for invalid BindingTypes
                if (components[i] is INeoSerializableComponent ||
                    components[i] is NeoSerializedGameObject ||
                    components[i] is Transform)
                {
                    components.RemoveAt(i);
                    continue;
                }

                // CheckAdd if formatter exists
                if (!NeoSerializationFormatters.ContainsFormatter(components[i]))
                {
                    components.RemoveAt(i);
                    continue;
                }

                if (ListContains(list.serializedProperty, components[i] as Component))
                    components.RemoveAt(i);
            }

            // Add list entries
            if (components.Count > 0)
            {
                menu.AddSeparator(string.Empty);
                for (int i = 0; i < components.Count; ++i)
                    menu.AddItem(new GUIContent(GetComponentName(components[i])), false, OnOtherComponnentListAddComponent, components[i]);
            }

            // Show menu
            menu.ShowAsContext();
        }

        void OnOtherComponnentListAddAll()
        {
            var targetObject = target as NeoSerializedGameObject;
            var components = new List<Component>(targetObject.GetComponents<Component>());

            // Filter out neo components (and this)
            for (int i = components.Count - 1; i >=0; --i)
            {
                // CheckAdd for invalid BindingTypes
                if (components[i] is INeoSerializableComponent ||
                    components[i] is NeoSerializedGameObject ||
                    components[i] is Transform)
                {
                    components.RemoveAt(i);
                    continue;
                }

                // CheckAdd if formatter exists
                if (!NeoSerializationFormatters.ContainsFormatter(components[i]))
                    components.RemoveAt(i);
            }

            var prop = serializedObject.FindProperty("m_OtherComponents");
            prop.arraySize = components.Count;
            for (int i = 0; i < components.Count; ++i)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = components[i] as Component;
            prop.serializedObject.ApplyModifiedProperties();
        }

        void OnOtherComponnentListAddComponent(object o)
        {
            var prop = serializedObject.FindProperty("m_OtherComponents");
            int index = prop.arraySize++;
            prop.GetArrayElementAtIndex(index).objectReferenceValue = (Component)o;
            prop.serializedObject.ApplyModifiedProperties();
        }

#endregion

#region OVERRIDES

        void ResetOverrides()
        {
            m_Overrides.Clear();
            for (int i = 0; i < SaveMode.count; ++i)
            {
                m_Overrides.Add(new NeoSerializedGameObjectOverrideEditor(this));
            }
        }

        void OnInspectOverrides(INeoSerializedGameObjectLimiter[] limiters)
        {
#pragma warning disable CS0162 // Unreachable code detected

            // CheckAdd the number of save BindingTypes
            if (SaveMode.count <= 1)
                return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Save Type Overrides", EditorStyles.boldLabel);

            // CheckAdd if can add new overrides
            var overridesProp = serializedObject.FindProperty("m_Overrides");
            if (overridesProp.arraySize < SaveMode.count - 1)
            {
                // Get list of options to check against
                List<SaveMode> options = new List<SaveMode>(SaveMode.count - 1);
                for (int i = 1; i < SaveMode.count; ++i)
                    options.Add(i);

                // Remove each override that's already set
                for (int i = 0; i < overridesProp.arraySize; ++i)
                    options.Remove(overridesProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_SaveMode").intValue);

                // Show add override dropdown
                if (EditorGUILayout.DropdownButton(new GUIContent("Add Override"), FocusType.Passive))
                {
                    var menu = new GenericMenu();
                    foreach (SaveMode st in options)
                        menu.AddItem(new GUIContent(st.ToString()), false, AddOverrride, st);
                    menu.ShowAsContext();
                }
            }
            else
            {
                GUI.enabled = false;
                EditorGUILayout.DropdownButton(new GUIContent("Add Override"), FocusType.Passive);
                GUI.enabled = true;
            }

            // Show overrides
            for (int i = 0; i < overridesProp.arraySize; ++i)
            {
                if (!m_Overrides[i].OnInspect(overridesProp.GetArrayElementAtIndex(i), limiters))
                    break;
            }

#pragma warning restore CS0162 // Unreachable code detected
        }

        void AddOverrride(object saveMode)
        {
            SaveMode s = (SaveMode)saveMode;
            
            // Get array
            var overridesProp = serializedObject.FindProperty("m_Overrides");

            // Resize and shift any overrides > saveType to the right
            ++overridesProp.arraySize;
            switch (overridesProp.arraySize)
            {
                case 1:
                    {
                        var p = overridesProp.GetArrayElementAtIndex(0);
                        CreateOverride(p, s);
                    }
                    break;
                case 2:
                    {
                        var p = overridesProp.GetArrayElementAtIndex(0);
                        if (p.FindPropertyRelative("m_SaveMode").intValue > s)
                            CreateOverride(p, s);
                        else
                            CreateOverride(overridesProp.GetArrayElementAtIndex(1), s);
                    }
                    break;
                default:
                    {
                        for (int i = overridesProp.arraySize - 2; i >= 0; --i)
                        {
                            var st = overridesProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_SaveMode").intValue;
                            if (st > s)
                                overridesProp.MoveArrayElement(i, i + 1);
                            else
                            {
                                CreateOverride(overridesProp.GetArrayElementAtIndex(i), s);
                                break;
                            }
                        }
                    }
                    break;
            }

            serializedObject.ApplyModifiedProperties();
            ResetOverrides();
        }

        void CreateOverride(SerializedProperty p, SaveMode mode)
        {
            p.FindPropertyRelative("m_SaveMode").intValue = mode;
            p.FindPropertyRelative("m_Position").enumValueIndex = 0;
            p.FindPropertyRelative("m_Rotation").enumValueIndex = 0;
            p.FindPropertyRelative("m_LocalScale").boolValue = false;
            p.FindPropertyRelative("m_FilterChildObjects").enumValueIndex = 0;
            p.FindPropertyRelative("m_ChildObjects").arraySize = 0;
            p.FindPropertyRelative("m_FilterNeoComponents").enumValueIndex = 0;
            p.FindPropertyRelative("m_NeoComponents").arraySize = 0;
            //p.FindPropertyRelative("m_OtherComponents").arraySize = 0;
            p.FindPropertyRelative("expandOverride").boolValue = true;
            p.FindPropertyRelative("expandChildObjects").boolValue = false;
            p.FindPropertyRelative("expandNeoComponents").boolValue = false;
        }

        public void RemoveOverride(SaveMode saveMode)
        {
            // Get array
            var overridesProp = serializedObject.FindProperty("m_Overrides");

            // Shift any overrides > saveType to the left
            if (overridesProp.arraySize > 1)
            {
                for (int i = 1; i < overridesProp.arraySize; ++i)
                {
                    var st = overridesProp.GetArrayElementAtIndex(i).FindPropertyRelative("m_SaveMode").intValue;
                    if (st > saveMode)
                        overridesProp.MoveArrayElement(i, i - 1);
                }
            }

            // Resize
            --overridesProp.arraySize;

            ResetOverrides();
        }

#endregion

#region HELPERS

        string GetComponentName<T>(T component)
        {
            var targetObject = target as NeoSerializedGameObject;
            var type = component.GetType();
            var components = targetObject.GetComponents(type);
            if (components.Length == 1)
                return type.Name;
            else
                return string.Format("{0} ({1})", type.Name, Array.IndexOf(components, component) + 1);
        }

        static string GetFilteredFoldoutLabel(SerializedProperty filterProp, SerializedProperty exceptionsProp, string what)
        {
            int count = exceptionsProp.arraySize;
            if (count > 0)
            {
                if (filterProp.enumValueIndex == 0)
                    return string.Format("{0} (Include: *All, {1} Exceptions)", what, count);
                else
                    return string.Format("{0} (Exclude: *All, {1} Exceptions)", what, count);
            }
            else
            {
                if (filterProp.enumValueIndex == 0)
                    return string.Format("{0} (Include: *All)", what);
                else
                    return string.Format("{0} (None)", what);
            }
        }

        static void OnListRemove(ReorderableList list)
        {
            list.serializedProperty.serializedObject.Update();
            for (int i = list.index + 1; i < list.serializedProperty.arraySize; ++i)
                list.serializedProperty.MoveArrayElement(i, i - 1);
            --list.serializedProperty.arraySize;
            list.index = -1;
        }

        static bool ListContains(SerializedProperty arrayProp, UnityEngine.Object o)
        {
            int count = arrayProp.arraySize;
            for (int i = 0; i < count; ++i)
            {
                var entry = arrayProp.GetArrayElementAtIndex(i);
                if (entry.objectReferenceValue == o)
                    return true;
            }
            return false;
        }

#endregion
    }
}

#endif