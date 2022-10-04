using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class mList<T>:List<T>
{
    public mList(int capacity) : base(capacity) { }   
}
public class mQueue<T> : mList<T> where T :class
{
    public mQueue(int capacity) : base(capacity) { }


    public T Last { get; private set; }
    public new int Count 
    { 
        get 
        {
            if (Last == null)
                return base.Count;
            else
                return base.Count + 1;
        } 
    }
    public T Peek()
    {
        if (Count == 0)
        {
            return null;
        }
        else if (Count == 1)
            return Last;
        else
        {
            return base[0];
        }
    }
    public void Enqueue(T item)
    {
        if (Last == null)
        {
            Last = item;
        }
        else
        {
            base.Add(Last);
            Last = item;
        }
    }
    public T Dequeue()
    {
        if (base.Count > 0)
        {
            var first = base[0];
            base.RemoveAt(0);
            return first;
        }
        else
        {
            var item = Last;
            Last = null;
            return item;
        }
    } 

    public void RemoveLast()
    {
        Last = null;
    }
}
