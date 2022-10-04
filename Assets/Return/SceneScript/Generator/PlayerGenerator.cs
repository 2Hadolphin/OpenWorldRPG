using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid;

namespace Return.SceneModule
{
    public class PlayerGenerator : AssetsGenerator
    {
        [SerializeField]
        private GameObject[] Player;
        public override void Generate()
        {

        }
        public void GenerateLocalHost()
        {
            //GameObject player = Instantiate(Player[0], manager.scmaData.storgeData.PlayerPos, manager.scmaData.storgeData.PlayerDir, null);
            //manager.scma.RegistePlayer(player.GetComponentInChildren<IPlayerFroegin>());
        }

        public override void Initialization()
        {

        }

    }
}