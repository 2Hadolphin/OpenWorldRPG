using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Humanoid;
using Return.Items;
using System;

namespace Return.Inventory
{
    public interface IBaseInventory
    {
        /// <summary>
        /// Search content.
        /// </summary>
        bool Search(Func<IArchiveContent, bool> method, out object obj);

        /// <summary>
        /// Check archive content qualify.
        /// </summary>
        bool CanStorage(object obj);

        /// <summary>
        /// Archive content.
        /// </summary>
        void Store(object obj);

        /// <summary>
        /// Valid withdraw content.
        /// </summary>
        bool CanExtract(object obj);

        /// <summary>
        /// Withdraw content.
        /// </summary>
        void Extract(object obj);

    }

    public interface IStorage : IBaseInventory
    {


    }


    /// <summary>
    /// 
    /// </summary>
    public interface IInventory : IBaseInventory
    {
        /// <summary>
        /// Bind inventory manager. 
        /// </summary>
        void RegisterHandler(IInventoryManager manager);

        [Obsolete]
        ICoordinate GetHandle(object obj);

        


        // set inventory space
        //void SetPlace(InventoryManager.SlotPlace slotPlace);

        //int getSN { get; }

        //void ApplySN(int sn);
        //Vector3Int Capacity { get; }

        //bool ConfirmTarget(int coordnate, out int confirmHash);
    }
}
