using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;


namespace Return.Items.Weapons
{
    public sealed class TrajectoryWhiteBoard : SingletonMono<TrajectoryWhiteBoard>
    {
        public static void RegisterBullet()
        {
            Instance.Register();
        }

        private void Register()
        {

        }
    }
}