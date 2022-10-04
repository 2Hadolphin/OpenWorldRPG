using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using Return.Items.Weapons;
using Return.CentreModule;
//using Return.Vehicle;

public class WeaponCaculator_Projectile : WeaponCaculator
{
    private Transform CamTF;



    public virtual bool LockTarget(out GameObject target)
    {
        RaycastHit _hit;

        if (Physics.Raycast(CamTF.position, CamTF.forward, out _hit, central.data.CurrentData.Range))
        {
            print("We hit" + _hit.collider.name);
            if (_hit.collider.CompareTag("Damageable"))
            {
                target = _hit.transform.root.gameObject;
                return true;
            }
        }

        print("Target Null");
        target = null;
        return false;
    }
    public override void Multi()
    {
        throw new System.NotImplementedException();
    }

    public override void CheckMotion()
    {
        throw new System.NotImplementedException();
    }

    public override bool Initialization(IWeaponBase central)
    {
        if (central == null)
        {
            gameObject.SetActive(false);
            return false;
        }

        FireRate = 1 / (central.data.CurrentData.FireRate / 60);


        if (central.io.ElementRef.Barrel != null)
        {
            chamber = central.io.ElementRef.Barrel.transform;
        }
        else
        {
            Debug.LogError("Weapon Missing Chamer Slot !");
        }

        if (central.io.ElementRef.Sight != null)
        {
            CamTF = central.io.ElementRef.Barrel.transform;
        }
        else
        {
            Debug.LogError("Weapon Missing Chamer Slot !");

        }




        return false;

    }

    public override void MathfOperate()
    {
        throw new System.NotImplementedException();
    }


    public override void Single()
    {
        throw new System.NotImplementedException();
    }
}
