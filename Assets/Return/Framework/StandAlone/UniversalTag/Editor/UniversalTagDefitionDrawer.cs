using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using System;

namespace Return.Editors
{
    [CustomEditor(typeof(UniversalTagDefinition))]
    public class UniversalTagDefitionDrawer : OdinEditor
    {
        //SerializedObject serializedObject;
        private SerializedProperty m_spTags;
        private SerializedProperty m_IsFlags;
        private bool m_requireTagsFoldout = true;


        protected override void OnEnable()
        {
            if (serializedObject.targetObject is UniversalTagDefinition def)
            {
                var names=def.TagNames;
            }

            m_IsFlags = serializedObject.FindProperty("IsFlag");
            m_spTags = serializedObject.FindProperty("m_tagNames");

        }

        

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            m_IsFlags.boolValue = EditorGUILayout.ToggleLeft(m_IsFlags.name, m_IsFlags.boolValue);

            if(m_spTags.arraySize==0)
                Debug.Log(m_spTags.arraySize);

            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            //m_requireTagsFoldout = m_EditorGUIUtility.DrawFoldout(
            //    "Tags", curHeight, EditorGUIUtility.currentViewWidth, m_requireTagsFoldout);

            m_requireTagsFoldout = EditorGUILayout.ToggleLeft("Tags", m_requireTagsFoldout);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_requireTagsFoldout)
            {
                SerializedProperty spTag;

                for (int i = 0; i < m_spTags.arraySize; ++i)
                {
                    spTag = m_spTags.GetArrayElementAtIndex(i);

                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Space(23f);

                    lastRect = GUILayoutUtility.GetLastRect();
                    lastRect.height = 17f;
                    lastRect.width = 17f;

                    if (i < 30)
                    {
                        spTag.stringValue = EditorGUILayout.TextField(new GUIContent("Tags " + (i + 1).ToString() + ": "), spTag.stringValue);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent("Tags " + (i + 1).ToString() + ": " + spTag.stringValue));
                    }
                    EditorGUILayout.EndHorizontal();

                    curHeight += 18f;
                }


                curHeight += 5f;
                GUILayout.Space(4f);
            }

            curHeight += 23f;

            //m_IsFlags.serializedObject.ApplyModifiedProperties();
            //ValueEntry.ApplyChanges();
            serializedObject.ApplyModifiedProperties();
        }

    }

}

