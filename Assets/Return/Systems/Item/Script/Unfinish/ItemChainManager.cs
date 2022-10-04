using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Return.Items;

public class ItemChainManager:MonoBehaviour
{
    // protected static ItemBag ItemBag;
    //protected static SelectionPointer selectSS;

    private ItemInfo _TargetItem; 

    private static ItemChainManager _ICM;

    public static ItemChainManager ICM
    {
        get
        {
            if (_ICM == null)
            {
                _ICM = new ItemChainManager();
            }

            return _ICM;
        }
    }

    private ItemChainManager() { }


    public ItemInfo TargetItem 
    {
        get
        {
            return _TargetItem;
        }

        set
        {
            if (_TargetItem != value)
            {
                _TargetItem = value;
                
            }
        }
    }



}
