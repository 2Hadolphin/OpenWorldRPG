#define TNet

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Return;
using Return.Modular;

namespace TNet
{

    public partial class TNObject : ITNO, IResolveHandle  // Extension of TNObject
    {
#if UNITY_EDITOR

        void Dev_Index()
        {
            //isMine;
            //Awake();
        }
#endif

        void IResolveHandle.InstallResolver(IModularResolver resolver)
        {
            resolver.RegisterModule<ITNO>(this);
            //OnStart
        }

        //void IResolveHandle.Unistallresolver(IModularResolver Resolver)
        //{
        //    Resolver.UnregisterModule(this);
        //}


        #region Overwrite

        void Awake()
        {
            mOwner = TNManager.isConnected ? TNManager.currentObjectOwner : TNManager.player;
            channelID = TNManager.lastChannelID;
            creatorPlayerID = TNManager.packetSourceID;
            TNUpdater.AddStart(this);
        }

        #endregion

        public void Register<T>(T module) where T : class
        {
            if (rebuildMethodList)
                RebuildMethodList();

            if (module.IsNull())
            {
                Debug.LogError($"Null register tno target added : {typeof(T)}");
                return;
            }

            var type = module.GetType();

            //  cache type reflection
            if (!mMethodCache.TryGetValue(type, out System.Collections.Generic.List<CachedMethodInfo> ret))
            {
                ret = new System.Collections.Generic.List<CachedMethodInfo>();
                var cache = type.GetCache().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (int b = 0, bmax = cache.Count; b < bmax; ++b)
                {
                    var ent = cache.buffer[b];
                    if (!ent.method.IsDefined(typeof(RFC), true))
                        continue;

                    var ci = new CachedMethodInfo
                    {
                        name = ent.name,
                        rfc = (RFC)ent.method.GetCustomAttributes(typeof(RFC), true)[0],

                        cf = new CachedFunc()
                    };

                    ci.cf.mi = ent.method;

                    ret.Add(ci);
                }

                mMethodCache.Add(type, ret);
            }

            //  binding reflection info to instance 
            for (int b = 0, bmax = ret.Count; b < bmax; ++b)
            {
                var ci = ret[b];

                var ent = new CachedFunc
                {
                    obj = module,
                    mi = ci.cf.mi
                };

                if (ci.rfc.id > 0)
                {
                    if (ci.rfc.id < 256)
                        mDict0[ci.rfc.id] = ent;
                    else
                        Debug.LogError("RFC IDs need to be between 1 and 255 (1 byte). If you need more, just don't specify an ID and use the function's name instead.");

                    mDict1[ci.name] = ent;
                }
                else if (ci.rfc.property != null)
                {
                    mDict1[ci.name + "/" + ci.rfc.GetUniqueID(module)] = ent;
                }
                else
                    mDict1[ci.name] = ent;
            }
        }
    }


    //public class FakeTno : BaseComponent
    //   {

    //	public virtual bool isMine
    //	{
    //		get => true;
    //		set => throw new KeyNotFoundException();
    //       }
    //   }

}