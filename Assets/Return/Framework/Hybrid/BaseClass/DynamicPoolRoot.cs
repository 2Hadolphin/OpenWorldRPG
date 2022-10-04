using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


[Serializable]
public class DynamicPoolRoot
{

    public int PoolHash = 0;
    public int ApplyNumber=0;
    public SpawnData Spawn=null;
    public IPoolDebit[] DebitCatch=new IPoolDebit[0];


}
