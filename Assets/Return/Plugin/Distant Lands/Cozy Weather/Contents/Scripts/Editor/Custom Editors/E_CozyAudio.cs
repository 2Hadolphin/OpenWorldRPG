using UnityEditor;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyAudio))]
    [CanEditMultipleObjects]
    public class E_CozyAudio : Editor
    {
        SerializedProperty transitionTime;


        void OnEnable()
        {
            transitionTime = serializedObject.FindProperty("transitionTime");



        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(transitionTime);
            serializedObject.ApplyModifiedProperties();

        }
    }
}