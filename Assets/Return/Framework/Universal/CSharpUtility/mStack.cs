using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Return;

[System.Serializable]
public class mStack<T>:IList<T>
{
    public mStack(int count=1)
    {
        Items = new(count);
    }

    public mStack(params T[] values)
    {
        Items = new List<T>(values);
    }

    [SerializeField]
    protected List<T> Items = new List<T>();

    public int Count => Items.Count;

    public bool IsReadOnly => false;

    T IList<T>.this[int index] { get => Items[index]; set => Items[index]=value; }
    public T this[int index]
    {
        get => Items[index];
    }
    public int Push(T item)
    {
        Items.Add(item);
        return Count - 1;
    }

    public void Push(T item,Func<T,bool> func)
    {
        var length = Items.Count;
        for (int i = 0; i < length; i++)
        {
            if (func(Items[i]))
                Items.Insert(i, item);
        }
    }

    public T Pop()
    {
        if (Items.Count > 0)
        {
            T temp = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
            return temp;
        }
        else
            return default(T);
    }

    public bool TryPop(out T value)
    {
        var valid = Items.Count > 0;

        if (valid)
        {
            value = Items[Items.Count - 1];
            Items.RemoveAt(Items.Count - 1);
        }
        else
            value = default(T);

        return valid;
    }


    public T Peek()
    {
        if (Items.Count > 0)
        {
            T temp = Items[Items.Count - 1];
            return temp;
        }
        else
            return default(T);
    }

    public bool TryPeek(out T value)
    {
        var valid = Items.Count > 0;

        if (valid)
            value = Items[Items.Count - 1];
        else
            value = default(T);

        return valid;
    }

    public bool Remove(T item)
    {
        return Items.Remove(item);
    }

    public int IndexOf(T item)
    {
        return Items.IndexOf(item);
    }

    public void Insert(int index, T item)
    {
        Items.Insert(index,item);
    }

    public void RemoveAt(int index)
    {
        Items.RemoveAt(index);
    }

    /// <summary>
    /// Move item to top.
    /// </summary>
    public void SetAsNew(T item)
    {
        Items.Remove(item);
        Push(item);
    }

    public void Add(T item)
    {
        Items.Add(item);
    }

    public void AddRange(params T[] items)
    {
        Items.AddRange(items);
    }

    public void Clear()
    {
        Items.Clear();
    }

    public bool Contains(T item)
    {
        return Items.Contains(item);
    }

    public void CopyTo(T[] array, int arrayIndex)
    {
        Items.CopyTo(array, arrayIndex);
    }

    bool ICollection<T>.Remove(T item)
    {
        return Items.Remove(item);
    }

    public IEnumerator<T> GetEnumerator()
    {
        return Items.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}