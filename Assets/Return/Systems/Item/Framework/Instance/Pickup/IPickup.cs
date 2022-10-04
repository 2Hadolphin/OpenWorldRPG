using UnityEngine;
using Return;
using Return.Agents;
using Cysharp.Threading.Tasks;
using System;

namespace Return.Items
{
    /// <summary>
    /// inherit by showcase or item behaviour 
    /// </summary>
    public interface IPickup
    {
        /// <summary>
        /// Provide pickable preset data.
        /// </summary>
        ItemPreset Preset { get; }


        #region Agnet

        /// <summary>
        /// Agent who control this item.
        /// </summary>
        IAgent Agent { get; }

        /// <summary>
        /// RegisterHandler all module to handler and bind agent.
        /// </summary>
        void Register(IAgent agent);

        /// <summary>
        /// Strip or remove module from handler and release agent binding.
        /// obsolete--(special module like performer required shutdown manually.)
        /// </summary>
        void Unregister(IAgent agent);

        /// <summary>
        /// SetHandler functions
        /// </summary>
        UniTask Activate();

        /// <summary>
        /// Deactivate functions, OnItemDeactivate will be invoke. 
        /// </summary>
        UniTask Deactivate();

        #endregion



        #region Drop Loot

        //  Pickupable item decide loot type and pool

        /// <summary>
        /// Release pickable to scene.
        /// </summary>
        /// <param name="pr">GUIDs and rotation to spawn showcase.</param>
        void Drop(PR pr);

        /// <summary>
        /// Release pickable to scene.
        /// </summary>
        /// <param name="pr">Custom module to set showcase. **drop type.</param>
        void Drop(IPickupableProvider provider);

        #endregion


        /// <summary>
        /// Evaluate item coordinate of grab pose 
        /// </summary>
        /// <param name="subtraction">????</param>
        /// <returns></returns>
        [Obsolete]
        ICoordinate Grab(ICoordinate subtraction = null);

        /// <summary>
        /// Evaluate item coordinate of handle pose **gun handle **sword handle
        /// </summary>
        /// <param name="key">**HandleOption.RightHand **HandleOption.LeftHand</param>
        [Obsolete]
        ICoordinate GetHandle(string key);
    }
}