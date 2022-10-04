using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Return
{
    public static class mFlagExtension
    {


        public static int[] GetFlagsValue<T>(this int flags) where T : Enum
        {
            var list = new List<int>();

            foreach (int r in Enum.GetValues(typeof(T)))
                if ((flags & r) != 0)
                    list.Add(r);

            return list.ToArray();
        }


        /// <summary>
        /// Return flags order E.g [] {1,4,8,32}
        /// </summary>
        public static int[] GetFlagsOrder<T>(this T flags) where T : struct
        {
            var list = new List<int>();
            var values = (int[])Enum.GetValues(typeof(T));
            var flag = (int)(object)flags;
            var length = values.Length;

            for (int i = 0; i < length; i++)
            {
                if ((flag & values[i]) != 0)
                    list.Add(i);
            }

            return list.ToArray();
        }




        public static Dictionary<int, string> EnumNamedValues<T>() where T : Enum
        {
            var result = new Dictionary<int, string>();
            var values = Enum.GetValues(typeof(T));

            foreach (int item in values)
                result.Add(item, Enum.GetName(typeof(T), item));
            return result;
        }
    }
}