using UnityEngine;
using System.Collections.Generic;
using Return.Definition;
using System;
using Cysharp.Threading.Tasks;
using System.Threading;
using Return.Physical;
using Sirenix.OdinInspector;

namespace Return.InteractSystem
{


    /// <summary>
    /// OnSensorUpdate environment object with tag or layer by raycast.
    /// </summary>
    public class SelectionPointer : BaseSelectionHandler
    {
        public override event Action OnSelectedChanged;
        public override event Action<InteractWrapper> OnSelected;
        public override event Action<InteractWrapper> OnDeselected;

        //  Senser Setting
        [SerializeField]
        private Transform m_ViewTransform;
        public Transform ViewTransform 
        {
            get 
            {
                //if (m_ViewTransform.IsNull())
                //    m_ViewTransform = CameraManager.mainCameraHandler.mainCameraTransform;

                return m_ViewTransform;
            }
            set => m_ViewTransform = value; 
        }

        public HashSet<Collider> IgnoreColliders { get; protected set; } = new(5);

        #region Routine

        protected virtual void OnEnable()
        {
            if (ViewTransform.IsNull())
            {
                enabled = false;
                return;
            }

            cacheColliders = new(maxRaycastNumber,TargetTypes.Count);
            //LastHits = new(maxRaycastNumber);

            id = id.Random();
            UniTask.Create(OnSensorUpdate);
        }

        protected virtual void OnDisable()
        {
            if (ViewTransform.IsNull())
                return;

            Deselect();
        }

        #endregion


        byte id;

        protected virtual async UniTask OnSensorUpdate()
        {
            var id = this.id;

            while (this && isActiveAndEnabled && id == this.id)
            {
                UpdateRaycast();
                if (updateRate <= 0)
                    await UniTask.NextFrame();
                else
                    await UniTask.Delay(TimeSpan.FromSeconds(updateRate), false, PlayerLoopTiming.LastUpdate);
            }
        }

        protected virtual void UpdateRaycast()
        {
            var ray = ViewTransform.GetRay();

            var handle = PhysicUtility.RayCastNonAllocAll(ray, senserRange, selectableMask, triggerInteraction);

            if (handle.hitCount > 0)
            {
                bool change = false;
                ps_newIDs.Clear();

                foreach (var hit in handle)
                {
                    var col = hit.collider;
                    var id = hit.colliderInstanceID;

                    if (!ps_newIDs.Add(id))
                    {
                        Debug.LogError("ColliderRepeat " + col);
                        continue;
                    }

                    change |= ProcessCollider(col, id);
                }

                {
                    // cache hash from last colliders 
                    using var loop = cacheColliders.Keys.CacheLoop();
                    foreach (int key in loop)
                    {
                        // check still exist
                        if (ps_newIDs.Contains(key))
                            continue;

                        // invoke cancel selected event if remove hash successfully.
                        if (cacheColliders.Remove(key, out var wrappers))
                            foreach (var wrapper in wrappers)
                                OnDeselected?.Invoke(wrapper);
                    }

                    if (change)
                        OnSelectedChanged?.Invoke();
                }

            }
            // clean all
            else if (cacheColliders.Count > 0)
            {
                Deselect();

                //LastHits.Clear();
                //DeSelected();
                //VFX.CloseGUI();
            }
        }


        /// <summary>
        /// Valid collider match require order.
        /// </summary>
        /// <param name="collider">ColliderBounds to valid.</param>
        /// <returns>Return true if qualify target added.</returns>
        protected virtual bool ProcessCollider(Collider collider, int hash)
        {
            if (IgnoreColliders.Contains(collider))
                return false;

            // already exist
            if (cacheColliders.ContainsKey(hash))
                return false;

            // add new
            if (string.IsNullOrEmpty(Tag) || collider.CompareTag(Tag))
            {
                foreach (var type in TargetTypes)
                {
                    if (!collider.TryGetComponent(type, out var component))
                        continue;

                    var wrapper = new InteractWrapper(component);


                    cacheColliders.Add(hash, wrapper);
                    Debug.LogFormat("Selected target at frame : {0} Name : {1} Type : {2}",Time.frameCount,collider.gameObject.name,type);
                    OnSelected?.Invoke(wrapper);
                }

                //lastHit = collider;
                //DeSelected();
            }
            else
            {
                //DeSelected();
                //VFX.CloseGUI();
            }

            return true;
        }


        /// <summary>
        /// Cancel select object(last one). **GUI **VFX
        /// </summary>
        protected virtual void Deselect()
        {
            //if (Selected != null) Selected = null;
            //VFX?.DeSelected();

            if (cacheColliders.NotNull())
            {
                foreach (var wrappers in cacheColliders.Values)
                    foreach (var wrapper in wrappers)
                    {
                        Debug.LogFormat("Deselect target at frame : {0} target : {1}", Time.frameCount, wrapper.Interactable);
                        OnDeselected?.Invoke(wrapper);
                    }


                cacheColliders.Clear();
            }
        }


#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (m_ViewTransform.IsNull())
                return;

            Gizmos.color = Color.green;
            //if (LastHits.NotNull())
            //    foreach (var hit in LastHits)
            //        Gizmos.DrawLine(m_Transform.position, hit.transform.position);



        }
#endif
    }
}