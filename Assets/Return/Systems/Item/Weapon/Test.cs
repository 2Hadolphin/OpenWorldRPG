using Return.Agents;
using Return.Items;
using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Assertions;

namespace Return.Items.Weapons
{
    public class Test : MonoBehaviour
    {
        [SerializeField]
        public Agents.Agent m_Agent;

        [SerializeField]
        ItemPreset Preset;

        [SerializeField]
        Transform ItemHandle;

        //[SerializeField]
        //public m_items m_Item;

        [SerializeField]
        public FirearmsPerformerPlayer FirearmsPerformerPlayer;


        public AssetReference Item;

        public SkinnedMeshRenderer ThirdPersonMesh;

        public List<SkinnedMeshRenderer> FirstPersonMeshs;

        public bool SetThirdPersonMesh=true;

        void Start()
        {
            OnItemCreate += (x) => StartCoroutine(LoadItem(x));

            if (m_Agent)
            {
                // if is mine 

                // as InventoryHandler channelID-> 
                // request system loading item -> 
                // callback after addressable loaded -- system cache 
                // Instantiate instance and apply tno id

                //var agentTNO = m_Agent.tno;

                if (m_Agent.Resolver.TryGetModule<TNet.TNObject>(out var agentTNO) && agentTNO.isMine)
                {
                    if (SetThirdPersonMesh)
                    {
                        ThirdPersonMesh.gameObject.SetActive(true);

                        foreach (var mesh in FirstPersonMeshs)
                            mesh.gameObject.SetActive(false);
                    }
                    else
                    {
                        if (agentTNO.isMine)
                            ThirdPersonMesh.gameObject.SetActive(false);
                        else
                            foreach (var mesh in FirstPersonMeshs)
                                mesh.gameObject.SetActive(false);
                    }


                    //var item=Preset.CreateCharacter(ItemHandle);
                    if(CreateItem)
                    TNet.TNManager.Instantiate(agentTNO.channelID, nameof(InitItem), Item, false, agentTNO.channelID, agentTNO.uid);
                    //TNet.TNManager.Instantiate
                }
            }

            // item => posture =>
            //m_Item.RegisterHandler(m_Agent);

            //FirearmsPerformerPlayer.ReloadAnimationPorts(m_Item);

            //FirearmsPerformerPlayer.LoadPerformerHandles();

            //AdditivePlayer.SetHandler(m_Item);
        }

        [SerializeField]
        bool CreateItem; 

        static Action<AbstractItem> OnItemCreate;

        IEnumerator LoadItem(AbstractItem item)
        {
            //var tnoUser = TNet.TNObject.Find(channelID, tnoID_user);

            Debug.Log(item);

            TNet.TNObject tno;

            while (!item.TryGetComponent(out tno))
                yield return new WaitForSeconds(1);

            if (!TryGetComponent<TNet.TNObject>(out var tnoUser))
                yield break;

            if (tnoUser && tnoUser.TryGetComponent<IAgent>(out var agent))
            {
                item.Register(agent);

                //if (item.TryGetComponent<AdditivePlayer_HumanoidItem>(out var player))
                //    player.SetHandler(item);
            }
            else
            {
                Debug.LogError("Failure to find target tno object.");
            }

            yield break;
        }


        [TNet.RCC]
        static public GameObject InitItem(GameObject prefab,int channelID,uint tnoID_user)
        {
            Assert.IsNotNull(prefab);

            Debug.Log(prefab);

            if (prefab.IsNull())
                return null;

            // Instantiate the prefab
            var go = Instantiate(prefab);

            go.name = prefab.name;
            go.SetActive(true);

            if (go.TryGetComponent<AbstractItem>(out var item))
                OnItemCreate.Invoke(item);

            return go;
            //var item = Instantiate(m_items, ItemHandle);
        }
    }
}