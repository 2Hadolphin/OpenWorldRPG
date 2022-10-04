using UnityEditor;

namespace DistantLands.Cozy.EditorScripts
{

    [CustomEditor(typeof(CozyLightningManager))]
    [CanEditMultipleObjects]
    public class E_LightningManager : Editor
    {
        SerializedProperty lightningPrefab;
        CozyLightningManager lightningManager;


        void OnEnable()
        {
            lightningPrefab = serializedObject.FindProperty("lightningPrefab");
            lightningManager = (CozyLightningManager)target;

        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.PropertyField(lightningPrefab);
            Undo.RecordObject(lightningManager.lightningPrefab.GetComponent<CozyLightning>(), "Lightning Changes");

            CreateEditor(lightningManager.lightningPrefab.GetComponent<CozyLightning>()).OnInspectorGUI();

            serializedObject.ApplyModifiedProperties();


        }
    }
}