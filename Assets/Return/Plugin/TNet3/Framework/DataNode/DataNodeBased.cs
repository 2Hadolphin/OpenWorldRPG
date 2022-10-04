using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace TNet
{
    public class DataNodeBased : IBinarySerializable, IDataNodeSerializable
    {
        [HideInInspector]
        public DataNode dataNode = new ();

        public override string ToString()
        {
            return dataNode.ToString();
        }

        #region Serialization

        public virtual void Deserialize(BinaryReader reader)
        {
            dataNode.Merge(reader.ReadDataNode());
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            writer.Write(dataNode);
        }

        public virtual void Deserialize(DataNode node)
        {
            dataNode.Merge(node);
        }

        public virtual void Serialize(DataNode node)
        {
            node.Merge(dataNode);
        }
        #endregion


        #region Property

        public virtual T Get<T>(string name)
        {
            if (dataNode == null) 
                return default;

            return dataNode.GetChild<T>(name);
        }

        public virtual T Get<T>(string name, T defaultVal)
        {
            if (dataNode == null) 
                return defaultVal;

            return dataNode.GetChild(name, defaultVal);
        }

        public virtual void Set(string name, object val)
        {
            if (val != null)
            {
                if (dataNode == null) 
                    dataNode = new DataNode();

                if (val is DataNode node)
                    dataNode.GetChild(name, true).Merge(node);
                else 
                    dataNode.SetChild(name, val);
            }
            else if (dataNode != null)
            {
                dataNode.RemoveChild(name);
            }
        }

        #endregion
    }
}