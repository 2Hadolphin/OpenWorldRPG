using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Return.Modular;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;

namespace Return.InteractSystem
{
    /// <summary>
    /// PhysicsOverlap at require volume and invoke targeted event.
    /// </summary>
    public class SelectionVolume : BaseSelectionHandler
    {
        public override event Action OnSelectedChanged;
        public override event Action<InteractWrapper> OnSelected;
        public override event Action<InteractWrapper> OnDeselected;

        //  Senser Setting
        [ShowInInspector]
        private ICoordinate m_Transform;
        public ICoordinate Transform { get => m_Transform; set => m_Transform = value; }


        // cache
        protected Collider[] colliders;

        #region Routine

        protected virtual void OnEnable()
        {
            if (Transform.IsNull())
            {
                enabled = false;
                return;
            }

            cacheColliders = new(maxRaycastNumber,TargetTypes.Count);
            colliders = new Collider[maxRaycastNumber];

            id = id.Random();
            UniTask.Create(OnSensorUpdate);
        }

        protected virtual void OnDisable()
        {
            if (Transform.IsNull())
                return;

            //Deselect();
        }
        #endregion


        byte id;

        protected virtual async UniTask OnSensorUpdate()
        {
            var id = this.id;

            while (this && isActiveAndEnabled && id == this.id)
            {
                // cache State
                bool change = false;
                ps_newIDs.Clear();

                var pos = Transform.position;

                var num = Physics.OverlapSphereNonAlloc(pos, senserRange, colliders, selectableMask, triggerInteraction);

                for (int i = 0; i < num; i++)
                {
                    var col = colliders[i];
                    var hash = col.GetInstanceID();

                    if (!ps_newIDs.Add(id))
                    {
                        Debug.LogError("ColliderRepeat " + col);
                        continue;
                    }

                    if (cacheColliders.ContainsKey(hash))
                        continue;

                    foreach (var type in TargetTypes)
                    {
                        if (!col.TryGetComponent(type, out var component))
                            continue;

                        change |= true;
                        var wrapper = new InteractWrapper(component);
                        cacheColliders.Add(hash, wrapper);
                        OnSelected?.Invoke(wrapper);
                    }
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

                if (updateRate <= 0)
                    await UniTask.NextFrame();
                else
                    await UniTask.Delay(TimeSpan.FromSeconds(updateRate));
            }
        }


        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, senserRange);
        }
    }

}