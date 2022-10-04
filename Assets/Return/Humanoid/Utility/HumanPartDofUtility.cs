using UnityEngine;

namespace Return
{
    internal static class HumanPartDofUtility
    {



        public static HumanPartDof[] RightHand = new HumanPartDof[]
        {
            HumanPartDof.RightThumb,
            HumanPartDof.RightIndex,
            HumanPartDof.RightMiddle,
            HumanPartDof.RightRing,
            HumanPartDof.RightLittle,
        };

        public static HumanPartDof[] LeftHand = new HumanPartDof[]
        {
            HumanPartDof.LeftThumb,
            HumanPartDof.LeftIndex,
            HumanPartDof.LeftMiddle,
            HumanPartDof.LeftRing,
            HumanPartDof.LeftLittle
        };
    }
}