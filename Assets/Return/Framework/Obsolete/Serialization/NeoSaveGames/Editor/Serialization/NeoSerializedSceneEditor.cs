#if UNITY_EDITOR

using UnityEditor;
using NeoSaveGames.Serialization;

namespace NeoFPSEditor.SaveGames
{
    [CustomEditor(typeof(NeoSerializedScene), true)]
    public class NeoSerializedSceneEditor : SaveInfoBaseEditor
    {
        public sealed override void OnInspectorGUI()
        {
            OnInspectorGUITop();
            InspectRecreatableItems();
            OnInspectorGUIBottom();
            serializedObject.ApplyModifiedProperties();
        }

        public virtual void OnInspectorGUITop()
        { }

        public virtual void OnInspectorGUIBottom()
        {
            var prop = serializedObject.FindProperty("m_Assets");
            while (prop.NextVisible(false))
                EditorGUILayout.PropertyField(prop, true);
        }
    }
}

#endif