using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace Return
{
    public partial class GizmosUtil  // Extract handle for Unitask
    {
        public class GizmosDelegate : IDrawGizmos,IDisposable
        {
            public Action DrawGizmos;

            public void Dispose()
            {
                DrawGizmos = null;
            }

            void IDrawGizmos.DrawGizmos()
            {
                DrawGizmos();
            }

            public static implicit operator GizmosDelegate(Action action)
            {
                return new GizmosDelegate() { DrawGizmos = action };
            }
        }

        /// <summary>
        /// Return draw gizmos delegate handle. 
        /// </summary>
        /// <param name="center"></param>
        /// <param name="size"></param>
        /// <param name="color"></param>
        /// <returns></returns>
        public static GizmosDelegate DrawWireCube(Vector3 center, Vector3 size, Color color = default)
        {
            void draw()
            {
                Gizmos.color = color;
                Gizmos.DrawWireCube(center, size);
            }
            
            return (GizmosDelegate)draw;
        }

        public static GizmosDelegate DrawWireSphere(Vector3 center, float radius, Color color = default)
        {
            void draw()
            {
                Gizmos.color = color;
                Gizmos.DrawWireSphere(center, radius);
            }
            return (GizmosDelegate)draw;
        }

        public static GizmosDelegate DrawWireMesh(Mesh mesh, int subMeshIndex, Vector3 position, Quaternion rotation, Vector3 scale, Color color = default)
        {
            void draw()
            {
                Gizmos.color = color;
                Gizmos.DrawWireMesh(mesh, subMeshIndex, position, rotation, scale);
            }
            return (GizmosDelegate)draw;
        }



    }


}


