#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;
using NeoSaveGames;
using NeoSaveGames.Serialization;
using System.IO;
using System.Collections.Generic;
using System;

namespace NeoFPSEditor.SaveGames
{
    [CustomEditor(typeof(SaveGameManager), true)]
    public class SaveGameManagerEditor : SaveInfoBaseEditor
    {
        readonly GUIContent k_SceneFallbackLabel = new GUIContent("Fallback Texture", "The fallback texture to use if the scene texture can't be found.");
        readonly GUIContent k_ScreenshotFallbackLabel = new GUIContent("Fallback Texture", "The fallback texture to use if the screenshot can't be used.");
        readonly GUIContent k_ThumbnailTextureLabel = new GUIContent("Texture", "The texture to use.");

        public override void OnInspectorGUI()
        {
            InspectLocation();
            InspectQuickSaves();
            InspectAutoSaves();
            InspectManualSaves();
            InspectContinue();
            InspectThumbnails();
            InspectRecreatableItems();

            serializedObject.ApplyModifiedProperties();
        }

        void InspectLocation()
        {
            var pathProp = serializedObject.FindProperty("m_SavePath");
            var subFolderProp = serializedObject.FindProperty("m_SaveSubFolder");
            EditorGUILayout.PropertyField(pathProp);

            // Sub-folder property (filter to valid path characters)
            var input = EditorGUILayout.DelayedTextField("Sub-Folder", subFolderProp.stringValue);
            if (input != subFolderProp.stringValue)
                subFolderProp.stringValue = SaveGameUtilities.FilterPathString(input);

            if (GUILayout.Button("Explore To Folder"))
            {
                var cast = target as SaveGameManager;
                cast.CheckSaveFolder();
                Application.OpenURL(cast.GetSaveFolder());
            }

            if (GUILayout.Button("Save Game Inspector"))
            {
                SaveGameInspector.ShowWindow((SavePathRoot)pathProp.enumValueIndex, subFolderProp.stringValue);
            }
        }

        void InspectQuickSaves()
        {
            var canQuickSave = serializedObject.FindProperty("m_CanQuickSave");
            EditorGUILayout.PropertyField(canQuickSave);
            if (canQuickSave.boolValue)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_QuickLoadAll"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_NumQuicksaves"));
            }
        }

        void InspectAutoSaves()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_NumAutosaves"));
        }

        void InspectManualSaves()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_CanManualSave"));
        }

        void InspectContinue()
        {
            EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ContinueFrom"));
        }

        void InspectThumbnails()
        {
            bool screenshot = false;

            var thumbnailProp = serializedObject.FindProperty("m_QuicksaveThumbnail");
            EditorGUILayout.PropertyField(thumbnailProp);
            switch (thumbnailProp.enumValueIndex)
            {
                case 0: // None
                    break;
                case 1: // Texture
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_QsThumbnailTexture"), k_ThumbnailTextureLabel);
                    --EditorGUI.indentLevel;
                    break;
                case 2: // TextureFromScene
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_QsThumbnailTexture"), k_SceneFallbackLabel);
                    --EditorGUI.indentLevel;
                    break;
                case 3: // Screenshot
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_QsThumbnailTexture"), k_ScreenshotFallbackLabel);
                    --EditorGUI.indentLevel;
                    screenshot = true;
                    break;
            }

            thumbnailProp = serializedObject.FindProperty("m_AutosaveThumbnail");
            EditorGUILayout.PropertyField(thumbnailProp);
            switch (thumbnailProp.enumValueIndex)
            {
                case 0: // None
                    break;
                case 1: // Texture
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AsThumbnailTexture"), k_ThumbnailTextureLabel);
                    --EditorGUI.indentLevel;
                    break;
                case 2: // TextureFromScene
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AsThumbnailTexture"), k_SceneFallbackLabel);
                    --EditorGUI.indentLevel;
                    break;
                case 3: // Screenshot
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_AsThumbnailTexture"), k_ScreenshotFallbackLabel);
                    --EditorGUI.indentLevel;
                    screenshot = true;
                    break;
            }

            thumbnailProp = serializedObject.FindProperty("m_ManualSaveThumbnail");
            EditorGUILayout.PropertyField(thumbnailProp);
            switch (thumbnailProp.enumValueIndex)
            {
                case 0: // None
                    break;
                case 1: // Texture
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MsThumbnailTexture"), k_ThumbnailTextureLabel);
                    --EditorGUI.indentLevel;
                    break;
                case 2: // TextureFromScene
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MsThumbnailTexture"), k_SceneFallbackLabel);
                    --EditorGUI.indentLevel;
                    break;
                case 3: // Screenshot
                    ++EditorGUI.indentLevel;
                    EditorGUILayout.PropertyField(serializedObject.FindProperty("m_MsThumbnailTexture"), k_ScreenshotFallbackLabel);
                    --EditorGUI.indentLevel;
                    screenshot = true;
                    break;
            }

            if (screenshot)
            {
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScreenshotSize"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_ScreenshotCompression"));
                EditorGUILayout.PropertyField(serializedObject.FindProperty("m_UsingLinearRendering"));
            }
        }
    }
}

#endif