using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System;
using Object = UnityEngine.Object;
using UnityEngine.ResourceManagement.AsyncOperations;
using TNet;
using System.IO;
using System.Text;
using UnityEngine.Assertions;
using UnityEngine.ResourceManagement.ResourceLocations;
using Return;
using Cysharp.Threading.Tasks;

namespace TNet
{
	[DoNotObfuscate]
	public enum ReturnPacket
    {
		/// <summary>
		/// Begin custom packets here.
		/// </summary>

		UserPacket = 128,

		CustomPacket = 256,

		RequestCreateAddressalbeObject,

		ResponseCreateAddressableObject,
	}


	public partial class TNManager // plug-in
	{

#if UNITY_EDITOR
		public const string Plugin = "TNManagerPlugin";
#endif

		public static void Init()
		{
			mInstance.Init_Custom();
		}

		protected virtual void Init_Custom()
		{
            if (TNServerInstance.isActive)
            {
				TNServerInstance.game.onCustomPacket += TNServerInstance.game.ProcessCustomPacket;
			}
            else
            {
				Debug.Log("Init with remote TNserver "+TNServerInstance.lobby);
            }


			TNManager.SetPacketHandler(Packet.ResponseCreateAddressableObject, TNManager.client.ProcessCustomPacket);

			//mClient.onCreate = OnCreateObject;
			mClient.onCreate = OnTryCreateObject;

			//TNManager.client.onCreate = TNManager.OnResponseCreateAddressableObject;
		}


		static public void OnTryCreateObject(int channelID, int creator, uint objectID, BinaryReader reader)
		{
			currentObjectOwner = GetPlayer(creator) ?? GetHost(channelID);


			lastChannelID = channelID;
			var rccID = reader.ReadByte();
			var funcName = (rccID == 0) ? reader.ReadString() : null;
			//var func = GetRCC(rccID, funcName);

			// Load the object from the resources folder
			var prefabPath = reader.ReadString();

			Debug.Log(nameof(OnTryCreateObject)+" " +currentObjectOwner.name + " create " + objectID + " via " + funcName);

			

			if (UnityTools.TryLoadFromCache(prefabPath, out var prefab))
			{
				// immediately create if prefab not null then close this thred.
				Debug.Log("Get prefab via cache " + prefab);
			}

			if (prefab.NotNull() && UnityTools.LoadPrefabFromResource(prefabPath, out prefab))
			{
				// try load from resources immediately create if prefab not null then close this thred.
				Debug.Log("Get prefab via resource " + prefab);
			}





			// Custom creation function
			var count = reader.ReadInt() + 1;
			var objs = new object[count];

			objs[0] = prefab;
			for (int i = 1; i < count; ++i)
				objs[i] = reader.ReadObject();


			var sb = new StringBuilder();

			sb.AppendLine(objectID.ToString());

			foreach (var obj in objs)
			{
				if (obj.NotNull())
					sb.AppendLine(obj.ToString());
				else
					sb.AppendLine("Null");
			}


			if (prefab.NotNull())
			{
				Debug.LogError("Generate immediately");
				LoadPrefabTno(channelID, objectID, rccID, funcName, prefab, objs);
			}
			else
			{
				Debug.Log(TNManager.GetPlayer(creator).name + " generate parameter nums : " + sb.ToString());
				instance.StartCoroutine(ProcessResponseCreateAddressableObject(channelID, creator, objectID, rccID, funcName, prefabPath, objs));
			}
		}

		public bool AddressableResourceExists(object key)
		{
			foreach (var l in Addressables.ResourceLocators)
			{
				if (l.Locate(key, typeof(GameObject), out var locs))
					return true;
			}
			return false;
		}



		//		static public void Instantiate(int channelID, string funcName, AssetReference asset, bool persistent, params object[] objs)
		//        {
		//			var rccID = 0;

		//            var path = Addressables.LoadResourceLocationsAsync(asset).WaitForCompletion().First()?.PrimaryKey;

		//			if (path == null)
		//            {
		//				Debug.LogError(new KeyNotFoundException(funcName + asset.AssetGUID));
		//				path = "";
		//			}

		//			//lastChannelID = channelID;
		//			var player=TNManager.GetPlayer(TNManager.playerID);

		//            if (player != null)
		//            {
		//				Debug.Log(player.id);
		//            }

		//            //var newID = TNObject.GetUniqueID(true);



		//            if (instance != null)
		//			{
		//				var func = GetRCC(rccID, funcName);

		//				if (func == null)
		//				{
		//					Debug.LogError("Unable to locate RCC " + rccID + " " + funcName);
		//				}
		//#if !MODDING
		//				else if (isConnected)
		//				{
		//					if (!TNManager.IsInChannel(channelID))
		//					{
		//						if (!TNManager.IsJoiningChannelDefault(channelID))
		//							TNManager.JoinChannel(channelID, true);

		//						instance.StartCoroutine(SendRequestCreateAddressalbeObject(channelID, rccID, funcName, path, persistent, objs));
		//						return;
		//					}
		//                    else
		//                    {
		//						Debug.Log("Create immediately inside the channel.");
		//						instance.StartCoroutine(SendRequestCreateAddressalbeObject(channelID, rccID, funcName, path, persistent, objs));
		//					}
		//					//					if (IsJoiningChannel(channelID))
		//					//					{
		//					//#if UNITY_EDITOR
		//					//						Debug.LogWarning("Trying to create an object while switching scenes. Call will be ignored.");
		//					//#endif
		//					//						return;
		//					//					}


		//					//					if (!IsInChannel(channelID))
		//					//					{
		//					//#if UNITY_EDITOR
		//					//						Debug.LogWarning("Must join the channel first before calling instantiating objects.");
		//					//#endif
		//					//						return;
		//					//					}

		//				}
		//#endif
		//				else 
		//				{
		//					// Offline mode

		//					TNManager.instance.CreateAddressableObject_Offline(
		//						channelID,
		//						GetHost(channelID),
		//						rccID,
		//						funcName,
		//						TNObject.GetUniqueID(true),
		//						path,
		//						objs);

		//					//currentRccObjectID = TNObject.GetUniqueID(true);
		//					//objs = BinaryExtensions.CombineArrays(go, objs);
		//					//go = func.Execute(objs) as GameObject;
		//					//UnityTools.Clear(objs);

		//                    //var loader = new AddressableLoader()
		//                    //{
		//                    //    rccID = rccID,
		//                    //    funcName = funcName,
		//                    //    channelID = channelID,
		//                    //    persistent = persistent,
		//                    //    objs = objs,
		//                    //    path = path
		//                    //};

		//                    //UnityTools.LoadAddressablePrefab(loader);

		//                }
		//			}
		//#if UNITY_EDITOR
		//			else Debug.LogError("Unable to load " + path);
		//#endif


		//		}

		//static IEnumerator SendRequestCreateAddressalbeObject(int channelID, int rccID, string funcName, string path, bool persistent, params object[] objs)
		//{
		//	if (!TNManager.IsInChannel(channelID) || TNManager.IsJoiningChannelDefault(channelID))
		//		yield return instance.StartCoroutine(WaitJoinChannel(channelID));

		//	var writer = mInstance.mClient.BeginSend(Packet.RequestCreateAddressalbeObject);
		//	writer.Write(playerID);
		//	writer.Write(channelID);
		//	writer.Write(persistent ? (byte)1 : (byte)2);

		//	if (rccID > 0 && rccID < 256)
		//	{
		//		writer.Write((byte)rccID);
		//	}
		//	else
		//	{
		//		writer.Write((byte)0);
		//		writer.Write(funcName);
		//	}

		//	writer.Write(path);
		//	writer.WriteArray(objs);
		//	EndSend(channelID, true);

		//	yield break;
		//}

		/// <summary>
		/// Create instance with addressable asset and setup via custome function.
		/// </summary>
		/// <param name="channelID">Channel to spawn instance.</param>
		/// <param name="funcName">Function to setup content(GameObject).</param>
		/// <param name="asset">Addressable asset.</param>
		/// <param name="persistent">Stay after player leave channel.</param>
		/// <param name="objs">Parameters of setup function.</param>
		static public async void Instantiate(int channelID, string funcName, AssetReference asset, bool persistent, params object[] objs)
		{
			var rccID = 0;

			//var assets = Addressables.LoadResourceLocationsAsync(asset).WaitForCompletion();

			//foreach (var location in assets)
			//{
			//	var path = location.PrimaryKey;

			//	if (path == null)
			//	{
			//		Debug.LogError(new KeyNotFoundException(funcName + asset.AssetGUID));
			//		path = "";
			//	}
			//}

			if (instance != null)
			{
				var func = GetRCC(rccID, funcName);

				if (func == null)
				{
					Debug.LogError("Unable to locate RCC " + rccID + " " + funcName);
				}
#if !MODDING
				else if (isConnected)
				{
					var key = (string)asset.RuntimeKey;

					if (!TNManager.IsInChannel(channelID))
					{
						if (!TNManager.IsJoiningChannelDefault(channelID))
							TNManager.JoinChannel(channelID, true);

						Debug.LogFormat("Send RCC-{0} addressable<{1}> after join channel [{2}]",funcName, key, channelID);
					}

					await SendRequestCreateAddressalbeObject(channelID, rccID, funcName, key, persistent, objs);
					return;

				}
#endif
				else
				{
					// Offline mode

					TNManager.instance.CreateAddressableObject_Offline(
						channelID,
						GetHost(channelID),
						rccID,
						funcName,
						TNObject.GetUniqueID(true),
						null,
						objs);
				}
			}
#if UNITY_EDITOR
			else Debug.LogError("Unable to load " + asset.RuntimeKey);
#endif
		}



		static async UniTask SendRequestCreateAddressalbeObject(int channelID, int rccID, string funcName, string runTimeKey, bool persistent, params object[] objs)
		{
			if (!TNManager.IsInChannel(channelID) || TNManager.IsJoiningChannelDefault(channelID))
				await WaitJoinChannel(channelID);

			var writer = mInstance.mClient.BeginSend(Packet.RequestCreateAddressalbeObject);
			writer.Write(TNManager.playerID);
			writer.Write(channelID);
			writer.Write(persistent ? (byte)1 : (byte)2);

			if (rccID > 0 && rccID < 256)
			{
				writer.Write((byte)rccID);
			}
			else
			{
				writer.Write((byte)0);
				writer.Write(funcName);
			}

			writer.Write(runTimeKey.ToString());
			writer.WriteArray(objs);
			EndSend(channelID, true);
		}


		static public IEnumerator WaitConnection(string address = null, float waitTime = 7)
		{
			var connected = false;
			var msg = string.Empty;

			TNManager.onConnect += (bool success, string message) =>
			{
				connected = success;
				msg = message;
			};


			var duringTime = 0f;

			var wait = new WaitForSeconds(1f);

			while (TNManager.isTryingToConnect && duringTime <= waitTime)
			{
				Debug.Log("Connecting to " + address + ".." + duringTime);
				var time = Time.time;
				yield return wait;
				duringTime += Time.time - time;
			}

			if (connected)
			{
				Debug.Log("Connect to " + msg);
			}
			else if (duringTime >= waitTime)
			{
				Debug.Log("Timeout connection : " + address);
			}
			else
			{
				Debug.Log("Failure connect to " + address);
			}

			yield break;
		}

        #region Channels

		static public async UniTask StartJoinChannel(int channelID,bool persistent=false, bool checkChannelLock = false,object sender=null)
        {
			if (!TNManager.isActive)
				return;

			while (!TNManager.isConnected)
            {
				Debug.Log((sender==null?"":sender)+" waiting connection..");
				await UniTask.Delay(TimeSpan.FromSeconds(1), true);
			}

			if (!TNManager.IsInChannel(channelID)&&!TNManager.IsJoiningChannelDefault(channelID))
				TNManager.JoinChannel(channelID, persistent);

			await WaitJoinChannel(channelID, checkChannelLock);
        }

        static public async UniTask<bool> WaitJoinChannel(int channelID, bool checkChannelLock = false)
		{
			if (checkChannelLock && IsChannelLocked(channelID))
			{
#if UNITY_EDITOR
				Debug.LogWarning("Trying to create an object in a locked channel. Call will be ignored.");
#endif
				return false;
			}

			if (IsInChannel(channelID))
				return true;


			bool finishJoin = false;
			bool joinResult = false;
			string resultMessage = null;

			TNEvents.OnJoinChannel action = (int resultChannelID, bool success, string message) =>
			{
                if (resultChannelID == channelID)
                {
                    Debug.Log($"{(success ?"Success":"Faliure")} join channel : {resultChannelID}");


                    finishJoin = true;
					joinResult = success;
					resultMessage = message;
				}
			};


			TNManager.onJoinChannel += action;

			await UniTask.WaitUntil(() =>
			{
				//Debug.Log(Time.frameCount + " waiting connection : "+channelID);
				return finishJoin;
			});

			TNManager.onJoinChannel -= action;
			action = null;

            try
            {
				Assert.IsTrue(TNManager.IsInChannel(channelID));
			}
			catch (Exception e)
            {
				Debug.LogError(e);
				return false;
			}

			return true;
		}

		static public IEnumerator WaitLeaveChannel(int channelID)
		{
			int resultChannelID = 0;

			TNEvents.OnLeaveChannel action = (int channelID) =>
			{
				Debug.Log(" leave channel : " + channelID);
				resultChannelID = channelID;
			};


			TNManager.onLeaveChannel += action;


			yield return new WaitUntil(() => {
				//Debug.Log(Time.frameCount + " waiting connection : "+channelID);
				return !TNManager.IsInChannel(channelID);
			});

			TNManager.onLeaveChannel -= action;
			action = null;

			Assert.IsFalse(TNManager.IsInChannel(channelID));

			yield break;
		}

        #endregion

        async void CreateAddressableObject_Offline(int channelID, Player creator, int rccID, string funcName, uint objectID, string path, params object[] objs)
		{
			await ProcessResponseCreateAddressabkeObject_Offline(channelID, creator, rccID, funcName, objectID, path, objs);
		}

		async UniTask ProcessResponseCreateAddressabkeObject_Offline(int channelID, Player creator, int rccID, string funcName, uint objectID, string prefabPath, params object[] objs)
		{
			currentObjectOwner = creator;

			lastChannelID = channelID;
			var func = GetRCC(rccID, funcName);

			GameObject prefab = null;

			await UnityTools.LoadAddressablePrefab(prefabPath,
				(x) => prefab = x);

			if (prefab == null)
			{
				prefab = UnityTools.GetDummyObject();

#if UNITY_EDITOR
				if (!string.IsNullOrEmpty(prefabPath))
					Debug.LogError("[TNet] Unable to find prefab \"" + prefabPath + "\". Make sure it's in the Resources folder.");
#else
				if (!string.IsNullOrEmpty(prefabPath)) 
					Debug.LogError("[TNet] Unable to find prefab \"" + prefab + "\"");
#endif
			}

			if (!TNManager.IsInChannel(channelID) || TNManager.IsJoiningChannelDefault(channelID))
				await WaitJoinChannel(channelID);


			currentRccObjectID = objectID;

			if (func != null)
			{
				// Custom creation function
				prefab = func.Execute(objs) as GameObject;
				UnityTools.Clear(objs);
			}
			// Fallback to a very basic function
			else prefab = OnCreate1(prefab);

			if (prefab != null)
			{
				// Network objects should only be destroyed when leaving their channel
				var t = prefab.transform;

				if (t.parent == null)
					DontDestroyOnLoad(prefab);

				// If an object ID was requested, assign it to the TNObject
				if (objectID != 0)
				{
					var obj = prefab.InstanceIfNull<TNObject>();

					obj.channelID = channelID;
					obj.uid = objectID;
					prefab.SetActive(true);
					obj.Register();
				}
				else
				{
#if UNITY_EDITOR
					Debug.LogWarning("Object ID is 0. Intentional?", prefab);
#endif
					prefab.SetActive(true);
				}
			}

			currentObjectOwner = null;
		}

		/// <summary>
		/// Notification of a new object being created.
		/// </summary>
		static public void OnResponseCreateAddressableObject(int channelID, int creator, uint objectID, BinaryReader reader)
		{
			var rccID = reader.ReadByte();
			var funcName = (rccID == 0) ? reader.ReadString() : null;

			// Load the object from the resources folder
			var prefabPath = reader.ReadString();

			// Custom creation function
			var objs = reader.ReadArray(null);

#if UNITY_EDITOR

			Debug.LogFormat("Player {0} {1} with func [{2}]",
				TNManager.GetPlayer(creator),
				nameof(OnResponseCreateAddressableObject),
				funcName);

#endif

			ProcessResponseCreateAddressableObject(channelID, creator, objectID, rccID, funcName, prefabPath, objs).ToUniTask();
		}


		/// <summary>
		/// Response create addressable object and
		/// </summary>
		static IEnumerator ProcessResponseCreateAddressableObject(int channelID, int creator, uint objectID, byte rccID, string funcName, string prefabPath, object[] objs)
		{
			// not match async
			currentObjectOwner = GetPlayer(creator) ?? GetHost(channelID);

			lastChannelID = channelID;


			GameObject prefab = null;

			var frameStart= Time.frameCount;

			prefab =UnityTools.LoadAddressablePrefab(prefabPath);

			if (prefab == null)
			{
				Debug.Log(prefabPath + " get result null.");
				prefab = UnityTools.GetDummyObject();

#if UNITY_EDITOR
				if (!string.IsNullOrEmpty(prefabPath)) 
					Debug.LogError("[TNet] Unable to find prefab \"" + prefabPath + "\". Make sure it's in the Resources folder.");
#else
				if (!string.IsNullOrEmpty(prefabPath)) 
					Debug.LogError("[TNet] Unable to find prefab \"" + prefab + "\"");
#endif
			}


#if UNITY_EDITOR

			var sb = new StringBuilder();

			sb.AppendLine(objectID.ToString());

            foreach (var obj in objs)
            {
				if (obj.NotNull())
					sb.AppendLine(obj.ToString());
				else
					sb.AppendLine("Null");
            }

			Debug.Log(TNManager.GetPlayer(creator).name + " generate parameter nums : " + sb.ToString());

#endif

			objs[0] = prefab;

			LoadPrefabTno(channelID, objectID, rccID, funcName, prefab, objs);

			yield break;
		}

		/// <summary>
		/// Binding object with TNObject.
		/// </summary>
		static void LoadPrefabTno(int channelID, uint objectID, int rccID, string funcName, GameObject prefab, params object[] objs)
		{
			currentRccObjectID = objectID;
			var func = TNManager.GetRCC(rccID, funcName);

			if (func != null)
			{
				Debug.LogFormat("Load TNObject on prefab {0} with {1} parameters.", prefab,objs.Length);
				prefab = func.Execute(objs) as GameObject;
				UnityTools.Clear(objs);
			}
			// Fallback to a very basic function
			else
				prefab = OnCreate1(prefab);


			if (prefab != null)
			{
				// Network objects should only be destroyed when leaving their channel
				var t = prefab.transform;

				if (t.parent == null)
					DontDestroyOnLoad(prefab);

				// If an object ID was requested, assign it to the TNObject
				if (objectID != 0)
				{
					var tno = prefab.InstanceIfNull<TNObject>();

					tno.channelID = channelID;
					tno.uid = objectID;
					prefab.SetActive(true);
					tno.Register();

					
				}
				else
				{
#if UNITY_EDITOR
					Debug.LogWarning("Object ID is 0. Intentional?", prefab);
#endif
					prefab.SetActive(true);
				}
			}

			currentObjectOwner = null;
		}
	}



//    [Obsolete]
//    public class AddressableLoader : GameServer
//    {
//        public int rccID = 0;
//        public string funcName;
//        public int channelID;
//        public bool persistent;

//        public object[] objs;
//        public uint uid;

//        public string path;

//        GameObject go;

//        [Obsolete]
//        void OnCreateAddressable(TcpPlayer player, Buffer buffer, BinaryReader reader, byte requestByte, bool reliable)
//        {
//            byte packedID = byte.Parse("CreateAddressableItem");
//            if (packedID != requestByte)
//                return;


//            var playerID = reader.ReadInt32();

//            // Exploit: echoed packet of another player
//            if (playerID != player.id)
//            {
//                player.LogError("Tried to echo a create packet (" + playerID + " vs " + player.id + ")", null);
//                RemovePlayer(player);
//                return;
//            }

//            var channelID = reader.ReadInt32();
//            var ch = player.GetChannel(channelID);
//            var type = reader.ReadByte();

//            if (ch != null && (!ch.isLocked || player.isAdmin))
//            {
//                uint uniqueID = 0;

//                if (type != 0)
//                {
//                    uniqueID = ch.GetUniqueID();

//                    var obj = new Channel.CreatedObject();
//                    obj.playerID = player.id;
//                    obj.objectID = uniqueID;
//                    obj.type = type;

//                    if (buffer.size > 0)
//                    {
//                        buffer.MarkAsUsed();
//                        obj.data = buffer;
//                    }
//                    ch.AddCreatedObject(obj);
//                }

//                // Inform the channel
//                var writer = BeginSend(Packet.ResponseCreateAddressableObject);
//                writer.Write(playerID);
//                writer.Write(channelID);
//                writer.Write(uniqueID);
//                writer.Write(buffer.buffer, buffer.position, buffer.size);
//                EndSend(ch, null, true);
//            }
//        }

//        [Obsolete]
//        public void LoadAddressableGameObject(AsyncOperationHandle<GameObject> obj)
//        {

//            if (obj.Status != AsyncOperationStatus.Succeeded)
//            {
//                Debug.LogError(obj.Status);
//                return;
//            }


//            //prefab = Instantiate(obj.Result);
//            var prefab = obj.Result;
//            //UnityTools.Register(path, prefab);
//            //TNServerInstance.game.onCustomPacket += OnCreateAddressable;


//            //if (prefab == null)
//            //{
//            //	// Load it from resources as a binary asset
//            //	var bytes = UnityTools.LoadBinary(path);

//            //	if (bytes != null)
//            //	{
//            //		// Parse the DataNode hierarchy
//            //		var data = DataNode.Read(bytes);

//            //		if (data != null)
//            //		{
//            //			// Instantiate and immediately disable the object
//            //			prefab = data.Instantiate(null, false);

//            //			if (prefab != null)
//            //			{
//            //				mPrefabs.Add(path, prefab);
//            //				Object.DontDestroyOnLoad(prefab);
//            //				prefab.transform.parent = prefabRoot;

//            //				loader.LoadGameObject(prefab);
//            //				return;
//            //			}
//            //		}
//            //	}
//            //}

//            //TNManager.CreateChannel(null, false, 50, null, false);

//            CheckChannelAndCreateObj(prefab);

//            //while (TNManager.IsJoiningChannelDefault(channelID) || !TNManager.IsInChannel(channelID))
//            //{
//            //	Debug.Log(TNManager.lastChannelID + " loading channel : " + channelID);
//            //	yield return null;
//            //}


//        }

//        [Obsolete]
//        public void CheckChannelAndCreateObj(GameObject prefab = null)
//        {
//            if (!IsInChannel(channelID))
//            {
//                if (!IsJoiningChannelDefault(channelID))
//                    JoinChannel(channelID, true);

//                go = prefab;

//                TNManager.onJoinChannel += WaitJoinChannel;
//                return;
//            }


//            Debug.Log("Create addressable obj in channel : " + channelID + " result : " + IsInChannel(channelID));
//            CreateObj(prefab);
//        }

//        protected virtual void WaitJoinChannel(int channelID, bool success, string message)
//        {
//            //Debug.Log(channelID);

//            if (channelID != this.channelID)
//            {
//                //Debug.Log("Join Channel : " + channelID);
//                return;
//            }


//            TNManager.onJoinChannel -= WaitJoinChannel;

//            if (success)
//                CreateObj(go);
//            else
//                Debug.LogError(message);
//        }


//        void CreateObj(GameObject go = null)
//        {
//            Debug.Log("Apply tno to instantiate object : " + go);

//            go ??= UnityTools.GetDummyObject();

//            if (go != null && instance != null)
//            {
//                var func = GetRCC(rccID, funcName);

//                if (func == null)
//                {
//                    Debug.LogError("Unable to locate RCC " + rccID + " " + funcName);
//                }
//#if !MODDING
//                else if (isConnected)
//                {
//                    if (IsJoiningChannel(channelID))
//                    {
//#if UNITY_EDITOR
//								Debug.LogWarning("Trying to create an object while switching scenes. Call will be ignored. " + path);
//#endif
//                        return;
//                    }

//                    if (IsChannelLocked(channelID))
//                    {
//#if UNITY_EDITOR
//								Debug.LogWarning("Trying to create an object in a locked channel. Call will be ignored. " + path);
//#endif
//                        return;
//                    }

//                    if (!IsInChannel(channelID))
//                    {
//#if UNITY_EDITOR

//								Debug.LogWarning("Must join the channel first before calling instantiating objects, require id : " + channelID + path);

//								Debug.Log(lastChannelID + " ** " + channelID);
//								Debug.Log(TNManager.channels.Count);

//								foreach (var channel in channels)
//									Debug.Log(channel.id);

//#endif
//                        return;
//                    }

//                    //var packedID = Encoding.UTF8.GetBytes("CreateAddressableItem");

//                    //var writer = mInstance.mClient.BeginSend(Packet.RequestCreateObject);



//                    var writer = mInstance.mClient.BeginSend(Packet.RequestCreateAddressalbeObject);
//                    writer.Write(playerID);
//                    writer.Write(channelID);
//                    writer.Write(persistent ? (byte)1 : (byte)2);

//                    if (rccID > 0 && rccID < 256)
//                    {
//                        writer.Write((byte)rccID);
//                    }
//                    else
//                    {
//                        writer.Write((byte)0);
//                        writer.Write(funcName);
//                    }

//                    writer.Write(path);
//                    writer.WriteArray(objs);
//                    TNManager.EndSend(channelID);
//                }
//#endif
//                else
//                {
//                    Debug.Log("Offline load addressable obj : " + path);
//                    // Offline mode
//                    currentRccObjectID = TNObject.GetUniqueID(true);
//                    objs = BinaryExtensions.CombineArrays(go, objs);
//                    go = func.Execute(objs) as GameObject;
//                    UnityTools.Clear(objs);

//                    if (go != null)
//                    {
//                        var tno = go.GetComponent<TNObject>();
//                        if (tno == null) tno = go.AddComponent<TNObject>();
//                        tno.uid = currentRccObjectID;
//                        tno.channelID = channelID;
//                        go.SetActive(true);
//                        tno.Register();
//                    }
//                }
//            }
//#if UNITY_EDITOR
//					else Debug.LogError("Unable to load " + path);
//#endif
//        }

//    }

}