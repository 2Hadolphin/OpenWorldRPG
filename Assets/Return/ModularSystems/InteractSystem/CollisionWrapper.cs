using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace Return.Items
{
    /// <summary>
    /// Wrapping item collider via Ucollider (**Physic Interact **Selected Box)
    /// </summary>
    public abstract class CollisionWrapper : BaseComponent
    {
        [ShowInInspector]
        public Rigidbody RB { get; protected set; }
        protected ContactPoint[] Contacts = new ContactPoint[5];

        public event Action<int,ICollection<ContactPoint>> ContactPost;

        //wrap mode => real rigidbody or simulation
        public enum WrapMode 
        { 
            /// <summary>
            /// Collision only
            /// </summary>
            None, 
            /// <summary>
            /// SetHandler rigidbody
            /// </summary>
            Rigidbody ,
            /// <summary>
            /// Kinematic
            /// </summary>
            Simulation
        }

        WrapMode _Mode=WrapMode.None;

        public WrapMode Mode
        {
            get => _Mode;
            set
            {
                if (value == _Mode)
                    return;

                switch (value)
                {
                    case WrapMode.None:
                        if (RB)
                            Destroy(RB);
                        break;
                    case WrapMode.Rigidbody:
                        RB = gameObject.InstanceIfNull<Rigidbody>();
                        RB.isKinematic = false;
                        break;
                    case WrapMode.Simulation:
                        RB = gameObject.InstanceIfNull<Rigidbody>();
                        RB.isKinematic = true;
                        break;
                }
                _Mode = value;
            }
        }

        protected virtual void Awake()
        {

        }

        protected virtual void OnEnable()
        {


        }

        protected virtual void OnDisable()
        {

        }

        private void OnCollisionEnter(Collision collision)
        {
            
        }
        private void OnCollisionStay(Collision collision)
        {
            var length=collision.GetContacts(Contacts);

            if (length > 0)
                ContactPost?.Invoke(length, Contacts);

        }
        private void OnCollisionExit(Collision collision)
        {
            
        }


    }
}