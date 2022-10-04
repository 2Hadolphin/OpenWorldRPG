using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Agents;
using TNet;
using System;
using UnityEngine.Assertions;
using Return.Modular;

namespace Return.Items
{
    /// <summary>
    /// ???
    /// </summary>
    public static class ItemAgentExtension
    {
        public static bool IsLocalUser(this AbstractItem item)
        {
            return item.Agent.IsLocalUser();
        }

        /// <summary>
        /// Create and activate user control handle if agent is ILocalHumanoidAgent
        /// </summary>
        //[System.Obsolete]
        //public static THandle ValidLocalUserHandle<THandle, TModule>(this AbstractItem item,TModule module) where THandle: usein<TModule> where TModule:class
        //{
        //    var agent = item.Agent;

        //    if (agent is ILocalHumanoidAgent localUser)
        //    {
        //        if (localUser.Resolver.TryGetModule<TNObject>(out var tno))
        //        {
        //            if (tno.isMine)
        //            {
        //                if (typeof(THandle).IsSubclassOf(typeof(Component)))
        //                {

        //                    item.gameObject.InstanceIfNull<THandle>();
        //                    handle.ActivateHandle(item, module);
        //                    return handle;
        //                }
        //                else
        //                    Debug.LogError("Missing mono type " + typeof(THandle));
        //            }
        //        }
        //    }

        //    return default;
        //}

        
        //public static THandle ValidMonoHandle<UAgent,THandle>(this m_items item, MonoItemModule parentModule) where UAgent :IAgent where THandle: MonoItemModuleHandle
        //{
        //    if (item.Agent is UAgent agent)
        //    {
        //        //if (agent.Authorize<TNObject>(out var tno))
        //        {
        //            //if (tno.isMine)
        //            {
        //                if (typeof(THandle).IsSubclassOf(typeof(Component)))
        //                {
        //                    var handle = item.CharacterRoot.InstanceIfNull<THandle>();
        //                    handle.SetHandler(item, parentModule);
        //                    return handle;
        //                }
        //                else
        //                    Debug.LogError("Missing mono type " + typeof(THandle));
        //            }

        //        }
        //    }

        //    return default;
        //}

        /// <summary>
        /// Valid the module handle can been created or not.
        /// Bind agnet => Instance => Activae => RegisterHandler => DebugLog
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="module"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public static ValidMonoHandle ValidMonoHandle<T>(this MonoItemModule parentModule, IItem item,bool singleton=true) where T:class
        {
            var handle = Items.ValidMonoHandle.Handle;
                //typeof(TSerializable).IsSubclassOf(typeof(Component)) ?
                //ValidMonoHandle.Handle :
                //ValidCSharpHandle.Handle;

            if(singleton && item.resolver.TryGetModule<T>(out var module))
            {
                handle.valid = false;
                handle.Module = module as IItemModule;
            }
            else
                handle.Set(item, item.Agent, parentModule, typeof(T));
   

            return handle;
        }


        public static ValidCSharpHandle ValidHandle<T>(this MonoItemModule module, IItem item) where T:EnabledCheck
        {
            var handle = ValidCSharpHandle.Handle;
            //typeof(TSerializable).IsSubclassOf(typeof(Component)) ?
            //ValidMonoHandle.Handle :
            //ValidCSharpHandle.Handle;

            handle.Set(item, item.Agent, module, typeof(T));

            return handle;
        }

        /// <summary>
        /// 
        /// </summary>

        public static T BindLocalUser<T>(this T handle) where T: ValidHandleLog
        {
            handle.valid &= handle.Agent.IsLocalUser();

            return handle;
        }

        /// <summary>
        /// Bind require agnet to filter parentModule handle.
        /// </summary>
        public static T BindAgent<UAgent,T>(this T handle) where T : ValidHandleLog where UAgent : IAgent
        {
            handle.valid &= handle.Agent is UAgent;

            return handle;
        }

        /// <summary>
        /// Create handle instance.
        /// </summary>
        public static T Instance<T>(this T handle) where T : ValidHandleLog
        {
            if (handle.valid)
            {
                if (handle.type.IsSubclassOf(typeof(Component)) && handle is ValidMonoHandle monoHandle)
                    return monoHandle.Instance(null) as T;
                else if (!handle.type.IsSubclassOf(typeof(UnityEngine.Object)))
                    return handle.InstanceCSharp();
            }

      

            handle.valid = false;
            return handle;
        }

        /// <summary>
        /// Create handle instance.
        /// </summary>
        /// <typeparam name="THandle">Type of handle to created.</typeparam>
        /// <param name="obj">Which CharacterRoot to add the component.</param>
        public static T Instance<T>(this T handle, GameObject obj) where T: ValidMonoHandle
        {
            //handle.type = typeof(THandle);

            if (handle.valid)
            {
                var valid =handle.type.Instance(out var handleModule, () => obj);

                //object handleModule;

                //if (handle.type.IsComponent())
                //{
                //    if (obj == null)
                //        if (handle.Item.gameObject == null)
                //        {
                //            handle.valid = false;
                //            return handle;
                //        }


                //    handleModule = obj.InstanceIfNull(handle.type);
                //}
                //else
                //{

                //}

                //handle.valid = handleModule.NotNull();

                handle.valid = valid;

                if (handle.valid)
                    handle.result = handleModule;
            }

            return handle;
        }

        /// <summary>
        /// Create handle instance.
        /// </summary>
        /// <typeparam name="THandle">Type of handle to created.</typeparam>
        private static T InstanceCSharp<T>(this T handle) where T : ValidHandleLog
        {
            //handle.type = typeof(THandle);

            if (handle.valid)
            {
                var handleMudle = Activator.CreateInstance(handle.type);
                //var handleMudle = new THandle();

                handle.valid &= handleMudle.NotNull();

                if (handle.valid)
                    handle.result = handleMudle;
            }

            return handle;
        }

        /// <summary>
        /// SetHandler the handle via IItemModuleHandle.
        /// </summary>
        public static T SetModule<T>(this T handle) where T : ValidHandleLog
        {
            if (handle.valid)
            {
                if (handle.result is IItemModuleHandle moduleHandle)
                {
                    try
                    {
                        moduleHandle.SetHandler(handle.Item);
                        moduleHandle.SetModule(handle.Item, handle.Module);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Activate item parentModule handle failure.");
                        Debug.LogException(e);
                        handle.valid = false;
                    }
                }
                else if (handle.result is IModule module)
                {
                    try
                    {
                        module.Register();
                        module.Activate();
                    }
                    catch (Exception e)
                    {
                        Debug.LogException(e);
                        handle.valid = false;
                    }   
                }
                else
                {
                    Debug.LogError($"Unable to activate handle {handle.result}.");
                }
            }

            return handle;
        }

        /// <summary>
        /// RegisterHandler parentModule handle to item container.
        /// </summary>
        public static T Register<T>(this T handle) where T : ValidHandleLog
        {
            if (handle.valid && handle.result.NotNull())
                handle.Item.resolver.RegisterModule(handle.result);

            return handle;
        }

        /// <summary>
        /// Return handle as instance, return null when log is invalid.
        /// </summary>
        public static T AsHandle<T>(this ValidHandleLog handle) where T :class
        {
            if (handle.valid && handle.result.Parse(out T moduleHandle))
                return moduleHandle;

            Debug.LogException(new InvalidCastException($"Create {typeof(T)} handle faliure from {handle.result}."));

            return null;
        }

        public static UHandle AsHandle<T,UHandle>(this UHandle handle,ref T prop) where UHandle:ValidHandleLog where T : class 
        {
            if (handle.valid )
            {
                prop = handle.AsHandle<T>();
            }

            return handle;
        }

        public static THandle DebugLog<THandle>(this THandle handle) where THandle:ValidHandleLog
        {
            var log = string.Format("{0} loading {1} {2}", handle.Module,handle.type , handle.valid ? "successed" : "failure") + '.';

            if (handle.valid)
                Debug.Log(log);
            else
                Debug.LogError(log);

            return handle;
        }

    }



    public abstract class ValidHandleLog : IDisposable
    {
        public IItem Item { get; protected set; }
        public IAgent Agent { get; protected set; }

        public Type type;
        public IItemModule Module;
        public object result;

        public bool valid;

        bool dirty;

        public string msg;

        public virtual void Set(IItem item, IAgent agent, MonoItemModule module, Type type)
        {
            Dispose();



            Item = item;
            Module = module;
            Agent = item.Agent;

            this.type = type;

            valid = true;

            dirty = true;
        }

        public virtual void Clean()
        // clean
        {
            Item = null;
            Agent = null;
            Module = null;

            type = null;
            result = null;

            valid = true;

            msg = string.Empty;

            dirty = false;
        }

        public void Dispose()
        {
            if (dirty)
                Clean();
        }
    }


    public class ValidMonoHandle : ValidHandleLog
    {
        public static readonly ValidMonoHandle Handle = new();
    }

    public class ValidCSharpHandle : ValidHandleLog
    {
        public static readonly ValidCSharpHandle Handle = new();
    }

    public class ValidInputHandle : ValidCSharpHandle
    {

    }
}