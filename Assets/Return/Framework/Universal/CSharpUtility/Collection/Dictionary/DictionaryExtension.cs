using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

namespace Return
{
    public static class DictionaryExtension
    {
        public static bool TryGetKey<TKey,TValue>(this IDictionary<TKey, TValue> dic,TValue value,out TKey key)
        {
            var result = dic.FirstOrDefault(x => x.Value.Equals(value));

            if(result.Equals(default))
            {
                key = default;
                return false;
            }

            key = result.Key;
            return true;
        }


        public static bool Register<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value, bool Override)
        {
            if (dictionary.ContainsKey(key))
            {
                if (Override)
                {
                    dictionary[key] = value;
                }
                else
                    return false;
            }
            else
                dictionary.Add(key, value);

            return true;
        }

        /// <summary>
        /// Add the pair if dictionary doesn't contain it, otherwise change it to new value.
        /// </summary>
        /// <typeparam name="T1"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="dictionary"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SafeAdd<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value)
        {
            if (key == null)
                return;

            if (dictionary.ContainsKey(key))
                dictionary[key] = value;
            else
                dictionary.Add(key, value);
        }

        /// <summary>
        /// Add new pair if dictionary doesn't contains this key
        /// </summary>
        public static void CheckAdd<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key, T2 value = default)
        {
            Assert.IsTrue(key != null);

            if (dictionary.ContainsKey(key))
                return;
            else
                dictionary.Add(key, value);
        }




        public static void SafeRemove<T1, T2>(this Dictionary<T1, T2> dictionary, T1 key)
        {
            Assert.IsTrue(key != null);

            if (dictionary.ContainsKey(key))
                dictionary.Remove(key);
        }

        public static void RemoveByValue<T1, T2>(this Dictionary<T1, T2> dictionary, T2 value)
        {
            if (value == null)
                return;



            if (dictionary.ContainsValue(value))
            {
                var Key = dictionary.FirstOrDefault(x => x.Value.Equals(value)).Key;
                dictionary.Remove(Key);
            }
        }

        public static void RemoveValues<T1, T2>(this Dictionary<T1, T2> dictionary, T2 value)
        {
            if (value == null)
                return;

            if (dictionary.ContainsValue(value))
            {
                using  var keys = dictionary.
                Where(x => x.Value.Equals(value)).
                Select(x => x.Key).
                CacheLoop();

                foreach (T1 key in keys)
                {
                    dictionary.Remove(key);
                }
            }
        }
    }
}