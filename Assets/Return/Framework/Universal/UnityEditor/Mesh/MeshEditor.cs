using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEditor;
using System.Linq;
using Return;


public class MeshEditor: OdinEditorWindow
{
    [MenuItem("Tools/Mesh/Editor")]
    private static void OpenWindow()
    {
        GetWindow<MeshEditor>()
            .position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }
    public const string DataPath = "MMeshEditorDataPath";



    public string Path
    {
        get => EditorPrefs.GetString(DataPath, EditorHelper.RootPath);
        set => EditorPrefs.SetString(DataPath, value);
    }

    #region SingleMesh
    [TabGroup("Mode", "Single")]
    [FolderPath]
    [ShowInInspector]
    [PropertyOrder(-1)]
    string mPath { get => Path; set => Path = value; }


    [HideLabel]
    [HorizontalGroup("Mode/Single/Asset")]
    [PropertySpace(10,10)][OnValueChanged(nameof(UpdataData))]
    public Mesh mMesh;


    [HorizontalGroup("Mode/Single/Asset")]
    [Button("Clean")]
    [PropertySpace(10, 10)]
    void Clean()
    {
        mMesh = null;
        UpdataData();
    }

    #region Size
    [VerticalGroup("Mode/Single/Edit")]
    [HideLabel]
    [ReadOnly]
    public Bounds Bounds;

    [BoxGroup("Mode/Single/Edit/Size", ShowLabel =false)]
    [PropertySpace(10, 0)]
    public float Size=1f;

    [Button("Resize")]
    [BoxGroup("Mode/Single/Edit/Size", ShowLabel = false)]
    [PropertySpace(0, 10)]
    public void Resize()
    {
        if (mMesh)
            mResize(mMesh);
        foreach (var mesh in Meshes)
        {
            mResize(mesh);
        }
    }

    public void mResize(Mesh mesh)
    {
        var length = mesh.vertexCount;
        var vertices = mesh.vertices;
        for (int i = 0; i < length; i++)
        {
            vertices[i] = vertices[i].Multiply(Size);
        }

        mesh.vertices = vertices;
        UpdateMesh(mesh);
    }

    #endregion


    #region Offset

    [BoxGroup("Mode/Single/Edit/Offset", ShowLabel = false)]
    [PropertySpace(10, 0)]
    public Vector3 Offset;

    [Tooltip("Get the center of bounds.")]
    [Button("GetCenter")]
    [BoxGroup("Mode/Single/Edit/Offset", ShowLabel = false)]
    public void SetCenter()
    {
        Offset = -Bounds.center;
    }

    [Tooltip("Move all vartices with offset.")]
    [Button("SetOffset")]
    [BoxGroup("Mode/Single/Edit/Offset", ShowLabel = false)]
    [PropertySpace(0, 10)]
    public void SetOffset()
    {
        var length = mMesh.vertexCount;
        var vertices = mMesh.vertices;
        for (int i = 0; i < length; i++)
        {
            vertices[i] = vertices[i] + Offset;
        }

        mMesh.vertices = vertices;
        UpdateMesh(mMesh);
    }


    #endregion

    #region Rotation


    [BoxGroup("Mode/Single/Edit/Rotate", ShowLabel = false)]
    public Vector3 Rotation;
    [Tooltip("Rotate all vartices from pivot with rotation.")]
    [BoxGroup("Mode/Single/Edit/Rotate", ShowLabel = false)]
    [Button("Rotate")]
    [PropertySpace(0, 10)]
    public void Rotate()
    {
        var matrix = UnityEngine.Matrix4x4.TRS(Vector3.zero, Quaternion.Euler(Rotation), Vector3.one);

        if (mMesh)
        {
            var length = mMesh.vertexCount;
            var vertices = mMesh.vertices;

            for (int i = 0; i < length; i++)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }

            mMesh.vertices = vertices;
            UpdateMesh(mMesh);
        }


        foreach (var mesh in Meshes)
        {
            var length = mesh.vertexCount;
            var vertices = mesh.vertices;

            for (int i = 0; i < length; i++)
            {
                vertices[i] = matrix.MultiplyPoint(vertices[i]);
            }

            mesh.vertices = vertices;
            UpdateMesh(mesh);
        }


    }

    #endregion

    #region SkinRenderer



    [PropertySpace(10, 0)]
    [BoxGroup("Mode/Single/Edit/Binding", ShowLabel = false)]
    public SkinnedMeshRenderer renderer;

    public Transform NewRoot;
    bool hasRenderer => renderer;

    [EnableIf(nameof(hasRenderer))]
    [PropertySpace(0, 10)]
    [Tooltip("Reset all bindposes with world matrix.")]
    [BoxGroup("Mode/Single/Edit/Binding", ShowLabel = false)]
    [Button("BindBone")]
    void RebindMesh()
    {
        Rebind(renderer,mMesh?mMesh:renderer.sharedMesh?renderer.sharedMesh:null);
    }
    [ShowInInspector]
    public Transform[] Bones;

    [EnableIf(nameof(hasRenderer))]
    [PropertySpace(0, 10)]
    [Tooltip("Reset all bindposes with world matrix.")]
    [BoxGroup("Mode/Single/Edit/Binding", ShowLabel = false)]
    [Button("SwitchRendererBone")]
    void SwitchRendererBones()
    {
        SwitchBone(renderer);
    }

    [EnableIf(nameof(hasRenderer))]
    [PropertySpace(0, 10)]
    [Tooltip("Reset all bindposes with world matrix.")]
    [BoxGroup("Mode/Single/Edit/Binding", ShowLabel = false)]
    [Button("Debug_MSG")]
    void Debug_MSG()
    {
        //if (renderer.bones == null)
            Debug.Log(renderer.bones);
        //if (renderer.bones.Length == 0)
            Debug.Log(renderer.bones.Length);
        renderer.bones = renderer.rootBone.GetComponentsInChildren<Transform>();

        Debug.Log(string.Join("\n", renderer.bones.Select(x => x.name)));
        
        Debug.Log(string.Join("\n", renderer.sharedMesh.boneWeights.Select(x => string.Join('*',x.boneIndex0,x.boneIndex1,x.boneIndex2,x.boneIndex3))));
    }

    [EnableIf(nameof(hasRenderer))]
    [PropertySpace(0, 10)]
    [Tooltip("Reset all bindposes with world matrix.")]
    [BoxGroup("Mode/Single/Edit/Binding", ShowLabel = false)]
    [Button("ADDBone")]
    void ADDBone()
    {
        renderer.bones = renderer.rootBone.GetComponentsInChildren<Transform>();
    }

    #endregion

    #endregion

    #region Batch

    [TabGroup("Mode", "Batch")]
    [FolderPath]
    [ShowInInspector]
    [PropertyOrder(-1)]
    string nPath { get => Path; set => Path=value; }

    [HideLabel]
    [BoxGroup("Mode/Batch/Mesh",ShowLabel =false)]
    public List<Mesh> Meshes=new List<Mesh>();

    [PropertySpace(0, 10)]
    [Tooltip("Reset all bindposes with world matrix.")]
    [BoxGroup("Mode/Batch/Mesh", ShowLabel = false)]
    [Button("RebindViaMesh")]
    void RebindMeshesViaMeshs()
    {
        if (renderers.Count == 0)
            return;

        var rd = renderers[0];
        if (!rd)
            return;

        Rebind(rd, Meshes.ToArray());
    }



    [HideLabel]
    [PropertySpace(10, 0)]
    [BoxGroup("Mode/Batch/Binding", ShowLabel = false)]
    public List<SkinnedMeshRenderer> renderers;

    [BoxGroup("Mode/Batch/Binding", ShowLabel = false)]
    [Button("ExtractMeshViaRenderers")]
    void ExtractViaRenderers()
    {
        foreach (var renderer in renderers)
        {
            var newMesh = mArchiveMesh(renderer.sharedMesh);
            renderer.sharedMesh = newMesh;
        }
    }


    [PropertySpace(0, 10)]
    [Tooltip("Reset all bindposes with world matrix.")]
    [BoxGroup("Mode/Batch/Binding", ShowLabel = false)]
    [Button("RebindViaRenderer")]
    void RebindMeshesViaRenderer()
    {
        foreach (var renderer in renderers)
        {
            Rebind(renderer,renderer.sharedMesh);
        }
    }


    [PropertySpace(0, 10)]
    [Tooltip("Reset all bones and bindposes.")]
    [BoxGroup("Mode/Batch/Binding", ShowLabel = false)]
    [Button("SwitchRenderersBone")]
    void SwitchRenderersBone()
    {
        foreach (var renderer in renderers)
        {
            SwitchBone(renderer);
        }
    }


    #endregion

    void Rebind(SkinnedMeshRenderer renderer,params Mesh[] meshs)
    {
        if (meshs == null)
            return;


        //var tfs = renderer.bones;

        //var length = tfs.Length;

        //var tf = renderer.transform;
        //var rendererMat = tf.localToWorldMatrix;
        //var inverse = Matrix4x4.Inverse(rendererMat);
        //foreach (var mesh in meshs)
        //{
        //    var oBindPoses = mesh.bindposes;
        //    Debug.Log(oBindPoses.Length == length);
        //    for (int i = 0; i < length; i++)
        //    {
        //        var bindpose = inverse*oBindPoses[i] ;
        //        var rotMat = Matrix4x4.Rotate(Quaternion.Inverse(tfs[i].rotation) * oBindPoses[i].rotation);
        //        bindpose = rotMat*bindpose;
        //        oBindPoses[i] = bindpose* rendererMat;
        //    };
        //    //-----------
        //    mesh.bindposes = oBindPoses;
        //    UpdateMesh(mesh);
        //}


        var tfs = renderer.bones;
        var length = tfs.Length;
        var bindPose = new Matrix4x4[length];
        var tf = renderer.transform;
        for (int i = 0; i < length; i++)
        {
            bindPose[i] = tfs[i].worldToLocalMatrix * tf.localToWorldMatrix;

        }

        foreach (var mesh in meshs)
        {

            mesh.bindposes = bindPose;
            UpdateMesh(mesh);
        }
    }


    void UpdateMesh(Mesh mesh)
    {
        if (!mesh)
            return;
        mesh.RecalculateNormals();
        mesh.RecalculateTangents();
        mesh.RecalculateBounds();
        UnityEditor.EditorUtility.SetDirty(mesh);
        UpdataData();
    }

    void UpdataData()
    {
        if (mMesh == null)
        {
            Bounds = default;
        }
        else
        {
            Bounds = mMesh.bounds;

        }

    }

    [PropertySpace(10, 10)]
    [BoxGroup("Edit", ShowLabel = false)]
    [Tooltip("Save mesh as new asset.")]
    [Button("ArchiveMesh", ButtonSizes.Large)]
    public void ArchiveMesh()
    {

        /* var newMesh = new Mesh();
         newMesh.vertices = mMesh.vertices;
         newMesh.triangles = mMesh.triangles;
        */
        if (mMesh)
            mMesh = mArchiveMesh(mMesh);

        var newMeshs = new List<Mesh>();
        foreach (var mesh in Meshes)
        {
            newMeshs.Add(mArchiveMesh(mesh));
        }

        Meshes.Clear();
        Meshes.AddRange(newMeshs);
    }

    Mesh mArchiveMesh(Mesh mesh)
    {
        var newMesh = Mesh.Instantiate(mesh);
        EditorHelper.WriteFile(newMesh, typeof(Mesh), Path + '/' + mesh.name);// EditorUtilityTools.OpenFolderPanel("Archive m_Path", "", ""));
        return newMesh;
    }

    void SwitchBone(SkinnedMeshRenderer renderer)
    {
        var bones = renderer.bones;
        var length = bones.Length;
        var dic = new Dictionary<string, int>(length);

        for (int i = 0; i < length; i++)
            dic.Add(bones[i].name, i);


        var mesh = renderer.sharedMesh;
        var weights = mesh.boneWeights;
        var bindPoses = mesh.bindposes;

        //Debug.Log(weights.Length);

        var newBones = renderer.rootBone.GetComponentsInChildren<Transform>().Where(x => dic.ContainsKey(x.name)).ToArray();

        //Debug.Log(newBones.Length + "**" + length + "**" + bindPoses.Length);


        var tf = renderer.transform;
        var newBindPoses = new Matrix4x4[length];

        var newboneArray = new Transform[length];

        for (int i = 0; i < length; i++)
        {
            if (dic.TryGetValue(newBones[i].name, out var sn))
            {
                newboneArray[sn] = newBones[i];
                newBindPoses[sn] = newBones[i].worldToLocalMatrix * tf.localToWorldMatrix;
                //Debug.Log(newBones[i].name == bones[sn].name ? true + ":" + newBones[i].name : newBones[i].name + bones[sn].name);
            }
        }

        for (int i = 0; i < length; i++)
        {
            if (!newboneArray[i].name.Equals(renderer.bones[i].name))
                Debug.LogError(newboneArray[i] + "-" + renderer.bones[i]);
        }
        renderer.bones = newboneArray;
        mesh.bindposes = newBindPoses;
        UpdateMesh(mesh);
    }
}
