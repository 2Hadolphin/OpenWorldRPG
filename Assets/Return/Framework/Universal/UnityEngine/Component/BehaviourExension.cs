using System;
using UnityEngine;

namespace Return
{
    public static class BehaviourExension
    {
        public static bool IsBehaviour(this Type type)
        {
            return type.IsSubclassOf(typeof(Behaviour));
        }

        /// <summary>
        /// CheckAdd behaviour enable and change state.
        /// </summary>
        /// <returns>return true if state going to change</returns>
        public static bool CheckEnable<T>(this T behaviour, bool enable = true) where T : Behaviour
        {
            var changeState = behaviour.enabled != enable;

            if (changeState)
                behaviour.enabled = enable;

            return changeState;
        }



    }
}