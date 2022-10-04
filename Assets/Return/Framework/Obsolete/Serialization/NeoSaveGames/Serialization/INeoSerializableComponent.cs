using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializableComponent
    {
        void WriteProperties(INeoSerializer writer, NeoSerializedGameObject nsgo, SaveMode saveMode);
        void ReadProperties(INeoDeserializer reader, NeoSerializedGameObject nsgo);
    }
}