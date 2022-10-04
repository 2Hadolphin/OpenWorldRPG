using Assets.Scripts.Item.Weapon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Items.Weapons
{
    [RequireComponent(typeof(WeaponCentral))]
    public class WeaponIO : WeaponSystemMethods
    {
        [SerializeField]
        private WeaponIORef IORefs;
        [SerializeField]
        private WeaponElementRef ElementRefs;
        public WeaponElementRef ElementRef { get { return ElementRefs; } }
        [SerializeField]
        private AudioClip[] audioClips=null;
        public AudioClip[] Clips { get { return audioClips; } }

        private IWeaponBase central;
        public void SetATKValue()
        {
           
        }

        public virtual bool Initialization(IWeaponBase central)
        {
            if (central == null)
            {
                gameObject.SetActive(false);
                Debug.LogError("Weapon System Error ! : " + this.name);
                return false;
            }
            return true;
        }



        public void InitializationData()
        {
            ResetData();
        }

        private void ResetData()
        {
            
        }

        public void PostMagState()
        {
            // ? set ui last bullet numbers
        }

        public void PostDamageEvent(IDamageState port, DamageState.Package package)
        {
            port.ReceiveDamage(package);
        }
        /*
        private void DrawShootLine()
        {
            rows.positionCount = 2;

            if (!ImpactPoint.HasValue || !PenetractionPoint.HasValue)
            {
                rows.SetPosition(0, Chamber.transform.position);
                rows.SetPosition(1, EndPoint);
            }
            else
            {
                ImpactPoints.Add(EndPoint);
                rows.positionCount = ImpactPoints.Count;
                rows.SetPositions(ImpactPoints.ToArray());
            }
        }
        */

        /*
        private void DrawBallisticGizmos()
        {
            Perform();
            if (!ImpactPoint.HasValue || !PenetractionPoint.HasValue)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(Chamber.transform.position, EndPoint);
                //print("Null Target");
            }
            else
            {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(Chamber.transform.position, ImpactPoints[0]);
                Gizmos.color = Color.white;
                for (int i = 1; i < ImpactPoints.Count; i++)
                {
                    Gizmos.DrawLine(ImpactPoints[i - 1], ImpactPoints[i]);
                }
                Gizmos.color = Color.red;
                Gizmos.DrawLine(ImpactPoint.m_Value, PenetractionPoint.m_Value);
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(PenetractionPoint.m_Value, EndPoint);
            }
            ImpactPoints.Clear();
            End.transform.position = ray.origin;
        }
        */
    }
}