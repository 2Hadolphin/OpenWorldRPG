using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializableAsset
    {
        void WriteProperties(INeoSerializer writer);
        void ReadProperties(INeoDeserializer reader);

        int GetInstanceID();
    }
}