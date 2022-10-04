using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class Inspector_Bezier : MonoBehaviour
{
    [Serializable]
    protected struct ClampCoordnate
    {
        [Range(0f,1f)]
        public float X;
        [Range(0f,float.MaxValue)]
        public float Y;
    }
    [SerializeField]
    protected List<ClampCoordnate> Point;
    public virtual Vector3 Evaluate_3D(Vector3 StartPoint,Vector3 EndPoint,Vector3 Normal,float p)
    {
        var length = Point.Count;
        Vector3[] Points = new Vector3[length+2];
        Points[0] = StartPoint;
        Points[Points.Length - 1] = EndPoint;
        for (int i =0 ; i < length; i++)
        {
            Points[i+1] = Cal2To3(StartPoint, EndPoint, Normal, new Vector2(Point[i].X, Point[i].Y));
        }

        length = Points.Length-2;

        for (;length>=1 ;length-- )
        {
            for (int i = 0; i < length; i++)
            {
                Points[i] = Vector3.Lerp(Points[i], Points[i + 1], p);
            }
        }

        return Points[0];
    }
    protected static Vector3 Lerp1Sigma(Vector3 a,Vector3 b, Vector3 c,float t)
    {
        var alpha = Vector3.Lerp(a, b, t);
        var beta = Vector3.Lerp(b, c, t);
        return Vector3.Lerp(alpha, beta, t);
    }

    protected static Vector3 Lerp2Sigma(Vector3 a, Vector3 b, Vector3 c,Vector3 d, float t)
    {
        var alpha = Lerp1Sigma(a, b,c, t);
        var beta = Lerp1Sigma(b, c,d, t);
        return Vector3.Lerp(alpha, beta, t);
    }

    protected static Vector3 Cal2To3(Vector3 StartPoint,Vector3 EndPoint,Vector3 Direction,Vector2 Stylus)
    {
        return (EndPoint - StartPoint) * Stylus.x+ Direction * Stylus.y;
    }



}
