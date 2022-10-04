using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
//using Cysharp.Threading.Tasks;

namespace Return
{
    /// <summary>
    /// Invoke event when transform has changed.
    /// </summary>
    public class TransformPoster : MonoBehaviour
    {
        public virtual event Action<Transform> OnMove;
        public virtual event Action<Vector3,Quaternion> OnTransformUpdate;


        [SerializeField]
        Transform m_Transform;

        public Transform Transform 
        { 
            get=>m_Transform; 
            set
            {
                if (value != null)
                    value.hasChanged = false;

                m_Transform = value;
            }
        }


        protected virtual void Awake()
        {
            Transform = transform;
        }

        protected virtual void Update()
        {
            if (Transform.hasChanged)
            {
                OnMove?.Invoke(Transform);
                OnTransformUpdate?.Invoke(Transform.position,Transform.rotation);

                Transform.hasChanged = false;
            }
        }
    }
}