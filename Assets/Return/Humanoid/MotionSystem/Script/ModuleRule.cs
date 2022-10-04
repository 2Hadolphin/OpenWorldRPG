using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ModuleRule : ScriptableObject
{
    public abstract float[] ApplyWeights(float[] weights);
}
