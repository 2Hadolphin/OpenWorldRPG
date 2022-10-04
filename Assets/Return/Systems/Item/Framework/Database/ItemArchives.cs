using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Return.Items;

public class ItemArchives : Archives<ItemArchives,ItemPreset, ItemInfo>
{
    public override string Path => nameof(ItemArchives);

    public override string Name => nameof(ItemArchives);


    

    public override void SaveData()
    {
        base.SaveData();
    }
    public override void LoadData()
    {
        base.LoadData();
        foreach (var resource in Resources)
        {
            if(resource is ISerializationCallbackReceiver serialization)
                serialization.OnAfterDeserialize();
        }

    }

    protected override void OnEnable()
    {
        base.OnEnable();
        var items=UnityEngine.Resources.LoadAll<ItemPreset>("");
        Resources = items.ToList();

    }

    
}
