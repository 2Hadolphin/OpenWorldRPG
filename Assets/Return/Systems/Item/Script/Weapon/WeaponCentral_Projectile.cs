using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Return.Humanoid;
using Return.Definition;

namespace Return.Items.Weapons
{
    public class WeaponCentral_Projectile : WeaponCentral_Arm
    {
        [SerializeField]
        protected GameObject projectile = null;//get by mag

        protected List<IAccessed> Targets;
        protected bool isAiming;
        [SerializeField]
        protected bool deleteEmptyMag = false;// turn to mag

        public override void Initialization( )
        {
            Targets = new List<IAccessed>(5);
        }

        public override void Aim()
        {
            base.Aim();
            isAiming = true;
        }

        public override void Disaim()
        {
            base.Disaim();
            isAiming = false;
        }

        public override void WeaponMainUse()
        {
            if (isAiming)
            {
                caculator.Single();
            }
            else
            {
                Eject();
            }
        }

        public virtual void Eject()
        {
            GameObject ejected = Instantiate(projectile, io.ElementRef.Ejector.transform);
            ejected.name = "pj_";
            ejected.GetComponent<ProjectTileManager>().Activate();
            ejected.GetComponent<ProjectTileManager>().Launch();
        }


        public virtual void AddTarget(IAccessed target)
        {

        }
        public virtual void RemoveTarget(IAccessed target)
        {

        }
        public virtual void ClearTargets()
        {
            Targets.Clear();
        }
    }
}