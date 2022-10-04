using UnityEngine;
using System;

namespace Return.Items.Weapons
{
    [Serializable]
    public struct FireModeBinding
    {

        public FireMode FireMode;

        [Tooltip("Fire in rapid, normally sholud be 1. (not shotgun)")]
        public int Burst;
 
        [Tooltip("Interval time between shots ( Include pulling hamer)")]
        public float Rof;
    }
}