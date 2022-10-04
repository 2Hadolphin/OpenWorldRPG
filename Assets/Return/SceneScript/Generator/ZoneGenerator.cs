using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Return.SceneModule
{
    public class ZoneGenerator : AssetsGenerator
    {
        [SerializeField]
        private Data_RespawnFlag data_Respawn;
        [SerializeField]
        private GameObject Prefab;

        private SpawnFlag[] Flags;

        public GameObject GetZone(int sn)
        {
            if (sn < Flags.Length)
                return Flags[sn].gameObject;
            else
                return this.gameObject;
        }

        public override void Generate()
        {
            int length = data_Respawn.Flags.Length;
            Data_RespawnFlag.Flag data;
            Flags = new SpawnFlag[length];
            Transform tf = this.transform;
            for (int i = 0; i < length; i++)
            {
                data = data_Respawn.Flags[i];
                Flags[i] = Instantiate(Prefab,data.position, Quaternion.Euler(data.rotation)).GetComponent<SpawnFlag>();
                Flags[i].transform.SetParent(tf,true);
                Flags[i].Initialization(data);
            }
            print( length+" Respawn Flags Generate completed ! ");
        }

        public void Initialization(GeneratorManager m)
        {
            manager = m;
            pool = m.Ref.SAP;
            // Load Assets from path
        }

        public override void Initialization()
        {
            manager.Ref.DAP.Init();
        }

    }
}