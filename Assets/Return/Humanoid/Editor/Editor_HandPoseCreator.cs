using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Return;

public class Editor_HandPoseCreator : OdinEditorWindow
{
    [MenuItem("Tools/Animation/HandPoseCreator")]
    private static void OpenWindow()
    {
        GetWindow<Editor_HandPoseCreator>().position = GUIHelper.GetEditorWindowRect().AlignCenter(800, 600);
    }

    [OnValueChanged(nameof(LoadHand),InvokeOnInitialize =false)]
    public bool RightHand, LeftHand;


    public string ClipName;
    [FolderPath]
    public string Path;
    public AnimationClip Clip;

    [ReadOnly]
    public Material mat;

    protected override void OnEnable()
    {
        base.OnEnable();
        mat= new Material(     Shader.Find("Hidden/Internal-Colored") );
        mat.hideFlags = HideFlags.HideAndDontSave;
    }

    protected override void OnDestroy()
    {
        DestroyImmediate(mat);
        base.OnDestroy();

    }

    public Texture Image;
    public HandPose RightPose;

    [Button("CreateClip")]
    public void CreateClip()
    {
        var clip = new AnimationClip();
        Clip=EditorHelper.WriteAsset(clip, ClipName, Path);
    }
    

    public void LoadHand()
    {
        if (RightHand)
        {
            if (RightPose != null)
                return;

            var bones= Return.HumanBodyBonesUtility.RightArmBones;
            var tfs = new Transform[bones.Length];
            for (int i = 0; i < 4; i++)
            {
                var newBone = new GameObject(bones[i].ToString());
                tfs[i] = newBone.transform;
                if (i > 0)
                    tfs[i].SetParent(tfs[i - 1]);
            }
            RightPose = new HandPose() { Bones = tfs };
        }

    }
    [System.Serializable]
    public class HandPose
    {
        public Transform[] Bones;
    }

    protected override void OnGUI()
    {


        base.OnGUI();
        
        return;
        var rect = new Rect(new Vector2(0,400), new Vector2(100, 100));
        GUILayout.BeginArea(rect);
        if (Event.current.type == EventType.Repaint)
        {
            mat.SetPass(0);
            GLTexture(rect, Color.red);
        }
        GUILayout.EndArea();

        if (Event.current.type == EventType.MouseDown)
            Debug.Log(Event.current.mousePosition);
    }
    public static void GLTexture(Rect rect, Color color)
    {
        GL.PushMatrix();
        GL.LoadPixelMatrix();
        GL.Begin(GL.QUADS);
        GL.Color(color * 4);
        GL.Vertex3(rect.x, rect.y, 0);
        GL.Vertex3(rect.x, rect.y + rect.height, 0);
        GL.Vertex3(rect.x + rect.width, rect.y + rect.height, 0);
        GL.Vertex3(rect.x + rect.width, rect.y, 0);
        GL.End();
        GL.PopMatrix();
    }

    

}
