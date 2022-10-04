using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    [Serializable]
    public class NeoSerializedGameObjectChildContainer : NeoSerializedGameObjectContainerBase
    {
        [SerializeField]
        private NeoSerializedGameObject m_GameObject = null;
        
        public override Transform rootTransform
        {
            get { return m_GameObject.transform; }
        }

        public override bool isValid
        {
            get { return m_GameObject != null; }
        }

        public NeoSerializedGameObjectChildContainer(NeoSerializedGameObject nsgo)
        {
            m_GameObject = nsgo;
        }
    }
}
