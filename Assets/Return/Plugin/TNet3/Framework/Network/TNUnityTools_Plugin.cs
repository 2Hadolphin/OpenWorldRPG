using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;
using Return;
using Cysharp.Threading.Tasks;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace TNet
{
    static public partial class UnityTools // plug-in
	{
		/// <summary>
		/// Try to get it from cache
		/// </summary>

		static public bool TryLoadFromCache(string prefabPath , out GameObject prefab)
        {
			return UnityTools.mPrefabs.TryGetValue(prefabPath, out prefab);
		}

		static public bool LoadPrefabFromResource(string path,out GameObject prefab)
		{
			prefab = null;

			if (string.IsNullOrEmpty(path))
				return false;

			//if (!Application.isPlaying)
			//{
			//	prefab= Resources.Load(key, typeof(GameObject)) as GameObject;
			//	return prefab.NotNull();
			//}

			// Try the custom function first
			if (onLoadPrefab != null)
				prefab = onLoadPrefab(path);

			// Load it from resources as a Game Object
			if (prefab == null)
			{
				prefab = Resources.Load(path, typeof(GameObject)) as GameObject;

				if (prefab == null)
				{
					// Load it from resources as a binary asset
					var bytes = UnityTools.LoadBinary(path);

					if (bytes != null)
					{
						// Parse the DataNode hierarchy
						var data = DataNode.Read(bytes);

						if (data != null)
						{
							// Instantiate and immediately disable the object
							prefab = data.Instantiate(null, false);

							if (prefab != null)
							{
								//mPrefabs.Add(key, prefab);
                                UnityEngine.Object.DontDestroyOnLoad(prefab);
								prefab.transform.parent = prefabRoot;
								return prefab.NotNull();
							}
						}
					}
				}
			}

			return false;
		}




		//[Obsolete]
		//static public void LoadAddressablePrefab(TNManager.AddressableLoader loader)
		//{
		//	var key = loader.key;

		//	if (string.IsNullOrEmpty(key))
		//	{
		//		Debug.LogError("Missing addressable key.");
		//		loader.CheckChannelAndCreateObj();
		//		return;
		//	}
		//	else
		//	{
		//		//onLoadPrefab = new LoadPrefabFunc(Addressables.LoadAssetT<GameObject>(key));
		//	}

		//	if (!Application.isPlaying)
		//	{
		//		Addressables.LoadAssetAsync<GameObject>(key).Completed += loader.LoadAddressableGameObject;
		//		return;
		//	}

		//	GameObject prefab = null;

		//	// Try to get it from cache
		//	if (mPrefabs.TryGetValue(key, out prefab))
		//	{
		//		loader.CheckChannelAndCreateObj(prefab);
		//		return;
		//	}
		//	else
		//	//if (prefab == null)
		//	{
		//		// Try the custom function first
		//		if (onLoadPrefab != null)
		//			prefab = onLoadPrefab(key);

		//		// Load it from resources as a Game Object
		//		if (prefab == null)
		//		{
		//			Addressables.LoadAssetAsync<GameObject>(key).Completed += loader.LoadAddressableGameObject;

		//			//prefab = Resources.Load(key, typeof(GameObject)) as GameObject;
		//		}
		//	}
		//}

		/// <summary>
		/// Load addressable reference first then set callback function
		/// </summary>
		static public GameObject LoadAddressablePrefab(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError($"Missing addressable key : {key}.");
				return null;
			}

			// Try to get it from cache
			if (mPrefabs.TryGetValue(key, out var prefab))
			{
				Debug.Log($"Load cache prefab with key : {prefab}");
			}
			else
			{
				prefab=Addressables.LoadAssetAsync<GameObject>(key).WaitForCompletion();

				if (prefab != null)
					Register(key, prefab);
			}

			return prefab;
		}

		static public async UniTask<GameObject> LoadAddressablePrefabAsync(string key)
		{
			if (string.IsNullOrEmpty(key))
			{
				Debug.LogError($"Missing addressable key : {key}.");
				return null;
			}

			// Try to get it from cache
			if (mPrefabs.TryGetValue(key, out var prefab))
			{
				Debug.Log($"Load cache prefab with key : {prefab}");
			}
			else
			{
				var handle = Addressables.LoadAssetAsync<GameObject>(key);

				await handle.ToUniTask(PlayerLoopTiming.FixedUpdate);

				if (handle.IsValid() && handle.Status == AsyncOperationStatus.Succeeded)
					prefab = handle.Result;
				else
					Debug.LogError($"Failure to load addressable target {key}.");

				if (prefab != null)
					Register(key, prefab);
			}

			return prefab;
		}


		static public IEnumerator LoadAddressablePrefab(string path,Action<GameObject> callback) 
		{
			Assert.IsNotNull(callback);

			if (string.IsNullOrEmpty(path))
			{
				Debug.LogError("Missing addressable path.");
				yield break ;
			}
			else
			{
				//onLoadPrefab = new LoadPrefabFunc(Addressables.LoadAssetT<GameObject>(key));
			}


			// Try to get it from cache
			if (mPrefabs.TryGetValue(path, out var prefab))
			{
				Debug.Log(prefab);
			}
			else
			{
				// Try the custom function first
				//if (onLoadPrefab != null)
				//	prefab = onLoadPrefab(key);

				// Load it from addressable as a Game Object

				var completed = false;

				Addressables.LoadAssetAsync<GameObject>(path).Completed += (x) =>
				{
					completed = true;
					prefab = x.Result;

					if(x.Status.HasFlag(AsyncOperationStatus.Failed))
						Debug.LogError(x.Status);
				};

				yield return new WaitUntil(() => completed == true);


				if (prefab == null)
				{
					yield break;
				}
				else
				{
					Register(path, prefab);
					callback(prefab);
					yield break;
				}

			}

			callback(prefab);
			yield break;
		}


		/// <summary>
		/// Save cache pair of key and prefab. 
		/// </summary>
		/// <param name="key">Resource path or addressable runtime key.</param>
		/// <param name="prefab">Prefab to cache</param>
		public static void Register(string key, GameObject prefab)
		{
			Assert.IsNotNull(prefab,"Loading cache prefab failure at "+key);


			if(mPrefabs.TryGetValue(key,out var oldPrefab))
            {
				mPrefabs[key] = prefab;
            }
            else
            {
				mPrefabs.Add(key, prefab);
			}
		}
	}
}
