using System.Collections;
using UnityEngine;
using Steamworks;
using TNet;
using TMPro;
using UnityEngine.Events;
using Return.UI;

namespace Return.Games
{
    public class CreateLobbyWindow : CustomWindow
    {
        public TextMeshProUGUI RoomName;


        public int TestChannel = 1;

        protected bool CanFriendJoin = true;

        private const string HostAddressKey = "HostAddressKey";

        public enum IPType
        {
            IP_v_4,
            IP_v_6,
        }

        public int serverTcpPort = 5127;
        public IPType addressFamily = IPType.IP_v_4;

        protected Callback<LobbyCreated_t> LobbyCreatedCallback;
        protected Callback<GameLobbyJoinRequested_t> GameLobbyJoinRequestedCallback;
        protected Callback<LobbyEnter_t> LobbyEnterCallback;


        protected int Port;
        protected string Password;

        protected override void OnEnable()
        {
            base.OnEnable();
            ShowPage.Invoke();
        }

        public virtual void OnPortChange(string port)
        {
            if (string.IsNullOrEmpty(port))
                Port = 1;
            else
                Port = int.Parse(port);
        }

        public virtual void OnPasswordChange(string password)
        {
            if (string.IsNullOrEmpty(password))
                Password = null;
            else
                Password = password;
        }

        public virtual void OnGameModeClick(GameObject @object)
        {
            // show game setting **ttk **resources **map
        }

        private void Awake()
        {
            if (!SteamManager.Initialized)
            {
                Debug.LogError("Steam init failaur");
                // disable server option and pop error

            }
            else
            {
                var id = SteamFriends.GetPersonaName();//SteamUser.GetSteamID().GetAccountID().m_AccountID.ToString();
                var uid = SteamUser.GetSteamID().m_SteamID.ToString();

                if (string.IsNullOrEmpty(id))
                    RoomName.text = "Failure to login steam account.";
                else
                    RoomName.text = string.Format(RoomName.text, id);
            }


            ShowPage.Invoke();
            return;

            LobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            GameLobbyJoinRequestedCallback = Callback<GameLobbyJoinRequested_t>.Create(OnGameJoinLobbyRequested);
            LobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);



            #region ?
            if (Application.isPlaying)
            {
                // Choose IPv6 or IPv4
                TcpProtocol.defaultListenerInterface = addressFamily == IPType.IP_v_6 ? System.Net.IPAddress.IPv6Any : System.Net.IPAddress.Any;

                // TNet will automatically switch UDP to IPv6 if TCP uses it, but you can specify an explicit one if you like
                //UdpProtocol.defaultNetworkInterface = useIPv6 ? System.Net.IPAddress.IPv6Any : System.Net.IPAddress.Any;

                //if (mInst == null)
                {
                    //mInst = this;
                    DontDestroyOnLoad(gameObject);
                }
                //else Destroy(CharacterRoot);
            }
            #endregion
        }

        public virtual void SetRoomPublic(bool canFriendJoin)
        {
            CanFriendJoin = canFriendJoin;
        }

        /// <summary>
        /// Func invoke by unity button click under game mode_start. 
        /// </summary>
        public virtual void CreateRoom(string sceneName)
        {
            // disable create lobby button
            // private & number
            //SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 32);

            //Start a LAN Server
            if (true || CanFriendJoin)
            {
                // Start a local server, loading the saved data if possible
                // The UDP port of the server doesn't matter much as it's optional,
                // and the clients get notified of it via Packet.ResponseSetUDP.
                if (Port == 0)
                    Port = Random.Range(10000, 40000);

                var lobby = new GameObject("LobbyClient").AddComponent<TNUdpLobbyClient>();

                if (lobby == null)
                {
                    if (TNServerInstance.Start(serverTcpPort, Port, "server.dat"))
                        TNManager.Connect();
                }
                else
                {
                    TNServerInstance.Type type = lobby is TNUdpLobbyClient ?
                        TNServerInstance.Type.Udp : TNServerInstance.Type.Tcp;

                    if (TNServerInstance.Start(serverTcpPort, Port, lobby.remotePort, "server.dat", type))
                        TNManager.Connect();
                }
                //Server started

                //if (GUILayout.Button("Connect", button))
                //{
                //    // We want to connect to the specified destination when the button is clicked on.
                //    // "OnConnect" function will be called sometime later with the result.
                //    TNManager.Connect(mAddress);
                //    mMessage = "Connecting...";
                //}


            }
            // Start a local server that doesn't use sockets. It's ideal for testing and for single player gameplay.
            //Start a Virtual Server => single player
            else
            {
                TNServerInstance.Start("server.dat");
                TNManager.Connect();
            }

            // load scene

            if (!Application.isPlaying || UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != sceneName)
            {
                TNManager.CreateChannel(sceneName, false, 30, null, false);
                //TNManager.JoinChannel(1, sceneName, true, 32, Password);
                MainPage.OnMainPageClose();
                Destroy(gameObject);
            }

            //if (TNManager.channels.size > 0)
            //{
            //    TNManager.LeaveAllChannels();
            //}
        }



        #region JoinServer

        void JoinServerList()
        {
            // List of discovered servers
            List<ServerList.Entry> list = LobbyClient.knownServers.list;

            // Server list example script automatically collects servers that have recently announced themselves
            for (int i = 0; i < list.size; ++i)
            {
                var ent = list.buffer[i];

                // NOTE: I am using 'internalAddress' here because I know all servers are hosted on LAN.
                // If you are hosting outside of your LAN, you should probably use 'externalAddress' instead.
                TNManager.Connect(ent.internalAddress, ent.internalAddress);
            }
        }


        void DisconnectFromServer()
        {
            // Disconnecting while in some channel will cause "OnLeaveChannel" to be sent out first,
            // followed by "OnDisconnect". Disconnecting while not in a channel will only trigger
            // "OnDisconnect".
            TNManager.Disconnect();
        }


        #endregion

        #region Game back to MainMenu
        void backToMenu()
        {
            // Leaving the channel will cause the "OnLeaveChannel" to be sent out.
            TNManager.LeaveAllChannels();
        }
        #endregion

        protected virtual void OnLobbyCreated(LobbyCreated_t callback)
        {
            if (callback.m_eResult != EResult.k_EResultOK)
            {
                // enable create lobby button
                return;
            }

            SteamMatchmaking.SetLobbyData(
                new CSteamID(callback.m_ulSteamIDLobby),
                HostAddressKey,
                SteamUser.GetSteamID().ToString()
                );

        }

        protected virtual void OnGameJoinLobbyRequested(GameLobbyJoinRequested_t callback)
        {
            var api = SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
        }


        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            var hostAddress = SteamMatchmaking.GetLobbyData
                (
                new CSteamID(callback.m_ulSteamIDLobby),
                HostAddressKey
                );
        }

        void TNetTest()
        {
            TNServerInstance.Start("MyServer");
            TNManager.Connect();

        }

    }

}