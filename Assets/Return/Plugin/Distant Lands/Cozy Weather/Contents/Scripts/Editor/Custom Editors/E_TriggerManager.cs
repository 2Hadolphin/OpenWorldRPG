using UnityEditor;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyTriggerManager))]
    [CanEditMultipleObjects]
    public class E_TriggerManager : Editor
    {
        SerializedProperty cozyTriggerTag;


        void OnEnable()
        {
            cozyTriggerTag = serializedObject.FindProperty("cozyTriggerTag");



        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(cozyTriggerTag);
            serializedObject.ApplyModifiedProperties();


        }
    }
}