using UnityEngine;
using Return.CentreModule;

namespace Return.SceneModule
{

    public class SceneDataStruct
    {
        [System.Serializable]
        public struct Reference
        {
            public GeneratorManager assetsGenerator;
            public LoginLobby lobby;
            public GUICentre guiCentre;
        }

        [System.Serializable]
        public struct Prefab
        {
            public GameObject Controller;
            public GameObject Generator;
            public GameObject Lobby;
            public GameObject GUICentre;
            public GameObject SceneCamera;
        }



    }
}