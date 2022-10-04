using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using System;
public class FBXClipEditor : OdinEditorWindow
{

    FBXClipEditor()
    {
        this.titleContent = new GUIContent("Clips Setting");
    }

    [MenuItem("Tools/Animation/FBX Animation Editor")]
    static void Init()
    {
        var window = (FBXClipEditor)EditorWindow.GetWindow(typeof(FBXClipEditor));
        window.Show();
    }

    [AssetsOnly]
    public List<UnityEngine.Object> SourceObjects;

    [ShowInInspector]
    public Type Type;


    [Button(nameof(Search))]
    public void Search()
    {
        Targets.Clear();
        foreach (var obj in SourceObjects)
        {
            var path=AssetDatabase.GetAssetPath(obj);
            var subAssets = AssetDatabase.LoadAllAssetRepresentationsAtPath(path);

            foreach (var asset in subAssets)
            {
                if(asset.GetType()==Type)
                    Targets.Add(asset);
            }
        }
    }

    [ReadOnly]
    public List<UnityEngine.Object> Targets=new List<UnityEngine.Object>();


    [Button(nameof(Edit))]
    public void Edit()
    {
        foreach (var target in Targets)
        {
            var clip = (AnimationClip)target;

            var setting = AnimationUtility.GetAnimationClipSettings(clip);
            setting.keepOriginalPositionXZ = false;
            AnimationUtility.SetAnimationClipSettings(clip, setting);
            Debug.Log(clip.name);
            break;
        }
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }


}
