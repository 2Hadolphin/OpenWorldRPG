using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using Sirenix.OdinInspector;


namespace Return.Items.Weapons
{
    public class AmmunitionPreset : FirearmsModulePreset<AmmunitionModule>
    {
        /// <summary>
        /// Returns true if this gun has a chamber, false otherwise. (Read Only) ?? useful?
        /// </summary>
        public bool HasChamber=>m_ChamberCapacity>0;

        
        [SerializeField]
        [Range(1,256)]
        uint m_ChamberCapacity=1;

        public uint ChamberCapacity => m_ChamberCapacity;

        //public m_Location m_ShellEjector;
        //public m_Location m_Magazine;
        //public m_Location m_Bolt;

        [Title("performer Assets")]
        [Tooltip("Bolt")]
        public PerformerPreset Loaded;

        [SerializeField]
        public Token LoadedToken; 


        public PerformerPreset Reload;

        //[SerializeField]
        //public UTags event_OnLoaded;

        [SerializeField]
        public FirearmsEvent Events;

        // _checkCache ammo ?
    }
}