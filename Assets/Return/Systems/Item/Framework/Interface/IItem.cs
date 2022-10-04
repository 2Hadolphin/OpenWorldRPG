using Return.Agents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Modular;

namespace Return.Items
{
    /// <summary>
    /// Instance item behaviour interface
    /// </summary>
    public interface IItem : IMonoProvider, IPickup
    {
        #region Net

        bool isMine { get; }

        #endregion


        #region Life Time


        event Action OnItemDeactivate;
        event Action OnUnregister;



        #endregion

        #region resolver

        IModularResolver resolver { get; }

        #endregion


        #region Agent or player ( HUD )
        /// <summary>
        /// load this while execute by player ( HUD )
        /// </summary>
        //public void LoadHostHUD();
        #endregion

        // --------------------------unfinish___wait to deldet


        //ColliderBounds BoundBox { get; }

        //bool LoadAsData();

    }
}