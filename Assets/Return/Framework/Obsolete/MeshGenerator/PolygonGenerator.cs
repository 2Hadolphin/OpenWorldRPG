using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PolygonGenerator : ScriptableObject
{
    public const string Path ="Mesh/";

    /// <summary>
    /// Radius as 1
    /// </summary>
    public virtual Mesh PolygonalPlane(int vertex)
    {
        if(vertex < 3)
        {
            Debug.LogError("Inputs number of vertex must greater or equal to 3 !");
            vertex = 3;
        }

        float deg = 360 / vertex;
        var rad = deg * Mathf.Deg2Rad;
        var rads = 0f;

        Vector3[] vertexs = new Vector3[vertex];
        Vector2[] uv = new Vector2[vertex];
        int[] triangles = new int[(vertex-2)*3];

        if (vertex % 2 == 0)
            rads += rad / 2;

        for (int i = 0; i < vertex; i++)
        {
            vertexs[i] = new Vector3(Mathf.Cos(rads) , 0, Mathf.Sin(rads));
            rads += rad;
        }


        for (int i = 0; i < vertex-2; i++)
        {
            triangles[i*3] = i+2;
            triangles[i*3+1] = i+1;
            triangles[i*3+2] = 0;
        }

        Mesh mesh = new Mesh() { vertices = vertexs, triangles = triangles};
        mesh.RecalculateNormals();

        return mesh;
    }

}
