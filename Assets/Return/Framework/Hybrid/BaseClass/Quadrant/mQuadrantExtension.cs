using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class mQuadrantExtension
    {

        public static Anchor TakeSide(this Transform origin, Transform Target)
        {
            var go = Vector3.Dot(origin.right, (Target.position - origin.position).normalized);//dot1>0==right <0==left
            if (go > 0)
            {
                return Anchor.Right;
            }
            else if (go < 0)
            {
                return Anchor.Left;
            }
            else
            {
                return Anchor.Middle;
            }
        }

        public static Anchor TakeSide(this Vector3 Target, Transform Origin)
        {
            var go = Vector3.Dot(Origin.right, (Target - Origin.position).normalized);//dot1>0==right <0==left
            if (go > 0)
            {
                return Anchor.Right;
            }
            else if (go < 0)
            {
                return Anchor.Left;
            }
            else
            {
                return Anchor.Middle;
            }
        }

        public static Order TakeOrder(this Vector3 Target, Transform Origin)
        {
            var go = Vector3.Dot(Origin.forward, (Target - Origin.position).normalized);//dot1>0==right <0==left
            if (go > 0)
            {
                return Order.Front;
            }
            else if (go < 0)
            {
                return Order.After;
            }
            else
            {
                return Order.Front | Order.After;
            }
        }
    }
}
