// Distant Lands 2021.



using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif



namespace DistantLands.Cozy.Data
{

    [System.Serializable]
    [CreateAssetMenu(menuName = "Distant Lands/Cozy/Atmosphere Profile", order = 361)]
    public class AtmosphereProfile : ScriptableObject
    {


        [Tooltip("Sets the color of the zenith (or top) of the skybox at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient skyZenithColor;
        [Tooltip("Sets the color of the horizon (or middle) of the skybox at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient skyHorizonColor;

        [Tooltip("Sets the main color of the clouds at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient cloudColor;
        [Tooltip("Sets the highlight color of the clouds at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient cloudHighlightColor;
        [Tooltip("Sets the color of the high altitude clouds at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient highAltitudeCloudColor;
        [Tooltip("Sets the color of the sun light source at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient sunlightColor;
        [Tooltip("Sets the color of the star particle FX and textures at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient starColor;
        [Tooltip("Sets the color of the zenith (or top) of the ambient scene lighting at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient ambientLightHorizonColor;
        [Tooltip("Sets the color of the horizon (or middle) of the ambient scene lighting at a certain time. Starts and ends at midnight.")]
        [GradientUsage(true)]
        public Gradient ambientLightZenithColor;
        [Tooltip("Sets the intensity of the galaxy effects at a certain time. Starts and ends at midnight.")]
        public AnimationCurve galaxyIntensity;


        [GradientUsage(true)]
        public Gradient fogColor1;
        [GradientUsage(true)]
        public Gradient fogColor2;
        [GradientUsage(true)]
        public Gradient fogColor3;
        [GradientUsage(true)]
        public Gradient fogColor4;
        [GradientUsage(true)]
        public Gradient fogColor5;
        [GradientUsage(true)]
        public Gradient fogFlareColor;


        public Material skyShader;
        public Material cloudShader;
        public Material fogShader;



        //HIDDEN PROPERTIES
        [HideInInspector]
        public float gradientExponent = 0.364f;
        [HideInInspector]
        public Vector2 atmosphereVariation = new Vector2(0.3f, 0.7f);
        [HideInInspector]
        public float atmosphereBias = 1;
        [HideInInspector]
        public float sunSize = 0.7f;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color sunColor;

        [HideInInspector]
        public float sunFalloff = 43.7f;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color sunFlareColor;
        [HideInInspector]
        public float moonFalloff = 24.4f;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color moonFlareColor;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color galaxy1Color;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color galaxy2Color;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color galaxy3Color;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color lightScatteringColor;
        [HideInInspector]
        public float rainbowPosition = 78.7f;
        [HideInInspector]
        public float rainbowWidth = 11;

        [HideInInspector]
        public float fogStart1 = 2;
        [HideInInspector]
        public float fogStart2 = 5;
        [HideInInspector]
        public float fogStart3 = 10;
        [HideInInspector]
        public float fogStart4 = 30;
        [HideInInspector]
        public float fogHeight = 0.85f;
        [HideInInspector]
        public float fogLightFlareIntensity = 1;
        [HideInInspector]
        public float fogLightFlareFalloff = 21;
        [HideInInspector]
        [ColorUsage(false, true)]
        public Color cloudMoonColor;
        [HideInInspector]
        public float cloudSunHighlightFalloff = 14.1f;
        [HideInInspector]
        public float cloudMoonHighlightFalloff = 22.9f;
        [HideInInspector]
        public float cloudWindSpeed = 3;
        [HideInInspector]
        public float clippingThreshold = 0.5f;
        [HideInInspector]
        public float cloudMainScale = 20;
        [HideInInspector]
        public float cloudDetailScale = 2.3f;
        [HideInInspector]
        public float cloudDetailAmount = 30;
        [HideInInspector]
        public float acScale = 1;
        [HideInInspector]
        public float cirroMoveSpeed = 0.5f;
        [HideInInspector]
        public float cirrusMoveSpeed = 0.5f;
        [HideInInspector]
        public float chemtrailsMoveSpeed = 0.5f;


        [HideInInspector]
        [ColorUsage(false, true)]
        public Color cloudTextureColor = Color.white;
        [HideInInspector]
        public float cloudCohesion = 0.75f;
        [HideInInspector]
        public float spherize = 0.361f;
        [HideInInspector]
        public float shadowDistance = 0.0288f;
        [HideInInspector]
        public float cloudThickness = 2f;
        [HideInInspector]
        public float textureAmount = 1f;


    }

#if UNITY_EDITOR
    [CustomEditor(typeof(AtmosphereProfile))]
    [CanEditMultipleObjects]
    public class E_AtmosphereProfile : Editor
    {

        SerializedProperty skyZenithColor;
        SerializedProperty skyHorizonColor;
        SerializedProperty cloudColor;
        SerializedProperty cloudHighlightColor;
        SerializedProperty highAltitudeCloudColor;
        SerializedProperty sunlightColor;
        SerializedProperty starColor;
        SerializedProperty ambientLightHorizonColor;
        SerializedProperty ambientLightZenithColor;
        SerializedProperty galaxyIntensity;
        SerializedProperty fogColor1;
        SerializedProperty fogColor2;
        SerializedProperty fogColor3;
        SerializedProperty fogColor4;
        SerializedProperty fogColor5;
        SerializedProperty fogFlareColor;
        SerializedProperty skyShader;
        SerializedProperty cloudShader;
        SerializedProperty fogShader;

        Vector2 scrollPos;
        AtmosphereProfile prof;



        void OnEnable()
        {

            prof = (AtmosphereProfile)target;

            skyZenithColor = serializedObject.FindProperty("skyZenithColor");
            skyHorizonColor = serializedObject.FindProperty("skyHorizonColor");
            cloudColor = serializedObject.FindProperty("cloudColor");
            cloudHighlightColor = serializedObject.FindProperty("cloudHighlightColor");
            highAltitudeCloudColor = serializedObject.FindProperty("highAltitudeCloudColor");
            sunlightColor = serializedObject.FindProperty("sunlightColor");
            starColor = serializedObject.FindProperty("starColor");
            ambientLightHorizonColor = serializedObject.FindProperty("ambientLightHorizonColor");
            ambientLightZenithColor = serializedObject.FindProperty("ambientLightZenithColor");
            galaxyIntensity = serializedObject.FindProperty("galaxyIntensity");

            fogColor1 = serializedObject.FindProperty("fogColor1");
            fogColor2 = serializedObject.FindProperty("fogColor2");
            fogColor3 = serializedObject.FindProperty("fogColor3");
            fogColor4 = serializedObject.FindProperty("fogColor4");
            fogColor5 = serializedObject.FindProperty("fogColor5");
            fogFlareColor = serializedObject.FindProperty("fogFlareColor");

            skyShader = serializedObject.FindProperty("skyShader");
            cloudShader = serializedObject.FindProperty("cloudShader");
            fogShader = serializedObject.FindProperty("fogShader");




        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            Undo.RecordObject(prof, prof.name + " Profile Changes");


            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            EditorGUILayout.LabelField("Sky Parameters", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(skyZenithColor);
            EditorGUILayout.PropertyField(skyHorizonColor);
            EditorGUILayout.PropertyField(cloudColor);
            EditorGUILayout.PropertyField(cloudHighlightColor);    
            EditorGUILayout.PropertyField(highAltitudeCloudColor);
            EditorGUILayout.PropertyField(starColor);
            EditorGUILayout.PropertyField(galaxyIntensity);

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Lighting Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(sunlightColor);
            EditorGUILayout.PropertyField(ambientLightHorizonColor);
            EditorGUILayout.PropertyField(ambientLightZenithColor);

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Fog Settings", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(fogColor1);
            EditorGUILayout.PropertyField(fogColor2);
            EditorGUILayout.PropertyField(fogColor3);
            EditorGUILayout.PropertyField(fogColor4);
            EditorGUILayout.PropertyField(fogColor5);
            EditorGUILayout.PropertyField(fogFlareColor);

            EditorGUILayout.Space(15);
            EditorGUILayout.LabelField("Shader References", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(skyShader);
            EditorGUILayout.PropertyField(cloudShader);
            EditorGUILayout.PropertyField(fogShader);


            EditorGUILayout.Space(20);


            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(prof);

            EditorGUILayout.EndScrollView();

        }
    }
#endif
}