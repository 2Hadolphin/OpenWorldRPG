
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Bezier
{

    public static Vector2 EvaluateQuadratic(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        Vector2 p0 = Vector2.Lerp(a, b, t);
        Vector2 p1 = Vector2.Lerp(b, c, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector2 EvaluateCubic(Vector2 a, Vector2 b, Vector2 c, Vector2 d, float t)
    {
        Vector2 p0 = EvaluateQuadratic(a, b, c, t);
        Vector2 p1 = EvaluateQuadratic(b, c, d, t);
        return Vector2.Lerp(p0, p1, t);
    }

    public static Vector3 EvaluateQuadratic(Vector3 a,Vector3 b,Vector3 c,float p)
    {
        Vector3 p0 = Vector3.Lerp(a, b, p);
        Vector3 p1 = Vector3.Lerp(b, c, p);
        return Vector3.Lerp(p0, p1, p);
    }

    public static Vector3 EvaluateArray(Vector3[] points,float t)
    {
        var nums = points.Length;
        var times = nums;
        for (int k = 0; k < times; k++)
        {
            for (int i = 0; i < nums - 1; i++)
            {
                points[i] = Vector3.Lerp(points[i], points[i + 1], t);
            }
            nums --;
        }

        return points[0];

    }
}