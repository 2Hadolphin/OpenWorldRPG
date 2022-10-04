using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

public class BatchClipInitialize : OdinEditorWindow
{
    [MenuItem("Tools/Animation/Initialize Clips")]
    private static void OpenWindow()
    {
        GetWindow<BatchClipInitialize>().position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }

    [AssetsOnly]
    public List<AnimationClip> Sources;

    [BoxGroup("Setting")][Button("Sample")][ShowInInspector][HideLabel]
    public void SampleSetting(AnimationClip clip)
    {
        Setting = AnimationUtility.GetAnimationClipSettings(clip);

    }

    

    [ShowInInspector][SerializeField][HideLabel][BoxGroup("Setting")]
    public AnimationClipSettings Setting;

    [Button("Initialize")]
    public void InitClips()
    {
        foreach (var clip in Sources)
        {
            var setting = AnimationUtility.GetAnimationClipSettings(clip);
            setting.loopTime = Setting.loopTime;
            setting.loopBlend = Setting.loopBlend;

            setting.loopBlendOrientation = Setting.loopBlendOrientation;
            setting.loopBlendPositionY = Setting.loopBlendPositionY;
            setting.loopBlendPositionXZ = Setting.loopBlendPositionXZ;


            setting.keepOriginalOrientation = Setting.keepOriginalOrientation;
            setting.keepOriginalPositionY = Setting.keepOriginalPositionY;
            setting.keepOriginalPositionXZ = Setting.keepOriginalPositionXZ;

            setting.heightFromFeet = Setting.heightFromFeet;

            setting.mirror = Setting.mirror;

            AnimationUtility.SetAnimationClipSettings(clip, setting);
            EditorUtility.SetDirty(clip);
            
        };


        AssetDatabase.SaveAssets();
    }


}
