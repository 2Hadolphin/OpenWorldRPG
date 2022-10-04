using UnityEngine;
using System;
using Return.Items;

/// <summary>
/// ??????
/// </summary>
public class ItemChainItemEventArgs:EventArgs
{
    private ItemInfo _Item;
    //private VehicleState _vehicleState;

    public const string TargetItem = "TargetItem";
    public ItemInfo SelectedItem
    {
        get
        {
            return _Item;
        }
    }
    //public VehicleState SelectedVehicle
    //{
    //    get
    //    {
    //        return _vehicleState;
    //    }
    //}

    //public ItemChainItemEventArgs(ItemInfo target,VehicleState target2)
    //{
    //    _Item = target;
    //    _vehicleState=target2;
    //    Debug.Log(_Item);
    //    Debug.Log(_vehicleState);
    //}
}
