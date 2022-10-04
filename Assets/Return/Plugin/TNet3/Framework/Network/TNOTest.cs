using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;



namespace TNet
{
	public enum IPType
	{
		IP_v_4,
		IP_v_6,
	}

	public class TNOTest : TNEventReceiver
    {
        public bool isMine { get; set; } = true;

		public int serverTcpPort = 5127;
		public IPType addressFamily = IPType.IP_v_4;

		/// <summary>
		/// Keep only one instance of this object.
		/// </summary>

		void Awake()
		{
			if (Application.isPlaying)
			{
				// Choose IPv6 or IPv4
				TcpProtocol.defaultListenerInterface = (addressFamily == IPType.IP_v_6) ? System.Net.IPAddress.IPv6Any : System.Net.IPAddress.Any;

				// TNet will automatically switch UDP to IPv6 if TCP uses it, but you can specify an explicit one if you like
				//UdpProtocol.defaultNetworkInterface = useIPv6 ? System.Net.IPAddress.IPv6Any : System.Net.IPAddress.Any;

			}
		}


		/// <summary>
		/// Start listening for incoming UDP packets right away.
		/// </summary>

		void Start()
		{
			if (Application.isPlaying)
			{
				// Start resolving IPs
				Tools.ResolveIPs(null);

				// We don't want mobile devices to dim their screen and go to sleep while the app is running
				Screen.sleepTimeout = SleepTimeout.NeverSleep;
			}
		}


		float mAlpha = 0f;

		/// <summary>
		/// Adjust the server list's alpha based on whether it should be shown or not.
		/// </summary>

		void Update()
		{
			if (Application.isPlaying)
			{
				float target = (LobbyClient.knownServers.list.size == 0) ? 0f : 1f;
				mAlpha = UnityTools.SpringLerp(mAlpha, target, 8f, Time.deltaTime);
			}
		}


		string mAddress = "127.0.0.1";



		void Connect()
        {
            if (GUILayout.Button("Connect"))
            {
                // We want to connect to the specified destination when the button is clicked on.
                // "OnConnect" function will be called sometime later with the result.
                TNManager.Connect(mAddress);
                //mMessage = "Connecting...";
            }
        }

		void CreateServer()
        {

			if (TNServerInstance.isActive)
			{
				GUI.backgroundColor = Color.red;

				if (GUILayout.Button("Stop the Server"))
				{
					// Stop the server, saving all the data
					TNServerInstance.Stop(); 
					//mMessage = "Server stopped";
				}
			}
			else
			{
				GUI.backgroundColor = Color.green;

				if (GUILayout.Button("Start a LAN Server"))
				{
					// Start a local server, loading the saved data if possible
					// The UDP port of the server doesn't matter much as it's optional,
					// and the clients get notified of it via Packet.ResponseSetUDP.
					int udpPort = Random.Range(10000, 40000);
					LobbyClient lobby = GetComponent<LobbyClient>();

					if (lobby == null)
					{
						if (TNServerInstance.Start(serverTcpPort, udpPort, "server.dat"))
							TNManager.Connect();
					}
					else
					{
						TNServerInstance.Type type = (lobby is TNUdpLobbyClient) ?
							TNServerInstance.Type.Udp : TNServerInstance.Type.Tcp;

						if (TNServerInstance.Start(serverTcpPort, udpPort, lobby.remotePort, "server.dat", type))
							TNManager.Connect();
					}
					//mMessage = "Server started";

				}

				// Start a local server that doesn't use sockets. It's ideal for testing and for single player gameplay.
				if (GUILayout.Button("Start a Virtual Server"))
				{
					//mMessage = "Server started";
					TNServerInstance.Start("server.dat");
					TNManager.Connect();
				}
			}
		}

        
        [SerializeField]
		public System.Collections.Generic.List<string> Scenes;

		void DrawSelectionMenu()
		{
			var count = Scenes.Count;

			for (int i = 0; i < count; ++i)
			{
				string sceneName = Scenes[i];

				if (GUILayout.Button(sceneName))
				{
					// When a button is clicked, join the specified channel.
					// Whoever creates the channel also sets the scene that will be loaded by everyone who joins.
					// In this case, we are specifying the name of the scene we've just clicked on.
					TNManager.JoinChannel(i + 1, sceneName, true, 255, null);
				}
			}
		}
	}
}