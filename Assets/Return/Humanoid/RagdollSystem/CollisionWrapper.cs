using UnityEngine;
using System;
using System.Collections.Generic;
using Return.Physical;

namespace Return.Physical
{
    public class CollisionWrapper : IDisposeEvent<CollisionWrapper>
    {
        public CollisionWrapper(params Collider[] colliders)
        {
            TargetColliders = new HashSet<Collider>(colliders);
        }
        public readonly HashSet<Collider> TargetColliders;

        public ContactsArg[] Args = new ContactsArg[3];

        #region Subscribe State
        public bool Enter { get; protected set; } = false;
        public event EventHandler<ContactsArg> OnCollisionEnter;
        public  bool Stay { get; protected set; } = false;
        public event EventHandler<ContactsArg> OnCollisionStay;
        public bool Exit { get; protected set; } = false;
        public event EventHandler<ContactsArg> OnCollisionExit;
        /// <summary>
        /// Event only fire when collsion with this layer
        /// </summary>
        public LayerMask SensorMask;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="threde">Collision threde to submit.</param>
        /// <param name="collisionNum">Enable collision number at once.</param>
        /// <param name="enable">Enable subscribe or disable subscribe</param>
        public virtual void RegisterCollisionEvent(CollisionThrede threde, int collisionNum, bool enable = true)
        {
            var argSn = (int)threde;

            if (enable)
                Args[argSn] = new ContactsArg(collisionNum);
            else
                Args[argSn] = null;

            switch (threde)
            {
                case CollisionThrede.Enter:
                    Enter = enable;
                    break;
                case CollisionThrede.Stay:
                    Stay = enable;
                    break;
                case CollisionThrede.Exit:
                    Stay = enable;
                    break;
            }
        }
        
        /// <summary>
        /// CheckAdd collision whether has require contact and invoke the event arg.
        /// </summary>
        public virtual void Contact(Collision collision, CollisionThrede threde)
        {
            if (!TargetColliders.Contains(collision.collider))
                return;

            var arg = Args[(int)threde];
            var num = 0;
            foreach (var contact in collision.contacts)
            {
                if (SensorMask != (SensorMask | (1 << contact.otherCollider.gameObject.layer)))
                    continue;

                arg.Contacts[num] = contact;

                if (num < arg.ContactCapacity)
                    num++;
                else
                    break;
            }

            if (num == default)
                return;

            arg.ContactNum = num;
            OnCollisionEnter.Invoke(this, arg);
        }


        public event Action<CollisionWrapper> OnDispose;
        public void Dispose()
        {
            Args = null;
            OnDispose?.Invoke(this);
        }
        #endregion
    }

    public class ContactsArg : EventArgs
    {
        public ContactsArg(int count)
        {
            Contacts = new ContactPoint[count];
            ContactCapacity = count;
        }
        public ContactPoint[] Contacts { get; protected set; }
        public readonly int ContactCapacity;
        public int ContactNum;
    }
}
