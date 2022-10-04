using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EnhancedUI.EnhancedScroller;
using TNet;
using Return.UI;
using UnityEngine.Events;
using TMPro;
using TheraBytes.BetterUi;
using Sirenix.OdinInspector;

namespace Return.Games
{
    public class JoinServerWindow : CustomWindow
    {
        public GameObject ServerRect;

        public TMP_InputField IP;
        public TMP_InputField Port;
        public TMP_InputField Password;

        public BetterImage PingStatus;

        public Gradient PingColor;

        string serverIP;
        string serverPort;
        string serverPassword;


        #region JoinServer Handle

        public virtual void ConntectServer()
        {
            Debug.Log($"Join server [{serverIP}] with port : {serverPort}.");


        }

        public void OnIPChange(string ip)
        {
            serverIP = ip;
        }

        public void OnPortChange(string port)
        {
            serverPort = port;
        }

        public void OnPasswordChange(string password)
        {
            serverPassword = password;
        }

        #endregion



        /// <summary>
        /// Update server status.
        /// </summary>
        protected virtual void FixedUpdate()
        {
            //update server status

            // current inspect server list

            // ping to color 
            foreach (var server in LobbyClient.knownServers.list)
            {
                var count = server.playerCount;
                LoadServerStatus(server);
            }
        }

        protected virtual void LoadServerStatus(ServerList.Entry server)
        {
            // **ping **playerNum  

        }

        private void OnGUI()
        {
            var list = LobbyClient.knownServers.list;
            //Debug.Log("Connecting to " + list.Count + TNManager.isJoiningChannel);

            return;

            // Server list example script automatically collects servers that have recently announced themselves
            for (int i = 0; i < list.size; ++i)
            {
                var ent = list.buffer[i];

                // NOTE: I am using 'internalAddress' here because I know all servers are hosted on LAN.

                // If you are hosting outside of your LAN, you should probably use 'externalAddress' instead.
                TNManager.Connect(ent.internalAddress, ent.internalAddress);
                //Debug.Log("Connecting to " + ent.internalAddress);

                if (TNManager.isConnected)
                    break;
            }
        }



    }
}