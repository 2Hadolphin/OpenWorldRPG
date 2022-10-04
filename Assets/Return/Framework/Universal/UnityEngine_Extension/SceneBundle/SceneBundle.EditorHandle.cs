#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;
using Sirenix.OdinInspector;


namespace Return.Scenes
{
    public partial class SceneBundle    // Editor handle
    {
        [TitleGroup("Editor Config")]
        [HorizontalGroup("Editor Config/Editor")]
        [Button(ButtonSizes.Large)]
        [PropertyTooltip("Set all scene assets from editor build setting to this bundle.")]
        protected void LoadScenesFromEditorBuild()
        {
            QuickScenes.Clear();

            foreach (var s in EditorBuildSettings.scenes)
            {
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path);

                if (QuickScenes.Any(x => x.EditorSceneAsset == scene))
                    continue;

                QuickScenes.Add(
                    new SceneData()
                    {
                        EditorSceneAsset= scene,
                        SceneID = scene.name
                    });
            }

            Dirty();
        }

        [HorizontalGroup("Editor Config/Editor")]
        [Button(ButtonSizes.Large)]
        [PropertyTooltip("Set all scene assets from this bundle to editor build setting.")]
        protected void PushScenesToEditorBuild()
        {
            EditorBuildSettings.scenes =
                QuickScenes.
                Select(
                    x => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(x.EditorSceneAsset),true)
                ).
                ToArray();
        }

        [HorizontalGroup("Editor Config/EditorPart")]
        [Button(ButtonSizes.Large)]
        [PropertyTooltip("Add scene asset from editor build setting to this bundle.")]
        protected void AddScenesFromEditorBuild()
        {
            foreach (var s in EditorBuildSettings.scenes)
            {
                var scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(s.path);

                if (QuickScenes.Any(x => x.EditorSceneAsset == scene))
                    continue;

                QuickScenes.Add(
                    new SceneData()
                    {
                        EditorSceneAsset = scene,
                        SceneID = scene.name
                    });
            }

            Dirty();
        }

        [HorizontalGroup("Editor Config/EditorPart")]
        [Button(ButtonSizes.Large)]
        [PropertyTooltip("Add scene asset from this bundle to editor build setting.")]
        protected void AddScenesToEditorBuild()
        {
            var scenes = EditorBuildSettings.scenes.ToHashSet();

            var addScenes = QuickScenes.
                Where(x=> !scenes.Any(scene=>scene.path == AssetDatabase.GetAssetPath(x.EditorSceneAsset))).
                Select
                (
                    x => new EditorBuildSettingsScene(AssetDatabase.GetAssetPath(x.EditorSceneAsset), true)
                );

            scenes.AddRange(addScenes);

            EditorBuildSettings.scenes = scenes.ToArray();
        }
    }
}
#endif