#if UNITY_EDITOR

using UnityEditor;
using NeoSaveGames;

namespace NeoFPSEditor.SaveGames
{
    [CustomEditor(typeof(SceneSaveInfo), true)]
    public class SceneSaveInfoEditor : NeoSerializedSceneEditor
    {
        public override void OnInspectorGUITop()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_DisplayName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ThumbnailTexture"));
        }

        public override void OnInspectorGUIBottom()
        {
            // Clear to prevent drawing top properties at bottom too
        }
    }
}

#endif