#if UNITY_EDITOR

using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System;

namespace NeoSaveGames.Serialization
{
    public class NeoSerializedGameObjectOverrideEditor
    {
        private NeoSerializedGameObjectEditor m_Editor = null;
        private SerializedProperty m_SerializedProperty = null;
        private ReorderableList m_ChildObjectsList = null;
        private ReorderableList m_NeoComponentsList = null;
        private ReorderableList m_OtherComponentsList = null;

        //public SaveMode saveMode
        //{
        //    get { return m_SerializedProperty.FindPropertyRelative("m_SaveMode").intValue; }
        //}

        public NeoSerializedGameObjectOverrideEditor(NeoSerializedGameObjectEditor editor)
        {
            m_Editor = editor;
        }

        public bool OnInspect(SerializedProperty prop, INeoSerializedGameObjectLimiter[] limiters)
        {
            m_SerializedProperty = prop;
            InitialiseChildObjectList();
            InitialiseNeoComponentList();
            InitialiseOtherComponentList();

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            ++EditorGUI.indentLevel;
            
            // Draw dropdown for override
            SaveMode st = m_SerializedProperty.FindPropertyRelative("m_SaveMode").intValue;

            var foldout = m_SerializedProperty.FindPropertyRelative("expandOverride");
            foldout.boolValue = EditorGUILayout.Foldout(foldout.boolValue, st.ToString(), true);

            if (foldout.boolValue)
            {
                EditorGUILayout.BeginHorizontal();
                GUILayout.Space(16);
                bool remove = GUILayout.Button("Remove Override");
                EditorGUILayout.EndHorizontal();
                if (remove)
                {
                    m_Editor.RemoveOverride(st);

                    --EditorGUI.indentLevel;
                    EditorGUILayout.EndVertical();

                    return false;
                }
                else
                {
                    EditorGUILayout.PropertyField(m_SerializedProperty.FindPropertyRelative("m_Position"));
                    EditorGUILayout.PropertyField(m_SerializedProperty.FindPropertyRelative("m_Rotation"));
                    EditorGUILayout.PropertyField(m_SerializedProperty.FindPropertyRelative("m_LocalScale"));

                    // Serialized contents
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Serialized Contents", EditorStyles.boldLabel);
                    OnInspectChildObjects(limiters);
                    OnInspectNeoComponents(limiters);
                    OnInspectOtherComponents(limiters);
                    GUILayout.Space(2);
                }
            }

            --EditorGUI.indentLevel;
            EditorGUILayout.EndVertical();

            return true;
        }

        #region CHILD OBJECTS

        void InitialiseChildObjectList()
        {
            if (m_ChildObjectsList == null)
            {
                m_ChildObjectsList = new ReorderableList(
                    m_SerializedProperty.serializedObject,
                    m_SerializedProperty.FindPropertyRelative("m_ChildObjects"),
                    true, true, true, true
                    );
                m_ChildObjectsList.drawHeaderCallback = DrawChildObjectListHeader;
                m_ChildObjectsList.drawElementCallback = DrawChildObjectListElement;
                m_ChildObjectsList.onAddDropdownCallback = OnChildObjectListAdd;
                m_ChildObjectsList.onRemoveCallback = OnListRemove;
            }
            else
            {
                m_ChildObjectsList.serializedProperty = m_SerializedProperty.FindPropertyRelative("m_ChildObjects");
            }
        }

        void OnInspectChildObjects(INeoSerializedGameObjectLimiter[] limiters)
        {
            var foldoutProp = m_SerializedProperty.FindPropertyRelative("expandChildObjects");
            var filterProp = m_SerializedProperty.FindPropertyRelative("m_FilterChildObjects");
            var exceptionsProp = m_SerializedProperty.FindPropertyRelative("m_ChildObjects");

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
                    if (filterProp.enumValueIndex != 0)
                        m_ChildObjectsList.DoLayoutList();
                }

                EditorGUILayout.Space();
            }
        }

        void DrawChildObjectListHeader(Rect rect)
        {
            var filterProp = m_SerializedProperty.FindPropertyRelative("m_FilterChildObjects");
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
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
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
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
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
            var prop = m_SerializedProperty.FindPropertyRelative("m_ChildObjects");
            prop.arraySize = children.Count;
            for (int i = 0; i < children.Count; ++i)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = children[i];
            prop.serializedObject.ApplyModifiedProperties();
        }

        void OnChildObjectListAddObject(object o)
        {
            var prop = m_SerializedProperty.FindPropertyRelative("m_ChildObjects");
            int index = prop.arraySize++;
            prop.GetArrayElementAtIndex(index).objectReferenceValue = (NeoSerializedGameObject)o;
            prop.serializedObject.ApplyModifiedProperties();
        }

        bool CompareNearestParent(NeoSerializedGameObject current, NeoSerializedGameObject nearest)
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
            if (m_NeoComponentsList == null)
            {
                m_NeoComponentsList = new ReorderableList(
                    m_SerializedProperty.serializedObject,
                    m_SerializedProperty.FindPropertyRelative("m_NeoComponents"),
                    true, true, true, true
                    );
                m_NeoComponentsList.drawHeaderCallback = DrawNeoComponentListHeader;
                m_NeoComponentsList.drawElementCallback = DrawNeoComponentListElement;
                m_NeoComponentsList.onAddDropdownCallback = OnNeoComponentListAdd;
                m_NeoComponentsList.onRemoveCallback = OnListRemove;
            }
            else
            {
                m_NeoComponentsList.serializedProperty = m_SerializedProperty.FindPropertyRelative("m_NeoComponents");
            }
        }
        
        void OnInspectNeoComponents(INeoSerializedGameObjectLimiter[] limiters)
        {
            var foldoutProp = m_SerializedProperty.FindPropertyRelative("expandNeoComponents");
            var filterProp = m_SerializedProperty.FindPropertyRelative("m_FilterNeoComponents");
            var exceptionsProp = m_SerializedProperty.FindPropertyRelative("m_NeoComponents");

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
                    EditorGUILayout.HelpBox("Child object settings restricted by an attached behaviour that implements INeoSerializedGameObjectLimiter", MessageType.Info);
                }
                else
                {
                    // Show properties
                    EditorGUILayout.PropertyField(filterProp);
                    if (filterProp.enumValueIndex != 0)
                        m_NeoComponentsList.DoLayoutList();
                }

                EditorGUILayout.Space();
            }
        }

        void DrawNeoComponentListHeader(Rect rect)
        {
            var filterProp = m_SerializedProperty.FindPropertyRelative("m_FilterNeoComponents");
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
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
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
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
            var components = targetObject.GetComponents<INeoSerializableComponent>();

            var prop = m_SerializedProperty.FindPropertyRelative("m_NeoComponents");
            prop.arraySize = components.Length;
            for (int i = 0; i < components.Length; ++i)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = components[i] as Component;
            prop.serializedObject.ApplyModifiedProperties();
        }

        void OnNeoComponentListAddComponent(object o)
        {
            if (o is INeoSerializableComponent)
            {
                var prop = m_SerializedProperty.FindPropertyRelative("m_NeoComponents");
                int index = prop.arraySize++;
                prop.GetArrayElementAtIndex(index).objectReferenceValue = (Component)o;
                prop.serializedObject.ApplyModifiedProperties();
            }
        }

        #endregion

        #region OTHER COMPONENTS

        void InitialiseOtherComponentList()
        {
            if (m_OtherComponentsList == null)
            {
                m_OtherComponentsList = new ReorderableList(
                    m_SerializedProperty.serializedObject,
                    m_SerializedProperty.FindPropertyRelative("m_OtherComponents"),
                    true, true, true, true
                    );
                m_OtherComponentsList.drawHeaderCallback = DrawOtherComponentListHeader;
                m_OtherComponentsList.drawElementCallback = DrawOtherComponentListElement;
                m_OtherComponentsList.onAddDropdownCallback = OnOtherComponentListAdd;
                m_OtherComponentsList.onRemoveCallback = OnListRemove;
            }
            else
            {
                m_OtherComponentsList.serializedProperty = m_SerializedProperty.FindPropertyRelative("m_OtherComponents");
            }
        }

        string GetOtherComponentsString(SerializedProperty overrideProp, SerializedProperty filterProp)
        {
            if (overrideProp.boolValue)
            {
                int count = filterProp.arraySize;
                if (count > 0)
                    return string.Format("Other Components (Include: {0})", filterProp.arraySize);
                else
                    return "Other Components (None)";
            }
            else
                return "Other Components (Default Settings)";
        }

        void OnInspectOtherComponents(INeoSerializedGameObjectLimiter[] limiters)
        {
            var foldoutProp = m_SerializedProperty.FindPropertyRelative("expandOtherComponents");
            var overrideProp = m_SerializedProperty.FindPropertyRelative("m_OverrideOtherComponents");
            var arrayProp = m_SerializedProperty.FindPropertyRelative("m_OtherComponents");

            foldoutProp.boolValue = EditorGUILayout.Foldout(foldoutProp.boolValue, GetOtherComponentsString(overrideProp, arrayProp), true);
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
                    EditorGUILayout.HelpBox("Child object settings restricted by an attached behaviour that implements INeoSerializedGameObjectLimiter", MessageType.Info);
                }
                else
                {
                    // Show properties
                    EditorGUILayout.PropertyField(overrideProp);
                    if (overrideProp.boolValue)
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
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
            var components = new List<Component>(targetObject.GetComponents<Component>());

            // Filter out invalid objects
            for (int i = components.Count - 1; i >= 0; --i)
            {
                // CheckAdd for invalid BindingTypes
                if (components[i] is INeoSerializableComponent ||
                    components[i] is NeoSerializedGameObject ||
                    components[i] is Transform ||
                    components[i] is INeoSerializableComponent)
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
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
            var components = new List<Component>(targetObject.GetComponents<Component>());

            // Filter out neo components (and this)
            for (int i = components.Count - 1; i >= 0; --i)
            {
                // CheckAdd for invalid BindingTypes
                if (components[i] is INeoSerializableComponent ||
                    components[i] is NeoSerializedGameObject ||
                    components[i] is Transform ||
                    components[i] is INeoSerializableComponent)
                {
                    components.RemoveAt(i);
                    continue;
                }

                // CheckAdd if formatter exists
                if (!NeoSerializationFormatters.ContainsFormatter(components[i]))
                    components.RemoveAt(i);
            }

            var prop = m_SerializedProperty.FindPropertyRelative("m_OtherComponents");
            prop.arraySize = components.Count;
            for (int i = 0; i < components.Count; ++i)
                prop.GetArrayElementAtIndex(i).objectReferenceValue = components[i] as Component;
            prop.serializedObject.ApplyModifiedProperties();
        }

        void OnOtherComponnentListAddComponent(object o)
        {
            var prop = m_SerializedProperty.FindPropertyRelative("m_OtherComponents");
            int index = prop.arraySize++;
            prop.GetArrayElementAtIndex(index).objectReferenceValue = (Component)o;
            prop.serializedObject.ApplyModifiedProperties();
        }

        #endregion

        #region HELPERS

        string GetComponentName<T>(T component)
        {
            var targetObject = m_SerializedProperty.serializedObject.targetObject as NeoSerializedGameObject;
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
                switch (filterProp.enumValueIndex)
                {
                    case 0:
                        return what + " (Default Settings)";
                    case 1:
                        return string.Format("{0} (Include: *All, {1} Exceptions)", what, count);
                    case 2:
                        return string.Format("{0} (Exclude: *All, {1} Exceptions)", what, count);
                }
            }
            else
            {
                switch (filterProp.enumValueIndex)
                {
                    case 0:
                        return what + " (Default Settings)";
                    case 1:
                        return string.Format("{0} (Include: *All)", what);
                    case 2:
                        return string.Format("{0} (None)", what);
                }
            }
            return what;
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
