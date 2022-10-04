using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    /// <summary>
    /// Right-Left-Both
    /// </summary>
    [Flags]
    public enum Side { Right = 1, Left = 2, Both = Right | Left }
    public enum Anchor { Middle = 0, Right = 1, Left = 2 }

    /// <summary>
    /// Right-Left-Both-Deny
    /// </summary>
    [Flags]
    public enum Symmetry { Deny = 0, Right = 1, Left = 2, Both = 3 }


    [Flags]
    public enum Order { Front = 1, After = 2,Both=3 }


    public enum Set { Intersection, Union, Subtraction }


    public static class QuadrantExtension 
    {

        
        public static Symmetry Mirror(this Symmetry enableHand)
        {
            //enableHand ^= enableHand;

            if (enableHand.HasFlag(Symmetry.Right))
                enableHand ^= Symmetry.Right;

            if (enableHand.HasFlag(Symmetry.Left))
                enableHand ^= Symmetry.Left;

            return enableHand;

            switch (enableHand)
            {
                case Symmetry.Right:
                    return Symmetry.Left;
                case Symmetry.Left:
                    return Symmetry.Right;
            }

            Debug.LogError( new KeyNotFoundException(enableHand.ToString()));

            return enableHand;
        }

    }


}
