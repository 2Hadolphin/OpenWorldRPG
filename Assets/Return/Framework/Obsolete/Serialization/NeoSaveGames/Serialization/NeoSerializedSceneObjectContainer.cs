using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeoSaveGames.Serialization
{
    [Serializable]
    public class NeoSerializedSceneObjectContainer : NeoSerializedGameObjectContainerBase
    {
        [SerializeField]
        private NeoSerializedScene m_Scene = null;

        public override Transform rootTransform
        {
            get { return null; }
        }

        public override bool isValid
        {
            get { return m_Scene != null; }
        }

        public NeoSerializedScene serializedScene
        {
            get { return m_Scene; }
        }

        public NeoSerializedSceneObjectContainer(NeoSerializedScene scene)
        {
            m_Scene = scene;
        }

        public override void RegisterObject(NeoSerializedGameObject nsgo)
        {
            base.RegisterObject(nsgo);

            if (isBuildingHierarchy)
                SceneManager.MoveGameObjectToScene(nsgo.gameObject, m_Scene.scene);
        }
    }
}
