using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return.Items.Weapons
{
    /// <summary>
    /// Port of procedural animation
    /// </summary>
    public interface IFirearmsRecoilPort
    {
        public event Action<bl_RecoilBase.RecoilData> OnRecoil;
    }

}

