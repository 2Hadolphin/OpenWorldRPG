using System;
using Cysharp.Threading.Tasks;

namespace Return.Inventory
{
    /// <summary>
    /// Interface to manage stocks.
    /// </summary>
    public interface IInventoryManager
    {
        #region InventoryManager

        void Register(IInventory inventory);
        void Unregister(IInventory inventory);

        #endregion


        #region Inventory

        /// <summary>
        /// Get item via note.
        /// </summary>
        bool ExtractStock(object item);

        /// <summary>
        /// Valid object storageable.
        /// </summary>
        bool CheckStore(object obj);

        /// <summary>
        /// Storage object.
        /// </summary>
        void Store(object obj);

        /// <summary>
        /// Search item from inventories.
        /// </summary>
        /// <param name="method"></param>
        /// <returns>Target match search criteria.</returns>
        UniTask<object> Search(Func<object, bool> method);

        #endregion

    }
}
