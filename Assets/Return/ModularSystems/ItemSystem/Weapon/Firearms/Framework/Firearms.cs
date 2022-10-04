using UnityEngine;
using Sirenix.OdinInspector;
using Return.Agents;
using UnityEngine.Assertions;
using Return.Humanoid;
using Cysharp.Threading.Tasks;
using Return;
using System.Collections.Generic;
using Return.Modular;
using System;

namespace Return.Items.Weapons.Firearms
{
    [Obsolete]
    public partial class Firearms : Weapon,IToken
    {
        /// <summary>
        /// GunData Asset is a container responsible for defining individual weapon modules.
        /// </summary>
        protected FirearmsPreset FirearmsPreset => Preset as FirearmsPreset;

        #region Iresolver

        public override void InstallResolver(IModularResolver resolver)
        {
            base.InstallResolver(resolver);

            //if (agent.Resolver.TryGetModule<TNet.ITNO>(out var tno))
            //LoadTNO(tno);
        }

        #endregion

        #region IItem


        public override void Register(IAgent agent)
        {
              base.Register(agent);
        }

        public override void Unregister(IAgent agent)
        {
            base.Unregister(agent);
        }

        public override async UniTask Activate()
        {
            Assert.IsFalse(tno == null, "Missing item tno.");
            await base.Activate();
        }

        public override async UniTask Deactivate()
        {
            await base.Deactivate();
        }

        #endregion



        [Button]
        protected override void LoadPresetModules(GameObject go)
        {
            base.LoadPresetModules(go);

            // Load host player handles **CameraMotion **GUI
            if (Agent.IsLocalUser())
            {
                var modules = FirearmsPreset.LocalUserConfig.LoadModules(gameObject);

                foreach (var module in modules)
                    if (module is IItemModule itemModule && itemModule.CycleOption.HasFlag(ControlMode.Register))
                    {
                        //itemModule.SetHandler(this);
                        //if(itemModule.CycleOption.HasFlag(ControlMode.Register))
                        //    itemModule.Register();
                        resolver.RegisterModule(itemModule);
                    }
            }
            else
                Debug.LogError("Can't loading item module because missing require agent : "+Agent);

            // performer Handle **audio **animation **
            {
                var modules = FirearmsPreset.UniversalConfig.LoadModules(gameObject);
                //Debug.Log(modules.Length);
                foreach (var module in modules)
                    if (module is IItemModule itemModule)
                    {
                        //itemModule.SetHandler(this);
                        //if(itemModule.CycleOption.HasFlag(ControlMode.Register))
                        //    itemModule.Register();
                        resolver.RegisterModule(itemModule);
                    }
            }
        }


        /// Statistics of all modules
        #region --Obsolete BlackBoard 

        List<IToken> StatusTokens=new();

        public async void WaitCanSetStatus(IToken token)
        {
            await UniTask.WaitUntil(() => CanSetStatus(token));
        }

        public bool CanSetStatus(IToken token=null)
        {
            bool valid, order=false;

            valid = token.NotNull();

            if (!valid)
                token = this;

            foreach (var curToken in StatusTokens)
            {
                if (valid)
                    order &= curToken.CompareTo(token) >= 0;
                else
                    order &= true;
            }

            if (order && token.NotNull())
                StatusTokens.CheckAdd(token);

            return order;
        }

        public void RemoveStatus(IToken token = null)
        {
            if (token.NotNull())
                StatusTokens.Remove(token);
            else
                StatusTokens.Remove(this);
        }

        public virtual int CompareTo(object other)
        {
            return 1;
        }


        #endregion
    }
}