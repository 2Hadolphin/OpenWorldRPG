using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Return.Items
{
    public interface IItemPostureHandler:IItemModule
    {
        UniTask Equip();
        UniTask Dequip();
    }
}