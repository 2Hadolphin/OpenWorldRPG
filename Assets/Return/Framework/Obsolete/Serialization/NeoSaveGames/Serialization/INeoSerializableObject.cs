using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializableObject
    {
        void WriteProperties(INeoSerializer writer);
        void ReadProperties(INeoDeserializer reader);
    }
}