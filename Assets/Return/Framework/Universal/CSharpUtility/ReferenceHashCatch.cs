using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
namespace Return
{

    public class ReferenceHashCatch<TContent, TKey, TContainer>:ICollection<TContent> where TContainer: IList<TKey>
    {
        private ReferenceHashCatch() { }
        public ReferenceHashCatch(Func<TContent, TKey> func) 
        {
            Func = func;
            m_Dictionary = new Dictionary<TKey, Counter>(); 
        }
        readonly Func<TContent, TKey> Func;
        public ReferenceHashCatch(Func<TContent,TKey>func,out TKey[] indexs, params TContent[] values)
        {
            Func = func;
            var length = values.Length;
            indexs = new TKey[length];
            m_Dictionary = new Dictionary<TKey, Counter>(length);
            for (int i = 0; i < length; i++)
            {
                var value = values[i];
                var hash = func(value);

                Add(hash, value);

                indexs[i] = hash;
            }
            m_Dictionary.TrimExcess();
        }
        
        void Add(TKey key,TContent content)
        {
            if (m_Dictionary.TryGetValue(key, out var counter))
                counter.Count++;
            else
                m_Dictionary.Add(key, new Counter(content));
        }

        public class Counter
        {
            public Counter(TContent value)
            {
                this.Content = value;
                Count = 1;
            }
            public readonly TContent Content;
            public int Count;

            //public static implicit operator int(Counter counter)
            //{
            //    return counter.Count;
            //}
        }
        #region Properties
        protected Dictionary<TKey, Counter> m_Dictionary;
        public TContainer m_Data;
        #endregion

        public int Count => m_Data.Count;

        public bool IsReadOnly => false;

        public void Add(TContent item)
        {
            var key = Func(item);
            Add(key, item);
        }

        public void Clear()
        {
            m_Data.Clear();
            m_Dictionary.Clear();
        }

        public bool Contains(TContent item)
        {
            var key = Func(item);
            return m_Dictionary.ContainsKey(key);
        }

        public void CopyTo(TContent[] array, int arrayIndex)
        {
            TKey[] keys=new TKey[arrayIndex];
            m_Data.CopyTo(keys, arrayIndex);
            keys.Select(x => m_Dictionary[x].Content).ToArray().CopyTo(array, arrayIndex);
        }
        public bool Remove(TContent item)
        {
            var key = Func(item);
            if(m_Dictionary.TryGetValue(key,out var counter))
            {
                if (counter.Count--.Equals(0))
                    m_Dictionary.Remove(key);

                m_Data.Remove(key);

                return true;
            }
            return false;
        }

        public IEnumerator<TContent> GetEnumerator()
        {
            return m_Data.Select(x => m_Dictionary[x].Content).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


    }

    public class StackPoolCatch<T>: ReferenceHashCatch<T, int, mStack<int>>, IEnumerable<T>
    {
        public StackPoolCatch():base((T) => T.GetHashCode()) { m_Data = new mStack<int>(); }
        public StackPoolCatch(params T[] values):base ((T)=>T.GetHashCode(),out var hash,values)
        {
            m_Data = new mStack<int>(hash);
        }
    }


}