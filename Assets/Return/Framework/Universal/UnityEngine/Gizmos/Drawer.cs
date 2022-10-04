using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    [Obsolete]
    public class Drawer
    {
        static DrawingHandle Handler;

        public static DrawingHandle GetHandler()
        {
            if (Handler.IsNull())
            {
                Handler = new GameObject("Drawer").AddComponent<DrawingHandle>();
                Handler.hideFlags = HideFlags.DontSave;
                Handler.Jobs = new(1);
            }

            return Handler;
        }


        /// <summary>
        /// Draw lines in gizmos and delegate to handle.
        /// </summary>
        public static DrawLines DrawLines(Color color,bool seal, params Vector3[] points)
        {
            var handle = new DrawLines()
            {
                Color = color,
                seal = seal,
                Points = points
            };

            GetHandler().Jobs.Add(handle);

            return handle;
        }

        public static void DrawFireBox(Color color,float edge,Vector3 position,Quaternion rotation)
        {

        }

        /// <summary>
        /// Draw lines in gizmos.
        /// </summary>
        public static void DrawGizmosLines(Color color,bool seal, params Vector3[] points)
        {
            if (color == default)
                color = Color.white;

            Gizmos.color = color;

            var length = points.Length;

            Vector3 lastPoint= points[0];

            if (seal)
                Gizmos.DrawLine(points[length - 1], lastPoint);


            for (int i = 1; i < length; i++)
            {
                var pos = points[i];
                Gizmos.DrawLine(lastPoint, pos);

                lastPoint = pos;
            }           
        }



        /// <summary>
        /// Delegate GL drawer 
        /// </summary>
        public static void DrawLines(Color color, float time, params Vector3[] points)
        {
            
        }


        public static void DrawLines(Color color, params Vector3[] points)
        {
            if (color == default)
                color = Color.white;

            var length = points.Length;


            GL.Begin(GL.LINES);

            GL.Color(color);

            GL.Vertex(points[length - 1]);

            for (int i = 0; i < length; i++)
            {
                GL.Vertex(points[i]);
            }

            GL.End();
        }



    }

    [Obsolete]
    public abstract class DrawJob : NullCheck,IDisposable
    {
        public virtual bool valid { get; set; } = true;

        public abstract void Draw();

        public virtual bool Valid() => valid;

        public virtual void Dispose() { }
    }

    public abstract class BaseDrawGizmos : DrawJob
    {

    }

    public abstract class DrawGL : DrawJob
    {

    }

    public class DrawLines : BaseDrawGizmos
    {
        public Color Color;
        public Vector3[] Points;

        public bool seal = false;

        public override void Draw()
        {
            Drawer.DrawGizmosLines(Color,seal, Points);
        }

        public override void Dispose()
        {
            Points = null;
        }
    }

    public class DrawGizmos : BaseDrawGizmos
    {
        

        public override void Draw()
        {

        }

        public override void Dispose()
        {

        }
    }

    [Obsolete]
#if UNITY_EDITOR
    [ExecuteInEditMode]
#endif
    public class DrawingHandle : MonoBehaviour
    {
        [SerializeField]
        Material m_Material;

        public Material Material 
        {
            get => m_Material; 
            set
            {
                if (Material != null)
                {
                    if(!Materials.IsManaged(Material))
                    {
#if UNITY_EDITOR
                        if (!UnityEditor.EditorApplication.isPlaying)
                            DestroyImmediate(m_Material, false);
                        else
                            Destroy(m_Material);
#else
                        Destroy(m_Material);
#endif
                    }

                    m_Material = value;
                }

            }
        }

        protected virtual void Start()
        {
            if (Material == null)
                Material = Materials.Unlit;

            if (Jobs == null)
                Jobs = new();
        }

        public List<DrawJob> Jobs;

        
        bool Valid(DrawJob job)
        {
            var valid = job.Valid();

            if (!valid)
                Jobs.Remove(job);

            return valid;
        }

        protected virtual void OnDrawGizmos()
        {
            if (Jobs.NullorEmpty())
                return;

            foreach (var job in Jobs)
            {
                try
                {
                    if (job is not BaseDrawGizmos)
                        continue;

                    if (!Valid(job))
                        return;

                    job.Draw();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        protected virtual void OnRenderObject()
        {
            if (Jobs.NullorEmpty())
                return;

            foreach (var job in Jobs)
            {
                try
                {
                    if (job is not DrawGL)
                        continue;

                    if (!Valid(job))
                        return;

                    job.Draw();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}