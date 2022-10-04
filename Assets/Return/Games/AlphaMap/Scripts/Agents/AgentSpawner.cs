using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Cysharp.Threading.Tasks;
using System;
using Return.Agents;
using UnityEngine.Assertions;
using TNet;

namespace Return.Games.AlphaMap
{
    [Obsolete]
    public class AgentSpawner : BaseComponent
    {

        /// <summary>
        /// AssetReference => ValidRuntimeKey => TNManager.Instantiate() => callback
        /// </summary>
        public virtual async UniTask<IAgent> CreateAgent(AssetReference asset)
        {
            IAgent agent;

            try
            {
                Assert.IsTrue(asset.RuntimeKeyIsValid());

                //TNManager.Instantiate(
                //    ChannelList.Agent,
                //    nameof(LoadCharacter),
                //    asset,
                //    false,
                //    transform.position,
                //    transform.rotation
                //    );

                var prefab = await UnityTools.LoadAddressablePrefabAsync((string)asset.RuntimeKey);

                agent = prefab.GetComponentInChildren<IAgent>();

                Assert.IsTrue(agent != null);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return default;
            }

            return agent;
        }



    }
}