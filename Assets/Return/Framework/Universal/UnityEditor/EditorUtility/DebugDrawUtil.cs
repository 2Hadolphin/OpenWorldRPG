using UnityEngine;
namespace Return
{
    public static class DebugDrawUtil
    {
        public static void DrawCoordinate(Vector3 position, Quaternion rotation)
        {
            Debug.DrawRay(position, rotation * Vector3.up, Color.green);
            Debug.DrawRay(position, rotation * Vector3.right, Color.red);
            Debug.DrawRay(position, rotation * Vector3.forward, Color.blue);

        }

        public static void DrawLines(Vector3[] points, Color color = default,bool seal=false)
        {
            var num = points.Length;
            Gizmos.color = color;
            for (int i = 0; i < num-1; i++)
            {
                Gizmos.DrawLine(points[i], points[i + 1]);
            }

            if(seal)
            Gizmos.DrawLine(points[0], points[num-1]);
        }

        public static void DrawArrow(Vector3 from,Vector3 to,Vector3 normal,Color color = default)
        {
            var angle = 155f;
            var vector = (to - from)*(0.17f);


            Gizmos.color = color;
            Gizmos.DrawLine(from, to);
            Gizmos.DrawRay(to, Quaternion.AngleAxis(angle, normal) * vector);
            Gizmos.DrawRay(to, Quaternion.AngleAxis(-angle, normal) * vector);
        }
    }
}