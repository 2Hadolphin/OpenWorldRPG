using UnityEngine;
using System;
using Sirenix.OdinInspector;
using System.Linq;

[Serializable]
public class AnimationLibrary_Vector3 : AnimationLibrary<Vector3>
{
#if UNITY_EDITOR

    [PropertyOrder(-1)]
    [ButtonGroup("Control")]
    [Button("LoadResource",ButtonSizes.Small,ButtonStyle.FoldoutButton)]
    void LoadResource()
    {
        
        var path= UnityEditor.EditorUtility.OpenFolderPanel("Find Animation Clips m_Path", "Assets/Animation/Resources/ClipSet", "");
        var resource = "Resources/";
        path = path.Substring(path.IndexOf(resource)+resource.Length);

        var clips = Resources.LoadAll<AnimationClip>(path).Select(x => new ClipCollection() { Clip = x }).ToArray() ;
        Collections.AddRange(clips);
        OnValidate();
    }
    [ButtonGroup("Control")]
    [PropertyOrder(-0.8f)]
    [Button("Clear")]
    void Clear()
    {
        Collections.Clear();
    }

    public void OnValidate()
    {

        var num = Collections.Count;

        for(var i=0;i<num;i++)
        {
            var clip = Collections[i].Clip;

            if (!clip)
                continue;

            var bindings = UnityEditor.AnimationUtility.GetCurveBindings(clip);
            var rootVector=Vector3.zero;


            foreach(var binding in bindings)
            {
                if (binding.propertyName.Contains("RootT.width"))
                {
                    var curve= UnityEditor.AnimationUtility.GetEditorCurve(clip, binding);
                    rootVector.x=curve.Evaluate(1)-curve.Evaluate(0);
                    if (Math.Abs(rootVector.x) < 0.01)
                        rootVector.x = 0;
                }
                else if (binding.propertyName.Contains("RootT.height"))
                {
                    var curve = UnityEditor.AnimationUtility.GetEditorCurve(clip, binding);
                    rootVector.y = curve.Evaluate(1) - curve.Evaluate(0);
                    if (Math.Abs(rootVector.y) < 0.01)
                        rootVector.y = 0;
                }
                else if (binding.propertyName.Contains("RootT.z"))
                {
                    var curve = UnityEditor.AnimationUtility.GetEditorCurve(clip, binding);
                    rootVector.z = curve.Evaluate(1) - curve.Evaluate(0);
                    if (Math.Abs(rootVector.z) < 0.01)
                        rootVector.z = 0;
                }
            }

            if (Collections[i].Mirror)
                rootVector.x *= -1;


            Collections[i] = new ClipCollection { Clip = clip, Parameter = rootVector,Mirror=Collections[i].Mirror};
        }
    }
#endif
    public bool HasMirrorClip
    {
        get
        {
            foreach(var clipData in Collections)
            {
                if (clipData.Mirror)
                    return true;
            }

            return false;
        }
    }
}

