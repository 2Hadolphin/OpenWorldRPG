using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;
using Sirenix.OdinInspector;
using Return;
using Return.Humanoid;
using Return.Definition;
using Return.Items.Weapons;
using Return.Cameras;
using Return.Preference;

namespace Return.Agents
{

    public partial class xxx_WeaponUsingSystem : MonoBehaviour//WeaponHolster
    {
        //public HumanoidInteractSystem IUS;

        /// <summary>
        /// The Default Weapon is equipped if the Equipped Weapons list is empty.
        /// </summary>
        [SerializeField]
        [Tooltip("The Default Weapon is equipped if the Equipped Weapons list is empty.")]
        private MeleeWeapon m_DefaultWeapon;

        private void Start()
        {
            //m_Camera = ConstCache.PrimeCamDirector;

            // Disable all weapons
            if (m_DefaultWeapon != null)
                m_DefaultWeapon.Viewmodel.SetActive(false);

            for (int i = 0, c = m_WeaponList.Count; i < c; i++)
            {
                //if (m_WeaponList[i] != null)
                //    m_WeaponList[i].Viewmodel.SetActive(false);
            }

            /*
            if (m_FragGrenade != null)
                m_FragGrenade.CharacterRoot.SetActive(false);
            */

            if (m_EquippedWeaponsList.Count > 0)
            {
                for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
                {
                    if (m_EquippedWeaponsList[i] == null)
                        continue;

                    // Initialize the weapon ammo
                    //m_EquippedWeaponsList[i].InitializeMagazineAsDefault();

                    //if (m_CurrentWeapon == null)
                    //{
                    //    Select(m_EquippedWeaponsList[i], m_EquippedWeaponsList[i].CurrentRounds);
                    //}
                }
            }

            if (m_CurrentWeapon == null && m_DefaultWeapon != null)
            {
                Select(m_DefaultWeapon, 0);
            }

            CalculateWeight();

            // Inputs Bindings
            //m_WeaponMap = SettingsManager.Instance.GetActionMap("Weapons");
            //m_WeaponMap.Enable();

            //m_NextWeaponAction = m_WeaponMap.FindAction("Next Weapon");
            //m_PreviousWeaponAction = m_WeaponMap.FindAction("Previous Weapon");
            //m_LethalEquipmentAction = m_WeaponMap.FindAction("Lethal Equipment");
            //m_TacticalEquipmentAction = m_WeaponMap.FindAction("Tactical Equipment");

            //m_MovementMap = SettingsManager.Instance.GetActionMap("Movement");
            //m_MovementMap.Enable();

            //m_InteractAction = m_MovementMap.FindAction("Interact");



            //IUS.m_Controller.LadderEvent += ClimbingLadder;
        }
        /// <summary>
        /// Determines the GameObject tag identifier to ammo pickups.
        /// </summary>
        [SerializeField]
        //[Tags(AllowUntagged = false)]
        [Tooltip("Determines the GameObject tag identifier to ammo pickups.")]
        private string m_AmmoTag = "Ammo";

        /// <summary>
        /// Determines the GameObject tag identifier to adrenaline packs pickups.
        /// </summary>
        [SerializeField]
        //[Tags(AllowUntagged = false)]
        [Tooltip("Determines the GameObject tag identifier to adrenaline packs pickups.")]
        private string m_AdrenalinePackTag = "Adrenaline Pack";

        /// <summary>
        /// Sound played when the character pick up an m_items or Weapon.
        /// </summary>
        [SerializeField]
        [Tooltip("Sound played when the character pick up an m_items or Weapon.")]
        private AudioClip m_ItemPickupSound;

        /// <summary>
        /// Defines the volume of m_items Pickup Sound played when the character pick up an m_items or Weapon.
        /// </summary>
        [SerializeField]
        [Range(0, 1)]
        [Tooltip("Defines the volume of m_items Pickup Sound played when the character pick up an m_items or Weapon.")]
        private float m_ItemPickupVolume = 0.3f;

        /// <summary>
        /// When activated, the character will change their current weapon for others instantly.
        /// </summary>
        [SerializeField]
        [Tooltip("When activated, the character will change their current weapon for others instantly.")]
        private bool m_FastChangeWeapons;

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private List<mmgun> m_EquippedWeaponsList = new List<mmgun>();

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private List<mmgun> m_WeaponList = new List<mmgun>();

        /// <summary>
        /// 
        /// </summary>
        [SerializeField]
        private List<AmmoInstance> m_AmmoList = new List<AmmoInstance>();




        /*
/// <summary>
/// Defines the reference to the Frag Grenade item.
/// </summary>
[SerializeField]
[Tooltip("Defines the reference to the Frag Grenade item.")]
private Grenade m_FragGrenade;
*/
        private HumanoidCameraBase m_Camera;

        private IWeapon m_CurrentWeapon;


        private InputActionMap m_WeaponMap;
        private InputActionMap m_MovementMap;
        private InputAction m_NextWeaponAction;
        private InputAction m_PreviousWeaponAction;
        private InputAction m_LethalEquipmentAction;
        private InputAction m_TacticalEquipmentAction;
        private InputAction m_InteractAction;

        #region PROPERTIES

        /// <summary>
        /// Returns the current state of the equipped weapon.
        /// </summary>
        public WeaponState State
        {
            get
            {
                return WeaponState.Idle;
                //switch (m_CurrentWeapon)
                //{
                //    case null:
                //        return WeaponState.IdleState;
                //    case mmgun gun when gun.IdleState:
                //        return WeaponState.IdleState;
                //    case mmgun gun when gun.Firing:
                //        return WeaponState.Firing;
                //    case mmgun gun when gun.Reloading:
                //        return WeaponState.Reloading;
                //    case mmgun gun when gun.MeleeAttacking:
                //        return WeaponState.MeleeAttacking;
                //    case mmgun gun when gun.Interacting:
                //        return WeaponState.Interacting;
 
                //    case MeleeWeapon meleeWeapon when meleeWeapon.IdleState:
                //        return WeaponState.IdleState;
                //    case MeleeWeapon meleeWeapon when meleeWeapon.MeleeAttacking:
                //        return WeaponState.MeleeAttacking;
                //    case MeleeWeapon meleeWeapon when meleeWeapon.Interacting:
                //        return WeaponState.Interacting;
     
                //    default:
                //        return WeaponState.IdleState;
                //}
            }
        }

        /// <summary>
        /// The current weapon type.
        /// </summary>
        public System.Type Type => m_CurrentWeapon?.GetType();

        /// <summary>
        /// The current accuracy.
        /// </summary>
        public float? Accuracy
        {
            get
            {
                return null;
                //if (m_CurrentWeapon != null && m_CurrentWeapon.GetType() == typeof(mmgun))
                //    return ((mmgun)m_CurrentWeapon).Accuracy;
                
            }
        }

        /// <summary>
        /// The amount of ammunition of the current weapon.
        /// </summary>
        public int CurrentAmmo
        {
            get
            {
                //if (m_CurrentWeapon != null && m_CurrentWeapon.GetType() == typeof(mmgun))
                //    return ((mmgun)m_CurrentWeapon).CurrentRounds;
                return -1;
            }
        }

        /// <summary>
        /// The amount of ammunition remaining for the current weapon.
        /// </summary>
        public int Magazines
        {
            get
            {
                //if (m_CurrentWeapon is mmgun gun)
                //{
                //    return GetAmmoInstance(gun.GunData.AmmoType).Amount;
                //}
                return -1;
            }
        }

        /// <summary>
        /// The ID of the equipped gun.
        /// </summary>
        public int GunID
        {
            get
            {
                if (m_CurrentWeapon != null)
                    return m_CurrentWeapon.Identifier;
                return -1;
            }
        }

        /// <summary>
        /// The name of the equipped gun.
        /// </summary>
        public string GunName
        {
            get
            {
                //if (m_CurrentWeapon != null && m_CurrentWeapon.GetType() == typeof(mmgun))
                //{
                //    return ((mmgun)m_CurrentWeapon).GunName;
                //}
                return string.Empty;
            }
        }

        /// <summary>
        /// The fire mode of the equipped gun.
        /// </summary>
        public string FireMode
        {
            get
            {
                //if (m_CurrentWeapon != null && m_CurrentWeapon.GetType() == typeof(mmgun))
                //    return (((mmgun)m_CurrentWeapon).HasSecondaryMode ? ((mmgun)m_CurrentWeapon).FireMode.ToString() : string.Empty);
                return string.Empty;
            }
        }

        /// <summary>
        /// Returns true if the character can switch guns, false otherwise.
        /// </summary>
        public bool CanSwitch
        {
            get
            {
                if (m_CurrentWeapon == null)
                {
                    return true;
                }
                return m_CurrentWeapon != null && m_CurrentWeapon.CanSwitch;
            }
        }

        /// <summary>
        /// Returns the current equipped weapons list.
        /// </summary>
        public mmgun[] EquippedWeapons => m_EquippedWeaponsList.ToArray();

        /// <summary>
        /// Returns the current equipped gun.
        /// </summary>
        public mmgun CurrentGun
        {
            get
            {
                if (m_CurrentWeapon != null && m_CurrentWeapon.GetType() == typeof(mmgun))
                {
                    return (mmgun)m_CurrentWeapon;
                }

                return null;
            }
        }
        /*
        public FirstAidKit Adrenaline => m_Adrenaline;

        public Grenade FragGrenade => m_FragGrenade;

        */
        /// <summary>
        /// The GameObject tag used for ammunition.
        /// </summary>
        public string AmmoTag => m_AmmoTag;

        /// <summary>
        /// The GameObject tag used for adrenaline packs.
        /// </summary>
        public string AdrenalinePackTag => m_AdrenalinePackTag;

        /// <summary>
        /// Returns true if the equipped gun is a shotgun, false otherwise.
        /// </summary>
        public bool IsShotgun
        {
            get
            {
                //if (m_CurrentWeapon != null && m_CurrentWeapon.GetType() == typeof(mmgun))
                //    return ((mmgun)m_CurrentWeapon).FireMode == Return.m_items.Weapon.FireMode.ShotgunAuto || ((mmgun)m_CurrentWeapon).FireMode == Return.m_items.Weapon.FireMode.ShotgunSingle;
                return false;
            }
        }

        /// <summary>
        /// Returns true if the character has a free slot for a new gun, false otherwise.
        /// </summary>
        public bool HasFreeSlot
        {
            get
            {
                for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
                {
                    if (!m_EquippedWeaponsList[i])
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        #endregion

        #region EDITOR

        public void AddWeaponSlot()
        {
            m_EquippedWeaponsList.Add(null);
        }

        public void RemoveWeaponSlot(int index)
        {
            m_EquippedWeaponsList.RemoveAt(index);
        }

        public void AddWeapon()
        {
            m_WeaponList.Add(null);
        }

        public void RemoveWeapon(int index)
        {
            m_WeaponList.RemoveAt(index);
        }

        #endregion

      
        ///
        /// <summary>
        /// Notifies the equipped weapon that the character is climbing a ladder.
        /// </summary>
        /// <param name="climbing">Is the character climbing?</param>
        //private void ClimbingLadder(bool climbing)
        //{
        //    if (IUS.m_Climbing == climbing)
        //        return;

        //    IUS.m_Climbing = climbing;
        //    if (IUS.m_OnLadder)
        //    {
        //        OnExitLadder();
        //    }
        //}

        /// <summary>
        /// Deselect the current weapon to simulate climbing a ladder.
        /// </summary>
        //private void OnEnterLadder()
        //{
        //    IUS.m_OnLadder = true;
        //    IUS.m_ItemCoolDown = true;
        //    m_CurrentWeapon.Deselect();
        //}

        /// <summary>
        /// Select the previous weapon and reactive all weapon features.
        /// </summary>
        //private void OnExitLadder()
        //{
        //    m_CurrentWeapon.Select();
        //    IUS.m_ItemCoolDown = false;
        //    IUS.m_OnLadder = false;
        //}

        private void SelectByPreviousAndNextButtons()
        {
            //int weaponIndex = GetEquippedWeaponIndexOnList(m_CurrentWeapon.Identifier);

            //if (m_EquippedWeaponsList.Count > 1)
            //{
            //    if (m_NextWeaponAction.triggered)
            //    {
            //        int newIndex = ++weaponIndex % m_EquippedWeaponsList.Count;
            //        if (m_EquippedWeaponsList[newIndex])
            //            StartCoroutine(Switch(m_CurrentWeapon, m_EquippedWeaponsList[newIndex], m_EquippedWeaponsList[newIndex].CurrentRounds));
            //    }
            //    else if (m_PreviousWeaponAction.triggered)
            //    {
            //        int newIndex = --weaponIndex < 0 ? m_EquippedWeaponsList.Count - 1 : weaponIndex;
            //        if (m_EquippedWeaponsList[newIndex])
            //            StartCoroutine(Switch(m_CurrentWeapon, m_EquippedWeaponsList[newIndex], m_EquippedWeaponsList[newIndex].CurrentRounds));
            //    }
            //}
        }

        private void Update()
        {


            //if (!IUS.m_motionSystem.Controllable)
                return;

            // Switch equipped weapons
            if (m_CurrentWeapon != null)
            {
                if (m_CurrentWeapon.CanSwitch)
                {
                    // If the character is climbing a ladder
                    //if (IUS.m_Climbing && !IUS.m_OnLadder)
                    //{
                    //    OnEnterLadder();
                    //}
                    //else
                    //{
                    //    SelectByPreviousAndNextButtons();
                    //}
                }
            }
            else
            {
               // m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, GameplayManager.Instance.FieldOfView, Time.deltaTime * 10);
            }
        }






        public bool CanRefillAmmo()
        {
            for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
            {
                if (!m_EquippedWeaponsList[i])
                    continue;

                //AmmoInstance ammoInstance = GetAmmoInstance(m_EquippedWeaponsList[i].GunData.AmmoType);
                //if (ammoInstance.Amount != ammoInstance.MaxAmount)
                //    return true;
            }

            /*
            if (m_FragGrenade.CanRefill)
                return true;
            */

            return false;
        }

        /// <summary>
        /// Refills the character magazines for all equipped weapons.
        /// </summary>
        private IEnumerator RefillAmmo()
        {
            //IUS.m_ItemCoolDown = true;

            m_CurrentWeapon.Interact();
            yield return new WaitForSeconds(m_CurrentWeapon.InteractDelay);

            for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
            {
                if (!m_EquippedWeaponsList[i])
                    continue;

                //AmmoInstance ammoInstance = GetAmmoInstance(m_EquippedWeaponsList[i].GunData.AmmoType);
                //ammoInstance.Amount = ammoInstance.MaxAmount;
            }

            //IUS.m_PlayerBodySource.ForcePlay(m_ItemPickupSound, m_ItemPickupVolume);

            // Also refill the grenades
            /*
            if (m_FragGrenade)
                m_FragGrenade.Refill();
            */

            yield return new WaitForSeconds(Mathf.Max(m_CurrentWeapon.InteractAnimationLength - m_CurrentWeapon.InteractDelay, 0));
            //IUS.m_ItemCoolDown = false;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ammoType"></param>
        /// <returns></returns>
        public int GetAmmo(AmmunitionData ammoType)
        {
            return GetAmmoInstance(ammoType)?.Amount ?? 0;
        }

        private AmmoInstance GetAmmoInstance(AmmunitionData ammoType)
        {
            int c = m_AmmoList.Count;
            if (!ammoType && c <= 0) return null;

            for (int i = 0; i < c; i++)
            {
                if (m_AmmoList[i].Instance && m_AmmoList[i].Instance.GetInstanceID() == ammoType.GetInstanceID())
                {
                    return m_AmmoList[i];
                }
            }

            return null;
        }

        /// <summary>
        /// Returns the requested amount of ammo of the ammo type.
        /// </summary>
        /// <param name="ammoType">Type of ammunition requested.</param>
        /// <param name="amount">Number of rounds requested by the gun.</param>
        public int RequestAmmunition(AmmunitionData ammoType, int amount)
        {
            AmmoInstance instance = GetAmmoInstance(ammoType);

            if (instance == null)
                return 0;

            if (instance.Amount >= amount)
            {
                instance.Amount -= amount;
                return amount;
            }

            int remainingAmmo = instance.Amount;
            instance.Amount = 0;
            return remainingAmmo;
        }




        /// <summary>
        /// Switch the weapons the character is equipped with.
        /// </summary>
        /// <param name="current">The current weapon.</param>
        /// <param name="target">The desired weapon.</param>
        /// <param name="currentRounds"></param>
        private IEnumerator Switch(IWeapon current, IWeapon target, int currentRounds)
        {
            current.Deselect();
            yield return new WaitForSeconds(current.HideAnimationLength);

            current.Viewmodel.SetActive(false);
            Select(target, currentRounds);
        }

        /// <summary>
        /// Change the weapons the character is equipped with.
        /// </summary>
        /// <param name="current">The current weapon.</param>
        /// <param name="target">The desired weapon.</param>
        /// <param name="currentRounds"></param>
        private IEnumerator Change(IWeapon current, IWeapon target, int currentRounds)
        {
            current.Deselect();
            if (!m_FastChangeWeapons)
            {
                yield return new WaitForSeconds(current.HideAnimationLength);
            }

            current.Viewmodel.SetActive(false);
            Select(target, currentRounds);
        }

        /// <summary>
        /// Replace the current weapon for the target weapon and Schedule_Drop it.
        /// </summary>
        /// <param name="current">The current weapon.</param>
        /// <param name="target">The desired weapon.</param>
        /// <param name="drop">The current weapon Model.</param>
        /// <param name="currentRounds"></param>
        private IEnumerator DropAndChange(IWeapon current, IWeapon target, FirearmsShowcase drop, int currentRounds)
        {
            current.Deselect();
            //if (!m_FastChangeWeapons)
            //{
            //    yield return new WaitForSeconds(((mmgun)current).HideAnimationLength);
            //}

            //if (((mmgun)current).DroppablePrefab)
            //// ReSharper disable once Unity.InefficientPropertyAccess
            //{
            //    GameObject newGunPickup = Instantiate(((mmgun)current).DroppablePrefab, Schedule_Drop.transform.position, Schedule_Drop.transform.rotation);
            //    //newGunPickup.GetComponent<FirearmsShowcase>().CurrentRounds = ((Firearms)current).CurrentRounds;
            //}

            Destroy(drop.transform.gameObject);

            current.Viewmodel.SetActive(false);
            Select(target, currentRounds);
            yield break;
        }

        /// <summary>
        /// Select the target weapon.
        /// </summary>
        /// <param name="weapon">The weapon to be draw.</param>
        /// <param name="currentRounds"></param>
        private void Select(IWeapon weapon, int currentRounds)
        {
            m_CurrentWeapon = weapon;
            weapon.SetCurrentRounds(currentRounds);
            weapon.Viewmodel.SetActive(true);
            weapon.Select();
        }

        /// <summary>
        /// Calculates the weight the character is carrying on based on the equipped weapons.
        /// </summary>
        private void CalculateWeight()
        {
            float weight = 0;
            //for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
            //{
            //    if (m_EquippedWeaponsList[i] && m_EquippedWeaponsList[i].GetType() == typeof(mmgun))
            //        weight += m_EquippedWeaponsList[i].Weight;
            //}
            //IUS.Weight = weight;
        }

        /// <summary>
        /// Makes the character equip a weapon based on its index on the list.
        /// </summary>
        /// <param name="index">The weapon index.</param>
        public void EquipWeapon(int index)
        {
            if (HasFreeSlot)
            {
                for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
                {
                    if (m_EquippedWeaponsList[i])
                        continue;

                    m_EquippedWeaponsList[i] = m_WeaponList[index];
                    return;
                }
            }
        }

        /// <summary>
        /// Makes the character unequip a weapon based on its index on the list.
        /// </summary>
        /// <param name="index"></param>
        public void UnequipWeapon(int index)
        {
            m_EquippedWeaponsList[index] = null;
        }

        /// <summary>
        /// Is the weapon on the Equipped Weapons List?
        /// </summary>
        /// <param name="weapon">The target weapon.</param>
        public bool IsEquipped(IWeapon weapon)
        {
            if (m_DefaultWeapon)
            {
                if (weapon.Identifier == m_DefaultWeapon.Identifier)
                    return true;
            }

            for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
            {
                if (!m_EquippedWeaponsList[i])
                    continue;

                //if (m_EquippedWeaponsList[i].Identifier == weapon.Identifier)
                //{
                //    return true;
                //}
            }
            return false;
        }

        public int GetEquippedWeaponIndexOnList(int id)
        {
            for (int i = 0, c = m_EquippedWeaponsList.Count; i < c; i++)
            {
                if (!m_EquippedWeaponsList[i])
                    continue;

                //if (m_EquippedWeaponsList[i].Identifier == id)
                //{
                //    return i;
                //}
            }
            return -1;
        }

        public int GetWeaponIndexOnList(int id)
        {
            for (int i = 0, c = m_WeaponList.Count; i < c; i++)
            {
                if (!m_WeaponList[i])
                    continue;

                //if (m_WeaponList[i].Identifier == id)
                //{
                //    return i;
                //}
            }
            return -1;
        }

        public IWeapon GetWeaponByID(int id)
        {
            for (int i = 0, c = m_WeaponList.Count; i < c; i++)
            {
                if (!m_WeaponList[i])
                    continue;

                //if (m_WeaponList[i].Identifier == id)
                //{
                //    return m_WeaponList[i];
                //}
            }
            return null;
        }
    }

}
