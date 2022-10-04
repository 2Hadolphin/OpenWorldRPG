//#if !UNITY_EDITOR
#if !UNITY_IPHONE && !UNITY_ANDROID && !MODDING
#define STEAM
#endif
//#endif

using System;
using UnityEngine;
using TNet;
using Buffer = TNet.Buffer;

#if STEAM
using System.IO;
using Steamworks;
#endif

/// <summary>
/// Wrapper class that simplifies the functionality of Steamworks into one handy class that uses TNet.
/// To use it, simply attach it to a game object present in your first game scene. This game object will become persistent.
///
/// The OnConnect() function below automatically calls AllowFriendsToJoin(true), which enables the ability of your friends to
/// right-click your player name and choose the Join Game option. If using matchmaking, you may want to control when
/// AllowFriendsToJoin() is called instead -- to do this simply comment it out from the OnConnect(), then call it yourself.
///
/// To connect to a friend manually by using their Steam ID, just call Steam.Connect("userID"), where 'userID' is their Steam ID,
/// such as Steam.Connect("76561198012345678").
/// </summary>

public class Steam : MonoBehaviour
{
	/// <summary>
	/// Application ID we'll be working with.
	/// TODO: SET THIS TO YOUR GAME'S OWN ID !!!
	/// </summary>

	public const int appID = 1771490;
	static ulong mID = 0;

#if STEAM
	static Steam mInst = null;
	static bool mIsActive = false;
	static string mUsername = null;
	static CSteamID mUserID;

	/// <summary>
	/// Whether the Steam API is available.
	/// </summary>

	static public bool isActive { get { return mIsActive; } }

	/// <summary>
	/// Steam ID is a fairly long number.
	/// </summary>

	static public ulong userID
	{
		get
		{
#if UNITY_EDITOR && !MODDING
			if (mID == 0) mID = mIsActive ? (ulong)mUserID : (ulong)SystemInfo.deviceUniqueIdentifier.GetHashCode();
#else
			if (mID == 0) mID = mIsActive ? (ulong)mUserID : (ulong)12345678901234567890; // Non-zero so that it can be banned by
#endif
			return mID;
		}
	}

	/// <summary>
	/// Get the Steam username.
	/// </summary>

	static public string username
	{
		get
		{
			if (string.IsNullOrEmpty(mUsername))
			{
				mUsername = mIsActive ? SteamFriends.GetPersonaName() : "Guest";
			}
			return mUsername;
		}
	}

	/// <summary>
	/// Command-rows argument used to auto-connect an external server.
	/// This will be set if Steam launches your game as the result of the "Join Game" choice on the player's list or player's profile.
	/// </summary>

	static internal string autoConnectString
	{
		get
		{
			try
			{
				bool connectStringIsNext = false;
				var args = Environment.GetCommandLineArgs();

				if (args != null)
				{
					for (int i = 0; i < args.Length; ++i)
					{
						string val = args[i];
						if (connectStringIsNext) return val;
						else if (val == "+connect") connectStringIsNext = true;
					}
				}
			}
			catch (Exception) { }
			return null;
		}
	}

	/// <summary>
	/// Whether the specified steam account is a friend.
	/// </summary>

	static internal bool HasFriend (ulong steamID)
	{
		if (isActive)
		{
			return SteamFriends.HasFriend(new CSteamID(steamID), EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagClanMember);
		}
		return false;
	}

	/// <summary>
	/// Whether the specified steam account is a friend.
	/// </summary>

	static internal bool HasFriend (string steamID)
	{
		if (isActive)
		{
			ulong id;
			if (ulong.TryParse(steamID, out id)) return SteamFriends.HasFriend(new CSteamID(id), EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagClanMember);
		}
		return false;
	}

	/// <summary>
	/// Whether the specified steam account is a friend.
	/// </summary>

	static internal bool HasFriend (CSteamID steamID)
	{
		if (isActive)
		{
			return SteamFriends.HasFriend(steamID, EFriendFlags.k_EFriendFlagImmediate | EFriendFlags.k_EFriendFlagClanMember);
		}
		return false;
	}

	/// <summary>
	/// Initialize Steamworks.
	/// </summary>

	void Awake ()
	{
		// Makes sure that only one instance of this object is in use at a time
		if (mInst != null)
		{
			Destroy(gameObject);
			return;
		}

		DontDestroyOnLoad(gameObject);
		mInst = this;

		if (!Packsize.Test())
		{
			// Likely using a Linux/OSX build on Windows or vice versa
			Debug.LogError("Steamworks.Packsize.Test() fail!");
			return;
		}

		if (!DllCheck.Test())
		{
			Debug.LogError("Steamworks.DllCheck.Test() fail!");
			return;
		}

		mIsActive = false;

		try
		{
			// This would make Steam launch your game if you want
			//if (SteamAPI.RestartAppIfNecessary(new AppId_t((uint)appID)))
			//{
			//	Application.Quit();
			//	return;
			//}

			mIsActive = SteamAPI.Init();
		}
		catch (Exception) { }

		if (!mIsActive)
		{
			mInst = null;
			Destroy(this);
			return;
		}

		mUserID = SteamUser.GetSteamID();
		Debug.Log("Steam " + mUserID);

		EnableNetworkingSupport();

		// Infrequently update Steam
		InvokeRepeating("RunCallbacks", 0.001f, 0.1f);
	}

	/// <summary>
	/// Auto-connect to the chosen user. The 'autoConnectString' will only be valid if the game was launched from Steam as the
	/// result of choosing the "Join Game" option. In this case Steam encodes the connect information in the launch arguments.
	/// Steam.Connect("userID") returns 'false' if it's not a valid user ID to connect to, in which case is then passed
	/// to TNManager.Connect instead, just in case the +connect string contains an IP instead.
	/// </summary>

	void Start ()
	{
		var str = autoConnectString;
		if (!string.IsNullOrEmpty(str) && !Connect(str)) TNManager.Connect(str);
	}

	[TNet.DoNotObfuscate]
	void RunCallbacks () { SteamAPI.RunCallbacks(); }

	[System.NonSerialized] static byte[] mTemp;
	[System.NonSerialized] static System.Collections.Generic.Dictionary<CSteamID, TcpProtocol> mOpen = new System.Collections.Generic.Dictionary<CSteamID, TcpProtocol>();
	[System.NonSerialized] static System.Collections.Generic.HashSet<CSteamID> mClosed = new System.Collections.Generic.HashSet<CSteamID>();

	class P2PConnection : IConnection
	{
		public CSteamID id;
		public bool connecting = false;
		public bool disconnected = false;

		public bool isConnected { get { return !disconnected; } }

		public bool SendPacket (Buffer buffer)
		{
			var offset = buffer.position;
			var bufferSize = offset + buffer.size;
			var packetSize = buffer.PeekInt(offset);
			if (packetSize == 0) return false;

			var packetStart = offset + 4; // Skip past the size
			int counter = 0;

			while (packetStart < bufferSize)
			{
				if (packetStart + packetSize > bufferSize) break;

				var fs = packetSize + 4;
				if (!Send(id, buffer.buffer, offset, fs)) return false;

				++counter;

				// Advance past the packet that was just sent
				offset += fs;
				if (offset == bufferSize) break;

				packetStart = offset + 4; // Skip past the size

				// Packets can't be below 5 bytes (4 bytes for size + 1 byte for ID)
				if (packetStart + 5 < bufferSize)
				{
					packetSize = buffer.PeekInt(offset);
					continue;
				}
				break;
			}
			return true;
		}

		public void ReceivePacket (out Buffer buffer) { buffer = null; }

		public void OnDisconnect ()
		{
			if (!disconnected)
			{
				disconnected = true;

				BeginSend(Packet.Disconnect);
				EndSend(id);

				lock (mOpen)
				{
					mOpen.Remove(id);
					if (!mClosed.Contains(id)) mClosed.Add(id);
				}

				if (TNManager.custom == this) TNManager.custom = null;
			}
		}
	}

	// Callbacks are added to a list so they don't get discarded by GC
	List<object> mCallbacks = new List<object>();

	/// <summary>
	/// Event subscriptions needed to get networking to work.
	/// </summary>

	void EnableNetworkingSupport ()
	{
		// P2P connection request
		mCallbacks.Add(Callback<P2PSessionRequest_t>.Create(delegate (P2PSessionRequest_t val)
		{
			if (TNServerInstance.isListening)
			{
				Debug.Log("P2P Request: " + val.m_steamIDRemote);

				// Want only friends to join? Use this:
				// if (HasFriend(val.m_steamIDRemote))

				SteamNetworking.AcceptP2PSessionWithUser(val.m_steamIDRemote);
			}
		}));

		// P2P connection error
		mCallbacks.Add(Callback<P2PSessionConnectFail_t>.Create(delegate (P2PSessionConnectFail_t val)
		{
			Debug.LogError("P2P Error: " + val.m_steamIDRemote + " (" + val.m_eP2PSessionError + ")");

			if (TNServerInstance.isListening)
			{
				var id = val.m_steamIDRemote;
				TcpProtocol tcp;

				if (mOpen.TryGetValue(id, out tcp))
				{
					// Existing connection
					var p2p = tcp.custom as P2PConnection;
					if (p2p != null) p2p.OnDisconnect();
				}
			}

			CancelInvoke("CancelConnect");
			CancelConnect();
		}));

		// Join a friend -- this is used when Steam launches the game as a result of choosing "Join Game" from the friend's list
		mCallbacks.Add(Callback<GameRichPresenceJoinRequested_t>.Create(delegate (GameRichPresenceJoinRequested_t val)
		{
			Debug.Log("P2P Join " + val.m_rgchConnect);

			if (!TNManager.isConnected && !TNManager.isTryingToConnect)
			{
				TNManager.playerName = username;
				var addr = val.m_rgchConnect;
				addr = addr.Replace("+connect ", "");
				if (!Connect(addr)) TNManager.Connect(addr);
			}
		}));

		TNManager.onDisconnect += OnDisconnect;
		TNManager.onConnect += OnConnect;
		TNManager.onUpdate += OnUpdate;
	}

	/// <summary>
	/// Connection notification: allow friends to join (if on a public server), and set the visible presence.
	/// </summary>

	static void OnConnect (bool success, string msg)
	{
		// This is what makes it possible for your Steam friends to right-click your name and join your game.
		// If you want to control when this joining will be allowed or disallowed instead, call this yourself from a more appropriate place.
		AllowFriendsToJoin(success);
	}

	/// <summary>
	/// Notification of being disconnected from the game server.
	/// </summary>

	static void OnDisconnect ()
	{
		if (mIsActive && TNServerInstance.isActive)
		{
			var p2p = TNManager.custom as P2PConnection;

			if (p2p != null)
			{
				p2p.OnDisconnect();
				Debug.Log("OnDisconnect " + mIsActive);
			}
		}

		AllowFriendsToJoin(false);
	}

	void CancelConnect ()
	{
		var p2p = TNManager.custom as P2PConnection;

		if (p2p != null && p2p.connecting)
		{
			Debug.Log("CancelConnect " + (p2p != null ? p2p.connecting.ToString() : "?"));
			TNManager.client.stage = TcpProtocol.Stage.NotConnected;
			TNManager.onConnect(false, "Unable to connect");
			TNManager.custom = null;
		}
	}

	/// <summary>
	/// Start a new P2P connection with the specified player.
	/// </summary>

	static public bool Connect (string str)
	{
		ulong steamID;

		if (mInst != null && isActive && !str.Contains(".") && ulong.TryParse(str, out steamID))
		{
			mInst.ConnectP2P(new CSteamID(steamID));
			return true;
		}
		return false;
	}

	/// <summary>
	/// Start a new P2P connection with the specified player.
	/// </summary>

	void ConnectP2P (CSteamID id)
	{
		if (TNManager.custom == null && !TNManager.isConnected && !TNManager.isTryingToConnect && mInst != null)
		{
			CancelInvoke("CancelConnect");

			var p2p = new P2PConnection();
			p2p.id = id;
			p2p.connecting = true;
			TNManager.custom = p2p;
			TNManager.client.stage = TcpProtocol.Stage.Verifying;

			Debug.Log("Connecting to " + id);

			var writer = BeginSend(Packet.RequestID);
			writer.Write(Player.version);
			writer.Write(TNManager.playerName);
			writer.Write(TNManager.playerData);
			EndSend(id);

			Invoke("CancelConnect", 8f);
		}
#if UNITY_EDITOR
		else Debug.Log("Already connecting, ignoring");
#endif
	}

	static bool Send (CSteamID id, byte[] data, int offset, int size)
	{
		if (offset == 0)
		{
			// Single packet -- send it as-is
			var retVal = SteamNetworking.SendP2PPacket(id, data, (uint)size, EP2PSend.k_EP2PSendReliable);
			if (!retVal) Debug.LogError("P2P failed to send the packet (" + size + " bytes)");
			return retVal;
		}
		else
		{
			// Multiple packets in the same buffer: Steam's API doesn't offer an offset index option, so the packet must be copied
			var temp = Buffer.Create();
			var writer = temp.BeginWriting();
			writer.Write(data, offset, size);
			temp.EndWriting();

			var retVal = SteamNetworking.SendP2PPacket(id, temp.buffer, (uint)size, EP2PSend.k_EP2PSendReliable);
			if (!retVal) Debug.LogError("P2P failed to send the packet (" + size + " bytes)");
			temp.Recycle();
			return retVal;
		}
	}

	[System.NonSerialized] static Buffer mTempBuffer;

	static BinaryWriter BeginSend (Packet packet)
	{
		mTempBuffer = Buffer.Create();
		return mTempBuffer.BeginPacket(packet);
	}

	static bool EndSend (CSteamID id)
	{
		if (mTempBuffer != null)
		{
			var size = mTempBuffer.EndPacket();
			var retVal = Send(id, mTempBuffer.buffer, 0, size);

			mTempBuffer.Recycle();
			mTempBuffer = null;
			return retVal;
		}
		return false;
	}

	/// <summary>
	/// Receive all incoming P2P packets.
	/// </summary>

	static void OnUpdate ()
	{
		// If Steam's multiplayer is being used, we should run the callbacks more often to ensure that there is minimal delay between packets
		if (TNServerInstance.isListening || (TNManager.custom as P2PConnection != null))
		{
			UnityEngine.Profiling.Profiler.BeginSample("Steam.OnUpdate(Run callbacks)");
			SteamAPI.RunCallbacks();
			UnityEngine.Profiling.Profiler.EndSample();
		}

		uint size;
		if (!SteamNetworking.IsP2PPacketAvailable(out size)) return;

		UnityEngine.Profiling.Profiler.BeginSample("Steam.OnUpdate(Process packets)");
		CSteamID id;

		lock (mOpen)
		{
			for (; ; )
			{
				if (mTemp == null || mTemp.Length < size) mTemp = new byte[size < 4096 ? 4096 : size];

				if (SteamNetworking.ReadP2PPacket(mTemp, size, out size, out id))
				{
					AddPacketP2P(id, mTemp, size);
				}

				if (!SteamNetworking.IsP2PPacketAvailable(out size))
				{
					UnityEngine.Profiling.Profiler.EndSample();
					return;
				}
			}
		}
	}

	/// <summary>
	/// Add an incoming network packet to be processed.
	/// </summary>

	static void AddPacketP2P (CSteamID id, byte[] data, uint size)
	{
#if UNITY_EDITOR
		if (!Application.isPlaying) return;
#endif
		TcpProtocol tcp;

		if (mOpen.TryGetValue(id, out tcp))
		{
			// Existing connection
			var p2p = tcp.custom as P2PConnection;
			if (p2p != null && p2p.connecting) p2p.connecting = false;
		}
		else if (TNServerInstance.isListening)
		{
			// New connection
			var p2p = new P2PConnection();
			p2p.id = id;

			lock (mOpen)
			{
				tcp = TNServerInstance.AddPlayer(p2p);
				mOpen[id] = tcp;
				mClosed.Remove(id);
			}
		}
		else if (TNManager.custom != null)
		{
			// New connection
			var p2p = TNManager.custom as P2PConnection;
			if (p2p == null) return;

			p2p.id = id;
			tcp = TNManager.client.protocol;

			lock (mOpen)
			{
				mOpen[id] = tcp;
				mClosed.Remove(id);
			}
		}
		else return;

		tcp.OnReceive(data, 0, (int)size);
	}

	/// <summary>
	/// Cleanup.
	/// </summary>

	void OnDestroy ()
	{
		if (mInst == this)
		{
			TNManager.onDisconnect -= OnDisconnect;
			TNManager.onConnect -= OnConnect;
			TNManager.onUpdate -= OnUpdate;

			mInst = null;
			foreach (var pair in mOpen) SteamNetworking.CloseP2PSessionWithUser(pair.Key);
			foreach (var id in mClosed) SteamNetworking.CloseP2PSessionWithUser(id);
			mClosed.Clear();
			mOpen.Clear();

			SteamAPI.Shutdown();
			mIsActive = false;
		}
	}

	/// <summary>
	/// If set to 'true', friends will be able to right-click to join the game.
	/// If the server is not actually listening for incoming connections, the 'allow' parameter is going to be treated as 'false'.
	/// </summary>

	static public void AllowFriendsToJoin (bool allow)
	{
		if (mIsActive)
		{
			if (allow)
			{
				if (TNServerInstance.isActive)
				{
					if (TNServerInstance.isListening)
					{
						// This would allow joining by IP:
						//SteamFriends.SetRichPresence("connect", "+connect " + Tools.externalAddress + ":" + TNServerInstance.listeningPort);

						// This allows joining by player ID:
						SteamFriends.SetRichPresence("connect", "+connect " + mUserID);
					}
					else SteamFriends.SetRichPresence("connect", "");
				}
				else if (TNManager.client != null && TNManager.client.custom != null)
				{
					var p2p = TNManager.client.custom as P2PConnection;
					if (p2p != null) SteamFriends.SetRichPresence("connect", "+connect " + p2p.id);
					else SteamFriends.SetRichPresence("connect", "");
				}
				else if (TNManager.isConnected)
				{
					SteamFriends.SetRichPresence("connect", "+connect " + TNManager.tcpEndPoint);
				}
				else SteamFriends.SetRichPresence("connect", "");
			}
			else SteamFriends.SetRichPresence("connect", "");
		}
	}
#else
	static public bool isActive { get { return false; } }
	static public ulong userID { get { if (mID == 0) mID = (ulong)SystemInfo.deviceUniqueIdentifier.GetHashCode(); return mID; } }
	static public string username { get { return "Guest"; } }
	static internal string autoConnectString { get { return null; } }

	static public bool Connect (string str) { return false; }
	static public void AllowFriendsToJoin (bool allow) { }
	static internal void SetPresence (string key, string text) { }
#endif // STEAM
}
