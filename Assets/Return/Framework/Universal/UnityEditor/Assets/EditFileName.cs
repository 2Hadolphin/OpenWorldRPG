using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
public class EditFileName : OdinEditorWindow
{
    EditFileName()
    {
        this.titleContent = new GUIContent("Edit File Name");
    }

    [MenuItem("Tools/EditFileName")]
    static void Init()
    {
        var window = (EditFileName)EditorWindow.GetWindow(typeof(EditFileName));
        window.Show();


    }

    [BoxGroup("Filter")]
    [Button(nameof(Remove))]
    public void Remove(string fliter, params UnityEngine.Object[] @objects)
    {
        foreach (var obj in @objects)
        {
            obj.name = obj.name.Replace(fliter, string.Empty);
            AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), obj.name);
        }

        AssetDatabase.Refresh();
    }
}
