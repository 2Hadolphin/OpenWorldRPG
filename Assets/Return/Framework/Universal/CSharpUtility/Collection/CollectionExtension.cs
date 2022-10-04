using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Assertions;

namespace Return
{
    public static class CollectionExtension
    {

        /// <summary>
        /// Get first value inside collection with is type of U.
        /// Invalid cast **dictionary-key value pair.
        /// </summary>
        public static bool TryGetValueOfType<T, U>(this ICollection<U> collection, out T value) //where T : U
        {
            foreach (var item in collection)
            {
                //Debug.Log($"Valid collection element [{item} : {typeof(T)}].");
                if (item is T targetValue)
                {
                    value = targetValue;
                    return true;
                }
            }

            value = default;
            return false;
        }

        public static IEnumerable<T2> SelectWhere<T, T2>(this IEnumerable<T> values, Func<T, bool> valid, Func<T, T2> convert)
        {
            foreach (var value in values)
                if (valid(value))
                    yield return convert(value);
        }
    }
}