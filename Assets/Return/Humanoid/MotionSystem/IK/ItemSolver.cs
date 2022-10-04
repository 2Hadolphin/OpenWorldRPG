using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public class ItemSolver : MonoBehaviour
    {
        protected Transform Transform;
        protected Vector3 Offset;

        /// <summary>
        /// virtual handle position relavite to item transform
        /// </summary>
        public virtual Vector3 HandlerPosition { get; set; }
        public virtual Quaternion HandlerRotation { get; set; }
        public Renderer Renderer;
        public Bounds Bounds => Renderer.bounds;

        void Start()
        {
            Transform = transform;
        }

        public ICoordinate GetCoordinate()
        {
            return new Coordinate_Offset(Transform, Bounds.center, Quaternion.identity);
        }
    }
}