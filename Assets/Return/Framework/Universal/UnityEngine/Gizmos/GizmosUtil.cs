using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using System;
using System.Linq;

namespace Return
{
    public partial class GizmosUtil : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void LoadInstance()
        {
            var util = new GameObject(nameof(GizmosUtil)).AddComponent<GizmosUtil>();
            util.hideFlags = HideFlags.DontSave;
            DontDestroyOnLoad(util);
            instance = util;
        }

        private static GizmosUtil instance;

        static HashSet<IDrawGizmos> drawers;
        public static event Action OnGizmos
        {
            add
            {
                if (instance == null)
                    LoadInstance();

                instance.OnDrawGizmo -= value;
                instance.OnDrawGizmo += value;
            }

            remove
            {
                if (instance!=null)
                    instance.OnDrawGizmo -= value;
            }
        }

        public static void AddDrawer(IDrawGizmos drawer)
        {
            drawers.Add(drawer);
        }

        public static void RemoveDrawer(IDrawGizmos drawer)
        {
            drawers.Remove(drawer);
        }

        public event Action OnDrawGizmo;

        private void Awake()
        {
            drawers = new(50);
        }

        private void OnDrawGizmos()
        {
            OnDrawGizmo?.Invoke();

            foreach (var draw in drawers)
            {
                try
                {
                    draw?.DrawGizmos();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    drawers.Remove(draw);
                    return;
                }
            }
        }


        public static void DrawWireRegularPolygon(int vertexCount, Vector3 center, Quaternion rotation, float radius)
        {
            Vector3 previousPosition = Vector3.zero;

            float step = 2f * Mathf.PI / vertexCount;

            float offset = Mathf.PI * 0.5f + ((vertexCount % 2 == 0) ? step * 0.5f : 0f);

            for (int i = 0; i <= vertexCount; i++)
            {
                float theta = step * i + offset;

                float x = radius * Mathf.Cos(theta);
                float y = radius * Mathf.Sin(theta);

                Vector3 nextPosition = center + rotation * new Vector3(x, y, 0f);

                if (i == 0)
                {
                    previousPosition = nextPosition;
                }
                else
                {
                    Gizmos.DrawLine(previousPosition, nextPosition);
                }

                previousPosition = nextPosition;
            }
        }

        public class DrawCircle : IDrawGizmos
        {
            public DrawCircle(Vector3 position, Quaternion rotation, float radius, Color color, int edges = 12, Transform transform = null)
            {
                points = Geometry.Circle(position, rotation * Vector3.up, radius, edges);
                ITransform = transform;
                Color = color;
            }

            protected Vector3[] points;
            protected Transform ITransform;
            protected Color Color;

            public void DrawGizmos()
            {
                Gizmos.color = Color;
                if (points != null)
                {
                    var length = points.Length;
                    if (ITransform != null)
                    {
                        var lastPos = ITransform.TransformPoint(points[length - 1]);
                        for (int i = 0; i < length; i++)
                        {
                            var pos = ITransform.TransformPoint(points[i]);
                            Gizmos.DrawLine(lastPos, pos);
                            lastPos = pos;
                        }
                    }
                    else
                    {
                        var lastPos = points[length - 1];
                        for (int i = 0; i < length; i++)
                        {
                            var pos = points[i];
                            Gizmos.DrawLine(lastPos, pos);
                            lastPos = pos;
                        }
                    }
                }
            }
        }

        public class DrawCross : IDrawGizmos
        {
            public static Color Color_X = Color.red;
            public static Color Color_Y = Color.green;
            public static Color Color_Z = Color.blue;

            float Size = 1f;

            Vector3 Position;

            public DrawCross(Vector3 position, float size)
            {
                Size = size;
                Position = position;
            }

            public void DrawGizmos()
            {
                float d = Size * 0.5f;
                Vector3 p = Position;
                Gizmos.color = Color_X;
                Gizmos.DrawLine(p + Vector3.left * d, p + Vector3.right * d);
                Gizmos.color = Color_Y;
                Gizmos.DrawLine(p + Vector3.up * d, p + Vector3.down * d);
                Gizmos.color = Color_Z;
                Gizmos.DrawLine(p + Vector3.back * d, p + Vector3.forward * d);
            }
        }


    }

}

