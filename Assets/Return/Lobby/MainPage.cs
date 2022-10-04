using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using TMPro;
using Sirenix.OdinInspector;
using System;
using Return.UI;
using Cysharp.Threading.Tasks;
using Return.Cameras;


namespace Return.Games
{
    /// <summary>
    /// Main page to join server or host game.
    /// </summary>
    public class MainPage : BaseComponent, IStart
    {
        public static Action OnMainPageClose;

        #region Routine

        protected void Awake()
        {
            Routine.AddStartable(this);
        }

        async void IStart.Initialize()
        {
            ConnectSteam();

            // link Input menu
            //var Inputs


            // instantiate windows
            //if(false)
            {
                JoinServerWindow = GetWindow(JoinServerWindow);
                CreateLobbyWindow = GetWindow(CreateLobbyWindow);
                ExitWindow = GetWindow(ExitWindow);
                BindingOptionWindow = GetWindow(BindingOptionWindow);
                LoadScene = GetWindow(LoadScene);

                await UniTask.NextFrame();

                JoinServerWindow.enabled = false;
                CreateLobbyWindow.enabled = false;
                ExitWindow.enabled = false;
                BindingOptionWindow.enabled = false;
                LoadScene.enabled = false;
            }


      
        }

        [Inject(InjectOption.Framework)]
        void OnSceneLoad(CameraManager cameraManager)
        {
            var cam = CameraManager.mainCameraHandler;

            if (cam == null)
                cam = CameraManager.GetCamera<CinemachineSceneCamera>();

            cam.Activate();
        }

        private void OnEnable()
        {
            OnMainPageClose -= ClosePage;
            OnMainPageClose += ClosePage;
        }

        private void OnDisable()
        {
            OnMainPageClose -= ClosePage;
        }

        #endregion

        #region Main Page

        [Required, SerializeField]
        OptionUI m_ExitOptionWindow;

        public OptionUI ExitWindow { get => m_ExitOptionWindow; set => m_ExitOptionWindow = value; }

        /// <summary>
        /// Exit dialogue option window.
        /// </summary>
        [Required, SerializeField]
        OptionUI m_BindingOptionWindow;

        /// <summary>
        /// Binding input dialogue option window.
        /// </summary>
        public OptionUI BindingOptionWindow { get => m_BindingOptionWindow; set => m_BindingOptionWindow = value; }



        /// <summary>
        /// Close main page.
        /// </summary>
        void ClosePage()
        {
            gameObject.SetActive(false);
        }

        /// <summary>
        /// Set close game dialogue and callback.
        /// </summary>
        public virtual void ExitGamePage()
        {
            ExitWindow.RestOption();
            ExitWindow.SetCallback(ExitGame);
        }

        /// <summary>
        /// Shut down application.
        /// </summary>
        protected virtual void ExitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
        }

        #endregion

        #region Windows

        #region HostServer


        [Required, SerializeField]
        CreateLobbyWindow m_CreateLobby;

        public CreateLobbyWindow CreateLobbyWindow { get => m_CreateLobby; set => m_CreateLobby = value; }


        /// <summary>
        /// Open host server window.
        /// </summary>
        public virtual void CreateLobbyUI()
        {
            CreateLobbyWindow.enabled = true;
        }

        #endregion

        #region JoinServer

        [Required, SerializeField]
        JoinServerWindow m_JoinServer;

        public JoinServerWindow JoinServerWindow { get => m_JoinServer; set => m_JoinServer = value; }


        /// <summary>
        /// Open join server window.
        /// </summary>
        public virtual void JoinServerUI()
        {
            JoinServerWindow.enabled = true;
        }

        #endregion

        #region QuickGame

        [Required, SerializeField]
        LoadSceneWindow m_loadScene;

        public LoadSceneWindow LoadScene { get => m_loadScene; set => m_loadScene = value; }


        /// <summary>
        /// Random start game. **loading scene
        /// </summary>
        public virtual void RandomJoinGame()
        {
            Debug.Log("Random join game.");

            LoadScene.enabled = true;
            LoadScene.LoadScene();
            ClosePage();
        }

        #endregion

        protected T GetWindow<T>(T asset) where T : CustomWindow
        {
            if (!asset.IsInstance())
                asset = OverlapUIManager.InstantiateWindowUI(asset);

            return asset;
        }

        #endregion


        #region Steam

        //[ReadOnly][ShowInInspector]
        // protected SteamManager m_SteamManager;
        //public SteamManager GetSteamManager => m_SteamManager;

        public TextMeshProUGUI UserID;
        public TextMeshProUGUI UserUID;

        /// <summary>
        /// Display steam account window.
        /// </summary>
        public virtual void LinkProfile()
        {
            if (!SteamManager.Initialized)
            {
                // reconected
            }
            else
            {
                // show profile
            }
        }

        protected virtual void ConnectSteam()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam manager initialized fail.");
                //show ui disconnected

                return;
            }

            UserID.text = SteamFriends.GetPersonaName();//SteamUser.GetSteamID().GetAccountID().m_AccountID.ToString();
            UserUID.text = SteamUser.GetSteamID().m_SteamID.ToString();
        }

        #endregion
    }
}