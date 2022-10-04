using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Return.Database;


public class AssetsPresetBundle<T>: DataEntity
{
    [SerializeField]
    public List<T> Assets=new ();


}
