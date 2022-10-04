using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return;
using Return.Items;
using System;
using System.ComponentModel;
using System.Globalization;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using System.Reflection;
using Return.Editors;

public class ItemCreator
{
    public ItemCreator()
    {
        InventoryArchives = ItemArchives.Instance;
    }

    public const string TypeCatch = "ItemPresetCatch";
    [FolderPath]
    public string Path = "Assets/Return/Items";


    [HorizontalGroup("Resource")]
    public ItemArchives InventoryArchives;
    [HorizontalGroup("Resource")]
    [Button("LoadSource")]
    public void LoadSource()
    {
        InventoryArchives = ItemArchives.Instance;

        
    }

    bool Targeted => InventoryArchives != null;

    [SerializeField]
    [BoxGroup("Preset")]
    [ShowIf("Targeted")]
    [HideLabel]
    [AssetList]
    [OnValueChanged("UpdateData", InvokeOnInitialize = false, IncludeChildren = true)]
    public ItemPreset Current;

    [SerializeField]
    [BoxGroup("Preset")]
    [OnValueChanged("Edit", InvokeOnInitialize = false, IncludeChildren = true)]
    [ShowIf("Current")][HideLabel]
    public ItemPreset Data;

    [ShowIf("Targeted")]
    [HorizontalGroup("Selector")]
    [Button("Last")]
    public void Last()
    {
        DataIndex = MathfUtility.Loop(InventoryArchives.Count, DataIndex - 1);
        LoadData();
    }

    [ShowIf("Targeted")]
    [HorizontalGroup("Selector")]
    [PropertyRange("min", "max")]
    [OnValueChanged("LoadData")]
    public int DataIndex;

    [ShowIf("Targeted")]
    [HorizontalGroup("Selector")]
    [Button("Next")]
    public void Next()
    {
        DataIndex = MathfUtility.Loop(InventoryArchives.Count, DataIndex + 1);
        LoadData();
    }

    public int max
    {
        get
        {
            if (Targeted)
                return Mathf.Max(default, InventoryArchives.Count - 1);
            else
                return default;
        }
    }

    [Serializable][HideLabel]
    public class String2Type
    {
        public const char Separator = '$';
        public String2Type(string prefs)
        {
            var keys=EditorPrefs.GetString(prefs).Split(Separator);
            _Types = new HashSet<string>(keys);

            Prefs = prefs;
        }
        readonly string Prefs;
        public string Type;
        [ValueDropdown(nameof(_Types))]
        [OnValueChanged(nameof(KeyType))]
        [ShowInInspector]
        string _Type;

        [OnValueChanged(nameof(Archive))]
        [ShowInInspector]
        HashSet<string> _Types = new HashSet<string>();

        public void Add()
        {
            _Types.Add(Type);
            Archive();
        }
        public void Archive()
        {
            var keys = string.Join(Separator, _Types);
            EditorPrefs.SetString(Prefs, keys);
        }

        void KeyType()
        {
            Type = _Type;
        }

    }
    [SerializeField]
    public String2Type Type=new String2Type(TypeCatch);


    [ShowIf("Targeted")][HideLabel]
    [VerticalGroup("Create")]
    [Button("Create new item")]
    public void Create()
    {
        //var type = System.Type.GetType("ItemInfo");


        try
        {
            CreateItem(Type.Type);
            Type.Add();

        }
        catch (Exception e)
        {

            throw e;
        }

        //if(type.IsSubclassOf(typeof(ItemPreset)))


    }

    public void CreateItem(string type)
    {
        var so = ScriptableObject.CreateInstance(type) as ItemPreset;
        if (!so)
            throw new KeyNotFoundException(type);

        var info = so;

        InventoryArchives.Add(info);

        // to file utili
        //so = AssetsCreator.Archive(so, "NewItem", false, m_Path);

        
        Current = so;

        DataIndex = (InventoryArchives.Count - 1);

    }



    int min = -1;
    public void LoadData()
    {
        DataIndex = Mathf.Clamp(DataIndex, min, max);
        if (DataIndex >= 0)
            Current = InventoryArchives[DataIndex];

        UpdateData();
    }

    public void UpdateData()
    {
        if (Data == null)
            return;

        if (Data.name != Data.ID)
            EditorHelper.Rename(Data, Data.ID);

        Data = Current;
        Debug.Log(Current);
    }


    public void Edit()
    {
        if (null == Current)
            return;
        UnityEditor.EditorUtility.SetDirty(InventoryArchives);
        UnityEditor.EditorUtility.SetDirty(Current);
    }

    [ShowIf("Targeted")]
    [Button("Ping")]
    public void Ping()
    {
        UnityEditor.EditorGUIUtility.PingObject(InventoryArchives);
    }



}
