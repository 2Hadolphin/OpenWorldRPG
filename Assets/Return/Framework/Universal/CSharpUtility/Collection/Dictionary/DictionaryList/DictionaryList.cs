using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public class DictionaryList<TKey, TValue> : IEnumerable<KeyValuePair<TKey,List<TValue>>>
    {
        public DictionaryList(int capacity = 1, int _elementCapacity = 1)
        {
            elements = new(capacity);
            elementCapacity = _elementCapacity;
        }

        public int elementCapacity;

        // type - subscribe callback
        public Dictionary<TKey, List<TValue>> elements;

        public virtual bool TryGetValues(TKey key, out IList<TValue> values,bool createIfNone=false)
        {
            var valid= elements.TryGetValue(key, out var list);
            values = valid ? list : null;

            if(!valid && createIfNone)
            {
                list = new List<TValue>(1);
                elements.Add(key, list);
                values = list;
            }

            return valid;
        }

        public virtual bool ContainsKey(TKey key)
        {
            return elements.ContainsKey(key);
        }

        public virtual void Add(TKey key, TValue value)
        {
            if (!elements.TryGetValue(key, out var list))
            {
                list = new List<TValue>(elementCapacity);
                elements.Add(key, list);
            }

            if (!list.Contains(value))
                list.Add(value);
        }

        public virtual bool Remove(TKey key, TValue value,bool deleteIfNoneElement=true)
        {
            var valid = elements.TryGetValue(key, out var list);

            if(valid)
            {
                valid |=list.Remove(value);
                if (deleteIfNoneElement && list.Count == 0)
                    elements.Remove(key);
            }

            return valid;
        }

        public virtual bool Remove(TKey key, out List<TValue> values)
        {
            var valid = elements.Remove(key, out values);
            return valid;
        }


        public int Count => elements.Count;
        public virtual void Clear() => elements.Clear();

        public IEnumerator<KeyValuePair<TKey, List<TValue>>> GetEnumerator()
        {
            foreach (var pair in elements)
                yield return pair;
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public virtual ICollection<TKey> Keys => elements.Keys;
        public virtual ICollection<List<TValue>> Values => elements.Values;




    }


}