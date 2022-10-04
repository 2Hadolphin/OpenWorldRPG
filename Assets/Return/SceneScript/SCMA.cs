using UnityEngine;
using Return.Humanoid;


namespace Return.SceneModule
{
/// <summary>
/// SceneManager
/// </summary>
    public class SCMA : SceneModule
    {
        public static SCMA _Instance;
        public static SCMA Instance
        {
            get
            {
                if (!_Instance)
                {
                    var go = new GameObject("SCMA");
                    go.hideFlags = HideFlags.HideInHierarchy;
                    _Instance = go.AddComponent<SCMA>();
                }
                return _Instance;
            }
        }
        public SCMAData Data { get { return data; } }

        //private IPlayerFroegin[] Players;

        public float Gravity = -9.8f;
        public bool GamePause = false;


        public override void Init()
        {
            data = gameObject.GetComponent<SCMAData>();
            //data.dataInitialization();
            Manager = this;
            //Players = new IPlayerFroegin[64];
        }

        public void Wakeup()
        {
            //Preset.MonoModule.assetsGenerator.Init();
            //Preset.MonoModule.controllerCentre.Init();
            //Preset.MonoModule.guiCentre.Init();
        }

        protected virtual void Start()
        {
            //LoadScene();
        }

        private void LoadScene()
        {
            Data.Module.assetsGenerator.BuildScene();
        }

        private void BuildLoaclPlayer()
        {
            //if ((int)gameData.Mode == 1)   ???
            if(true)
                Data.Module.assetsGenerator.LoadHost();
        }

        //public void RegistePlayer(IPlayerFroegin player)
        //{
        //    for(int i = 0; i < 64; i++)
        //    {
        //        if (Players[i] == null)
        //        {
        //            Players[i] = player;
        //            break;
        //        }
        //    }

        //    player.GetRoot.GetComponentInChildren<HumanoidAgent>().Init();
        //}


        public void ExitGame()
        {
            print("KKKK");
            //Save Preset
            Application.Quit();

        }

     



        public void AssignUersData(string UID)
        {
            print("Loading data from" + UID + "..");
            GameObject.Destroy(Data.sceneCam.gameObject);
            
            BuildLoaclPlayer();
            //Preset.MonoModule.controllerCentre.SetCursorActive(false);
        }

        public void TakeData()  // Read Preset from files
        {

        }


        private void OnPauseGame(bool boool)
        {
            GamePause = boool;
        }

    }
}