using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Transform2Plane
{
    public static Plane TransformPlane(this Transform tf)
    {
        return new Plane(tf.up, tf.position);
    }

    public static Plane TransformPlane(this Transform tf,Vector3 up)
    {
        return new Plane(tf.rotation*up, tf.position);
    }
}
