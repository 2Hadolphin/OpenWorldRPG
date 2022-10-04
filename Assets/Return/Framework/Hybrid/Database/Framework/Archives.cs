using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using System;
using System.Linq;
using Return;


/// <typeparam name="T">serializable value</typeparam>
public abstract class Archives<Singleton, T> : SingletonSO<Singleton>, IReadOnlyCollection<T> where Singleton : Archives<Singleton, T>
{
    /// <summary>
    /// persistent path of the archives, loading json data from there
    /// </summary>
    public abstract string Path { get; }
    public abstract string Name { get; }

    [PropertyTooltip("whether serializable as unique file;  normal=> game history record ; unique=> planning loot map / character archives(standalone package)")]
    public bool Independence = true;

    [ShowInInspector]
    [SerializeField]
    protected virtual List<T> Resources { get; set; } = new List<T>();



    #region Hosting => Only supply reference
    [NonSerialized]
    [ShowInInspector]
    [ReadOnly]
    protected List<T> HostingResources = new List<T>();

    #endregion

    public virtual int Count => Resources.Count + HostingResources.Count;

    public virtual T this[int index]
    {
        get
        {
            if (index < Resources.Count)
                return Resources[index];
            else
                return HostingResources[index - Resources.Count + 1];
        }
    }

    public T[] ToArray => Enumerable.Concat(Resources, HostingResources).ToArray();

    /// <summary>
    /// if unity editor native source???????
    /// </summary>
    public virtual void Add(T item)
    {
        Resources.Add(item);
    }

    public virtual void Remove(T item)
    {
        Resources.Remove(item);
    }


    protected override void Awake()
    {
        base.Awake();
        if (WhetherThisSingleton(this))
        {
            LoadData();
            if (Independence)
            {
                Instance.HostingResources.AddRange(Resources);
            }
            else
            {
                Instance.Resources.AddRange(Resources);
                //clean data path
                this.Destroy();
            }
        }
        else // execute by singleton => load all bundle at 
        {

        }
    }

    [Button("Save")]
    public override void SaveData()
    {
   
        if (typeof(T).IsSubclassOf(typeof(ScriptableObject)))
            IOExtension.SaveJson(Path, Name + "json", Resources);
        else
            IOExtension.SaveJson(Path, Name + "json", Resources);


    }

    [Button("Load")]
    public override void LoadData()
    {
        //Debug.Log(typeof(TSerializable) + "_" + typeof(ScriptableObject) + "_" + typeof(TSerializable).IsSubclassOf(typeof(ScriptableObject)));
        List<T> values= IOExtension.LoadJson<List<T>>(Path, Name + "json");

        if (values != null && values.Count > 0)
        {
            Resources = new List<T>(values);
        }
    }

    public virtual IEnumerator<T> GetEnumerator()
    {
        foreach (var item in Resources)
        {
            yield return item;
        }

        foreach (var item in HostingResources)
        {
            yield return item;
        }
    }

     IEnumerator IEnumerable.GetEnumerator()
    {
        yield return GetEnumerator();
    }


}



/// <summary>
/// 
/// ???????
/// Archives contains native resources and serializable mResources
/// </summary>
/// <typeparam name="TSerializable">  serializable data of DNonSerialzable </typeparam>
/// <typeparam name="DNonSerialzable"> type of archives(nonSerializable)</typeparam>
public abstract class Archives<USingleton, TSerializable, DNonSerialzable> : Archives<USingleton, TSerializable> where USingleton : Archives<USingleton, TSerializable, DNonSerialzable>// where TSerializable : IValue<DNonSerialzable>
{

    public override IEnumerator<TSerializable> GetEnumerator()
    {
        foreach (var item in Resources)
        {
            yield return item;
        }

        foreach (var item in HostingResources)
        {
            yield return item;
        }
    }

}

[Serializable]
public class Archive<TKValue> : ISerializationCallbackReceiver// where TKValue : ScriptableObject
{
    public Archive() { }
    public Archive(List<TKValue> value)
    {
        Values = value;
    }

    public static Archive<TKValue> Create(List<TKValue> values)
    {
        var archive = new Archive<TKValue>();

        if (null == values)
            archive.Values = new List<TKValue>();
        else
            archive.Values = values;

        return archive;
    }

    public virtual void OnBeforeSerialize()
    {

    }

    public virtual void OnAfterDeserialize()
    {

    }

    [SerializeField]
    public List<TKValue> Values = new List<TKValue>();
}

