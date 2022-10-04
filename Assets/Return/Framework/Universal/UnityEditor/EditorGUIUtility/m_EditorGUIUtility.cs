using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Return.Editors
{
    public static class m_EditorGUIUtility
    {
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
                //myBoxStyle.normal.background = m_foldoutTexOpen;

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

    }

}

