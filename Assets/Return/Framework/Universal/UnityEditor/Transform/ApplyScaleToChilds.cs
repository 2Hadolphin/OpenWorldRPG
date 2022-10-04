using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;


public class ApplyScaleToChilds : OdinEditorWindow
{
    [MenuItem("Tools/ReadOnlyTransform/ApplyRelativeCoordinates")]
    private static void OpenWindow()
    {
        GetWindow<ApplyScaleToChilds>()
            .position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }


    public Transform Transform;
    public Vector3 NewScale;
    [Button("Apply")]
    void Apply()
    {
        if (!Transform)
            return;
        var scale = Transform.localScale;
        var parameter = new Vector3(1 / (NewScale.x / scale.x), 1 / (NewScale.y / scale.y), 1 / (NewScale.z / scale.z));

        var tfs = Transform.GetComponentsInChildren<Transform>();
        foreach (var tf in tfs)
        {
            var pos=tf.localPosition;
            tf.localPosition = new Vector3(pos.x * parameter.x, pos.y * parameter.y, pos.z * parameter.z);
        }

        Transform.localScale = NewScale;
    }
}
