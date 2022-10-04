using UnityEditor;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization.Editor;
using Sirenix.OdinInspector.Editor;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.Assertions;

public class ExtraAnimationClip : OdinEditorWindow
{
    [AssetsOnly]
    public List<UnityEngine.Object> clipdata;
    public bool Filter_Humanoid;
    public bool KeepFolder = true;
    public bool KeepSetting = true;
    ExtraAnimationClip()
    {
        this.titleContent = new GUIContent("Extract HumanoidClip");
    }

    [MenuItem("Tools/Animation/ExtractClips")]
    static void Init()
    {
        var window = (ExtraAnimationClip)EditorWindow.GetWindow(typeof(ExtraAnimationClip));
        window.Show();


    }

    [FolderPath]
    public string mPath;

    [Button("Archive")]
    public void SaveFiles()
    {
        foreach (var data in clipdata)
        {
            var path = AssetDatabase.GetAssetPath(data);
            
            // if folder
            if (AssetDatabase.IsValidFolder(path))
            {
                SearchClipsInPack(data.name,path);
                continue;
            }
            else // if asset
            {
                var clips = AssetDatabase.LoadAllAssetsAtPath(path).Where(x => x.GetType() == typeof(AnimationClip)).Where(x => !x.name.Contains("preview")).Select(x => x as AnimationClip).ToArray();
                CopyClips(clips, path, data.name);
                //ExtractClips(clips, path, mPath);
            }
        }


        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    void ExtractClips(UnityEngine.Object[] objs,string oldFolderPath,string newFolderPath)
    {

        foreach (var obj in objs)
        {
            var mPath = AssetDatabase.GetAssetPath(obj);
            var newPath = mPath.Replace(oldFolderPath, newFolderPath);
            newPath = Path.Combine(newPath, obj.name) + ".anim";
            newPath = AssetDatabase.GenerateUniqueAssetPath(newPath);

            
            Assert.IsTrue(AssetDatabase.CopyAsset(mPath, newPath));
        }


        //foreach (var obj in objs)
        //{
        //    var mPath = AssetDatabase.GetAssetPath(obj);
        //    mPath = mPath.Replace(oldFolderPath, newFolderPath);
        //    mPath = m_Path.Combine(mPath, obj.name) + ".anim";
        //    mPath = AssetDatabase.GenerateUniqueAssetPath(mPath);

        //    AssetDatabase.ExtractAsset(obj, mPath);
        //}

    }

    void CopyClips(AnimationClip[] clips,string path,string name)
    {
        foreach (var source in clips)
        {
            if (Filter_Humanoid)
                if (!source.isHumanMotion)
                    continue;

            var bindings = AnimationUtility.GetCurveBindings(source);

            var clip = new AnimationClip
            {
                name = source.name,
                
            };

            foreach (var binding in bindings)
            {
                var curve = AnimationUtility.GetEditorCurve(source, binding);
                clip.SetCurve(binding.path, typeof(Animator), binding.propertyName, curve);
            }

            if (KeepSetting)
            {
                var setting = AnimationUtility.GetAnimationClipSettings(source);
                AnimationUtility.SetAnimationClipSettings(clip, setting);
            }

            EditorHelper.WriteAsset(clip, string.IsNullOrEmpty(source.name) ? name : source.name, mPath);
        }
    }



    public void SearchClipsInPack(string folderName,string path)
    {
        var fbxs = EditorHelper.GetAssetsAtPath<UnityEngine.Object>(path);


        foreach (var data in fbxs)
        {
            path = AssetDatabase.GetAssetPath(data);
            var targetFolder = mPath;
            if (KeepFolder && fbxs.Length > 0)
            {
                targetFolder = Path.Combine(mPath, folderName);

                if (AssetDatabase.GetSubFolders(mPath).Where(x=>x.Contains(folderName)).Count()==0)
                    AssetDatabase.CreateFolder(mPath, folderName);
            }

            var clips = AssetDatabase.LoadAllAssetRepresentationsAtPath(path).Where(x => x.GetType() == typeof(AnimationClip)).Where(x => !x.name.Contains("preview")).Select(x => x as AnimationClip).ToArray();

            CopyClips(clips, targetFolder, data.name);
            //ExtractClips(clips, path, targetFolder);
           
        }
        AssetDatabase.SaveAssets();

    }



}
