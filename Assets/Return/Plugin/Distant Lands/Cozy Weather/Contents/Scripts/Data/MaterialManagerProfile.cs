// Distant Lands 2021.



using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Material Manager Profile", order = 361)]
    public class MaterialManagerProfile : ScriptableObject
    {


        [Tooltip("Changes the terrain tint color for these layers")]
        public TerrainLayerProfile[] terrainLayers;
        [Tooltip("Changes a certain color value for a material based on the current time of year")]
        public SeasonalColorMaterialProfile[] seasonalMaterials;
        [Tooltip("Changes a certain float value for a material based on the current time of year")]
        public SeasonalValueMaterialProfile[] seasonalValueMaterials;

        public Texture snowTexture;
        public float snowNoiseSize = 10;
        public Color snowColor = Color.white;
        public float puddleScale = 2;



        [System.Serializable]
        public class TerrainLayerProfile
        {
            public TerrainLayer layer;
            public Gradient color;


        }


        [System.Serializable]
        public class SeasonalColorMaterialProfile
        {
            public Material material;
            public string propertyToChange;
            public Gradient color;

        }

        [System.Serializable]
        public class SeasonalValueMaterialProfile
        {
            public Material material;
            public string propertyToChange;
            public AnimationCurve value;

        }

    }


#if UNITY_EDITOR
    [CustomEditor(typeof(MaterialManagerProfile))]
    [CanEditMultipleObjects]
    public class E_MaterialProfile : Editor
    {

        SerializedProperty terrainLayers;
        SerializedProperty seasonalMaterials;
        SerializedProperty seasonalValueMaterials;
        SerializedProperty snowTexture;
        SerializedProperty snowNoiseSize;
        SerializedProperty snowColor;
        SerializedProperty puddleScale;
        MaterialManagerProfile prof;

        void OnEnable()
        {

            terrainLayers = serializedObject.FindProperty("terrainLayers");
            seasonalMaterials = serializedObject.FindProperty("seasonalMaterials");
            seasonalValueMaterials = serializedObject.FindProperty("seasonalValueMaterials");
            snowTexture = serializedObject.FindProperty("snowTexture");
            snowNoiseSize = serializedObject.FindProperty("snowNoiseSize");
            snowColor = serializedObject.FindProperty("snowColor");
            puddleScale = serializedObject.FindProperty("puddleScale");
            prof = (MaterialManagerProfile)target;

        }


        public override void OnInspectorGUI()
        {

            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");

            EditorGUILayout.LabelField("Local Material Properties", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(terrainLayers);
            EditorGUILayout.PropertyField(seasonalMaterials);
            EditorGUILayout.PropertyField(seasonalValueMaterials);
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Global Snow Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(snowTexture);
            EditorGUILayout.PropertyField(snowNoiseSize);
            EditorGUILayout.PropertyField(snowColor);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Global Rain Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(puddleScale);

            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(prof);

        }
    }
#endif
}