using System.Collections;
using System.Collections.Generic;
using TNet;
using UnityEngine;
using System;
using System.Reflection;
using Return;
using Sirenix.OdinInspector;

[System.Serializable]
[SerializeProperties]
public class TestClassss// : IDataNodeSerializable
{
    //public void ReflectionSerialize(DataNode node)
    //{
    //    node.SetChild(nameof(privateSerializeField_V3), privateSerializeField_V3);
    //}

    //public void Deserialize(DataNode node)
    //{
    //    privateSerializeField_V3 = node.GetChild<Vector3>(nameof(privateSerializeField_V3));
    //}


    // yes
    [SerializeField]
    private Vector3 privateSerializeField_V3;

    // yes
    [SerializeField]
    private Quaternion privateSerializeField_Qt;

    // yes
    [SerializeField]
    private bool privateSerializeField_Boolean;

    // yes
    [SerializeField]
    private float privateSerializeField;

    // not
    [SerializeField]
    private float privateSerializeProperty { get; set; }
  
    // yes
    [SerializeField]
    public float publicSerializeField;

    [ShowInInspector]
    // yes
    public float publicProperty { get; set; }

    [SerializeReference]
    [SerializeField]
    Material m_Material;

    [IgnoredByTNet]
    protected Material Material { get => m_Material; set { m_Material = value; Debug.LogError(value); } }

    [SerializeField]
    SkinnedMeshRenderer m_SkinnedRenderer;

    //public void Reflect(object obj)
    //{
    //    Debug.Log(dataNode.children?.Count);

    //    var type = obj.GetType();
    //    var prop = type.GetProperties(Flag);

    //    Debug.Log(type + " : " + prop.Length);

    //    foreach (PropertyInfo x in prop)
    //    {
    //        Debug.Log("Found " + x.Name);



    //        var ans = dataNode.FindChild(x.Name)?.Get(x.DeclaringType);
    //        Debug.Log(ans);

    //        if (x.CanWrite && ans.NotNull())
    //            x.SetValue(obj, ans);
    //    }

    //}

    //protected override void LoadItem()
    //{
    //    var ob = dataNode.Get(GetType());
    //    //m_Material=dataNode.DeserializeMaterial();

    //    Reflect(this);

    //    return;

    //    //this.Reflect((x) => dataNode.FindChild(x));

    //    //if (dataNode.ResolveValue(GetType()))
    //    //{
    //    //    this.Reflect((x) => dataNode.FindChild(x));
    //    //}
    //    //else
    //    //    Debug.LogError("Can't resolve type.");

    //    var type = GetType();


    //    var props = type.GetProperties(Flag);

    //    LogProp(type, props);

    //    var prop = type.GetSerializableProperties();

    //    LogProp(type, prop.ToArray());


    //}

    void LogProp(Type type, params PropertyInfo[] prop)
    {
        Debug.Log(type + " " + prop.Length);

        foreach (var item in prop)
        {
            Debug.Log(item.Name);
        }
    }


}
