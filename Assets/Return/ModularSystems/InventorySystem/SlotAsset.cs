using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Return.Database;
using Sirenix.OdinInspector;
using System;

public class SlotAsset : DataEntity
{
    public enum BindType { Transform,Bone}

    public BindType Type;
    public string SlotName;
    public PR Offset;

    /// <summary>
    /// Creat an slot via preset of this asset 
    /// </summary>
    public InvSlot LoadSlot(Transform root)
    {
        Transform tf;
        switch (Type)
        {
            case BindType.Transform:
                tf=root.Find(SlotName);
                if (!tf)
                    goto default;

                break;
            case BindType.Bone:
                if (!Enum.TryParse(SlotName, out HumanBodyBones bone))
                    goto default;
                if (!root.TryGetComponentInChildren(out Animator animator))
                    goto default;

                tf = animator.GetBoneTransform(bone);
                if (!tf)
                    goto default;

                break;
            default:
                throw new KeyNotFoundException(Type.ToString());
        }


        return CreateSlot(tf);
    }

    [Button("Create")]
    public virtual InvSlot CreateSlot(Transform tf)
    {
        var slot= new GameObject(SlotName);
        slot.transform.SetParent(tf);
        slot.transform.SetLocalPR(Offset);

        return slot.AddComponent<InvSlot>();
    }
}
