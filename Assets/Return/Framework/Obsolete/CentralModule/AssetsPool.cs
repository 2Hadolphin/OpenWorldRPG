using UnityEngine;
using System;

namespace Return.SceneModule
{
    public class AssetsPool : MonoBehaviour
    {
        [Serializable]
        public struct AssetPool
        {
            public GameObject[] Items;
            public GameObject[] Vehicles;
            public GameObject[] Agent;
            public GameObject[] Player;
        }

        [SerializeField]
        private AssetPool assets;

        public AssetPool Assets { get { return assets; } }


    }
}