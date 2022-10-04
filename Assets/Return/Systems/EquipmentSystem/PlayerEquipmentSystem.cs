using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Return.Humanoid;
//using Return.Humanoid.Animation;
//public class PlayerEquipmentSystem : HumanoidModularSystem
//{
//    private Gear[] Equipments=new Gear[0];
//    private Animator anim;

    
//    public void Init(HumanoidAgent_Exhibit reference)
//    {
//        //anim = (Agent.I.mAniamator as IAnimator).GetAnimator;

//    }
//    private bool CheckWearable(GearStyle style)
//    {
//        switch (style)
//        {
//            case GearStyle.Glasses:
//                return IsExist(style);
//            case GearStyle.Hat:
//                return IsExist(style);
//            case GearStyle.Up:
//                break;
//            case GearStyle.Bottom:
//                break;
//            case GearStyle.Shoes:
//                return IsExist(style);
//            case GearStyle.Armor:
//                return IsExist(style);
//            case GearStyle.InventoryHandler:
//                break;
//        }
//        return true;
//    }

//    private bool IsExist(GearStyle stytle)
//    {
//        int length = Equipments.Length;
//        for(int i = 0; i < length; i++)
//        {
//            if (Equipments[i].Style.Equals(stytle))
//                return true;
//        }
//        return false;
//    } 

//    public bool Wear(Gear gear)
//    {
//        if (!CheckWearable(gear.Style))
//            return false;

//        var item=gear.LoadModule(CharacterRoot);
//        ReadOnlyTransform tf;
//        switch (gear.Style)
//        {
//            case GearStyle.Glasses:
//                break;
//            case GearStyle.InventoryHandler:
//                //tf = item.transform;
//                //tf.SetParent(anim.GetBoneTransform(HumanBodyBones.Chest), false);
//                //tf.localPosition = Vector3.zero;
//                //set pos
//                break;
//            case GearStyle.Hat:
//                break;
//            case GearStyle.Up:
//                break;
//            case GearStyle.Bottom:
//                break;
//            case GearStyle.Shoes:
//                break;
//            case GearStyle.Armor:
//                break;
//        }

//        Add(gear);
//        return true;
//    }

//    private void Add(Gear gear)
//    {
//        List<Gear> old = new List<Gear>(Equipments.Length + 1);

//        old.AddRange(Equipments);
//        old.Add(gear);

//        Equipments = old.ToArray();
//    }

//    private void Remove(Gear gear)
//    {
//        List<Gear> old = new List<Gear>(Equipments.Length);

//        old.AddRange(Equipments);
//        if (old.Contains(gear))
//            old.Remove(gear);

//        Equipments = old.ToArray();
//    }

//}
