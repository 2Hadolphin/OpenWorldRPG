using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public abstract class NeoSerializationFormatter<T> : INeoSerializationFormatter where T : Component
    {
        public void WriteProperties(INeoSerializer writer, Component c, NeoSerializedGameObject nsgo)
        {
            WriteProperties(writer, c as T, nsgo);
        }

        public void ReadProperties(INeoDeserializer reader, Component c, NeoSerializedGameObject nsgo)
        {
            ReadProperties(reader, c as T, nsgo);
        }

        protected abstract void WriteProperties(INeoSerializer writer, T from, NeoSerializedGameObject nsgo);
        protected abstract void ReadProperties(INeoDeserializer reader, T to, NeoSerializedGameObject nsgo);

    }
}