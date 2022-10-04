using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Physical;

namespace Return.Humanoid.Character
{
    public interface ILimbCollision:ICollision
    {
        Collider Collider { get; }

        public event Action<Collision> Impact;
    }
}