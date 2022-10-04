using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;



namespace Return
{
    public class Geometry
    {
        /// <summary>
        /// Return regular polygon points in 3D sapce(2D plane).
        /// </summary>
        public static Vector3[] RegularPolygon(int vertexCount, Vector3 center, Quaternion rotation, float radius)
        {
            Assert.IsFalse(vertexCount < 3);

            var points = new Vector3[vertexCount];

            float step = 2f * Mathf.PI / vertexCount;

            float offset = Mathf.PI * 0.5f + ((vertexCount % 2 == 0) ? step * 0.5f : 0f);


            for (int i = 0; i < vertexCount; i++)
            {
                float theta = step * i + offset;

                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);

                points[i] = center + rotation * new Vector3(x, y, 0f);
            }

            return points;
        }

        public static Vector3[] Circle(Vector3 position, Vector3 normal, float radius, int subdivisions = 12)
        {
            if (subdivisions < 3)
                subdivisions = 3;
            var theta = 0f;
            var deltaTheta = 2f * Mathf.PI / subdivisions;
            var transformAxis = Quaternion.FromToRotation(Vector3.forward, normal);
            var points = new Vector3[subdivisions];


            for (int i = 0; i < subdivisions; i++)
            {
                theta += deltaTheta;
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);
                points[i] = position + transformAxis * new Vector3(x, y, 0);
            }


            return points;
        }


        public static Vector2[] Circle(Vector2 position, float radius, int subdivisions = 12, bool mirror = false)
        {
            var theta = 0f;
            var deltaTheta = 2f * Mathf.PI / subdivisions;

            if (mirror)
                if (!subdivisions.IsOdd())
                    theta += deltaTheta / 2;

            var points = new Vector2[subdivisions];


            for (int i = 0; i < subdivisions; i++)
            {
                theta += deltaTheta;
                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);
                points[i] = position + new Vector2(x, y);
            }

            return points;
        }


        public static int[][] SubdividedTriangle(int[] points, bool invertNormal = false)
        {
            var length = points.Length;
            if (length < 3)
                return null;

            if (invertNormal)
                Array.Reverse(points);


            if (length == 3)
                return new int[1][] { points };


            var faces = new int[length - 2][];
            var p0 = points[0];

            for (int i = 2; i < length; i++)
            {
                faces[i - 2] = new int[] { p0, points[i - 1], points[i] };
            }

            return faces;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="vertices0">buttom/right</param>
        /// <param name="vertices1">top/left</param>
        public static int[][] AutoFace_Cylinder(int[] vertices0, int[] vertices1, bool seal = false)
        {
            var length = Mathf.Min(vertices0.Length, vertices1.Length);
            var faces = new int[seal ? length * 2 : length * 2 - 2][];

            var num = 0;
            for (int i = 0; i < length - 1; i++)
            {
                faces[num] = new int[3] { vertices1[i], vertices0[i], vertices0[i + 1] };
                faces[num + 1] = new int[3] { vertices0[i + 1], vertices1[i + 1], vertices1[i] };
                num += 2;
            }

            faces[num] = new int[3] { vertices1[length - 1], vertices0[length - 1], vertices0[0] };
            faces[num + 1] = new int[3] { vertices0[0], vertices1[0], vertices1[length - 1] };

            return faces;
        }

        public static int[][] AutoFace_Cone(int top, int[] vertices)
        {
            var length = vertices.Length - 1;
            var faces = new int[length][];

            for (int i = 0; i < length; i++)
            {
                faces[i] = new int[3] { top, vertices[i], vertices[i + 1] };
            }

            return faces;
        }
    }
}