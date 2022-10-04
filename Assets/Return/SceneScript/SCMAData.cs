using UnityEngine;
using Return.Humanoid;
using System.Collections.Generic;
using System.Text;

namespace Return.SceneModule
{
    public class SCMAData : MonoBehaviour
    {
        [SerializeField]
        private SceneDataStruct.Reference module;
        public SceneDataStruct.Reference Module { get { return module; } }

        [SerializeField]
        private AudioResource Audio_UI = null;
        [SerializeField]
        private AudioResource Audio_Player = null;
        public enum ResourceVolume { UI=0,Player=1 }
        public AudioResource GetAudioResource(ResourceVolume volume)
        {
            switch (volume)
            {
                case ResourceVolume.UI:
                    return Audio_UI;
                case ResourceVolume.Player:
                    return Audio_Player;
            }
            return null;
        }


        [SerializeField]
        private Camera SceneCam;
        public Camera sceneCam { get { return SceneCam; } }

        //PlayerMode

        //private List<IPlayerFroegin> Players;



        [Header("Model")]
        [SerializeField]
        private SceneDataStruct.Prefab BaseManager;
  

        private const string managerTag = "Manager";

        //public void RegisterHandler(IPlayerFroegin foreignWindow)
        //{
        //    Players.Add(foreignWindow);
        //}

        //public void DisRegister(IPlayerFroegin foreignWindow)
        //{
        //    Players.Remove(foreignWindow);
        //}

        //public void dataInitialization()
        //{
        //    Players = new List<IPlayerFroegin>();

        //    if(MonoModule.lobby==null)
        //        module.lobby = Instantiate(BaseManager.Lobby,transform.parent).GetComponent<LoginLobby>();
        //    if (MonoModule.assetsGenerator == null)
        //        module.assetsGenerator = Instantiate(BaseManager.Generator, transform.parent).GetComponent<GeneratorManager>();
        //    if (MonoModule.guiCentre == null)
        //        module.guiCentre= Instantiate(BaseManager.GUICentre, transform.parent).GetComponent<GUICentre>();
           
        //    if (SceneCam == null)
        //        SceneCam = Instantiate(BaseManager.SceneCamera, transform.parent).GetComponent<Camera>();

        //    GameObject[] managers = GameObject.FindGameObjectsWithTag(managerTag);
        //    int length = managers.Length;
        //    StringBuilder sb = new StringBuilder(200);
        //    for(int i=0;i<length;i++)
        //    {
        //        sb.Append(managers[i].name) ;
        //        sb.AppendLine(" Manager has been activated !");
        //        //  IO
        //        if (MonoModule.controllerCentre == null)
        //            if (managers[i].TryGetComponent(out module.controllerCentre))
        //            {
        //                print(module.controllerCentre);
        //                continue;
        //            }

        //        // Lobby
        //        if (MonoModule.lobby == null)
        //            if (managers[i].TryGetComponent(out module.lobby))
        //            {
        //                print(MonoModule.lobby);
        //                continue;
        //            }

        //        //  Player


        //        //GUI
        //        if (MonoModule.guiCentre == null)
        //            if (managers[i].TryGetComponent(out module.guiCentre))
        //            {
        //                print(MonoModule.guiCentre.ToString());
        //                continue;
        //            }

        //        // Controller
        //        if (MonoModule.controllerCentre == null)
        //            if (managers[i].TryGetComponent(out module.controllerCentre))
        //            {
        //                print(MonoModule.controllerCentre.ToString());
        //                continue;
        //            }

        //        // Generator
        //        if (MonoModule.assetsGenerator == null)
        //            if (managers[i].TryGetComponent(out module.assetsGenerator))
        //            {
        //                print(MonoModule.assetsGenerator.ToString());
        //            }

        //        // ItemBag


        //    }
        //    print(sb.ToString());
        //}
    }
}