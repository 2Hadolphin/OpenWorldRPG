using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    public static class EnumExtension
    {
        /// <summary>
        /// CheckAdd if a flag is selected in the give flag enum property
        /// </summary>
        /// <returns></returns>
        public static bool IsEnumFlagPresent<T>(this T value, T lookingForFlag) where T : Enum
        {
            int intValue = (int)(object)value;
            int intLookingForFlag = (int)(object)lookingForFlag;
            return ((intValue & intLookingForFlag) == intLookingForFlag);
        }
    }
}