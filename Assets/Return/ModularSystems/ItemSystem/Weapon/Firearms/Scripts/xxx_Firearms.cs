using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using UnityEngine.InputSystem;
using Return.Agents;
using System;
using Random = UnityEngine.Random;
using Return.Items;
using Return.Motions;

namespace Return.Items.Weapons
{


    public class xxx_Firearms : Weapon
    {

       



        //--------------------------------------------------------------------------------------

        /// <summary>
        /// Defines the reference to the inventory system.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Defines the reference to the inventory system.")]
        private xxx_WeaponUsingSystem m_InventoryManager;

        /// <summary>
        /// Defines the reference to the character¡¦s Main Camera transform.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Defines the reference to the character¡¦s Main Camera transform.")]
        private Transform m_CameraTransformReference;

        /// <summary>
        /// Defines the reference to the First Person Character Controller.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Defines the reference to the First Person Character Controller.")]
        private MotionSystem_Humanoid m_FPController;

        /// <summary>
        /// Defines the reference to the Camera Animator.
        /// </summary>
        [SerializeField]
        [Required]
        [Tooltip("Defines the reference to the Camera Animator.")]
        private CameraAnimator m_CameraAnimationsController;

        /// <summary>
        /// The WeaponSwing component.
        /// </summary>
        [SerializeField]
        private WeaponSwing m_WeaponSwing = new WeaponSwing();

        /// <summary>
        /// The MotionAnimation component.
        /// </summary>
        [SerializeField]
        private MotionAnimation m_MotionAnimation = new MotionAnimation();

        [SerializeField]
        private WeaponKickbackAnimation m_WeaponKickbackAnimation;

        [SerializeField]
        private WeaponKickbackAnimation m_AimingWeaponKickbackAnimation;

        [SerializeField]
        private CameraKickbackAnimation m_CameraKickbackAnimation;

        [SerializeField]
        private CameraKickbackAnimation m_AimingCameraKickbackAnimation;

        /// <summary>
        /// The GunAnimator component.
        /// </summary>
        [SerializeField]
        private GunAnimator m_GunAnimator = new GunAnimator();

        /// <summary>
        /// The GunEffects component.
        /// </summary>
        [SerializeField]
        private GunEffects m_GunEffects = new GunEffects();

        private bool m_GunActive;
        private bool m_Aiming;
        private bool m_IsReloading;
        private bool m_Attacking;

        private WaitForSeconds m_ReloadDuration;
        private WaitForSeconds m_CompleteReloadDuration;

        private WaitForSeconds m_StartReloadDuration;
        private WaitForSeconds m_InsertInChamberDuration;
        private WaitForSeconds m_InsertDuration;
        private WaitForSeconds m_StopReloadDuration;

        private float m_FireInterval;
        private float m_NextFireTime;
        private float m_NextReloadTime;
        private float m_NextSwitchModeTime;
        private float m_NextInteractTime;
        private float m_Accuracy;

        private Camera m_Camera;
        private float m_IsShooting;
        private Vector3 m_NextShootDirection;

        private InputActionMap m_InputBindings;
        private InputAction m_FireAction;
        private InputAction m_AimAction;
        private InputAction m_ReloadAction;
        private InputAction m_MeleeAction;
        private InputAction m_FireModeAction;

        #region EDITOR

        /// <summary>
        /// Returns the ReloadMode used by this gun.
        /// </summary>
        public ReloadMode ReloadType => default;// m_data != null ? m_data.ReloadType : ReloadMode.Magazines;


        /// <summary>
        /// Returns true if this gun has a chamber, false otherwise.
        /// </summary>
        public bool HasChamber => default; //m_data != null && m_data.HasChamber;

        /// <summary>
        /// Returns the name of the weapon displayed on the Inspector tab.
        /// </summary>
        public string InspectorName => Preset != null ? Preset.Name : "No Name";

        #endregion

        #region GUN PROPERTIES

        /// <summary>
        /// Returns true if this weapon is ready to be replaced, false otherwise.
        /// </summary>
        public virtual bool CanSwitch
        {
            get
            {
                if (!m_FPController)
                    return false;

                return m_GunActive && m_NextSwitchModeTime < Time.time && !m_Attacking && m_NextInteractTime < Time.time && m_NextFireTime < Time.time;
            }
        }

        /// <summary>
        /// Returns true if the character is not performing an action that prevents him from using items, false otherwise.
        /// </summary>
        public virtual bool CanUseEquipment
        {
            get
            {
                if (!m_FPController)
                    return false;

                return
                    m_GunActive
                    && !IsAiming
                    && !m_IsReloading
                    && m_NextReloadTime < Time.time
                    && m_NextFireTime < Time.time
                    && m_NextSwitchModeTime < Time.time
                    && !m_Attacking
                    && m_NextInteractTime < Time.time;
                //&& m_FPController.State != ObsoleteMotionState.Running
            }
        }

        /// <summary>
        /// Returns true if the character is performing an action that prevents him from vaulting, false otherwise.
        /// </summary>
        public virtual bool IsBusy
        {
            get
            {
                if (!m_FPController)
                    return true;

                return !m_GunActive || IsAiming || m_IsReloading || m_NextReloadTime > Time.time || m_NextFireTime > Time.time
                || m_NextSwitchModeTime > Time.time || m_Attacking || m_NextInteractTime > Time.time;
            }
        }

        /// <summary>
        /// Returns true if the character is aiming with this weapon, false otherwise.
        /// </summary>
        public virtual bool IsAiming => m_GunAnimator.IsAiming;

        /// <summary>
        /// Returns the maximum number of rounds a magazine can hold.
        /// </summary>
        public int RoundsPerMagazine
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the current number of rounds that are in the magazine coupled to the gun.
        /// </summary>
        public int CurrentRounds
        {
            get;
            protected set;
        }

        /// <summary>
        /// 
        /// </summary>
        public FirearmsInfo GunData => null;


        /// <summary>
        /// Return the name of the gun.
        /// </summary>
        public string GunName => Preset != null ? Preset.Name : "No Name";

        /// <summary>
        /// Returns the current accuracy of the gun.
        /// </summary>


        /// <summary>
        /// Returns the selected fire mode on the gun.
        /// </summary>
        public FireMode FireMode
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns the viewmodel (GameObject) of this gun.
        /// </summary>
        public GameObject Viewmodel => gameObject;

        /// <summary>
        /// Returns the dropped object when swapping the gun.
        /// </summary>
        public GameObject DroppablePrefab => Preset != null ? Preset.Model : null;




        /// <summary>
        /// Returns the length of the gun.
        /// </summary>
        public float Size => 0;

        /// <summary>
        /// 
        /// </summary>
        public float HideAnimationLength => m_GunAnimator.HideAnimationLength;

        public float DrawAnimationLenght => m_GunAnimator.DrawAnimationLength;

        public float InteractAnimationLength => m_GunAnimator.InteractAnimationLength;
        public float InteractDelay => m_GunAnimator.InteractDelay;

        public bool Reloading => m_IsReloading || m_NextReloadTime > Time.time;
        public bool Firing => m_NextFireTime > Time.time;
        public bool MeleeAttacking => m_Attacking;
        public bool Interacting => m_NextInteractTime > Time.time;

        public bool Idle => !Reloading && !Firing && !MeleeAttacking && !Interacting;

        public bool OutOfAmmo => CurrentRounds == 0;

        #endregion

       

        #region WEAPON CUSTOMIZATION

        public void UpdateAiming(Vector3 aimingPosition, Vector3 aimingRotation, bool zoomAnimation = false, float aimFOV = 50)
        {
            m_GunAnimator.UpdateAiming(aimingPosition, aimingRotation, zoomAnimation, aimFOV);
        }

        public void UpdateFireSound(AudioClip[] fireSoundList)
        {
            m_GunAnimator.UpdateFireSound(fireSoundList);
        }

        public void UpdateMuzzleFlash(ParticleSystem muzzleParticle)
        {
            m_GunEffects.UpdateMuzzleBlastParticle(muzzleParticle);
        }

        public void UpdateRecoil(Vector3 minCameraRecoilRotation, Vector3 maxCameraRecoilRotation)
        {

        }

        public void UpdateRoundsPerMagazine(int roundsPerMagazine)
        {
            RoundsPerMagazine = roundsPerMagazine;
        }

        #endregion
    }
}