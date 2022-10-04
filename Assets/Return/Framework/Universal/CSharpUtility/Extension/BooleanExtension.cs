using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public static class BooleanExtension
    {
        /// <summary>
        /// true Positive =>1, false Negative => -1
        /// </summary>
        public static int ToDirection(this bool value)
        {
            return value ? 1 : -1;
        }

        /// <summary>
        /// true Positive =>-1, false Negative => 1
        /// </summary>
        public static int ToInvertDirection(this bool value)
        {
            return value ? -1 : 1;
        }

        /// <summary>
        /// make value equals parameter prevent overwrite same shit
        /// </summary>
        /// <param name="value">Reference field</param>
        /// <param name="parameter">Target value</param>
        /// <returns>Has change field value</returns>
        public static bool SetAs(this ref bool value, bool parameter)
        {
            var valid = value ^ parameter;
            if (valid)
                value = parameter;

            return valid;
        }


        public static bool SetAs<T>(this ref T value, T parameter) where T : struct
        {
            var valid = !value.Equals(parameter);
            if (valid)
                value = parameter;

            return valid;
        }

        //public static bool SetAs<T>(this ref T value, T parameter) where T : UnityEngine.Object
        //{
        //    var valid = !value.Equals(parameter);
        //    if (valid)
        //        value = parameter;

        //    return valid;
        //}

    }
}