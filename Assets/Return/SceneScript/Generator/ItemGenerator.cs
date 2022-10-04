using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.SceneModule
{
    public class ItemGenerator : AssetsGenerator
    {
        public override void Generate()
        {

                Build();
        }

        private void Build()
        {
            int randomitem = Random.Range(0, manager.Ref.SAP.Assets.Items.Length);
            int randomZone = Random.Range(0, manager.Ref.AZM.Volume);
            //print(this);
            //GameObject m_items = Instantiate(manager.Ref.SAP.Assets.m_items[randomitem], Vector3.zero, Quaternion.Euler(0, Random.Distance(-360, 360), 0));
            //m_items.AddComponent<ItemSiteRigidboty>();
            //print("Creat " + manager.Ref.SAP.Assets.m_items[randomitem].name + " On " + manager.Ref.GZ.GetZone(randomZone).transform.parent.name);
        }

        public override void Initialization()
        {
 
        }


    }
}