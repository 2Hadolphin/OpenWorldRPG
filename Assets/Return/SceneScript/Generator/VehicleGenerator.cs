using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.SceneModule
{
    public class VehicleGenerator : AssetsGenerator
    {
        public override void Generate()
        {

            Build();

        }

        private void Build()
        {
            int randomVehicle = Random.Range(0, manager.Ref.SAP.Assets.Vehicles.Length);
            int randomZone = Random.Range(0, manager.Ref.AZM.Volume);
            GameObject vehicle = Instantiate(manager.Ref.SAP.Assets.Vehicles[randomVehicle], Vector3.zero, Quaternion.Euler(0, Random.Range(-360, 360), 0));
            vehicle.SetActive(true);
            //print("Creat " + AP.Vehicles[randomVehicle].name + " On " + GZC.VehicleZone[randomZone].transform.parent.name);
        }

        public override void Initialization()
        {

        }


    }
}