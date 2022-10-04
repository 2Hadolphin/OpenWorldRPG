using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Items;
using Sirenix.OdinInspector;
using System;
using Return.Database;

namespace Return.Items.Weapons
{

    /// <summary>
    /// Preset : Firearms
    /// </summary>
    [Serializable]
    [HideLabel]
    public class FirearmsInfo : Cloneable
    {
        #region UnityEditor
        protected const string FirearmsBasic_Box_Group = "FirearmsBasic";
        #endregion

        #region GENERAL

        [Tooltip("Effective Distance")]
        public float Range;


        /// <summary>
        /// Defines how fast this gun will be inaccurate due the character movement. (Read Only)
        /// </summary>
        public float DecreaseRateByWalking => m_DecreaseRateByWalking;

        /// <summary>
        /// Defines how fast this gun will be inaccurate due constant shooting. (Read Only)
        /// </summary>
        public float DecreaseRateByShooting => m_DecreaseRateByShooting;

        #endregion




        #region Melee Module **delete
        /// <summary>
        /// The gun size. (Used to define how far the character can hit with a melee attack)
        /// </summary>
        [SerializeField]
        [Range(0.001f, Mathf.Infinity)]
        [Tooltip("The gun size. (Used to define how far the character can hit with a melee attack)")]
        public float m_Size = 1;
        /// <summary>
        /// Defines how much damage will be inflict by a melee attack.
        /// </summary>
        /// set as melee modules
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how much damage will be inflict by a melee attack.")]
        private float m_MeleeDamage = 50;
        /// <summary>
        /// Defines how much force will be applied when melee attack.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how much force will be applied when melee attack.")]
        public float MeleeForce = 5;

        #endregion


        #region MAGAZINE 

        /// <summary>
        /// The ammo type defines the characteristics of the projectiles utilized by a gun.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("The ammo type defines the characteristics of the projectiles utilized by a gun.")]
        private AmmunitionData m_AmmoType;

        /// <summary>
        /// Defines how the gun is loaded with ammo.
        /// </summary>
        [SerializeField]
        [Tooltip("Defines how the gun is loaded with ammo.")]
        private ReloadMode m_ReloadMode = ReloadMode.Magazines;

        /// <summary>
        /// Defines how many bullets has in the magazine.
        /// </summary>
        [SerializeField]
        [Range(0, Mathf.Infinity)]
        [Tooltip("Defines how many bullets has in the magazine.")]
        private int m_RoundsPerMagazine = 30;

        /// <summary>
        /// Enabling the chamber will add an additional bullet to your gun.
        /// </summary>
        [SerializeField]
        [Tooltip("Enabling the chamber will add an additional bullet to your gun.")]
        private bool m_HasChamber;

        #endregion




        #region ACCURACY

        /// <summary>
        /// Sets the radius of the conical fustrum. Used to calculate the bullet spread angle.
        /// </summary>
        [SerializeField]
        [Range(0.001f, Mathf.Infinity)]
        [Tooltip("Sets the radius of the conical fustrum. Used to calculate the bullet spread angle.")]
        private float m_MaximumSpread = 1.75f;

        /// <summary>
        /// Defines the minimum accuracy while using this gun. (If the character is moving or holding the trigger it will lose accuracy until it reaches the Base Accuracy)
        /// </summary>
        [SerializeField]
        [Range(0.01f, 1)]
        [Tooltip("Defines the minimum accuracy while using this gun. (If the character is moving or holding the trigger it will lose accuracy until it reaches the Base Accuracy)")]
        private float m_BaseAccuracy = 0.75f;

        /// <summary>
        /// Defines the accuracy percentage when the character is shooting from the hip. (0 is totally inaccurate and 1 is totally accurate)
        /// </summary>
        [SerializeField]
        [Range(0.01f, 1)]
        [Tooltip("Defines the accuracy percentage when the character is shooting from the hip. (0 is totally inaccurate and 1 is totally accurate)")]
        private float m_HIPAccuracy = 0.6f;

        /// <summary>
        /// Defines the accuracy percentage when the character is aiming. (0 is totally inaccurate and 1 is totally accurate)
        /// </summary>
        [SerializeField]
        [Range(0.01f, 1)]
        [Tooltip("Defines the accuracy percentage when the character is aiming. (0 is totally inaccurate and 1 is totally accurate)")]
        private float m_AIMAccuracy = 0.9f;

        /// <summary>
        /// Defines how fast this gun will be inaccurate due the character movement.
        /// </summary>
        [SerializeField]
        [Range(0, 3)]
        [Tooltip("Defines how fast this gun will be inaccurate due the character movement.")]
        private float m_DecreaseRateByWalking = 1;

        /// <summary>
        /// Defines how fast this gun will be inaccurate due constant shooting.
        /// </summary>
        [SerializeField]
        [Range(0, 3)]
        [Tooltip("Defines how fast this gun will be inaccurate due constant shooting.")]
        private float m_DecreaseRateByShooting = 1;

        #endregion

     

        public class WeaponData
        {
            public float Damage;
            public float Range;
            public float Scope = 1.2f;
            public AudioClip Effect_Muzzle;
            public AudioClip Effect_Reload;
            public ParticleSystem Muzzle;
            public ParticleSystem Shell;
            public LayerMask DamageMask;
            public int Amount;
            public List<GameObject> UI;
            public float FireRate = 0.1f;
        }
    }
}