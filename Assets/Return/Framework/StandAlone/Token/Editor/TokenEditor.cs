using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Windows;
using UnityEditor;
using System;
using System.Linq;

namespace Return
{
    public class TokenEditor : OdinEditorWindow
    {

        #region Editor

        [MenuItem("Tools/Return/Modular/TokenEditor")]
        static void CreateWindow()
        {
            Open();
        }

        public static TokenEditor Open()
        {
            var window = GetWindow<TokenEditor>("TokenEditor");
            return window;
        }

        #endregion

        //[HorizontalGroup("Target")]
        [OnValueChanged(nameof(AddToken))]
        [ShowInInspector]
        [DropZone(Text = "Drop Token Here")]
        UnityEngine.Object m_Token;

        //[HorizontalGroup("Target")]
        [Button(ButtonSizes.Gigantic,Style =ButtonStyle.Box)]
        void Clear()
        {
            m_category = null;
            TokenType = null;

            if(Tokens!=null)
                Tokens.Clear();
        }

        [ShowInInspector]
        Category m_category;

        [ShowInInspector]
        Type TokenType;

        [AssetSelector]
        [ShowInInspector]
        HashSet<BaseToken> Tokens;

        void AddToken()
        {
            if (m_Token is not BaseToken toekn)
            {
                m_Token = null;
                return;
            }

            LoadTokens(toekn);

            m_Token = null;
        }

        void LoadTokens(BaseToken basetoken)
        {
            if (TokenType == null)
            {
                TokenType = basetoken.GetType();

                if (basetoken is Token token)
                    m_category = token.Category;
                else
                    m_category = null;



                var tokens =
                    AssetDatabase.FindAssets("t:" + TokenType).
                    Select(x =>
                    AssetDatabase.LoadAssetAtPath(
                        AssetDatabase.GUIDToAssetPath(x), TokenType));

                if (m_category)
                    tokens = tokens.Where(x => x is Token t && t.Category == m_category);

                    //ScriptableObject.FindObjectsOfType(TokenType, true);

                    //Debug.LogFormat("Load {0} tokens.",tokens.Length);

                    Tokens = tokens.
                    Where(x => x != null). // text type
                    Select(x => x as BaseToken).
                    ToHashSet();
            }
            else if (TokenType != m_Token.GetType())
                return;


            Tokens.Add(basetoken);
        }

        bool hasTokens => Tokens != null && Tokens.Count > 0;

        static Vector2 scroll;

        [ShowIf(nameof(hasTokens))]
        [OnInspectorGUI]
        void DrawPriorityRank()
        {
            scroll = EditorGUILayout.BeginScrollView(scroll);

            var tokens = Tokens.OfType<Token>().OrderBy(x=>x.Priority);

            foreach (var baseToken in tokens)
            {
                if(baseToken is Token token)
                {
                    GUILayout.Button(token.name);
                }
            }

            EditorGUILayout.EndScrollView();
        }


        [Button]
        void SetIconTest(UnityEngine.Object obj,Texture2D icon)
        {
            Editors.EditorAssetsUtility.SetCustomIconOnGameObject(obj, icon);
        }
    }
}
