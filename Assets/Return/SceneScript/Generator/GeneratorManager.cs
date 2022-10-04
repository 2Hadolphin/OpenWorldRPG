using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.SceneModule
{
    public class GeneratorManager : SceneModule
    {
        [System.Serializable]
        public struct refer
        {
            public AssetsPool SAP;
            public DynamicAssetsPool DAP;

            public ActiveZoneManager AZM;
            public ZoneGenerator GZ;
            public SpaceGenerator GS;
            public ResourceGenerator GR;
            public ObjectGenerator GO;



            public PlayerGenerator GP;
            public ItemGenerator GI;
            public VehicleGenerator GV;
            public AgentGenerator GA;
        }
        [SerializeField]
        private refer reference;

        public refer Ref { get { return reference; } }

        public SCMAData scmaData { get { return data; } }
        public SCMA scma { get { return Manager; } }

        public override void Init()
        {
            Ref.GZ.Initialization(this); // inhert
        }

        public void BuildScene()
        {
            Ref.DAP.Init();
            Ref.AZM.Init();
            Ref.GS.Initialization();
            Ref.GR.Initialization();
            Ref.GO.Initialization();
            Ref.GI.Initialization();
            Ref.GA.Initialization();
            Ref.GV.Initialization();
            Ref.GP.Initialization();


            
            Ref.GZ.Generate();
            Ref.GI.Generate();
            /*
            Ref.GS.Generate();
            Ref.GR.Generate();
            Ref.GO.Generate();
            Ref.GI.Generate();
            Ref.GA.Generate();
            Ref.GV.Generate();
            Ref.GP.Generate();
            */
        }
        public void LoadHost()
        {
            Ref.GP.GenerateLocalHost();
        }

        private void LoadSpace()
        {
            Ref.GS.Generate();
        }
        private void LoadZone()
        {
            Ref.GZ.Generate();
        }
        private void LoadResource()
        {
            Ref.GR.Generate();
        }
        private void LoadObject()
        {
            Ref.GO.Generate();
        }

        private void LoadItems()
        {
            Ref.GI.Generate();
        }

        private void LoadAgents()
        {
            Ref.GA.Generate();
        }

        private void LoadVehicles()
        {
            Ref.GV.Generate();
        }

        private void LoadPlayer() //? generate onlineplayers
        {
            Ref.GP.Generate();
        }




    }
}