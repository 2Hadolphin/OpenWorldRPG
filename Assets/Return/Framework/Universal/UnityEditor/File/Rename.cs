using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEditor;

public class Rename
{

    public List<UnityEngine.Object> Objects;

    public string Filter_Remove=string.Empty;
    public string Filter_Replace = string.Empty;

    [Button("Remove via filter")]
    void RemoveFilter()
    {
        foreach (var obj in Objects)
        {
            var name = obj.name.Replace(Filter_Remove, Filter_Replace);
            if (AssetDatabase.IsMainAsset(obj))
            {
                AssetDatabase.RenameAsset(AssetDatabase.GetAssetPath(obj), name);
            }
            else
            {
                obj.name = name;
            }

            EditorUtility.SetDirty(obj);
        }
    }

}
