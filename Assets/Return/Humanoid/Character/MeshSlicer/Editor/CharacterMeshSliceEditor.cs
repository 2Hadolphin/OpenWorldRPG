using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Return.Database.Assets;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Linq;
using System.Text.RegularExpressions;
using Return;
using Return.Humanoid;
using Return.Humanoid.Animation;
using UnityEngine.Assertions;
using Return.Creature;

/// <summary>
/// Split first person mesh from character
/// </summary>
public class CharacterMeshSliceEditor : OdinEditorWindow
{
    [MenuItem("Tools/Character/Slicer")]
    private static void OpenWindow()
    {
        GetWindow<CharacterMeshSliceEditor>().position = GUIHelper.GetEditorWindowRect().AlignCenter(500, 600);
    }

    [PropertyOrder(-1)]
    [OnInspectorGUI]
    void Workflow()
    {
        EditorGUILayout.BeginVertical();

        GUILayout.Label("1.Select skinned renderer");
        GUILayout.Label("2.Select the mesh side to preserve.");

        EditorGUILayout.EndVertical();
    }

    [BoxGroup("Manager")]
    [ShowInInspector][HideLabel]
    public MeshSliceManager SliceManager;


    public string MeshName;

    [SerializeField]
    public FolderPathFinder Path;

    /// <summary>
    /// Save mesh as file.
    /// </summary>
    [Button]
    public void SaveMesh()
    {
        var mesh = SliceManager.CombinedMesh;

        EditorHelper.WriteAsset(mesh, MeshName,Path);
    }



    #region Split Mesh

    [TabGroup("Split SkinnedMeshRenderer")]
    public bool UpdateWhenOffScreen = true;

    [Tooltip("Child renderers.")]
    [ShowInInspector]
    [ListDrawerSettings(ShowIndexLabels =true)]  
    public HashSet<Renderer> AttachRenderer=new();

    [Tooltip("Split renderer mesh.")]
    [TabGroup("Split SkinnedMeshRenderer")]
    [Button]
    void ExtractSkinnedMesh(SkinnedMeshRenderer renderer)
    {
        var index = renderer.transform.GetSiblingIndex();

        Debug.Log(index);


        renderer.updateWhenOffscreen = UpdateWhenOffScreen;

        renderer.bounds = renderer.sharedMesh.bounds;


        renderer.ResetBounds();
        renderer.ResetLocalBounds();

        foreach (var bone in renderer.bones)
        {
            var renderers=bone.GetComponentsInChildren<Renderer>();
            AttachRenderer.AddRange(renderers);
        }

        var character=GameObject.Instantiate(renderer.transform.GetRoot());

        var newRenderer= character.transform.GetChildOfIndex(index).GetComponent<SkinnedMeshRenderer>();
        //renderer = newRenderer;
        EditorGUIUtility.PingObject(newRenderer);
    }



    #endregion


}
