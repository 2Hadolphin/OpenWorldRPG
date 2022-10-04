// Distant Lands 2021.



using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DistantLands.Cozy.Data;
using System.Linq;

namespace DistantLands.Cozy.EditorScripts
{
    public class E_CozyCreateProfile : EditorWindow
    {

        public enum ProfileType { Weather, Ambience, Atmosphere}

        public ProfileType profileType;

        public string profileName = "New Profile";


        #region Weather & Ambience Variables
        public GameObject particleFX;
        public float minPlayTime = 30;
        public float maxPlayTime = 90;
        public float likelihood = 1;

        public AudioClip SFX;
        public float audioVolume = 1;
        public bool disableIndoors = true;

        public WeatherProfile.ChanceEffector[] chanceEffectors;
        public WeatherProfile[] forecastNext;
        public float minCloudCoverage = 0.3f;
        public float maxCloudCoverage = 0.6f;
        public float fogDensity = 1;
        public float snowAccumulation;
        public float waterAccumulation;
        public bool useThunder = false;
        public float saturation;
        public float value;
        public Color colorFilter = Color.white;
        public Color sunFilter = Color.white;
        public Color cloudFilter = Color.white;
        public WeatherProfile[] dontPlayDuring;
        #endregion

        #region Atmosphere Profille

        public Gradient skyZenithColor = new Gradient()
        {
            colorKeys = new GradientColorKey[7] {
new GradientColorKey(new Color(0f, 0f, 0f), 0.1794156f),
new GradientColorKey(new Color(0.03529412f, 0.1501841f, 0.254902f), 0.2911727f),
new GradientColorKey(new Color(0.08784264f, 0.2475414f, 0.3962264f), 0.3499962f),
new GradientColorKey(new Color(0.08784264f, 0.2475414f, 0.3962264f), 0.6558785f),
new GradientColorKey(new Color(0.01223745f, 0.05473636f, 0.1037736f), 0.7058824f),
new GradientColorKey(new Color(0.01176471f, 0.02745098f, 0.08039216f), 0.7529412f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.8088197f),
},
            alphaKeys = new GradientAlphaKey[6] {
new GradientAlphaKey(1f, 0.1799954f),
new GradientAlphaKey(1f, 0.2500038f),
new GradientAlphaKey(1f, 0.3499962f),
new GradientAlphaKey(1f, 0.6500038f),
new GradientAlphaKey(1f, 0.7499962f),
new GradientAlphaKey(1f, 0.8f),
}
        };
        public Gradient skyHorizonColor = new Gradient()
        {
            colorKeys = new GradientColorKey[7] {
new GradientColorKey(new Color(0.02943663f, 0.04203962f, 0.04245283f), 0.1852903f),
new GradientColorKey(new Color(0.5471698f, 0.3226237f, 0.3264296f), 0.2647135f),
new GradientColorKey(new Color(0.3764706f, 0.6087235f, 0.772549f), 0.3499962f),
new GradientColorKey(new Color(0.3764706f, 0.5839965f, 0.772549f), 0.6529488f),
new GradientColorKey(new Color(0.03671236f, 0.1659586f, 0.2358491f), 0.7147021f),
new GradientColorKey(new Color(0.1509434f, 0.09611962f, 0.1108285f), 0.7794156f),
new GradientColorKey(new Color(0.02943663f, 0.04203962f, 0.04245283f), 0.8088197f),
},
            alphaKeys = new GradientAlphaKey[5] {
new GradientAlphaKey(1f, 0.1799954f),
new GradientAlphaKey(1f, 0.2500038f),
new GradientAlphaKey(1f, 0.3499962f),
new GradientAlphaKey(1f, 0.6500038f),
new GradientAlphaKey(1f, 0.7499962f),
}
        };

        public Gradient cloudColor = new Gradient()
        {
            colorKeys = new GradientColorKey[7] {
new GradientColorKey(new Color(0.02156863f, 0.0372549f, 0.04313726f), 0.1852903f),
new GradientColorKey(new Color(0.9760846f, 0.5767773f, 0.5767773f), 0.282353f),
new GradientColorKey(new Color(0.7527342f, 0.8458441f, 0.8816556f), 0.3499962f),
new GradientColorKey(new Color(0.7489321f, 0.8084022f, 0.8312753f), 0.6500038f),
new GradientColorKey(new Color(0.4433962f, 0.2483855f, 0.2112406f), 0.7176471f),
new GradientColorKey(new Color(0.2735849f, 0.1494279f, 0.1122731f), 0.7647059f),
new GradientColorKey(new Color(0.02156863f, 0.0372549f, 0.04313726f), 0.8176547f),
},
            alphaKeys = new GradientAlphaKey[6] {
new GradientAlphaKey(1f, 0.1799954f),
new GradientAlphaKey(1f, 0.2500038f),
new GradientAlphaKey(1f, 0.3499962f),
new GradientAlphaKey(1f, 0.6500038f),
new GradientAlphaKey(1f, 0.7499962f),
new GradientAlphaKey(1f, 0.8f),
}
        };
        public Gradient cloudHighlightColor = new Gradient()
        {
            colorKeys = new GradientColorKey[8] {
new GradientColorKey(new Color(0.0752492f, 0.1315804f, 0.1792453f), 0.1911803f),
new GradientColorKey(new Color(3.36268f, 1.102607f, 0.9675635f), 0.2352941f),
new GradientColorKey(new Color(2.996078f, 2.447059f, 2.447059f), 0.2764782f),
new GradientColorKey(new Color(4.595026f, 4.595026f, 4.595026f), 0.3499962f),
new GradientColorKey(new Color(3.749019f, 3.749019f, 3.749019f), 0.6500038f),
new GradientColorKey(new Color(14.08545f, 3.038038f, 0f), 0.723537f),
new GradientColorKey(new Color(0.3773585f, 0.1085796f, 0.1800633f), 0.7882353f),
new GradientColorKey(new Color(0.0752492f, 0.1315804f, 0.1792453f), 0.8352941f),
},
            alphaKeys = new GradientAlphaKey[7] {
new GradientAlphaKey(1f, 0.1799954f),
new GradientAlphaKey(1f, 0.2500038f),
new GradientAlphaKey(1f, 0.3499962f),
new GradientAlphaKey(1f, 0.6500038f),
new GradientAlphaKey(1f, 0.7499962f),
new GradientAlphaKey(1f, 0.8f),
new GradientAlphaKey(1f, 0.8500038f),
}
        };
        public Gradient highAltitudeCloudColor = new Gradient()
        {
            colorKeys = new GradientColorKey[7] {
new GradientColorKey(new Color(0.5539197f, 0.9208916f, 0.8724236f), 0.1617609f),
new GradientColorKey(new Color(3.511422f, 4.008173f, 4.457614f), 0.2088197f),
new GradientColorKey(new Color(0.481132f, 1.755584f, 2f), 0.2617685f),
new GradientColorKey(new Color(4.595026f, 4.595026f, 4.595026f), 0.3499962f),
new GradientColorKey(new Color(3.749019f, 3.749019f, 3.749019f), 0.6264744f),
new GradientColorKey(new Color(20.99378f, 18.0895f, 14.18947f), 0.7088273f),
new GradientColorKey(new Color(0.5539197f, 0.9208916f, 0.8724236f), 0.8205844f),
},
            alphaKeys = new GradientAlphaKey[7] {
new GradientAlphaKey(1f, 0.1799954f),
new GradientAlphaKey(1f, 0.2500038f),
new GradientAlphaKey(1f, 0.3499962f),
new GradientAlphaKey(1f, 0.6500038f),
new GradientAlphaKey(1f, 0.7499962f),
new GradientAlphaKey(1f, 0.8f),
new GradientAlphaKey(1f, 0.8500038f),
}
        };

        public Gradient sunlightColor = new Gradient()
        {
            colorKeys = new GradientColorKey[6] {
new GradientColorKey(new Color(0f, 0f, 0f), 0.2176547f),
new GradientColorKey(new Color(0.5f, 0.2908882f, 0.2382075f), 0.2588235f),
new GradientColorKey(new Color(1f, 0.9790159f, 0.8349056f), 0.2970626f),
new GradientColorKey(new Color(0.9724993f, 0.9520778f, 0.8141054f), 0.6529488f),
new GradientColorKey(new Color(1f, 0.578578f, 0.4764151f), 0.7382315f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.802945f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(1f, 0f),
new GradientAlphaKey(1f, 1f),
}
        };
        public Gradient starColor = new Gradient()
        {
            colorKeys = new GradientColorKey[6] {
new GradientColorKey(new Color(4f, 4f, 4f), 0f),
new GradientColorKey(new Color(4f, 4f, 4f), 0.2088197f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.2647135f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.6647135f),
new GradientColorKey(new Color(4f, 4f, 4f), 0.7323567f),
new GradientColorKey(new Color(4f, 4f, 4f), 1f),
},
            alphaKeys = new GradientAlphaKey[6] {
new GradientAlphaKey(1f, 0f),
new GradientAlphaKey(1f, 0.2088197f),
new GradientAlphaKey(0f, 0.2588235f),
new GradientAlphaKey(0f, 0.6676432f),
new GradientAlphaKey(1f, 0.7529412f),
new GradientAlphaKey(1f, 1f),
}
        };

        public Gradient ambientLightHorizonColor = new Gradient()
        {
            colorKeys = new GradientColorKey[8] {
new GradientColorKey(new Color(0.1411765f, 0.2156863f, 0.1974347f), 0.1735256f),
new GradientColorKey(new Color(0.5188679f, 0.2716714f, 0.3481846f), 0.2205844f),
new GradientColorKey(new Color(0.5188679f, 0.369571f, 0.4591492f), 0.2588235f),
new GradientColorKey(new Color(0.4709861f, 0.6148742f, 0.6792453f), 0.3058824f),
new GradientColorKey(new Color(0.4709861f, 0.6148742f, 0.6792453f), 0.6588235f),
new GradientColorKey(new Color(0.7169812f, 0.4637502f, 0.3145247f), 0.720592f),
new GradientColorKey(new Color(0.1682093f, 0.1926969f, 0.3396226f), 0.8f),
new GradientColorKey(new Color(0.1411765f, 0.2156863f, 0.1974347f), 0.9147021f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(1f, 0f),
new GradientAlphaKey(1f, 1f),
}
        };
        public Gradient ambientLightZenithColor = new Gradient()
        {
            colorKeys = new GradientColorKey[5] {
new GradientColorKey(new Color(0.07355821f, 0.1641472f, 0.1792453f), 0.1794156f),
new GradientColorKey(new Color(0.4075738f, 0.6280917f, 0.6698113f), 0.2294194f),
new GradientColorKey(new Color(0.4261303f, 0.6387468f, 0.6792453f), 0.3058824f),
new GradientColorKey(new Color(0.4261303f, 0.6387468f, 0.6792453f), 0.7058824f),
new GradientColorKey(new Color(0.07355821f, 0.1641472f, 0.1792453f), 0.8088197f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(1f, 0f),
new GradientAlphaKey(1f, 1f),
}
        };
        public AnimationCurve galaxyMultiplier = new AnimationCurve()
        {
            keys = new Keyframe[6]
            {
                new Keyframe(0, 1),
                new Keyframe(0.2f, 1),
                new Keyframe(0.25f, 0),
                new Keyframe(0.7f, 0),
                new Keyframe(0.75f, 1),
                new Keyframe(1, 1),
            }

        };

        public Gradient fogColor1 = new Gradient()
        {
            colorKeys = new GradientColorKey[7] {
new GradientColorKey(new Color(0f, 0f, 0f), 0.1558862f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.2500038f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.3499962f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.679408f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.7588311f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.8f),
new GradientColorKey(new Color(0f, 0f, 0f), 0.8500038f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(0.007843138f, 0f),
new GradientAlphaKey(0.007843138f, 1f),
}
        };
        public Gradient fogColor2 = new Gradient()
        {
            colorKeys = new GradientColorKey[5] {
new GradientColorKey(new Color(0.002941191f, 0.01078431f, 0.01176471f), 0.2382391f),
new GradientColorKey(new Color(0f, 0.1738163f, 0.1981132f), 0.3499962f),
new GradientColorKey(new Color(0f, 0.1738163f, 0.1981132f), 0.6676432f),
new GradientColorKey(new Color(0.254717f, 0.0572084f, 0f), 0.7411765f),
new GradientColorKey(new Color(0.002941177f, 0.01078431f, 0.01176471f), 0.8f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(0.1372549f, 0f),
new GradientAlphaKey(0.1372549f, 1f),
}
        };
        public Gradient fogColor3 = new Gradient()
        {
            colorKeys = new GradientColorKey[6] {
new GradientColorKey(new Color(0.002941191f, 0.01078431f, 0.01176471f), 0.2088197f),
new GradientColorKey(new Color(0.01176471f, 0.1857695f, 0.254902f), 0.3499962f),
new GradientColorKey(new Color(0.01169941f, 0.190502f, 0.25446f), 0.6735332f),
new GradientColorKey(new Color(0.1490196f, 0.0299873f, 0f), 0.7470664f),
new GradientColorKey(new Color(0.0197716f, 0.01584192f, 0.03773582f), 0.7852903f),
new GradientColorKey(new Color(0.002941177f, 0.01078431f, 0.01176471f), 0.8441138f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(0.5019608f, 0f),
new GradientAlphaKey(0.5019608f, 1f),
}
        };
        public Gradient fogColor4 = new Gradient()
        {
            colorKeys = new GradientColorKey[7] {
new GradientColorKey(new Color(0.006007493f, 0.02528286f, 0.02830189f), 0.1941253f),
new GradientColorKey(new Color(0.06603773f, 0.1671612f, 0.2641509f), 0.2647135f),
new GradientColorKey(new Color(0.01176469f, 0.2239178f, 0.3882353f), 0.3499962f),
new GradientColorKey(new Color(0.01176469f, 0.2047133f, 0.3882353f), 0.6705883f),
new GradientColorKey(new Color(0.6313726f, 0.3289658f, 0.03529413f), 0.7294118f),
new GradientColorKey(new Color(0.01176471f, 0.01199307f, 0.04705882f), 0.7823606f),
new GradientColorKey(new Color(0.006007478f, 0.02528285f, 0.02830189f), 1f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(0.7568628f, 0f),
new GradientAlphaKey(0.7568628f, 1f),
}
        };
        public Gradient fogColor5 = new Gradient()
        {
            colorKeys = new GradientColorKey[8] {
new GradientColorKey(new Color(0.02834639f, 0.101573f, 0.1226415f), 0.1882353f),
new GradientColorKey(new Color(0.2473745f, 0.4110048f, 0.4811321f), 0.2558785f),
new GradientColorKey(new Color(0.3927999f, 0.6789472f, 0.8584906f), 0.3499962f),
new GradientColorKey(new Color(0.3725525f, 0.6769377f, 0.8679245f), 0.6764782f),
new GradientColorKey(new Color(0.9811321f, 0.4262812f, 0.2267711f), 0.7294118f),
new GradientColorKey(new Color(0.8207547f, 0.221733f, 0.04258633f), 0.7676509f),
new GradientColorKey(new Color(0.05490195f, 0.08870173f, 0.1882353f), 0.80589f),
new GradientColorKey(new Color(0.02834639f, 0.101573f, 0.1226415f), 0.9088274f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(1f, 0f),
new GradientAlphaKey(1f, 1f),
}
        };
        public Gradient fogFlareColor = new Gradient()
        {
            colorKeys = new GradientColorKey[8] {
new GradientColorKey(new Color(0.2943663f, 0.831844f, 0.990566f), 0.1323568f),
new GradientColorKey(new Color(2.977128f, 1.525439f, 1.446435f), 0.2f),
new GradientColorKey(new Color(3.115151f, 0.7181354f, 0.3673527f), 0.3117723f),
new GradientColorKey(new Color(1.083397f, 1.392001f, 1.382235f), 0.3411765f),
new GradientColorKey(new Color(1.083397f, 1.392001f, 1.382235f), 0.6882429f),
new GradientColorKey(new Color(2.831289f, 0.5960608f, 0f), 0.7735256f),
new GradientColorKey(new Color(0.9049675f, 0.4962013f, 1.669757f), 0.802945f),
new GradientColorKey(new Color(0.2943663f, 0.831844f, 0.990566f), 0.8382391f),
},
            alphaKeys = new GradientAlphaKey[2] {
new GradientAlphaKey(1f, 0f),
new GradientAlphaKey(1f, 1f),
}
        };

        public Material skyMat;
        public Material cloudMat;
        public Material fogMat;

        #endregion

        #region Perennial Profille

        public int ticksPerDay = 360;
        public float tickSpeed;
        public int daysPerYear = 48;


        #endregion

        public Vector2 scrollPos;






        [MenuItem("Distant Lands/Cozy/Create New Cozy Profile")]
        static void Init()
        {

            E_CozyCreateProfile window = (E_CozyCreateProfile)EditorWindow.GetWindow(typeof(E_CozyCreateProfile), false, "Create New Cozy Profile");
            window.Show();

        }


        private void OnGUI()
        {

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty chances = so.FindProperty("chanceEffectors");
            SerializedProperty forecast = so.FindProperty("forecastNext");
            SerializedProperty disable = so.FindProperty("dontPlayDuring");
            SerializedProperty disableInTrigger = so.FindProperty("disableIndoors");

            EditorGUILayout.LabelField("Profile Type");
            profileType = (ProfileType)EditorGUILayout.EnumPopup(profileType);
            EditorGUILayout.Space(20);

            switch (profileType)
            {

                case (ProfileType.Ambience):
                    {
                        profileName = EditorGUILayout.TextField("Ambience Name", profileName);
                        EditorGUILayout.Space(20);

                        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                        EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.whiteLabel);

                        EditorGUILayout.MinMaxSlider("Ambience Length", ref minPlayTime, ref maxPlayTime, 0, 150);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Min Ticks: " + Mathf.Round(minPlayTime).ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField("Max Ticks: " + Mathf.Round(maxPlayTime).ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();

                        likelihood = EditorGUILayout.Slider("Ambience Likelihood", likelihood, 0, 2);


                        EditorGUILayout.Space(10);

                        EditorGUILayout.PropertyField(chances, true);
                        EditorGUILayout.PropertyField(disable, true);

                        so.ApplyModifiedProperties();


                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Ambience FX", EditorStyles.whiteLabel);

                        particleFX = (GameObject)EditorGUILayout.ObjectField("Particle FX", particleFX, typeof(GameObject), true);
                        EditorGUILayout.PropertyField(disableInTrigger, true);
                        SFX = (AudioClip)EditorGUILayout.ObjectField("Sound FX", SFX, typeof(AudioClip), true);
                        audioVolume = EditorGUILayout.Slider("SFX Volume", audioVolume, 0, 1.1f);
                        break;
                    }
                case (ProfileType.Weather):
                    {
                        profileName = EditorGUILayout.TextField("Weather Name", profileName);
                        EditorGUILayout.Space(20);

                        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
                        EditorGUILayout.LabelField("Forecasting Behaviours", EditorStyles.whiteLabel);

                        EditorGUILayout.MinMaxSlider("Weather Length", ref minPlayTime, ref maxPlayTime, 0, 150);

                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Min Ticks: " + Mathf.Round(minPlayTime).ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField("Max Ticks: " + Mathf.Round(maxPlayTime).ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();

                        likelihood = EditorGUILayout.Slider("Weather Likelihood", likelihood, 0, 2);


                        EditorGUILayout.Space(10);

                        EditorGUILayout.PropertyField(chances, true);
                        EditorGUILayout.PropertyField(forecast, true);

                        so.ApplyModifiedProperties();


                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Weather FX", EditorStyles.whiteLabel);

                        particleFX = (GameObject)EditorGUILayout.ObjectField("Particle FX", particleFX, typeof(GameObject), true);
                        EditorGUILayout.PropertyField(disableInTrigger, true);
                        SFX = (AudioClip)EditorGUILayout.ObjectField("Sound FX", SFX, typeof(AudioClip), true);
                        audioVolume = EditorGUILayout.Slider("SFX Volume", audioVolume, 0, 1.1f);

                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Color Filters", EditorStyles.whiteLabel);


                        saturation = EditorGUILayout.Slider("Saturation", saturation, -1, 1);
                        value = EditorGUILayout.Slider("Value", value, -1, 1);
                        colorFilter = EditorGUILayout.ColorField("Main Filter Color", colorFilter);
                        cloudFilter = EditorGUILayout.ColorField("Cloud Filter Color", cloudFilter);
                        sunFilter = EditorGUILayout.ColorField("Sun Filter Color", sunFilter);

                        EditorGUILayout.Space(20);
                        EditorGUILayout.LabelField("Precipitation", EditorStyles.whiteLabel);
                        EditorGUILayout.MinMaxSlider("Cloud Coverage", ref minCloudCoverage, ref maxCloudCoverage, 0, 2);
                        EditorGUILayout.BeginHorizontal();
                        EditorGUILayout.LabelField("Min Coverage: " + (Mathf.Round(minCloudCoverage * 100) / 100).ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.LabelField("Max Coverage: " + (Mathf.Round(maxCloudCoverage * 100) / 100).ToString(), EditorStyles.miniLabel);
                        EditorGUILayout.EndHorizontal();
                        fogDensity = EditorGUILayout.Slider("Fog Density", fogDensity, 0, 5);
                        GUILayout.Space(10);
                        snowAccumulation = EditorGUILayout.Slider("Snow Accumulation Speed", snowAccumulation, 0, 0.05f);
                        waterAccumulation = EditorGUILayout.Slider("Puddle Accumulation Speed", waterAccumulation, 0, 0.05f);
                        break;
                    }
                case (ProfileType.Atmosphere):
                    {
                        //Setup Properties
                        SerializedProperty skyZenithColor = so.FindProperty("skyZenithColor");
                        SerializedProperty skyHorizonColor = so.FindProperty("skyHorizonColor");
                        SerializedProperty cloudColor = so.FindProperty("cloudColor");
                        SerializedProperty cloudHighlightColor = so.FindProperty("cloudHighlightColor");
                        SerializedProperty highAltitudeCloudColor = so.FindProperty("highAltitudeCloudColor");
                        SerializedProperty sunlightColor = so.FindProperty("sunlightColor");
                        SerializedProperty starColor = so.FindProperty("starColor");
                        SerializedProperty ambientLightHorizonColor = so.FindProperty("ambientLightHorizonColor");
                        SerializedProperty ambientLightZenithColor = so.FindProperty("ambientLightZenithColor");
                        SerializedProperty galaxyMultiplier = so.FindProperty("galaxyMultiplier");
                        SerializedProperty fogColor1 = so.FindProperty("fogColor1");
                        SerializedProperty fogColor2 = so.FindProperty("fogColor2");
                        SerializedProperty fogColor3 = so.FindProperty("fogColor3");
                        SerializedProperty fogColor4 = so.FindProperty("fogColor4");
                        SerializedProperty fogColor5 = so.FindProperty("fogColor5");
                        SerializedProperty fogFlareColor = so.FindProperty("fogFlareColor");
                        SerializedProperty skyMat = so.FindProperty("skyMat");
                        SerializedProperty cloudMat = so.FindProperty("cloudMat");
                        SerializedProperty fogMat = so.FindProperty("fogMat");

                        profileName = EditorGUILayout.TextField("Atmosphere Name", profileName);
                        EditorGUILayout.Space(20);

                        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

                        EditorGUILayout.LabelField("Sky Parameters", EditorStyles.whiteLabel);
                        EditorGUILayout.PropertyField(skyZenithColor, true);
                        EditorGUILayout.PropertyField(skyHorizonColor, true);
                        EditorGUILayout.PropertyField(cloudColor, true);
                        EditorGUILayout.PropertyField(cloudHighlightColor, true);
                        EditorGUILayout.PropertyField(highAltitudeCloudColor, true);
                        EditorGUILayout.PropertyField(starColor, true);
                        EditorGUILayout.PropertyField(galaxyMultiplier, true);

                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Light Settings", EditorStyles.whiteLabel);
                        EditorGUILayout.PropertyField(ambientLightHorizonColor, true);
                        EditorGUILayout.PropertyField(ambientLightZenithColor, true);
                        EditorGUILayout.PropertyField(sunlightColor, true);

                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Fog Parameters", EditorStyles.whiteLabel);
                        EditorGUILayout.PropertyField(fogColor1, true);
                        EditorGUILayout.PropertyField(fogColor2, true);
                        EditorGUILayout.PropertyField(fogColor3, true);
                        EditorGUILayout.PropertyField(fogColor4, true);
                        EditorGUILayout.PropertyField(fogColor5, true);
                        EditorGUILayout.PropertyField(fogFlareColor, true);


                        EditorGUILayout.Space(10);
                        EditorGUILayout.LabelField("Material References", EditorStyles.whiteLabel);
                        EditorGUILayout.PropertyField(skyMat, true);
                        EditorGUILayout.PropertyField(cloudMat, true);
                        EditorGUILayout.PropertyField(fogMat, true);




                        break;
                    }   
            }



            GUILayout.Space(100);

            EditorGUILayout.EndScrollView();
            GUILayout.Space(20);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Save"))
            Instantiate();



        }


        public void Instantiate()
        {

            string path = EditorUtility.OpenFolderPanel("Save Location", "Asset/", "");


            if (path.Length == 0)
                return;

            path = "Assets" + path.Substring(Application.dataPath.Length) + "/";

            switch (profileType)
            {
                case (ProfileType.Ambience):
                    {

                        AmbienceProfile i = CreateInstance<AmbienceProfile>();
                        List<AmbienceProfile.ChanceEffector> k = new List<AmbienceProfile.ChanceEffector>();

                        foreach (WeatherProfile.ChanceEffector j in chanceEffectors)
                        {

                            AmbienceProfile.ChanceEffector l = new AmbienceProfile.ChanceEffector();

                            l.limitType = (AmbienceProfile.ChanceEffector.LimitType)j.limitType;
                            l.curve = j.curve;

                            k.Add(l);


                        }

                        i.chances = k;
                        i.dontPlayDuring = dontPlayDuring;
                        i.FXVolume = audioVolume;
                        i.soundFX = SFX;

                        if (particleFX)
                        {
                            ParticleSystem.TriggerModule trigger = particleFX.GetComponent<ParticleSystem>().trigger;
                            trigger.enabled = disableIndoors;
                        }


                        i.particleFX = particleFX;
                        i.likelihood = likelihood;
                        i.playTime = new Vector2(minPlayTime, maxPlayTime);
                        i.name = profileName;

                        AssetDatabase.CreateAsset(i, path + "/" + i.name + ".asset");
                        Debug.Log("Saved asset to " + path + i.name + "!");
                        break;
                    }
                case (ProfileType.Weather):
                    {
                        WeatherProfile i = CreateInstance<WeatherProfile>();

                        if (chanceEffectors.Length > 0)
                        i.chances = chanceEffectors.ToList();
                        i.forecastNext = forecastNext;
                        i.FXVolume = audioVolume;
                        if (SFX)
                            i.soundFX = SFX;
                        if (particleFX)
                            i.particleFX = particleFX;
                        i.likelihood = likelihood;
                        i.weatherTime = new Vector2(minPlayTime, maxPlayTime);
                        i.name = profileName;

                        i.useThunder = useThunder;
                        i.weatherFilter = new WeatherProfile.WeatherFilter();
                        i.weatherFilter.colorFilter = colorFilter;
                        i.weatherFilter.saturation = saturation;
                        i.weatherFilter.value = value;
                        i.cloudFilter = cloudFilter;
                        i.sunFilter = sunFilter;

                        i.wetnessSpeed = waterAccumulation;
                        i.snowAccumulationSpeed = snowAccumulation;
                        i.fogDensity = fogDensity;
                        i.cloudSettings = new WeatherProfile.CloudSettings();
                        i.cloudSettings.cloudCoverage = new Vector2(minCloudCoverage, maxCloudCoverage);

                        if (particleFX)
                        {
                            ParticleSystem.TriggerModule trigger = particleFX.GetComponent<ParticleSystem>().trigger;
                            trigger.enabled = disableIndoors;
                        }

                        AssetDatabase.CreateAsset(i, path + "/" + i.name + ".asset");
                        Debug.Log("Saved asset to " + path + i.name + "!");
                        break;
                    }
                case (ProfileType.Atmosphere):
                    {
                        AtmosphereProfile i = CreateInstance<AtmosphereProfile>();

                        i.name = profileName;
                        i.skyZenithColor = skyZenithColor;
                        i.skyHorizonColor = skyHorizonColor;
                        i.cloudColor = cloudColor;
                        i.cloudHighlightColor = cloudHighlightColor;
                        i.highAltitudeCloudColor = highAltitudeCloudColor;
                        i.sunlightColor = sunlightColor;
                        i.starColor = starColor;
                        i.ambientLightHorizonColor = ambientLightHorizonColor;
                        i.ambientLightZenithColor = ambientLightZenithColor;
                        i.galaxyIntensity = galaxyMultiplier;
                        i.fogColor1 = fogColor1;
                        i.fogColor2 = fogColor2;
                        i.fogColor3 = fogColor3;
                        i.fogColor4 = fogColor4;
                        i.fogColor5 = fogColor5;
                        i.fogFlareColor = fogFlareColor;
                        i.skyShader = skyMat;
                        i.fogShader = fogMat;
                        i.cloudShader = cloudMat;


                        AssetDatabase.CreateAsset(i, path + "/" + i.name + ".asset");
                        Debug.Log("Saved asset to " + path + i.name + "!");
                        break;
                    }


            }


        }
    }
}