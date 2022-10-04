using System;
using UnityEngine;

public static class ItemPassEvent
{
    public delegate void PassEvent(object sender, ItemChainItemEventArgs e);
    public static event PassEvent PassItem=null;



    public static void ApplyEvent(object sender, ItemChainItemEventArgs e)
    {
        if (PassItem != null)
        {
            PassItem.Invoke(sender,e);
        }
    }

}
