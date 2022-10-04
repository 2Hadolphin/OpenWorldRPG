#region Ref
using Cysharp.Threading.Tasks;
using Return;
using Return.Agents;
using Return.Framework.Parameter;
using Return.Modular;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using System.Linq;
using TNet;
using UnityEngine;
using UnityEngine.Assertions;

// obsolete
//using VContainer;
//using VContainer.Unity;
using TNObject = TNet.TNObject;
#endregion

namespace Return.Items
{

    /// <summary>
    /// SetHandler itemModule enable to execute function via agent
    /// </summary>
    public abstract class AbstractItem : Pickup,IItem,IResolveHandle //, IInjectHandler,IModularItemHandler
    {
        #region IItem

        bool IItem.isMine => isMine;


        #endregion

        #region resolverHandle

        #region IResolveHandle

        /// <summary>
        /// Binding Resolver or lit-Resolver.
        /// </summary>
        public IModularResolver resolver { get; protected set; }

        /// <summary>
        /// Install parent container or create custom Resolver.
        /// </summary>
        void IResolveHandle.InstallResolver(IModularResolver resolver)
        {
            // default cache parent Resolver.

            resolver = resolver;
            
            InstallResolver(resolver);
        }

        public virtual void InstallResolver(IModularResolver resolver)
        {
            if (resolver == null)
            {
                resolver = InstanceIfNull<SimpleResolver>();
                resolver = resolver;
            }

            Assert.IsFalse(resolver == null);

            // bind handler
            if (resolver is IResolver container)
            {
                container.BindInterface(typeof(IItem)).
                    FromInstance(this).
                    AsSingleton();
            }
            else
                resolver.RegisterModule<IItem>(this);
        }

        #endregion



        #endregion

        #region Routine

        // RegisterHandler => (Equip => SetHandler )=> (Deactivate => Dequip)=>Destory

        // RegisterHandler =>
        // (SetHandler => Equip ) => handler
        // (Deactivate => Dequip) =>Dispose

        /// <summary>
        /// Register this itemModule, load agent and modules.
        /// </summary>
        /// <param name="agent">Agent to use this itemModule(User,Remote,AI).</param>
        public override void Register(IAgent agent)
        {
            base.Register(agent);

            // make sure network handle installed
            if (resolver == null)
                InstallResolver(resolver);

            LoadTNO();

            // load modules from preset and bind container
            LoadPresetModules(gameObject);

            using var modules = resolver.Modules.CacheLoop();

            // inject depend
            foreach (var content in modules)
            {
                try
                {
                    if(content is IModule module)
                    {
                        module.SetHandler(this);

                        resolver.Inject(module);

                        if (module.CycleOption.HasFlag(ControlMode.Register))
                            module.Register();

                        // subscribe deactivate
                        if (module.CycleOption.HasFlag(ControlMode.Unregister))
                        {
                            OnUnregister -= module.UnRegister;
                            OnUnregister += module.UnRegister;
                        }
                    }
                    else if(content is Behaviour bhv)
                    {
                        bhv.enabled = true;
                    }
                    else
                    {
                        Debug.LogWarning($"{content} is not valid item module, will not been register.");
                        continue;
                    }

                    //if (content is IItemModule itemModule)
                    //{

                    //}

                    //if (module is TNet.IDataNodeSerializable serializable)
                    //    serializable.Deserialize();
                }
                catch (Exception e)
                {
                    Debug.LogException(e, this);
                }
            }
        }

        public override void Unregister(IAgent agent)
        {
            OnUnregister?.Invoke();

            base.Unregister(agent);
        }

        /// <summary>
        /// SetHandler all modules after equip itemModule.
        /// </summary>
        public override async UniTask Activate()
        {
            if (resolver.TryGetModule<IItemPostureHandler>(out var posture))
            {
                posture.Activate();
                await posture.Equip();
            }

            // activate modules
            using var modules = resolver.Modules.CacheLoop();

            Debug.Log($"Loading item modules : {modules.count}");

            foreach (var content in modules)
            {
                Debug.Log("Activating " + content.GetType());

                if (content is IModular itemModule)
                {
                    if (itemModule.CycleOption.HasFlag(ControlMode.Activate))
                    {
                        //Debug.Log(/*transform.parent + */" SetHandler module : " + itemModule);
                        itemModule.Activate();
                    }

                    // subscribe deactivate
                    if (itemModule.CycleOption.HasFlag(ControlMode.Deactivate))
                    {
                        OnItemDeactivate -= itemModule.Deactivate;
                        OnItemDeactivate += itemModule.Deactivate;
                    }
                }
                else if (content is Behaviour bhv)
                {
                    bhv.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"{content} is not valid item module, will not been activate.");
                    continue;
                }
            }
        }

        public override async UniTask Deactivate()
        {
            try
            {
                // not **posture  **performer  **preception
                OnItemDeactivate?.Invoke();

                // dequip anim
                if (resolver.TryGetModule<IItemPostureHandler>(out var posture))
                    await posture.Dequip();

                // revert itemModule collision and visual layer
                if (resolver.TryGetModule<PerceptionModule>(out var preception) && preception is IItemModule module)
                    module.Deactivate();

                //Container.DisposeCore();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                Debug.LogError($"{Time.frameCount} Deactive itemModule {this.name}  at " + transform.position);

            }
        }


        #endregion

        #region Network

        public ITNO tno { get; protected set; }

        [ShowInInspector,ReadOnly]
        public Parameters<bool> isMine { get; protected set; }

        /// <summary>
        /// Load network handle
        /// </summary>
        public virtual void LoadTNO(ITNO handle = null)
        {
            if (handle == null)
                handle = InstanceIfNull<TNObject>();

            tno = handle;

            isMine = new Parameter<bool>(() => handle.isMine);

            resolver.RegisterModule<ITNO>(handle);
        }

        #endregion


        #region IPickup

        public override void Drop(PR pr)
        {
            transform.SetWorldPR(pr);



            // generate showcase wrapper
            //go.SetChildLayer(Layers.Physics_Dynamic);

            {
                //var cols = go.GetComponentsInChildren<ColliderBounds>();
                //foreach (var col in cols)
                //{
                //    col.gameObject.layer = Layers.Physics_Dynamic;
                //    col.isTrigger = false;
                //}

                //var renderers = go.GetComponentsInChildren<Renderer>();
                //foreach (var renderer in renderers)
                //{
                //    renderer.enabled = true;
                //}
            }


            //var tf = CameraManager.mainCameraHandler.mainCameraTransform;


            //var rb = gameObject.InstanceIfNull<Rigidbody>();
            //rb.AddForce(transform.forward.Multiply(3), ForceMode.Acceleration);
        }

        public override void Drop(IPickupableProvider provider)
        {
            // generate showcase wrapper


        }

        #endregion


        #region Modules Bus

        /// <summary>
        /// Invoke while itemModule deactivate function.
        /// </summary>
        public event Action OnItemDeactivate;
        public event Action OnUnregister;

        /// <summary>
        /// Load configable modules and register, itemModule will not sure to set in handle until finish all the modules.
        /// </summary>
        protected virtual void LoadPresetModules(GameObject @object)
        {
            foreach (var modulePreset in Preset.Modules)
            {
                try
                {
                    var module = modulePreset.LoadModule(@object, Agent);
                    resolver.RegisterModule(module);

                    // register handler
                    //itemModule.SetHandler(this);
                    //if(itemModule.CycleOption.HasFlag(ControlMode.Register))
                    //    itemModule.Register();

                    Debug.Log($"Load {module} preset {modulePreset}");
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        #endregion
    }
}