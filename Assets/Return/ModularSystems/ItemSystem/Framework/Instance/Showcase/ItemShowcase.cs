using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Cysharp.Threading.Tasks;
using Return.Agents;

namespace Return.Items
{
    /// <summary>
    /// Class to wrap item as preview object 
    /// </summary>
    public class ItemShowcase : Pickup, IPoolDebit
    {
        protected AbstractItem cacheInstance { get; set; }

        #region IPickup

        public override void Drop(PR pr)
        {

            transform.SetWorldPR(pr);
        }

        public override void Drop(IPickupableProvider provider)
        {
            provider.SetPickupable(this);
        }


        #endregion


        #region IItem

        /// <summary>
        /// Load child models.
        /// </summary>
        public override void Register(IAgent agent)
        {
            base.Register(agent);
        }

        /// <summary>
        /// SetHandler child models.
        /// </summary>
        public override async UniTask Activate()
        {
            
        }

        /// <summary>
        /// Unload child models.
        /// </summary>
        public override async UniTask Deactivate()
        {
            
        }

        #endregion



        public override ICoordinate GetHandle(string key)
        {

            return base.GetHandle(key);
        }


        #region Showcase
        //public virtual Pool mPool;
        public virtual void LoadInfo(ItemPreset preset)
        {
            Preset = preset;
        }

        public virtual void Retrieve()
        {
            //Clear();
            //mPool.Return;
        }

        /// <summary>
        /// Spawn at point
        /// </summary>
        public virtual void Spawn(Vector3 pos, Quaternion rot, bool enableRigidbody = false)
        {
            if (enableRigidbody)
                SetRigidbody();

            var tf = transform;
            tf.parent = null;
            tf.position = pos;
            tf.rotation = rot;
        }

        public virtual void Spawn(Bounds Zone, bool enableRigidbody=false)
        {
            if (enableRigidbody)
                SetRigidbody();
            var tf = transform;
            tf.parent = null;
            tf.position =
                new Vector3(
                Random.Range(-Zone.extents.x, Zone.extents.x),
                Random.Range(-Zone.extents.y, Zone.extents.y),
                Random.Range(-Zone.extents.z, Zone.extents.z))
                + Zone.center;
            tf.rotation = Random.rotation;
        }

        /// <summary>
        /// Spawn at target
        /// </summary>
        public virtual void Spawn(Transform parent,Vector3 pos, Quaternion rot)
        {
            var tf = transform;
            tf.parent = parent;
            tf.localPosition = pos;
            tf.localRotation = rot;
        }

        public virtual void Display()
        {
            // show and enable trigger
        }

        protected virtual void SetRigidbody()
        {
            var go = gameObject;
            var rb = go.InstanceIfNull<Rigidbody>();
            rb.mass = Preset.Weight;
        }
        #endregion

  

        #region Base
        //public virtual bool HasOwner(Humanoid.IPlayerFroegin certificate)
        //{
        //    return Owner == null ? false : Owner == certificate ? false : true;
        //}

        #endregion


        //?????
        public IEnumerator Motion(ICoordinate coordinate, PR acceleration)
        {
            var rb = gameObject.InstanceIfNull<Rigidbody>();

            rb.AddForce(acceleration.Position, ForceMode.VelocityChange);

            var times = 4;
            var wait = ConstCache.WaitForFixedUpdate;

            var getInertia = new GetCoordinateInertia(coordinate);

            for (int i = 0; i < times; i++)
            {
                yield return wait;
                getInertia.Record();
            }

            var ineria = getInertia.Inertia;
            rb.AddForce(ineria.Position, ForceMode.VelocityChange);


            yield break;
        }

        protected virtual void Awake()
        {
            //CollisionWrapper.Mode = CollisionWrapper.WrapMode.None;
            //?????? static or wait balance

            var tf = transform;
            var go = gameObject;
            go.layer = PhysicConfig.Instance.ItemMask.ToLayer();
            var col = go.AddComponent<MeshCollider>();
            col.convex = true;
            var radius = col.bounds.max.magnitude;

            var pos = tf.position;
            var results = UnityEngine.Physics.OverlapSphere(pos, radius);
            var v = Vector3.zero;
            foreach (var result in results)
            {
                var p = result.bounds.ClosestPoint(pos);
                v += pos - p;
            }

            v = v.Multiply(1f / results.Length);

            tf.position += Vector3.up;
        }


    }
}