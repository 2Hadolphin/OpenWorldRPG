using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Return.Humanoid.Character;
using Return.Items;
using UnityEngine.Assertions;

namespace Return.Gear
{
    public class EquipmentSlot : MonoBehaviour, IEquatable<BaseSlotBinding>
    {
        [ShowInInspector, ReadOnly]
        public new Transform transform { get; protected set; }
        [ShowInInspector, ReadOnly]
        public new GameObject gameObject { get; protected set; }
        [ShowInInspector,ReadOnly]
        public BaseSlotBinding Binding { get; protected set; }

        [ShowInInspector, ReadOnly]
        public IPickup Item;

        [Button("Add Slot Item")]
        public virtual bool AddItem(IPickup pickup)
        {
            Assert.IsNotNull(pickup);

            var bindings = Binding.EnableSlotType.GetEnumerator();

            bool enable=false;

            while (bindings.MoveNext() && !enable)
            {
                var SlotCategories = bindings.Current.EnableCategories.GetEnumerator();

                while (SlotCategories.MoveNext() && !enable)
                {
                    var itemTypes = pickup.Preset.Categories.GetEnumerator();

                    while (itemTypes.MoveNext() && !enable)
                    {
                        if (SlotCategories.Current.Contains(itemTypes.Current))
                            enable = true;
                    }
                }
            }

            if (enable)
            {
                Item = pickup;
                if (pickup is Component component)
                    component.ParentOffset(transform);
                //m_items.Grab().Transform.SetParent(transform);
            }

            return enable;
        }

        [Button("Init")]
        public virtual void Init(BaseSlotBinding binding)
        {
            if (!transform)
            {
                var tf = base.transform;
                gameObject = new GameObject(binding.Title);
                transform = gameObject.transform;
                transform.parent = tf;
            }
            Binding = binding;
        }

        private void OnDestroy()
        {
            Destroy(gameObject);
        }

        public bool Equals(BaseSlotBinding other)
        {
            return other == Binding;
        }
    }
}