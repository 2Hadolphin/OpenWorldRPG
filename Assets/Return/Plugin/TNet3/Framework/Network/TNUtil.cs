using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Return;
using UnityEngine.AddressableAssets;
using System;

namespace TNet
{
    /// <summary>
    /// TNetwork utility.
    /// </summary>
    public static partial class TNUtil
    {

        public static async UniTask WaitConnection(float updateRatio = 0.5f)
        {
            while (!TNManager.isConnected)
            {
                //Debug.Log("Waiting for TNManager connection..");

                await UniTask.Delay(TimeSpan.FromSeconds(updateRatio));
            }

        }



        #region RFC Creation

        /// <summary>
        /// Setup TNetwork handle to gameObject.
        /// </summary>
        /// <param name="booking"></param>
        /// <param name="go"></param>
        public static ITNO Setup(this TNOBooking booking,GameObject go)
        {
			var tno = go.InstanceIfNull<TNObject>();

			tno.channelID = booking.channelID;
			tno.uid = booking.objectID;

			tno.Register();

            return tno;
		}


		/// <summary>
		/// Push tno id to server for custom binding tno.
		/// **Send tno flag request, wait server respond.
		/// </summary>
		public static async UniTask<TNOBooking> BookTNO(int channelID, Player player = null)
		{
			Assert.IsTrue(TNManager.IsInChannel(channelID));

			if (player == null)
				player = TNManager.player;

			var ch = TNManager.GetChannel(channelID);

			var uniqueID = ch.GetUniqueID();

			// await valid all user channel uid

			var obj = new Channel.CreatedObject
			{
				playerID = player.id,
				objectID = uniqueID,
				type = 1
			};

			ch.AddCreatedObject(obj);

			return new TNOBooking()
            {
				playerID = player.id,
				objectID = uniqueID,
				type = 1,
				channelID=channelID,
			};
		}
        #endregion

        /// <summary>
        /// AssetReference => ValidRuntimeKey => TNManager.Instantiate() => callback
        /// </summary>
        public static async UniTask<GameObject> CreateFromTNetCache(this AssetReference asset)
        {
            GameObject prefab;

            try
            {
                Assert.IsTrue(asset.RuntimeKeyIsValid());

                prefab = await UnityTools.LoadAddressablePrefabAsync((string)asset.RuntimeKey);
                Assert.IsTrue(prefab != null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return null;
            }

            return prefab;
        }
    }

    /// <summary>
    /// Container to hold TNet object creation info.
    /// </summary>
    public class TNOBooking
	{
        /// <summary>
        /// User ID.
        /// </summary>
		public int playerID;
        /// <summary>
        /// TNObject uniquel ID. 
        /// </summary>
		public uint objectID;

		/// <summary>
		/// persistent ?
		/// </summary>
		public byte type;

        /// <summary>
        /// Channel ID of TNObject
        /// </summary>
		public int channelID;
	}

}