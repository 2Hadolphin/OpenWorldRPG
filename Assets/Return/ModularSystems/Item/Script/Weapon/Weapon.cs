using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    //public class Weapon : ItemData
    //{
    //    [SerializeField]
    //    protected WeaponSystem System;

    //    public WeaponSystem getSystem { get { return System; } }
    //    public enum WeaponStyle { Arms, Melee, Tacticle }
    //    public WeaponStyle Style { get; private set; }
    //    public bool getCentral(out IWeaponCentral I) 
    //    {
    //        I = System.getCentral;
    //        if (I != null)
    //            return true;
    //        return false;
    //    }

    //    public override ReadOnlyTransform Handle
    //    { 
    //        get
    //        {
    //            return System.getCentral.IKHandle;
    //        } 
    //    }
    //    public override ReadOnlyTransform parameterHandle
    //    {
    //        get
    //        {
    //            return System.getCentral.pamaterIKHandle;
    //        }
    //    }


    //    public override void SetSlot(out Vector3 pos, out Quaternion rot)
    //    {
    //        ReadOnlyTransform tf = System.getCentral.IKHandle;
    //        pos = this.transform.position-tf.position;
    //        rot = tf.localRotation;
    //    }
    //    private void Awake()
    //    {
    //        // Debug Initi
    //        State = ItemRef.State.Static;
    //        Type = m_category.Weapon;
    //        storgeRef = new ItemRef.StorageRef { volume = 1, weight = 1 };
    //        Style = WeaponStyle.Arms;

    //        Data_Storage = new ItemRef.GUIData { Sprite = Data_Active.Sprite, Name = Data_Active.NickName };
    //        if (targetTF == null)
    //            targetTF = this.transform;
    //    }



    //    public override bool Access() 
    //    {
    //        return false;
    //    }

    //    public override bool SetHandler(Return.Humanoid.IPlayerFroegin user)
    //    {
    //        base.SetHandler(user);
    //        return true;
    //    }

    //    public override void PrepareInitilization()
    //    {
    //        base.PrepareInitilization();
    //        System.Initialization(this);
    //    }


    //}
}