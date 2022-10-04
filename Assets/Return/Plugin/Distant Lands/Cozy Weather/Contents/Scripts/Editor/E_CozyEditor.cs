// Distant Lands 2021.



using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using DistantLands.Cozy.Data;

namespace DistantLands.Cozy.EditorScripts
{
    public class E_CozyEditor : EditorWindow
    {



        public Vector2 scrollPos;

        public int windowNum;
        public Texture icon1;
        public Texture icon2;
        public Texture icon3;
        public Texture icon4;
        public Texture icon5;
        public Texture icon6;

        public Texture titleWindow;

        public CozyWeather headUnit;



        //___________ATMOS_______________________________________________________________________

        public AtmosphereProfile profile;

        public bool a_sky;
        public bool a_clouds;
        public bool a_fog;
        public bool a_lighting;

        //___________CLIMATE_____________________________________________________________________

        //___________TIME________________________________________________________________________

        //___________MODULES_____________________________________________________________________

        bool calender = true;
        bool climate = true;
        bool forecast = true;
        bool mats = true;
        bool ambience = true;
        bool audio = true;
        bool trigger = true;
        bool lightning = true;
        bool saveLoad = true;
        bool wind = true;
        bool tooltips;


        //___________SETTINGS____________________________________________________________________

        public bool mobileMode;


        [MenuItem("Distant Lands/Cozy/Open Cozy Editor", false, 0)]
        static void Init()
        {

            E_CozyEditor window = (E_CozyEditor)EditorWindow.GetWindow(typeof(E_CozyEditor), false, "COZY: Weather");
            window.minSize = new Vector2(400, 500);
            window.Show();

        }


        private void OnGUI()
        {

            if (headUnit == null)
            headUnit = CozyWeather.instance;


            Undo.RecordObject(this, "Cozy Editor Changes");
            if (headUnit)
            {
                Undo.RecordObject(headUnit, "Cozy Editor Changes");
                Undo.RecordObject(headUnit.perennialProfile, "Perennial Profile Changes");
                Undo.RecordObject(headUnit.atmosphereProfile, "Atmosphere Profile Changes");
            }

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);


            List <Texture> icons = new List<Texture>();

            icons.Add(icon1);
            icons.Add(icon2);
            icons.Add(icon3);
            icons.Add(icon4);
            icons.Add(icon5);

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty chances = so.FindProperty("chanceEffectors");


            GUI.DrawTexture(new Rect(0,0, position.width, position.width * 1/3), titleWindow);
            EditorGUILayout.Space(position.width * 1 / 3);
            EditorGUILayout.Space(10);
            EditorGUILayout.Separator();


            if (headUnit)
            {

                GUIStyle iconStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
                iconStyle.fixedHeight = 50;
                iconStyle.fixedWidth = 50;
                iconStyle.margin = new RectOffset(10, 10, 10, 10);


                EditorGUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                windowNum = GUILayout.SelectionGrid(windowNum, icons.ToArray(), 5, iconStyle);
                GUILayout.FlexibleSpace();
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.Space(20);


                switch (windowNum)
                {
                    case (0):
                        DrawAtmosphereWindow();
                        break;
                    case (1):
                        DrawFogAndCloudsWindow();
                        break;
                    case (2):
                        DrawTimeWindow();
                        break;
                    case (3):
                        DrawModulesWindow();
                        break;
                    case (4):
                        DrawSettingsWindow();
                        break;
                    case (5):
                        DrawReportsWindow();
                        break;

                }

            } 
            else
            {

                EditorGUILayout.HelpBox("Make sure that your scene is properly setup!", MessageType.Warning, true);

                if (GUILayout.Button("Setup Scene", GUI.skin.GetStyle("MiniButton")))
                    SetupSceneForCozy();

            }

            GUILayout.FlexibleSpace();

            EditorGUILayout.EndScrollView();
        }

        void DrawAtmosphereWindow()
        {

            Color ogColor = GUI.backgroundColor;
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.fontSize = 16;

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(" - ATMOSPHERE - ", style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);

            if (!headUnit.atmosphereProfile)
            {

                EditorGUILayout.HelpBox("Make sure that you have an active atmosphere profile! Reset this on the Cozy Weather headunit.", MessageType.Warning, true);
                return;

            }



            Material skybox = headUnit.atmosphereProfile.skyShader;
            Material fog = headUnit.atmosphereProfile.fogShader;
            Material clouds = headUnit.atmosphereProfile.cloudShader;
            Undo.RecordObject(clouds, "Cloud Material Changes");
            Undo.RecordObject(fog, "Fog Material Changes");
            Undo.RecordObject(skybox, "Sky Material Changes");

            //Setup SOs                                                
            SerializedObject headUnitSO = new SerializedObject(headUnit);
            SerializedProperty atmosphere = headUnitSO.FindProperty("atmosphereProfile");
            AtmosphereProfile profile = headUnit.atmosphereProfile;

            ScriptableObject atmoProfSO = headUnit.atmosphereProfile;
            SerializedObject atmoProf = new SerializedObject(atmoProfSO);

            SerializedProperty zenithGradient = atmoProf.FindProperty("skyZenithColor");
            SerializedProperty horizonGradient = atmoProf.FindProperty("skyHorizonColor");
            SerializedProperty starColor = atmoProf.FindProperty("starColor");
            SerializedProperty galaxyIntensity = atmoProf.FindProperty("galaxyIntensity");

            SerializedProperty ambientLightHorizonColor = atmoProf.FindProperty("ambientLightHorizonColor");
            SerializedProperty ambientLightZenithColor = atmoProf.FindProperty("ambientLightZenithColor");
            SerializedProperty sunlightColor = atmoProf.FindProperty("sunlightColor");




            EditorGUILayout.PropertyField(atmosphere, true);

            EditorGUILayout.Space();


            a_sky = EditorGUILayout.Foldout(a_sky, "Sky Settings", true, foldoutStyle);

            if (a_sky)
            {



                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Skybox Parameters", labelStyle);
                EditorGUILayout.PropertyField(zenithGradient, true);
                EditorGUILayout.PropertyField(horizonGradient, true);

                if (skybox.HasProperty("_Power"))
                {
                    profile.gradientExponent = EditorGUILayout.Slider("Gradient Exponent", headUnit.atmosphereProfile.gradientExponent, 0, 1.5f);
                    skybox.SetFloat("_Power", profile.gradientExponent);
                }

                EditorGUILayout.Space();

                if (skybox.HasProperty("_PatchworkHeight"))
                {
                    EditorGUILayout.LabelField("Atmosphere Parameters", labelStyle);
                    float minAtmo = 1 - profile.atmosphereVariation.x;
                    float maxAtmo = profile.atmosphereVariation.y;
                    EditorGUILayout.MinMaxSlider("Atmosphere Variation", ref minAtmo, ref maxAtmo, 0, 1);
                    profile.atmosphereVariation = new Vector2(1 - minAtmo, maxAtmo);
                    skybox.SetFloat("_PatchworkHeight", maxAtmo);
                    skybox.SetFloat("_PatchworkVariation", 1 - minAtmo);
                    profile.atmosphereBias = EditorGUILayout.Slider("Atmosphere Bias", profile.atmosphereBias, 0, 1);
                    skybox.SetFloat("_PatchworkBias", profile.atmosphereBias);
                }


                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Celestial Parameters", labelStyle);

                if (skybox.HasProperty("_SunSize"))
                {
                    profile.sunSize = EditorGUILayout.Slider("Sun Size", profile.sunSize, 0, 40);
                    skybox.SetFloat("_SunSize", profile.sunSize);
                }
                if (skybox.HasProperty("_SunColor"))
                {
                    profile.sunColor = EditorGUILayout.ColorField(new GUIContent("Sun Color"), profile.sunColor, false, false, true);
                    skybox.SetColor("_SunColor", profile.sunColor);
                }
                EditorGUILayout.Space(2);
                headUnit.sunDirection = EditorGUILayout.Slider("Sun Direction", headUnit.sunDirection, 0, 360);
                headUnit.sunPitch = EditorGUILayout.Slider("Sun Pitch", headUnit.sunPitch, -90, 90);
                EditorGUILayout.Space(2);
                if (skybox.HasProperty("_SunFlareFalloff"))
                {
                    profile.sunFalloff = EditorGUILayout.Slider("Sun Flare Falloff", profile.sunFalloff, 0, 100);
                    skybox.SetFloat("_SunFlareFalloff", profile.sunFalloff);
                }
                if (skybox.HasProperty("_SunFlareColor"))
                {
                    profile.sunFlareColor = EditorGUILayout.ColorField(new GUIContent("Sun Flare Color"), profile.sunFlareColor, false, false, true);
                    skybox.SetColor("_SunFlareColor", profile.sunFlareColor);
                }
                EditorGUILayout.Space(2);


                if (skybox.HasProperty("_MoonFlareColor"))
                {
                    profile.moonFlareColor = EditorGUILayout.ColorField(new GUIContent("Moon Flare Color"), skybox.GetColor("_MoonFlareColor"), false, false, true);
                    skybox.SetColor("_MoonFlareColor", profile.moonFlareColor);
                }
                if (skybox.HasProperty("_MoonFlareFalloff"))
                {
                    profile.moonFalloff = EditorGUILayout.Slider("Moon Flare Falloff", profile.moonFalloff, 0, 100);
                    skybox.SetFloat("_MoonFlareFalloff", profile.moonFalloff);
                }

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Light FX Parameters", labelStyle);
                EditorGUILayout.PropertyField(starColor, true);
                EditorGUILayout.Space(3);
                EditorGUILayout.PropertyField(galaxyIntensity, true);
                if (skybox.HasProperty("_GalaxyColor1"))
                {
                    profile.galaxy1Color = EditorGUILayout.ColorField(new GUIContent("Galaxy Color 1"), profile.galaxy1Color, false, false, true);
                    skybox.SetColor("_GalaxyColor1", profile.galaxy1Color);
                }
                if (skybox.HasProperty("_GalaxyColor2"))
                {
                    profile.galaxy2Color = EditorGUILayout.ColorField(new GUIContent("Galaxy Color 2"), profile.galaxy2Color, false, false, true);
                    skybox.SetColor("_GalaxyColor2", profile.galaxy2Color);
                }
                if (skybox.HasProperty("_GalaxyColor3"))
                {
                    profile.galaxy3Color = EditorGUILayout.ColorField(new GUIContent("Galaxy Color 3"), profile.galaxy3Color, false, false, true);
                    skybox.SetColor("_GalaxyColor3", profile.galaxy3Color);
                }
                EditorGUILayout.Space(3);

                if (skybox.HasProperty("_LightColumnColor"))
                {
                    profile.lightScatteringColor = EditorGUILayout.ColorField(new GUIContent("Light Scattering Color"), profile.lightScatteringColor, false, false, true);
                    skybox.SetColor("_LightColumnColor", profile.lightScatteringColor);
                }
                EditorGUILayout.Space(3);
                if (skybox.HasProperty("_RainbowSize"))
                {
                    if (headUnit.cozyMaterials)
                    {
                        headUnit.cozyMaterials.useRainbow = EditorGUILayout.Toggle("Use Rainbow", headUnit.cozyMaterials.useRainbow);
                        if (headUnit.cozyMaterials.useRainbow)
                        {
                            profile.rainbowPosition = EditorGUILayout.Slider("Rainbow Position", profile.rainbowPosition, 0, 100);
                            skybox.SetFloat("_RainbowSize", profile.rainbowPosition);
                            profile.rainbowWidth = EditorGUILayout.Slider("Rainbow Width", profile.rainbowWidth, 0, 50);
                            skybox.SetFloat("_RainbowWidth", profile.rainbowWidth);
                        }
                    }
                }


            }


            EditorGUILayout.Space(20);

            a_lighting = EditorGUILayout.Foldout(a_lighting, "Lighting Settings", true, foldoutStyle);

            if (a_lighting)
            {

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Color Parameters", labelStyle);
                EditorGUILayout.PropertyField(sunlightColor, true);
                EditorGUILayout.PropertyField(ambientLightZenithColor, true);
                EditorGUILayout.PropertyField(ambientLightHorizonColor, true);




            }


            headUnitSO.ApplyModifiedProperties();
            atmoProf.ApplyModifiedProperties();

            headUnit.Update();
            headUnit.SetStaticShaderVariables();

            EditorGUILayout.Space(30);



        }


        void DrawFogAndCloudsWindow()
        {

            Color ogColor = GUI.backgroundColor;
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));
            foldoutStyle.fontStyle = FontStyle.Bold;
            foldoutStyle.fontSize = 16;




            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(" - FOG AND CLOUDS - ", style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!headUnit.atmosphereProfile)
            {

                EditorGUILayout.HelpBox("Make sure that you have an active atmosphere profile! Reset this on the Cozy Weather headunit.", MessageType.Warning, true);
                return;

            }


            mobileMode = headUnit.atmosphereProfile.name == "Mobile";

            //Setup SOs    
            AtmosphereProfile profile = headUnit.atmosphereProfile;
            SerializedObject atmoProf = new SerializedObject(profile);

            SerializedProperty cloudColor = atmoProf.FindProperty("cloudColor");
            SerializedProperty cloudHighlightColor = atmoProf.FindProperty("cloudHighlightColor");
            SerializedProperty cloudZenithColor = atmoProf.FindProperty("highAltitudeCloudColor");

            SerializedProperty fogColor1 = atmoProf.FindProperty("fogColor1");
            SerializedProperty fogColor2 = atmoProf.FindProperty("fogColor2");
            SerializedProperty fogColor3 = atmoProf.FindProperty("fogColor3");
            SerializedProperty fogColor4 = atmoProf.FindProperty("fogColor4");
            SerializedProperty fogColor5 = atmoProf.FindProperty("fogColor5");
            SerializedProperty fogFlareColor = atmoProf.FindProperty("fogFlareColor");


            EditorGUILayout.Space(10);

            SerializedObject headUnitSO = new SerializedObject(headUnit);
            SerializedProperty atmosphere = headUnitSO.FindProperty("atmosphereProfile");
            EditorGUILayout.PropertyField(atmosphere, true);


            Material fog = profile.fogShader;
            Material clouds = profile.cloudShader;
            Material skybox = profile.skyShader;

            Undo.RecordObject(clouds, "Cloud Material Changes");
            Undo.RecordObject(fog, "Fog Material Changes");
            Undo.RecordObject(skybox, "Sky Material Changes");



            EditorGUILayout.Space();

            a_fog = EditorGUILayout.Foldout(a_fog, "Fog Settings", true, foldoutStyle);

            if (a_fog)
            {

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Fog Color Parameters", labelStyle);
                EditorGUILayout.PropertyField(fogColor1, true);
                EditorGUILayout.PropertyField(fogColor2, true);
                EditorGUILayout.PropertyField(fogColor3, true);
                EditorGUILayout.PropertyField(fogColor4, true);
                EditorGUILayout.PropertyField(fogColor5, true);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Fog Generation Parameters", labelStyle);
                profile.fogStart1 = EditorGUILayout.Slider("Color 2 Starting Position", profile.fogStart1, 0, profile.fogStart2 - .01f);
                profile.fogStart2 = EditorGUILayout.Slider("Color 3 Starting Position", profile.fogStart2, profile.fogStart1, profile.fogStart3 - .01f);
                profile.fogStart3 = EditorGUILayout.Slider("Color 4 Starting Position", profile.fogStart3, profile.fogStart2, profile.fogStart4 - .01f);
                profile.fogStart4 = EditorGUILayout.Slider("Color 5 Starting Position", profile.fogStart4, profile.fogStart3, 50);
                fog.SetFloat("_ColorStart1", profile.fogStart1);
                fog.SetFloat("_ColorStart2", profile.fogStart2);
                fog.SetFloat("_ColorStart3", profile.fogStart3);
                fog.SetFloat("_ColorStart4", profile.fogStart4);
                profile.fogHeight = EditorGUILayout.Slider("Fog Height", profile.fogHeight, 0, 2);
                fog.SetFloat("_FogOffset", profile.fogHeight);

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Fog Flare Parameters", labelStyle);
                profile.fogLightFlareIntensity = EditorGUILayout.Slider("Light Flare Intensity", profile.fogLightFlareIntensity, 0, 2);
                fog.SetFloat("LightIntensity", profile.fogLightFlareIntensity);
                profile.fogLightFlareFalloff = EditorGUILayout.Slider("Light Flare Falloff", profile.fogLightFlareFalloff, 0, 40);
                fog.SetFloat("_LightFalloff", profile.fogLightFlareFalloff);
                EditorGUILayout.PropertyField(fogFlareColor, true);



            }


            EditorGUILayout.Space(20);

            a_clouds = EditorGUILayout.Foldout(a_clouds, "Cloud Settings", true, foldoutStyle);

            if (a_clouds)
            {

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Color Parameters", labelStyle);
                EditorGUILayout.PropertyField(cloudColor, true);
                EditorGUILayout.PropertyField(cloudHighlightColor, true);


                if (clouds.HasProperty("_AltoCloudColor"))
                    EditorGUILayout.PropertyField(cloudZenithColor, true);

                if (clouds.HasProperty("_CloudTextureColor"))
                {
                    profile.cloudTextureColor = EditorGUILayout.ColorField(new GUIContent("Cloud Texture Color"), profile.cloudTextureColor, true, false, true);
                    clouds.SetColor("_CloudTextureColor", profile.cloudTextureColor);

                }

                if (clouds.HasProperty("_MoonColor"))
                {
                    profile.cloudMoonColor = EditorGUILayout.ColorField(new GUIContent("Moon Color"), profile.cloudMoonColor, true, false, true);
                    clouds.SetColor("_MoonColor", profile.cloudMoonColor);
                }
                if (clouds.HasProperty("_SunFlareFalloff"))
                {
                    profile.cloudSunHighlightFalloff = EditorGUILayout.Slider("Sun Highlight Falloff", profile.cloudSunHighlightFalloff, 0, 50);
                    clouds.SetFloat("_SunFlareFalloff", profile.cloudSunHighlightFalloff);
                }
                if (clouds.HasProperty("_MoonFlareFalloff"))
                {
                    profile.cloudMoonHighlightFalloff = EditorGUILayout.Slider("Moon Highlight Falloff", profile.cloudMoonHighlightFalloff, 0, 50);
                    clouds.SetFloat("_MoonFlareFalloff", profile.cloudMoonHighlightFalloff);
                }


                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Main Noise Parameters", labelStyle);

                if (clouds.HasProperty("_WindSpeed"))
                {
                    profile.cloudWindSpeed = EditorGUILayout.Slider("Cloud Wind Speed", clouds.GetFloat("_WindSpeed"), 0, 10);
                    clouds.SetFloat("_WindSpeed", profile.cloudWindSpeed);
                }

                if (clouds.HasProperty("_CloudCohesion"))
                {
                    profile.cloudCohesion = EditorGUILayout.Slider("Cloud Cohesion", clouds.GetFloat("_CloudCohesion"), 0, 1);
                    clouds.SetFloat("_CloudCohesion", profile.cloudCohesion);
                }
                if (clouds.HasProperty("_Spherize"))
                {
                    profile.spherize = EditorGUILayout.Slider("Cloud Sphere Distortion", clouds.GetFloat("_Spherize"), 0, 1);
                    clouds.SetFloat("_Spherize", profile.spherize);
                }
                if (clouds.HasProperty("_ClippingThreshold"))
                {
                    profile.clippingThreshold = EditorGUILayout.Slider("Clipping Threshold", profile.clippingThreshold, 0.01f, 1);
                    clouds.SetFloat("_ClippingThreshold", profile.clippingThreshold);
                }
                if (clouds.HasProperty("_CloudThickness"))
                {
                    profile.cloudThickness = EditorGUILayout.Slider("Cloud Thickness", profile.cloudThickness, 0, 4);
                    clouds.SetFloat("_CloudThickness", profile.cloudThickness);
                }
                if (clouds.HasProperty("_MainCloudScale"))
                {
                    profile.cloudMainScale = EditorGUILayout.Slider("Main Scale", profile.cloudMainScale, 2, 30);
                    clouds.SetFloat("_MainCloudScale", profile.cloudMainScale);
                }
                if (clouds.HasProperty("_DetailScale"))
                {
                    profile.cloudDetailScale = EditorGUILayout.Slider("Detail Scale", profile.cloudDetailScale, 0.2f, 10);
                    clouds.SetFloat("_DetailScale", profile.cloudDetailScale);
                }
                if (clouds.HasProperty("_DetailAmount"))
                {
                    profile.cloudDetailAmount = EditorGUILayout.Slider("Detail Amount", profile.cloudDetailAmount, 0, 30);
                    clouds.SetFloat("_DetailAmount", profile.cloudDetailAmount);
                }
                if (clouds.HasProperty("_TextureAmount"))
                {
                    profile.textureAmount = EditorGUILayout.Slider("Texture Amount", profile.textureAmount, 0, 3);
                    clouds.SetFloat("_TextureAmount", profile.textureAmount);
                }


                EditorGUILayout.Space();

                if (clouds.HasProperty("_AltocumulusScale"))
                {
                    EditorGUILayout.LabelField("Cloud Type Parameters", labelStyle);
                    profile.acScale = EditorGUILayout.Slider("Altocumulus Scale", profile.acScale, 0.1f, 3);
                    clouds.SetFloat("_AltocumulusScale", profile.acScale);
                }
                if (clouds.HasProperty("_CirrostratusMoveSpeed"))
                {
                    profile.cirroMoveSpeed = EditorGUILayout.Slider("Cirrostratus Move Speed", profile.cirroMoveSpeed, 0, 3);
                    clouds.SetFloat("_CirrostratusMoveSpeed", profile.cirroMoveSpeed);
                }
                if (clouds.HasProperty("_CirrusMoveSpeed"))
                {
                    profile.cirrusMoveSpeed = EditorGUILayout.Slider("Cirrus Move Speed", clouds.GetFloat("_CirrusMoveSpeed"), 0, 3);
                    clouds.SetFloat("_CirrusMoveSpeed", profile.cirrusMoveSpeed);
                }
                if (clouds.HasProperty("_ChemtrailsMoveSpeed"))
                {
                    profile.chemtrailsMoveSpeed = EditorGUILayout.Slider("Chemtrails Move Speed", clouds.GetFloat("_ChemtrailsMoveSpeed"), 0, 3);
                    clouds.SetFloat("_ChemtrailsMoveSpeed", profile.chemtrailsMoveSpeed);
                }

            }

            EditorGUILayout.Space(30);

            headUnitSO.ApplyModifiedProperties();
            atmoProf.ApplyModifiedProperties();


            headUnit.Update();
            headUnit.SetStaticShaderVariables();

        }


        void DrawTimeWindow()
        {

            Color ogColor = GUI.backgroundColor;
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));




            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(" - TIME - ", style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (!headUnit.perennialProfile)
            {

                EditorGUILayout.HelpBox("Make sure that you have an active perennial profile! Reset this on the Cozy Weather headunit.", MessageType.Warning, true);
                return;

            }

            mobileMode = headUnit.atmosphereProfile.name == "Mobile";

            SerializedObject headUnitSO = new SerializedObject(headUnit);
            SerializedProperty per = headUnitSO.FindProperty("perennialProfile");

            ScriptableObject perProfSO = headUnit.perennialProfile;
            SerializedObject perProf = new SerializedObject(perProfSO);


            EditorGUILayout.Space(12);

            EditorGUILayout.LabelField("Current Profile", labelStyle);
            EditorGUILayout.ObjectField(per);

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Time Settings", labelStyle);

            headUnit.perennialProfile.currentTicks = EditorGUILayout.Slider("Current Time", headUnit.perennialProfile.currentTicks, 0, headUnit.perennialProfile.ticksPerDay);
            headUnit.perennialProfile.ticksPerDay = EditorGUILayout.FloatField("Ticks Per Day", headUnit.perennialProfile.ticksPerDay);
            if (calender)
            headUnit.perennialProfile.tickSpeed = EditorGUILayout.FloatField("Tick Speed", headUnit.perennialProfile.tickSpeed);
            headUnit.perennialProfile.tickSpeedMultiplier = EditorGUILayout.CurveField("Tick Speed Modifier", headUnit.perennialProfile.tickSpeedMultiplier);

            EditorGUILayout.Space(10);

            headUnit.perennialProfile.currentDay = EditorGUILayout.IntSlider("Current Day", headUnit.perennialProfile.currentDay, 0, headUnit.perennialProfile.daysPerYear - 1);
            headUnit.perennialProfile.daysPerYear = EditorGUILayout.IntField("Days Per Year", headUnit.perennialProfile.daysPerYear);
            headUnit.perennialProfile.currentYear = EditorGUILayout.IntField("Current Year", headUnit.perennialProfile.currentYear);

            headUnitSO.ApplyModifiedProperties();
            perProf.ApplyModifiedProperties(); 
            headUnit.Update();

            EditorGUILayout.Space(30);


        }


        void DrawReportsWindow()
        {


            Color ogColor = GUI.backgroundColor;
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(" - REPORTS - ", style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);


        }


        void DrawModulesWindow()
        {


            Color ogColor = GUI.backgroundColor;
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;





            GUIStyle buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            buttonStyle.fixedHeight = 0;
            buttonStyle.fixedWidth = 0;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));

            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(" - MODULES - ", style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(10);
            tooltips = EditorGUILayout.Toggle("Show tooltips", tooltips);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Basic Modules", labelStyle);
            calender = EditorGUILayout.Toggle("Calendar Module", calender);
            if (tooltips)
                EditorGUILayout.HelpBox("The calendar module manages changing time, days, and years. " +
                    "Use this module to allow time to pass at runtime as well as pass information to other modules. Required for complex forecasting.", MessageType.Info, true);

            climate = EditorGUILayout.Toggle("Climate Module", climate);
            if (tooltips)
                EditorGUILayout.HelpBox("The climate module manages changing temprature. " +
                    "Use this module in conjunction with the calendar module to change temprature based on the season. Required for complex forecasting.", MessageType.Info, true);

            forecast = EditorGUILayout.Toggle("Forecast Module", forecast);
            if (tooltips)
                EditorGUILayout.HelpBox("The forecast module manages changing weather profiles. " +
                    "Use this module to procedurally change the weather profile at runtime. For best results, use in combination with the climate module and the calendar module.", MessageType.Info, true);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Additional Modules", labelStyle);

            mats = EditorGUILayout.Toggle("Material Module", mats);
            if (tooltips)
                EditorGUILayout.HelpBox("The material module manages changing materials at runtime. " +
                    "Use this module to change material properties based on the material manager profile. Also manages changing snow and wetness accumulutaion.", MessageType.Info, true);

            ambience = EditorGUILayout.Toggle("Ambience Module", ambience);
            if (tooltips)
                EditorGUILayout.HelpBox("The ambience module functions as a secondary, lighter weather system that manages ambient noises and particles and SFX.", MessageType.Info, true);

            audio = EditorGUILayout.Toggle("Audio Module", audio);
            if (tooltips)
                EditorGUILayout.HelpBox("The audio module is a simple script that manages all audio played by the main system as well as the ambience system. " +
                    "Make sure to set actual SFX in the profiles themselves!", MessageType.Info, true);

            trigger = EditorGUILayout.Toggle("Trigger Manager Module", trigger);
            if (tooltips)
                EditorGUILayout.HelpBox("The trigger manager module manages creating and assigning triggers. " +
                    "Very light weight script that is mostly used for informational purposes by other modules.", MessageType.Info, true);

            lightning = EditorGUILayout.Toggle("Lightning Module", lightning);
            if (tooltips)
                EditorGUILayout.HelpBox("The lightning module procedurally spawns and despawns lightning prefabs. " +
                    "Be sure to setup lightning times in the weather profiles!", MessageType.Info, true);

            saveLoad = EditorGUILayout.Toggle("Save/Load Module", saveLoad);
            if (tooltips)
                EditorGUILayout.HelpBox("The save/load module manages saving and loading for the weather system.", MessageType.Info, true);

            wind = EditorGUILayout.Toggle("Wind Manager Module", wind);
            if (tooltips)
                EditorGUILayout.HelpBox("The wind manager module manages all of the shader-side wind and active windzone properties for the weather system.", MessageType.Info, true);


            if (GUILayout.Button("Apply Modules", buttonStyle))
            {

                if (calender && headUnit.calender == null)
                    headUnit.gameObject.AddComponent<CozyCalendar>();
                if (climate && headUnit.climate == null)
                    headUnit.gameObject.AddComponent<CozyClimate>();
                if (forecast && headUnit.forecast == null)
                    headUnit.gameObject.AddComponent<CozyForecast>();
                if (mats && headUnit.cozyMaterials == null)
                    headUnit.gameObject.AddComponent<CozyMaterialManager>();
                if (ambience && headUnit.ambience == null)
                    headUnit.gameObject.AddComponent<CozyAmbienceManager>();
                if (audio && headUnit.cozyAudio == null)
                    headUnit.gameObject.AddComponent<CozyAudio>();
                if (trigger && headUnit.triggerManager == null)
                    headUnit.gameObject.AddComponent<CozyTriggerManager>();
                if (lightning && headUnit.lightningManager == null)
                    headUnit.gameObject.AddComponent<CozyLightningManager>();
                if (lightning && headUnit.lightningManager == null)
                    headUnit.gameObject.AddComponent<CozyLightningManager>();
                if (saveLoad && headUnit.saveLoad == null)
                    headUnit.gameObject.AddComponent<CozySaveLoad>();
                if (wind && headUnit.wind == null)
                    headUnit.gameObject.AddComponent<CozyWindManager>();


                if (!calender && headUnit.calender)
                    headUnit.calender.DisableModule();
                if (!climate && headUnit.climate)
                    headUnit.climate.DisableModule();
                if (!forecast && headUnit.forecast)
                    headUnit.forecast.DisableModule();
                if (!mats && headUnit.cozyMaterials)
                    headUnit.cozyMaterials.DisableModule();
                if (!ambience && headUnit.ambience)
                    headUnit.ambience.DisableModule();
                if (!audio && headUnit.cozyAudio)
                    headUnit.cozyAudio.DisableModule();
                if (!trigger && headUnit.triggerManager)
                    headUnit.triggerManager.DisableModule();
                if (!lightning && headUnit.lightningManager)
                    headUnit.lightningManager.DisableModule();
                if (!saveLoad && headUnit.saveLoad)
                    headUnit.saveLoad.DisableModule();
                if (!wind && headUnit.wind)
                    headUnit.wind.DisableModule();

                headUnit.SetupModules();

            }

            EditorGUILayout.Space(30);
        }



        void DrawSettingsWindow()
        {


            Color ogColor = GUI.backgroundColor;
            GUIStyle style = new GUIStyle(GUI.skin.GetStyle("Label"));
            style.normal.textColor = Color.white;
            style.alignment = TextAnchor.MiddleCenter;
            style.fontSize = 18;

            GUIStyle labelStyle = new GUIStyle(GUI.skin.GetStyle("Label"));
            labelStyle.fontStyle = FontStyle.Bold;

            GUIStyle foldoutStyle = new GUIStyle(GUI.skin.GetStyle("Foldout"));

            GUIStyle buttonStyle = new GUIStyle(GUI.skin.GetStyle("Button"));
            buttonStyle.fixedWidth = 0;
            buttonStyle.fixedHeight = 0;


            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField(" - SETTINGS - ", style);
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(20);


            PerennialProfile perProf = headUnit.perennialProfile; 
            SerializedObject headUnitSO = new SerializedObject(headUnit);
            SerializedProperty quality = headUnitSO.FindProperty("m_ShaderProfile");



            EditorGUILayout.LabelField("General Settings", labelStyle);
            headUnit.lockToCamera = EditorGUILayout.Toggle("Lock to Camera", headUnit.lockToCamera);
            headUnit.allowRuntimeChanges = EditorGUILayout.Toggle("Allow Runtime Changes", headUnit.allowRuntimeChanges);
            EditorGUILayout.Space(10);
            if (calender)
            {
                headUnit.calender.resetTicksOnStart = EditorGUILayout.Toggle("Reset Ticks on Start", headUnit.calender.resetTicksOnStart);
                if (headUnit.calender.resetTicksOnStart)
                    headUnit.calender.startTicks = EditorGUILayout.Slider("Start Ticks", headUnit.calender.startTicks, 0, perProf.ticksPerDay);

            }

            EditorGUILayout.Space(20);






            EditorGUILayout.Space(30);

            EditorGUILayout.LabelField("Save/Load Control", labelStyle);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save", buttonStyle))
                headUnit.saveLoad.Save();
            if (GUILayout.Button("Load", buttonStyle))
                headUnit.saveLoad.Load();
            EditorGUILayout.EndHorizontal();

            headUnitSO.ApplyModifiedProperties();


        }

        static void SetupSceneForCozy()
        {

            if (FindObjectOfType<CozyWeather>())
            {
                EditorUtility.DisplayDialog("Cozy:Weather", "You already have a Cozy:Weather system in your scene!", "Ok");
                return;
            }

            if (!Camera.main)
            {
                EditorUtility.DisplayDialog("Cozy:Weather", "You need a main camera in your scene to setup for Cozy:Weather!", "Ok");
                return;
            }

            if (FindObjectsOfType<Light>().Length != 0)
                foreach (Light i in FindObjectsOfType<Light>())
                {

                    if (i.type == LightType.Directional)
                        if (EditorUtility.DisplayDialog("You already have a directional light!", "Do you want to delete " + i.gameObject.name + "? Cozy:Weather will properly light your scene", "Delete", "Keep this light"))
                            DestroyImmediate(i.gameObject);

                }
            if (!Camera.main.GetComponent<FlareLayer>())
                Camera.main.gameObject.AddComponent<FlareLayer>();



#if UNITY_POST_PROCESSING_STACK_V2


            if (!FindObjectOfType<UnityEngine.Rendering.PostProcessing.PostProcessVolume>())
            {
                List<string> path = new List<string>();
                path.Add("Assets/Distant Lands/Cozy Weather/Post FX/");


                GameObject i = new GameObject();

                i.name = "Post FX Volume";
                i.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>().profile = GetAssets<UnityEngine.Rendering.PostProcessing.PostProcessProfile>(path.ToArray(), "Post FX")[0];
                i.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessVolume>().isGlobal = true;
                i.layer = 1;




                if (!Camera.main.GetComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>())
                    Camera.main.gameObject.AddComponent<UnityEngine.Rendering.PostProcessing.PostProcessLayer>().volumeLayer = 2;
            }
#endif


            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.defaultReflectionMode = UnityEngine.Rendering.DefaultReflectionMode.Custom;
            RenderSettings.fog = false;

            List<string> str = new List<string>();
            str.Add("Assets/Distant Lands/Cozy Weather/Contents/Prefabs");

            GameObject weatherSphere = Instantiate(GetAssets<GameObject>(str.ToArray(), "Cozy Weather Sphere")[0]);

            weatherSphere.name = "Cozy Weather Sphere";
#if THE_VEGETATION_ENGINE                    
            weatherSphere.AddComponent<CozyTVEModule>();
#endif




        }

        public static List<T> GetAssets<T>(string[] _foldersToSearch, string _filter) where T : UnityEngine.Object
        {
            string[] guids = AssetDatabase.FindAssets(_filter, _foldersToSearch);
            List<T> a = new List<T>();
            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);
                a.Add(AssetDatabase.LoadAssetAtPath<T>(path));
            }
            return a;
        }

    }
}