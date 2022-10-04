using UnityEngine;
using System;

[Serializable]
public struct CameraConstraint
{
    public Vector3 Clamp_Max;
    public Vector3 Clamp_Min;

    /// <summary>
    /// **X=> Min **Y=>Max
    /// </summary>
    public Vector2 Clamp_X
    {
        get
        {
            return new Vector2(Clamp_Min.x, Clamp_Max.x);
        }
        set
        {
            Clamp_Min.x = value.x;
            Clamp_Max.x = value.y;
        }
    }
    /// <summary>
    /// **X=> Min **Y=>Max
    /// </summary>
    public Vector2 Clamp_Y
    {
        get
        {
            return new Vector2(Clamp_Min.y, Clamp_Max.y);
        }
        set
        {
            Clamp_Min.y = value.x;
            Clamp_Max.y = value.y;
        }
    }
    /// <summary>
    /// **X=> Min **Y=>Max
    /// </summary>
    public Vector2 Clamp_Z
    {
        get
        {
            return new Vector2(Clamp_Min.z, Clamp_Max.z);
        }
        set
        {
            Clamp_Min.z = value.x;
            Clamp_Max.z = value.y;
        }
    }

}

