using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializationFormatter
    {
        void WriteProperties(INeoSerializer writer, Component c, NeoSerializedGameObject nsgo);
        void ReadProperties(INeoDeserializer reader, Component c, NeoSerializedGameObject nsgo);
    }
}