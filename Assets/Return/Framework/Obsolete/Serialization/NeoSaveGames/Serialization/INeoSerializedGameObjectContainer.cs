using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializedGameObjectContainer
    {
        Transform rootTransform
        {
            get;
        }

        void Awake();
        void OnDestroy();

        bool Contains(NeoSerializedGameObject nsgo);
        void RegisterObject(NeoSerializedGameObject nsgo);
        void UnregisterObject(NeoSerializedGameObject nsgo);

        void WriteGameObjects(INeoSerializer writer, SaveMode saveMode);
        void WriteGameObjects(INeoSerializer writer, NeoSerializationFilter filter, NeoSerializedGameObject[] objects, SaveMode saveMode);
        void ReadGameObjectHierarchy(INeoDeserializer reader);
        void ReadGameObjectProperties(INeoDeserializer reader);
    }
}
