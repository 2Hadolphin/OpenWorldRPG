using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
[Serializable]
public class AniamtionLibrary_Float : AnimationLibrary<float>
{
    enum HumanBoneBinding { RootT,RootQ}
    private void OnValidate()
    {
        var num = Collections.Count;

        for(var i=0;i<num;i++)
        {
            var clip = Collections[i].Clip;

            if (!clip)
                continue;

            var bindings = AnimationUtility.GetCurveBindings(clip);
            var rootVector=Vector3.zero;


            foreach(var binding in bindings)
            {
                if (binding.propertyName.Contains("RootT.width"))
                {
                    var curve=AnimationUtility.GetEditorCurve(clip, binding);
                    rootVector.x=curve.Evaluate(1)-curve.Evaluate(0);
                }
                else if (binding.propertyName.Contains("RootT.height"))
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    rootVector.y = curve.Evaluate(1) - curve.Evaluate(0);
                }
                else if (binding.propertyName.Contains("RootT.z"))
                {
                    var curve = AnimationUtility.GetEditorCurve(clip, binding);
                    rootVector.z = curve.Evaluate(1) - curve.Evaluate(0);
                }
            }


            Collections[i] = new ClipCollection { Clip = clip, Parameter = rootVector.magnitude};
        }
    }
}

#endif