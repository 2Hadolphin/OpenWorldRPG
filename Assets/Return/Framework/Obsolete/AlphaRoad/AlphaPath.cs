using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AlphaPath
{
    [SerializeField, HideInInspector]
    public List<Vector3> points;

    public float range=5;

    public AlphaPath(Vector3 center)
    {
        points = new List<Vector3>
        {
            center+Vector3.left*range,                                  //(-1,0,0)
            center+(Vector3.left+Vector3.forward)*range*0.5f,    //(-1,0,1)
            center+(Vector3.right+Vector3.forward)*range*0.5f,  //( 1,0,1)
            center+Vector3.right*range                                 //(1,0,0)
        };

    }

    public Vector3 this[int i] 
    {
        get
        {
            return points[i];
        }
    }

    public int NumPoints 
    {
        get
        {
            return points.Count;
        }
    }


    public int NumSegment 
    {
        get
        {
            return (points.Count - 4) / 3 + 1;
        }
    }


    public void AddSegment(Vector3 anchorPos)
    {
        points.Add(points[points.Count - 1] * 2 - points[points.Count - 2]);//Point4+(Point4-Point3)
        points.Add((points[points.Count - 1] + anchorPos) * .5f);
        points.Add(anchorPos);
    }

    public Vector3[] GetPointsInSegment(int i)
    { 
        return new Vector3[] { points[i * 3], points[i * 3 + 1], points[i * 3 + 2], points[i * 3 + 3] };
    }

    public void MovePoint(int i,Vector3 pos)
    {
        points[i] = pos;
    }



}
