using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using Return;
using System.Linq;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using Return.Editors;

public class SlotCreator : OdinEditorWindow
{

    [MenuItem("Tools/Character/BindSlot")]
    private static void OpenWindow()
    {
        GetWindow<SlotCreator>().position = GUIHelper.GetEditorWindowRect().AlignCenter(200, 450);
    }

    protected override void OnDestroy()
    {
        DestorySlot();
    }

    [FolderPath]
    public string Path= "Assets/Return";

    [OnValueChanged(nameof(Init))]
    public Transform Root;
    public bool DestoryPrefabChange=true;

    [ShowInInspector]
    public Dictionary<InvSlot,SlotAsset> Slots = new Dictionary<InvSlot, SlotAsset>();
    [ReadOnly]
    public Animator Animator;

    [HideInInspector]
    public Dictionary<Transform, HumanBodyBones> Catch=new ();

    void Init()
    {

        DestorySlot();

        Catch.Clear();
        Slots.Clear();
        if (Root)
        {
            Root.TryGetComponent(out Animator);
            if (Animator)
            {
                var bones = Return.HumanBodyBonesUtility.AllHumanBodyBones;
                foreach (var bone in bones)
                {
                    var tf = Animator.GetBoneTransform(bone);
                    if (tf)
                        Catch.Add(tf, bone);
                }
            }
        }



    }

    void DestorySlot()
    {
        if (DestoryPrefabChange)
            foreach (var slot in Slots)
                DestroyImmediate(slot.Key.gameObject);
    }
    [BoxGroup("Operate")]
    public string SlotName=nameof(InvSlot);

    [BoxGroup("Operate")]
    [Button("Add Slot")]
    public void Add()
    {
        var selected = Selection.activeTransform;

        if (!selected)
            return;
     
        var slot = new GameObject("_Slot").AddComponent<InvSlot>();
        slot.transform.SetParent(selected);


        var so=EditorAssetsUtility.CreateInstanceSO<SlotAsset>(false, Path, SlotName);

        if (Catch.TryGetValue(selected, out var bone))
        {
            so.Type = SlotAsset.BindType.Bone;
            so.SlotName = bone.ToString();
        }
        else
        {
            so.SlotName = selected.name;
        }

        

        Slots.Add(slot, so);
    }

    [Button("Apply")]
    public void ApplyData()
    {
        foreach (var slot in Slots)
        {
            slot.Value.Offset = slot.Key.transform.GetLocalPR();
            UnityEditor.EditorUtility.SetDirty(slot.Value);
        }
    }
}
