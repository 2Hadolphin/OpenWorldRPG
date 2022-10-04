using System.IO;
using TNet;
using Sirenix.OdinInspector;
using System;

namespace Return
{
    /// <summary>
    /// TNet
    /// </summary>
    [Serializable]
    [Obsolete]
    public class DataNodeBased : IBinarySerializable, IDataNodeSerializable
    {
        
        public DataNode dataNode { get; set; } = new();

   
        public override string ToString()
        {
            return dataNode.ToString();
        }


        #region IBinarySerializable

        public virtual void Deserialize(BinaryReader reader)
        {
            int length = reader.ReadInt();
            dataNode.Merge(DataNode.Read(reader.ReadBytes(length)));
        }

        public virtual void Serialize(BinaryWriter writer)
        {
            byte[] data = dataNode.ToArray();
            writer.WriteInt(data.Length);
            writer.Write(data);
        }

        #endregion


        #region IDataNodeSerializable

        public virtual void Deserialize(DataNode node)
        {
            dataNode.Merge(node);
        }

        public virtual void Serialize(DataNode node)
        {
            node.Merge(dataNode);
        }

        #endregion

    }
}