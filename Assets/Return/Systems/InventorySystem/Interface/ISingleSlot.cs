using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Items;

namespace Return.Inventory
{
    public interface ISingleSlot : IInventory
    {
        Category category { get; }

        //void LoadGUIPack(bool uniquePack, out InventoryManager.InventoryData[] datas);
        //ReadOnlyTransform GetOriginTargetTF();
        //ReadOnlyTransform GetTargetTransform(int sn);
        //bool CheckSlot(int weight, int volume, out IInventory slot, out InventoryManager.SlotPlace slotPlace, out Vector2Int SN);
        //void Repay(int sn, IItem debt);
        //bool QuickSlot(out int SN, out InventoryManager.SlotPlace slotPlace, out ICoordinate Target);
    }
}