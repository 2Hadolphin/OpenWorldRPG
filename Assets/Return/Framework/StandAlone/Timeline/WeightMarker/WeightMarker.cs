using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;
using System;


[Serializable]
public class WeightMarker : Marker
{
    [SerializeField] 
    float m_Weight = 1;

    public float weight
    {
        get { return m_Weight; }
        set { m_Weight = value; }
    }
}
