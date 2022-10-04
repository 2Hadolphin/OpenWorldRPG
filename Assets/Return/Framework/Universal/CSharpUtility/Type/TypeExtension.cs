using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace Return
{
    public static class TypeExtension
    {
        /// <summary>
        /// Get fields with custome attribute. 
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type to match.</typeparam>
        /// <param name="type">Target type to search.</param>
        public static FieldInfo[] GetFields<TAttribute>(this Type type, BindingFlags bindingFlags)
        {
            var methods = type.GetFields(bindingFlags);
            return Strip(methods, typeof(TAttribute));
        }

        /// <summary>
        /// Get properties with custome attribute. 
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type to match.</typeparam>
        /// <param name="type">Target type to search.</param>
        public static PropertyInfo[] GetProps<T>(this Type type, BindingFlags bindingFlags)
        {
            var methods = type.GetProperties(bindingFlags);
            return Strip(methods, typeof(T));
        }

        /// <summary>
        /// Get methods with custome attribute. 
        /// </summary>
        /// <typeparam name="TAttribute">Attribute type to match.</typeparam>
        /// <param name="type">Target type to search.</param>
        public static MethodInfo[] GetMethods<T>(this Type type, BindingFlags bindingFlags)
        {
            var methods = type.GetMethods(bindingFlags);
            return Strip(methods, typeof(T));
        }


        /// <summary>
        /// Sort and filter memberInfos.
        /// </summary>
        public static T[] Strip<T>(T[] infos,Type define) where T: MemberInfo
        {
            var length = infos.Length;

            var queue = new Queue<T>(length);

            for (int i = 0; i < length; ++i)
            {
                var info = infos[i];

                if (!info.IsDefined(define, true))
                    continue;

                queue.Enqueue(info);
            }

            return queue.ToArray();
        }
    }


}