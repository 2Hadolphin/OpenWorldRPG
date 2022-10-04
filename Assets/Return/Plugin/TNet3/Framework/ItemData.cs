using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TNet
{
    public class ItemData : DataNodeBased
    {
        Sprite _sprite;
        public Sprite sprite { get { return _sprite ?? ResolveSprite(); } }

        public string id { get { return Get<string>("id"); } }
        public string name { get { return Get<string>("id"); } } // TODO localize
        public string spritePath { get { return Get<string>("sprite"); } }
        public string description { get { return ""; } } // TODO localize
        public int max { get { return Get("max", 99); } }

        public ItemData(string id, string sprite = "default-sprite", int max = 99)
        {
            dataNode = new DataNode("ItemDescriptor");
            dataNode.SetChild("id", id);
            dataNode.SetChild("sprite", sprite);
            if (max != 99) dataNode.SetChild("max", max);
            ResolveSprite();
            // TODO TYPE
        }

        public ItemData(DataNode dataNode)
        {
            this.dataNode = dataNode;
            ResolveSprite();
        }

        Sprite ResolveSprite()
        {
            return null;
            //_sprite = AssetLoader.instance.GetItemSprite(spritePath);
            //return _sprite;
        }


    }
}