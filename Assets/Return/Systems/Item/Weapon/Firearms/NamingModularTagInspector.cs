using System.Collections;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TagNamingModule))]
    public class NamingModularTagInspector : Editor
    {
        private SerializedProperty m_spTags;
        private bool m_tagsFoldout = true;

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public void OnEnable()
        {
            m_spTags = serializedObject.FindProperty("m_tagNames");
        }

        //============================================================================================
        /**
        *  @brief 
        *         
        *********************************************************************************************/
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("");
            Rect lastRect = GUILayoutUtility.GetLastRect();

            float curHeight = lastRect.y + 9f;

            curHeight = DrawTitle("Naming Modular Tag", curHeight);

            m_tagsFoldout = DrawFoldout(
                "Tags", curHeight, EditorGUIUtility.currentViewWidth, m_tagsFoldout);

            lastRect = GUILayoutUtility.GetLastRect();
            curHeight = lastRect.y + lastRect.height;

            if (m_tagsFoldout)
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
                        spTag.stringValue = EditorGUILayout.TextField(new GUIContent("Tag " + (i + 1).ToString() + ": "), spTag.stringValue);
                    }
                    else
                    {
                        EditorGUILayout.LabelField(new GUIContent("Tag " + (i + 1).ToString() + ": " + spTag.stringValue));
                    }
                    EditorGUILayout.EndHorizontal();

                    curHeight += 18f;
                }


                curHeight += 5f;
                GUILayout.Space(4f);
            }
            serializedObject.ApplyModifiedProperties();
        }

    public static bool DrawFoldout(string _title, float _curHeight, float _width, bool _foldoutBool, int _indent = 0, bool _thin = false)
    {
        GUIStyle myBoxStyle = new GUIStyle(GUI.skin.box);
        GUIStyle foldoutStyle = new GUIStyle(GUI.skin.label);

        _width = _width - (_indent * 10f);

        Rect lastRect = GUILayoutUtility.GetLastRect();
        Rect boxRect;



        float labelY = lastRect.y + lastRect.height + 3f;


        if (!_thin)
        {
            boxRect = new Rect(_indent * 10f, labelY, _width, 22f);
            GUI.Box(boxRect, "", myBoxStyle);
            foldoutStyle.fontSize = 14;
            foldoutStyle.fixedHeight = 22f;
        }
        else
        {
            boxRect = new Rect(_indent * 10f, labelY, _width, 18f);
            GUI.Box(boxRect, "", myBoxStyle);
            foldoutStyle.fontSize = 12;
            foldoutStyle.fixedHeight = 20f;
        }

        if (_foldoutBool)
        {
            if (!_thin)
                GUI.Box(new Rect(_indent * 10f, labelY + 1, _width, 20f), "", myBoxStyle);
            else
                GUI.Box(new Rect(_indent * 10f, labelY + 1, _width, 16f), "", myBoxStyle);
        }

        EditorGUILayout.BeginHorizontal();
        GUILayout.Space(_indent * 10f);

        bool ret = _foldoutBool;
        if (GUI.Button(boxRect, "", GUI.skin.label))
        {
            ret = !_foldoutBool;
        }

        EditorGUILayout.LabelField(_title, foldoutStyle);
        foldoutStyle.fixedHeight = 18f;

        EditorGUILayout.EndHorizontal();


        if (!_thin)
            GUILayout.Space(10);
        else
            GUILayout.Space(6);

        return ret;
    }

    public static float DrawTitle(string _title, float _height)
    {
        GUIStyle titleTxtStyle = new GUIStyle(GUI.skin.label);
        titleTxtStyle.fontSize = 22;
        titleTxtStyle.fontStyle = FontStyle.Bold;

        float txtWidth = titleTxtStyle.CalcSize(new GUIContent(_title)).x;

        EditorGUI.LabelField(new Rect((EditorGUIUtility.currentViewWidth - txtWidth) / 2f, _height, txtWidth, 32f),
                             new GUIContent(_title), titleTxtStyle);

        GUILayout.Space(37f);
        _height += 46f;

        return _height;
    }
}

