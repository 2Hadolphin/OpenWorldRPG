using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;

//[CreateAssetMenu(fileName = "AnimationLibrary", menuName = "MyData/Animation/AnimationLibrary")]

public class Editor_CreateAnimationLibrary : EditorWindow
{

    Editor_CreateAnimationLibrary()
    {
        this.titleContent = new GUIContent("CreateAnimationLibrary");
    }

    [MenuItem("Window/Animation/CreateAnimationLibrary")]
    static void Init()
    {
        var window = (Editor_CreateAnimationLibrary)EditorWindow.GetWindow(typeof(Editor_CreateAnimationLibrary));
        window.Show();
    }
 
    private void OnGUI()
    {
        Enum.TryParse(EditorGUILayout.EnumPopup("ChoseType", m_Type).ToString(),out m_Type);
        if(GUILayout.Button("Create Library"))
        {
            ScriptableObject so;
            switch (m_Type)
            {
                case ParameterType.Float:
                    so= CreateInstance<AniamtionLibrary_Float>();
                    break;
                case ParameterType.Int:
                    so = CreateInstance<AniamtionLibrary_Float>();
                    break;
                case ParameterType.Vector2:
                    so = CreateInstance<AniamtionLibrary_Vector2>();
                    break;
                case ParameterType.Vector3:
                    so = CreateInstance<AnimationLibrary_Vector3>();
                    break;
                case ParameterType.Vector4:
                    so = CreateInstance<AniamtionLibrary_Vector4>();
                    break;
                default:
                    so = CreateInstance<AniamtionLibrary_Float>();
                    break;
            }

            EditorHelper.WriteFile(so, so.GetType());
        }

    }

    public enum ParameterType { Float,Int,Vector2,Vector3,Vector4}
    public static ParameterType m_Type;
}
