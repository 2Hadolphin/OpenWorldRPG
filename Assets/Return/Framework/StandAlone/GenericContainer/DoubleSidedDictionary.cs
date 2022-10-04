using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Return;

public class DoubleSidedDictionary<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>//where T1:new () where T2 :new ()
{
    private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();
    private readonly List<T1> Broken_forward=new List<T1>();
    private readonly List<T2> Broken_reverse = new List<T2>();
    private bool T1Nullable;
    private bool T2Nullable;
    public DoubleSidedDictionary()
    {
        Forward = new Indexer<T1, T2>(_forward);
        Reverse = new Indexer<T2, T1>(_reverse);
        T1Nullable= !typeof(T1).IsValueType;      
        T2Nullable= !typeof(T2).IsValueType;
    }
    public DoubleSidedDictionary(int capacity)
    {
        _forward = new Dictionary<T1, T2>(capacity);
        _reverse = new Dictionary<T2, T1>(capacity);

        Forward = new Indexer<T1, T2>(_forward);
        Reverse = new Indexer<T2, T1>(_reverse);
    }

    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Reverse { get; private set; }

    public T2 this[T1 key]
    {
        get
        {
            if (key == null || !_forward.TryGetValue(key, out var value))
                throw new KeyNotFoundException();

            return value;
        }
        set
        {
            if (key == null)
                throw new KeyNotFoundException();


            if (value==null)
            {
                if (_forward.TryGetValue(key, out var revKey))
                {
                    _reverse.SafeRemove(revKey);
                    _forward[key] = value;
                }
                else
                {
                    _forward.Add(key, value);
                    _reverse.RemoveByValue(key);
                }
            }
            else
            {
                if (_reverse.TryGetValue(value, out var fwdKey))
                    if (_forward.ContainsKey(fwdKey))
                    {
                        _reverse.SafeRemove(value);
                        _forward[fwdKey] = default;
                    }


                if (_forward.TryGetValue(key, out var revKey))
                {
                    _reverse.SafeRemove(revKey);
                    _forward[key] = value;
                }
                else
                {
                    _forward.Add(key, value);
                    _reverse.RemoveByValue(key);
                }
                _reverse.SafeAdd(value, key);
            }

        }
    }

    public int Length => GetLength();

    protected int GetLength()
    {
        //3---4---3==14--3--3
        //5---9---3==26--5--3
        var count = _forward.Count+_reverse.Count;
        var broken = 0;
        foreach (var item in _forward)
        {
            if (!_reverse.ContainsValue(item.Key))
                broken++;
        }

        foreach (var item in _reverse)
        {
            if (!_forward.ContainsValue(item.Key))
                broken++;
        }

        count = (count - broken) / 2 + broken;
        return count;
    }


    public void Edit(T1 key,T2 oldValue, T2 newValue)
    {
        //print(string.Format("Using key : {0} . Remove : {1} . Add : {2} ", key, oldValue, newValue));

        if(oldValue!=null)
            if (_reverse.TryGetValue(oldValue, out var fwdOldKey))
                if (fwdOldKey != null)
                    if (_forward.ContainsKey(fwdOldKey))
                        _forward[fwdOldKey] = default;

        if (newValue != null)
            if (_reverse.TryGetValue(newValue, out var fwdOldKey))
                if (fwdOldKey != null)
                    if (_forward.ContainsKey(fwdOldKey))
                        _forward[fwdOldKey] = default;

        if(oldValue is not null)
            _reverse.SafeRemove(oldValue);

        if (newValue is not null)
            _reverse.SafeRemove(newValue);

        _forward.SafeAdd(key, newValue);
        _reverse.SafeAdd(newValue, key);
    }
    public void Edit(T2 key, T1 oldValue, T1 newValue)
    {
        if (oldValue != null)
            if (_forward.TryGetValue(oldValue, out var revOldKey))
                if (revOldKey != null)
                    if (_reverse.ContainsKey(revOldKey))
                        _reverse[revOldKey] = default;

        if (newValue != null)
            if (_forward.TryGetValue(newValue, out var revOldKey))
                if (revOldKey != null)
                    if (_reverse.ContainsKey(revOldKey))
                        _reverse[revOldKey] = default;

        _forward.SafeRemove(oldValue);
        _forward.SafeRemove(newValue);

        _reverse.SafeAdd(key, newValue);
        _forward.SafeAdd(newValue, key);
    }
    public bool InPairs(T1 e) 
    {
        if (!_forward.TryGetValue(e, out var revKey))
            return false;

        if (revKey == null)
            return false;

        if (!_reverse.ContainsKey(revKey))
            return false;

        return true;
    } 
    public bool InPairs(T2 e)
    {
        if (!_reverse.TryGetValue(e, out var fwdKey))
            return false;

        if (fwdKey == null)
            return false;

        if (!_forward.ContainsKey(fwdKey))
            return false;

        return true;
    }
    public void Add(T1 t1, T2 t2)
    {
        Edit(t1, default, t2);
    }

    public void Remove(T1 t1)
    {
        if (t1 == null)
            return;

        if(_forward.TryGetValue(t1,out T2 revKey))
        {
            _forward.Remove(t1);
            _reverse.Remove(revKey);
        }
        else
        {
            _reverse.RemoveByValue(t1);
        }

    }

    public void Remove(T2 t2)
    {
        if (t2 == null)
            return;

        if (_reverse.TryGetValue(t2, out T1 fwdKey))
        {
            _reverse.Remove(t2);
            _forward.Remove(fwdKey);
        }
        else
        {
            _forward.RemoveByValue(t2);
        }

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
    {
        return _forward.GetEnumerator();
    }

    public class Indexer<T3, T4>:IEnumerable,IEnumerable<KeyValuePair<T3,T4>>
    {
        private readonly Dictionary<T3, T4> _dictionary;

        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }

        public T4 this[T3 index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

        public bool Contains(T3 key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(T3 key,out T4 value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public IEnumerator GetEnumerator()
        {
            yield return _dictionary.GetEnumerator();
        }

        IEnumerator<KeyValuePair<T3, T4>> IEnumerable<KeyValuePair<T3, T4>>.GetEnumerator()
        {
            foreach (var pair in _dictionary)
            {
                yield return pair;
            }
        }

        public int Length => _dictionary.Count;
        public KeyValuePair<T3, T4>[] keyValuePairs => _dictionary.ToArray();

    }
}

public class DoubleSidedDictionaryInPairs<T1, T2> : IEnumerable<KeyValuePair<T1, T2>>//where T1:new () where T2 :new ()
{
    private readonly Dictionary<T1, T2> _forward = new Dictionary<T1, T2>();
    private readonly Dictionary<T2, T1> _reverse = new Dictionary<T2, T1>();

    public DoubleSidedDictionaryInPairs()
    {
        Forward = new Indexer<T1, T2>(_forward);
        Reverse = new Indexer<T2, T1>(_reverse);
    }
    public DoubleSidedDictionaryInPairs(int capacity)
    {
        _forward = new Dictionary<T1, T2>(capacity);
        _reverse = new Dictionary<T2, T1>(capacity);

        Forward = new Indexer<T1, T2>(_forward);
        Reverse = new Indexer<T2, T1>(_reverse);
    }

    public Indexer<T1, T2> Forward { get; private set; }
    public Indexer<T2, T1> Reverse { get; private set; }

    public T2 this[T1 key]
    {
        get
        {
            if (key == null || !_forward.TryGetValue(key, out var value))
                throw new KeyNotFoundException();

            return value;
        }
        set
        {
            if (key == null)
                throw new KeyNotFoundException();


            if (value == null)
            {
                if (_forward.TryGetValue(key, out var revKey))
                {
                    _reverse.SafeRemove(revKey);
                    _forward[key] = value;
                }
                else
                {
                    _forward.Add(key, value);
                    _reverse.RemoveByValue(key);
                }
            }
            else
            {
                if (_reverse.TryGetValue(value, out var fwdKey))
                    if (_forward.ContainsKey(fwdKey))
                    {
                        _reverse.SafeRemove(value);
                        _forward[fwdKey] = default;
                    }


                if (_forward.TryGetValue(key, out var revKey))
                {
                    _reverse.SafeRemove(revKey);
                    _forward[key] = value;
                }
                else
                {
                    _forward.Add(key, value);
                    _reverse.RemoveByValue(key);
                }
                _reverse.SafeAdd(value, key);
            }

        }
    }

    public int Length => GetLength();

    protected int GetLength()
    {
        //3---4---3==14--3--3
        //5---9---3==26--5--3
        var count = _forward.Count + _reverse.Count;
        var broken = 0;
        foreach (var item in _forward)
        {
            if (!_reverse.ContainsValue(item.Key))
                broken++;
        }

        foreach (var item in _reverse)
        {
            if (!_forward.ContainsValue(item.Key))
                broken++;
        }

        count = (count - broken) / 2 + broken;
        return count;
    }


    public void Edit(T1 key, T2 oldValue, T2 newValue)
    {
        //print(string.Format("Using key : {0} . Remove : {1} . Add : {2} ", key, oldValue, newValue));

        if (oldValue != null)
            if (_reverse.TryGetValue(oldValue, out var fwdOldKey))
                if (fwdOldKey != null)
                    if (_forward.ContainsKey(fwdOldKey))
                        _forward[fwdOldKey] = default;

        if (newValue != null)
            if (_reverse.TryGetValue(newValue, out var fwdOldKey))
                if (fwdOldKey != null)
                    if (_forward.ContainsKey(fwdOldKey))
                        _forward[fwdOldKey] = default;

        _reverse.SafeRemove(oldValue);
        _reverse.SafeRemove(newValue);

        _forward.SafeAdd(key, newValue);
        _reverse.SafeAdd(newValue, key);
    }
    public void Edit(T2 key, T1 oldValue, T1 newValue)
    {
        if (oldValue != null)
            if (_forward.TryGetValue(oldValue, out var revOldKey))
                if (revOldKey != null)
                    if (_reverse.ContainsKey(revOldKey))
                        _reverse[revOldKey] = default;

        if (newValue != null)
            if (_forward.TryGetValue(newValue, out var revOldKey))
                if (revOldKey != null)
                    if (_reverse.ContainsKey(revOldKey))
                        _reverse[revOldKey] = default;

        _forward.SafeRemove(oldValue);
        _forward.SafeRemove(newValue);

        _reverse.SafeAdd(key, newValue);
        _forward.SafeAdd(newValue, key);
    }
    public bool InPairs(T1 e)
    {
        if (!_forward.TryGetValue(e, out var revKey))
            return false;

        if (revKey == null)
            return false;

        if (!_reverse.ContainsKey(revKey))
            return false;

        return true;
    }
    public bool InPairs(T2 e)
    {
        if (!_reverse.TryGetValue(e, out var fwdKey))
            return false;

        if (fwdKey == null)
            return false;

        if (!_forward.ContainsKey(fwdKey))
            return false;

        return true;
    }
    public void Add(T1 t1, T2 t2)
    {
        Edit(t1, default, t2);
    }

    public void Remove(T1 t1)
    {
        if (t1 == null)
            return;

        if (_forward.TryGetValue(t1, out T2 revKey))
        {
            _forward.Remove(t1);
            _reverse.Remove(revKey);
        }
        else
        {
            _reverse.RemoveByValue(t1);
        }

    }

    public void Remove(T2 t2)
    {
        if (t2 == null)
            return;

        if (_reverse.TryGetValue(t2, out T1 fwdKey))
        {
            _reverse.Remove(t2);
            _forward.Remove(fwdKey);
        }
        else
        {
            _forward.RemoveByValue(t2);
        }

    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<KeyValuePair<T1, T2>> GetEnumerator()
    {
        return _forward.GetEnumerator();
    }

    public class Indexer<T3, T4>
    {
        private readonly Dictionary<T3, T4> _dictionary;

        public IDictionary<T3, T4> IDictionary => _dictionary;
        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }

        public T4 this[T3 index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

        public bool Contains(T3 key)
        {
            return _dictionary.ContainsKey(key);
        }

        public bool TryGetValue(T3 key, out T4 value)
        {
            return _dictionary.TryGetValue(key, out value);
        }

        public int Length => _dictionary.Count;
        public KeyValuePair<T3, T4>[] keyValuePairs => _dictionary.ToArray();
    }
}