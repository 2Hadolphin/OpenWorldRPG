using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using Random = UnityEngine.Random;

namespace Return.Items.Weapons
{
    [Serializable]
    public partial class AmmunitionData : PresetDatabase
    {
        #region UnityEditor
        public const string Bullet_Box_Group = "Bullet";
        public const string Shell_Box_Group = "Shell";
        #endregion
        /// <summary>
        /// Name of the ammunition.
        /// </summary>
        [SerializeField]
        [Tooltip("Name of the ammunition.")]
        public string m_AmmunitionName = "Ammunition";

        #region Bulltet

        [BoxGroup(Bullet_Box_Group)]
        [Tooltip("Bullet diameter in millimeter.")]
        [Range(3,100)]
        public float Caliber;

        [BoxGroup(Bullet_Box_Group)]
        [Tooltip("Bullet's mass in kilograms.")]
        [Range(0.001f, 0.4f)]
        [LabelText("Mass")]
        public float Mass_projectile = 0.02f;

        [BoxGroup(Bullet_Box_Group)]
        [SerializeField]
        [Tooltip("Defines how many bullets will be fired.(Shotgun>1)")]
        [Range(1, 1024)]
        public int BulletsAmount;

        [BoxGroup(Bullet_Box_Group)]
        [AssetList]
        [SerializeField]
        public BulletCharacteristic[] BulletCharacteristic;


        /// <summary>
        /// Defines the density threshold so that a bullet can bounce off a surface.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines the hardness threshold so that a bullet can bounce off a surface.")]
        [Range(0.001f, Mathf.Infinity)]
        private float m_RicochetHardnessThreshold = 1;
        #endregion


        #region Shell

        [BoxGroup(Shell_Box_Group)]
        [Tooltip("Shell length in millimeter.")]
        [Range(15, 2500)]
        public float Cartridge;

        [BoxGroup(Shell_Box_Group)]
        [Tooltip("Shell's mass in kilograms.")]
        [Range(0.001f, 0.4f)]
        [LabelText("Mass")]
        public float Mass_Shell = 0.02f;

        [BoxGroup(Shell_Box_Group)]
        [Tooltip("Gunpowder charge in thrust to weight ratio (m/s).")]
        [Range(150, 2000)]
        public float GunpowderVolume;

        #endregion

        #region Ballistics

        /// <summary>
        /// Refraction is the effect of altering the bullet path after transferring energy on the collision.
        /// </summary>
        [SerializeField]
        public Vector2 m_Refraction = new Vector2(-0.5f, 0.5f);

        /// <summary>
        /// Defines how the kinetic energy will be calculated based on the distance. (Read Only)
        /// </summary>
        [SerializeField]
        public AnimationCurve EnergyFalloffCurve;

        [Tooltip("Penetration ration of angle from 0 to 90")]
        [SerializeField]
        public AnimationCurve IncidenceAnglePenetrationCurve;

        #endregion    
    }
}