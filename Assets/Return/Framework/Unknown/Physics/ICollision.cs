using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Physical
{
    public interface ICollision
    {
        void Raycast<T>(T arg);
    }
}