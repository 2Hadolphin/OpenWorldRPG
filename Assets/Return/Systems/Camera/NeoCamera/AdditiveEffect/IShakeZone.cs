using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoFPS
{
    public interface IShakeZone
    {
        float GetStrengthAtPosition(Vector3 position);
    }
}