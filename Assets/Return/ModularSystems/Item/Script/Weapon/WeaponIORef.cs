using UnityEngine;
using System;
using Return.mGUI;

namespace Return.Items.Weapons
{

    public class WeaponIORef
    {
        [SerializeField]
        public WeaponGUI ui = null;
        [SerializeField]
        public AudioSource gunshoot = null;
        [SerializeField]
        public Camera Cam = null;
        [SerializeField]
        public LayerMask mask;
        [SerializeField]
        public ParticleSystem muzzle = null;
        [SerializeField]
        public GameObject Mag = null;

    }
    
}