using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Equipment.Slot
{
    /// <summary>
    /// Hold item on the transform.
    /// </summary>
    public class Slot : BaseComponent
    {
        protected virtual void Awake()
        {
            if (Transform == null)
                Transform = transform;
        }

        public Transform Transform { get; protected set; }

        public object Item { get; protected set; }


        public virtual bool TrySetSlot(object obj)
        {
            if (Item.NotNull())
                return false;

            Item = obj;
            return true;
        }

        public virtual void SetItem(object obj)
        {
            Item = obj;
        }



        /// <summary>
        /// CheckAdd item or AddItem
        /// </summary>
        //public IItem SlotItem 
        //{
        //    get => m_items.Peek(); 
        //    protected set =>m_items.Push(value);
        //}

        //public bool IsEmpty => SlotItem == null;

        //public void Grab(IPickup pickup)
        //{
        //    var grab = pickup.Grab();
        //    var item = grab.ReadOnlyTransform;

        //    Vector3 pos;
        //    switch (grab.OutputPositionSpace)
        //    {
        //        case Space.World:
        //            pos = item.InverseTransformVector(grab.position - item.position);
        //            break;
        //        case Space.Self:
        //            pos = grab.position;
        //            break;
        //        default:
        //            throw new KeyNotFoundException();
        //    }

        //    item.SetParent(Transform);
        //    item.localPosition = pos;
        //}

        //public void Hold(IPickup pickup, Symmetry hand, bool mirror)
        //{
        //    //pickup.GetHandles(out var prime,out var second);

        //    switch (hand)
        //    {
        //        case Symmetry.Right:

        //            break;
        //        case Symmetry.Left:

        //            break;
        //    }

        //}
        //public IItem Remove()
        //{
        //    return m_items.Pop();
        //}
    }

    /// <summary>
    /// Hold item and limit by category.
    /// </summary>
    public class EquipmentSlot : Slot
    {
        [SerializeField]
        Category m_category;

        /// <summary>
        /// Limit type of slot item.
        /// </summary>
        public Category category { get => m_category; set => m_category = value; }


    }

}