using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TNet;
using Return.Database;
using Sirenix.OdinInspector;
using Return.Agents;
using Return.Modular;
using UnityEngine.AddressableAssets;
using Return.Motions;
using UnityEngine.Assertions;

namespace Return.Games.AlphaMap
{
    public class LocalPlayerLoader : BaseComponent, IStart, IInjectable //, IResolveHandle
    {
        [Inject]
        protected ITNO tno { get; set; }

        #region Serialize

        [SerializeField]
        DataNodeHandler m_DataNodeHandler =new() { FileName=nameof(LocalPlayerLoader) };
        public DataNodeHandler DataNodeHandler { get => m_DataNodeHandler; set => m_DataNodeHandler = value; }

        DataNode Datas;

        [ShowInInspector]
        [HideLabel]
        public PlayerData PlayerDatas { get; protected set; }

        [Button]
        protected void LoadData()
        {
            if (PlayerDatas == null)
                PlayerDatas = new();

            if (Datas == null)
            {
                if(!DataNodeHandler.TryLoadData(out Datas))
                {
                    Debug.LogError("pref path return empty.");
                    Datas = new DataNode(nameof(LocalPlayerLoader));

                    // initilize data
                    var point = (Transform)RespawnPoint.Get(0);
                    PlayerDatas.Position = point.position;
                    PlayerDatas.Rotation = point.rotation;

                    return;
                }
            }

            PlayerDatas.Deserialize(Datas);

        }

        [Button]
        protected void SaveData()
        {
            if (Datas.NotNull())
            {
                if (Player != null)
                {
                    PlayerDatas.Position = Player.transform.position;
                    PlayerDatas.Rotation = Player.transform.rotation;
                }


                PlayerDatas.Serialize(Datas);
                DataNodeHandler.SaveData(Datas);
            }
        }

        public class PlayerData : DataNodeBased
        {
            [ShowInInspector]
            public Vector3 Position
            {
                get => dataNode.GetChild(nameof(Position), Vector3.zero);
                set => dataNode.SetChild(nameof(Position), value);
            }

            [ShowInInspector]
            public Quaternion Rotation
            {
                get => dataNode.GetChild(nameof(Rotation), Vector3.zero).ToEuler();
                set => dataNode.SetChild(nameof(Rotation), value.eulerAngles);
            }



        }
        #endregion

        [SerializeField, Required]
        AssetReference m_LocalUserCharacter;
        public AssetReference LocalUserCharacter { get => m_LocalUserCharacter; set => m_LocalUserCharacter = value; }

        [SerializeField, Required]
        TNet.List<Transform> m_RespawnPoint;
        public TNet.List<Transform> RespawnPoint { get => m_RespawnPoint; set => m_RespawnPoint = value; }

        /// <summary>
        /// Cache local player for record.
        /// </summary>
        public IAgent Player { get; protected set; }


        #region Routine

        public void Initialize()
        {
            LoadData();

            CreateCharacter();
        }

        private void OnApplicationQuit()
        {
            SaveData();
        }

        #endregion

        #region RFC Creation

        public virtual async void CreateCharacter()
        {
            //var booking = await TNUtil.BookTNO(tno.channelID);

            await TNUtil.WaitConnection();

            await TNManager.WaitJoinChannel(ChannelList.Agent);


            var booking = await TNUtil.BookTNO(ChannelList.Agent);
            tno.Send(nameof(CreatePlayerCharacter), Target.AllSaved, booking);
        }

        #endregion

        [RFC]
        protected async void CreatePlayerCharacter(TNOBooking booking)
        {
            // check hash to instance

            var prefab = await LocalUserCharacter.CreateFromTNetCache();
            
            if(!prefab.IsInstance())
                prefab = Instantiate(prefab, PlayerDatas.Position, PlayerDatas.Rotation);

            Player = prefab.GetComponent<IAgent>();

            Assert.IsFalse(Player == null);

            // bind tno booking to target object
            var tno = booking.Setup(Player.GameObject);

            if (Player is IModule module)
            {
                module.Register();
                module.Activate();
            }


            if (Player.Resolver.TryGetModule<IMotionSystem>(out var motions))
            {
                motions.SetCoordinate(PlayerDatas.Position, PlayerDatas.Rotation);
            }

        }
    }
}