using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System;
using System.Linq;

namespace Return
{
    public static class ArrayExtension
    {

        //public static bool Check<T,U>(this ref T list, U item) where T : class,IList<U>,new ()
        //{


        //    if (list.Contains(item))
        //        return false;

        //    list.Add(item);
        //    return true;
        //}

        /// <summary>
        /// CheckAdd element and add.
        /// </summary>
        /// <returns>return true if new item added.</returns>
        public static bool NullorEmpty<T>(this IList<T> list)
        {
            return list == null || list.Count == 0;
        }

        /// <summary>
        /// Add item to list, overwrite it if already has one
        /// </summary>
        public static void OverwriteRepeat<T>(this IList<T> list,T item)
        {
            var index = list.IndexOf(item);
            if (index > 0)
                list[index] = item;
            else
                list.Add(item);
        }


        #region Array

        public static T FirstNotNull<T>(params T[] values) where T : class
        {
            var length = values.Length;
            for (int i = 0; i < length; i++)
            {
                if (values[i] != null)
                    return values[i];
            }

            throw new KeyNotFoundException();
        }
        public static int[] Max(this float[] array, int numbers)
        {
            var field = new float[numbers];
            var sn = new int[numbers];
            var length = array.Length;
            for (int i = 0; i < length; i++)
            {
                for (int k = 0; k < numbers; k++)
                {
                    if (array[i] > field[k])
                    {
                        field[k] = array[i];
                        sn[k] = i;
                        break;
                    }
                }
            }

            return sn;
        }

        public static T Random<T>(this T[] array)
        {
            if (array.Length > 0)
                return array[UnityEngine.Random.Range(0, array.Length - 1)];
            else
                return default;
        }

        //public static void Remove<T>(this ICollection<T> list,IEnumerable<T> removes) where T :class
        //{
        //    using (var cache = list.CacheLoop())
        //    {
        //        cache

        //    }
        //}

        static HashSet<int> RepeateList = new HashSet<int>(50);
        static Array CacheArray;

        /// <summary>
        /// Please provide checklist for multithreading. Remeber to release cache list after looting.
        /// </summary>
        public static T RandomNotRepeat<T>(this T[] array,ICollection<int> repeatList=null)
        {
            Assert.IsNotNull(array);

            if (repeatList == null)
                repeatList = RepeateList;

            var length = array.Length;

            // handle cache list
            {
                var reset = CacheArray != array;

                if (reset || length == repeatList.Count)
                {
                    CacheArray = array;
                    //Debug.Log("Clean list : " + repeatList.Count);
                    repeatList.Clear();
                }
            }


            if (length > 0)
            {
                int index = UnityEngine.Random.Range(0, length - 1);

                while (repeatList.Contains(index))
                {
                    index++;

                    if (index >= length)
                        index = 0;

                    //Debug.Log("Repeate index : " + (index==0?length-1:index - 1));
                }

                RepeateList.Add(index);

                return array[index];
            }
            else
                return default;
        }

        public static void CleanRandomCache<T>(this T[] array)
        {
            if(CacheArray==array)
            {
                CacheArray = null;
                RepeateList.Clear();
            }    
        }

        public static T Random<T>(this List<T> array)
        {
            var c = array.Count;
            if (c > 0)
            {
                if (c == 1)
                    return array[0];
                else
                    return array[UnityEngine.Random.Range(0, c - 1)];
            }
            else
                return default;
        }

        public static T[] EditArray<T>(this T[] array, T start, Func<T, T> func)
        {
            var length = array.Length;
            array[0] = start;
            for (int i = 1; i < length; i++)
            {
                array[i] = func(array[i - 1]);
            }
            return array;
        }

        public static void AddRange<T>(this ICollection<T> list,IEnumerable<T> add)
        {
            foreach (var item in add)
            {
                list.Add(item);
            }
        }

        public static int Loop<T>(this ICollection<T> list, int sn)
        {
            var length = list.Count;
            return length == 0 ? -1 : MathfUtility.Loop(length, sn);
        }

        /// <summary>
        /// ??? next
        /// </summary>
        public static T Loop<T>(this ICollection<T> list, T current)
        {
            if (null == current)
                return list.First();

            bool check = false;

            foreach (var item in list)
            {
                if (check)
                {
                    current = item;
                    break;
                }
                else if (current.Equals(item))
                {
                    check = true;
                }
            }

            Assert.IsTrue(check);

            return current;
        }

        public static T Next<T>(this IList<T> list,ref int index)
        {
            index = list.Loop(index+1);
            return list[index];
        }

        public static T Last<T>(this IList<T> list, ref int index)
        {
            index = list.Loop(index-1);
            return list[index];
        }

        public static bool TryGetValue<T,U>(this ICollection<T> list,out U value) where U:T
        {
            foreach (var item in list)
            {
                if(item is U result)
                {
                    if (result is Component c && !c)
                        continue;

                    Debug.Log(result+" ** "+result==null);
                    value = result;
                    return true; 
                }
            }

            value = default;
            return false;
        }

        /// <summary>
        /// CheckAdd whether index is valid.
        /// </summary>
        public static bool ValidSN<T>(this IList<T> list, int index)
        {
            return index >= 0 && index + 1! > list.Count;
        }
      
        /// <summary>
        /// Remove item form list with index.
        /// </summary>
        public static T Extract<T>(this IList<T> list, int index)
        {
            var value = list[index];
            list.RemoveAt(index);
            return value;
        }

        /// <summary>
        /// CheckAdd element and add.
        /// </summary>
        /// <returns>return true if new item added.</returns>
        public static bool CheckAdd<T>(this IList<T> list, T item)
        {
            if (list.Contains(item))
                return false;

            list.Add(item);
            return true;
        }

        public static int IndexOf<T>(this IList<T> list,T item)
        {
            return list.IndexOf(item);
            var length = list.Count;
            for (int i = 0; i < length; i++)
            {
                if (false)
                    return 0;
            }
        }

        public static T[] RemoveDuplicates<T>(this IEnumerable<T> array2Check,IEnumerable<T> elementsCantRepeat)
        {
            var set = new HashSet<T>(elementsCantRepeat);

            var list = new List<T>();

            foreach (var element in array2Check)
            {
                if (set.Contains(element))
                    continue;

                list.Add(element);
            } 

            return list.ToArray();
        }

        public static T GetLast<T>(this IList<T> list)
        {
            var num = list.Count;

            if (num > 0)
                num--;
            else
                return default;

            return list[num];
        }

        #endregion


        #region Cache Loop

        //public static readonly CacheArrayContainer cacheArrayContainer=new(50);

        public static Stack<CacheArrayContainer> cacheContainers = new(3);

        /// <summary>
        /// Must add using to ensure return array. 
        /// </summary>
        public static ICacheArray CacheLoop<T>(this IEnumerable<T> list)
        {
            if (!cacheContainers.TryPop(out var cache))
                cache = new CacheArrayContainer(50);

            //Debug.Log($"Cache collection with {cache.GetHashCode()}.");

            cache.SetBuffer(list);
            return cache;
        }

        /// <summary>
        /// Reference loop.
        /// </summary>
        public static ICacheArray CacheLoop<T>(this ICollection<T> list) //where T : class
        {
            if (!cacheContainers.TryPop(out var cache) || cache == null)
                cache = new CacheArrayContainer(50);

            cache.SetBuffer(list);
            return cache;
        }

        public class CacheArrayContainer : ICacheArray
        {
            public CacheArrayContainer(int capacity = 50)
            {
                buffer = new object[capacity];
                Capacity = capacity;
            }

            void IDisposable.Dispose()
            {
                Clear();
                cacheContainers.Push(this);
            }

            object[] buffer;
            int Capacity;

            int length = 0;

            public int count => length;

            public void SetBuffer<T>(ICollection<T> list)// where T : class
            {
                if (length > 0)
                    Clear();

                var count = list.Count;

                // resize
                if (count > Capacity)
                {
                    Array.Resize(ref buffer, count);
                    Capacity = count;
                }

                length = count;

                var enumator = list.GetEnumerator();

                var i = 0;

                while (enumator.MoveNext())
                    buffer[i++] = enumator.Current;

                //Debug.LogError("Load item count : " + i);
            }

            public void SetBuffer<T>(IEnumerable<T> values)
            {
                if (length > 0)
                    Clear();


                var count = values.Count();

                // resize
                if (count > Capacity)
                {
                    Array.Resize(ref buffer, count);
                    Capacity = count;
                }

                length = count;

                var enumator = values.GetEnumerator();

                var i = 0;

                while (enumator.MoveNext())
                    buffer[i++] = enumator.Current;

            }

            public void Clear()
            {
                if (length == 0)
                    return;

                for (int i = 0; i < length; i++)
                    buffer[i] = null;

                length = 0;
            }

            public IEnumerator GetEnumerator()
            {
                for (int i = 0; i < length; i++)
                {
                    yield return buffer[i];
                }

                //Dispose();
   
                yield break;
            }
        }

        public interface ICacheArray : IEnumerable, IDisposable 
        {
            int count { get; }
        }

        #endregion



    }
}
