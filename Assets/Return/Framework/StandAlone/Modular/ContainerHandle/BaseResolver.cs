using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;


namespace Return.Modular
{
    [DefaultExecutionOrder(ExecuteOrderList.Container)]
    public abstract class BaseResolver : BaseComponent, IModularresolverHandle, IStart
    {
        #region Iresolver

        protected abstract IModularResolver Resolver { get; }

        void IModularResolver.Inject(object obj) => Resolver.Inject(obj);

        void IModularResolver.RegisterModule<T>(object obj) => Resolver.RegisterModule<T>(obj);
        void IModularResolver.RegisterModule(object obj) => Resolver.RegisterModule(obj);
        void IModularResolver.UnregisterModule(object obj) => Resolver.UnregisterModule(obj);


        bool IModularResolver.TryGetModule<T>(out T module) => Resolver.TryGetModule(out module);
        bool IModularResolver.TryGetModule(Type type, out object module) => Resolver.TryGetModule(type, out module);

        void IModularResolver.SubscribeModule<T>(Action<T> callback, bool searchExisting) => Resolver.SubscribeModule<T>(callback, searchExisting);


        IEnumerable<object> IModularResolver.Modules => Resolver.Modules;
        IEnumerable<T> IModularResolver.GetModules<T>() => Resolver.GetModules<T>();
        IEnumerable<object> IModularResolver.GetModules(Type type) => Resolver.GetModules(type);

        #endregion


        #region Preset Binding

        [SerializeField]
        bool autoInject = false;


#if UNITY_EDITOR
        [ListDrawerSettings(Expanded = true, NumberOfItemsPerPage = 10, CustomAddFunction = nameof(EditorCheckAddModule))]
#endif
        [SerializeField]
        List<Object> m_Modules;

        public List<Object> PresetModules { get => m_Modules; set => m_Modules = value; }


        protected virtual void Awake()
        {
            SetupContainer();

            // inject pararmeter at next frame if auto execute toggle
            if (autoInject)
                Routine.AddStartable(this);

            LoadPresetModules();
        }

        protected virtual void SetupContainer() { } 

        void IStart.Initialize() => Activate();


        public virtual void Activate()
        {
             using var modules = Resolver.Modules.CacheLoop();

            Debug.Log($"{name} start inject parameters with {modules.count} contents.");

            foreach (var module in modules)
            {
                try
                {
                    if(module is not IInjectable)
                    {
                        Debug.Log($"Inject ignore because target is not IInjectable {module}.");
                        continue;
                    }

                    Resolver.Inject(module);

                    //if(module is MonoBehaviour)
                    //{
                    //    Debug.Log($"Inject mono {module}.");
                    //    Resolver.Inject(module);
                    //}
                    //else if(module is not Component)
                    //{
                    //    Debug.Log($"Inject {(module.IsNull()?"Null":module.GetType())}.");
                    //    Resolver.Inject(module);
                    //}
                    //else
                    //{
                    //    Debug.Log($"Inject ignore because target is not support {module}.");
                    //}
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }

        /// <summary>
        /// Install preset modules.
        /// </summary>
        protected void LoadPresetModules()
        {
            foreach (var preset in PresetModules)
            {
                try
                {
                    RegisterPreset(preset);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                finally
                {
                    Debug.Log($"Resolver register preset module : {preset}.");
                }
            }

            //CleanPresetModule();
        }

        protected virtual void RegisterPreset(Object preset)
        {
            Assert.IsFalse(preset == null);

            // install module with Resolve handle
            if (preset is IResolveHandle resolver)
                resolver.InstallResolver(Resolver);
            else
                Resolver.RegisterModule(preset);
        }

        public virtual void CleanPresetModule()
        {
            PresetModules.Clear();
            PresetModules.Capacity = 0;
        }


        #endregion

        #region Subscribe Callback

        protected DictionaryList<Type, ActionWrapper> ModuleCallbacks = new();

        public virtual void SubscribeModule<T>(Action<T> callback, bool searchExisting = true) where T : class
        {
            Assert.IsNotNull(callback);

            var type = typeof(T);

            ModuleCallbacks.Add(type, new ActionWrapper<T>(callback));

            if (searchExisting)
            {
                var modules = Resolver.GetModules<T>();

                foreach (var module in modules)
                    callback(module);
            }
        }

        protected virtual void OnModuleRegister(object module)
        {
            var type = module.GetType();

            var interfaces = type.GetInterfaces();

            foreach (var binding in interfaces)
                BindingModuleRegister(module, binding);

            BindingModuleRegister(module, type);
        }

        protected virtual void BindingModuleRegister(object module, Type type)
        {
            try
            {
                if (ModuleCallbacks.TryGetValues(type, out var list))
                {
                    Debug.Log("Found module register : " + list.Count);

                    foreach (ActionWrapper callback in list)
                    {
                        if (callback.NotNull())
                            callback.OnRecive(module);
                        else
                            Debug.LogError("Missing callback");
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

        }



        #endregion


        #region Editor

#if UNITY_EDITOR
        [ToggleLeft]
        [SerializeField]
        bool allowComponent = false;


        [ShowInInspector]
        [DropZone(typeof(Object),Text = "Drop Install Target.",AllowSceneObject =true)]
        [OnValueChanged(nameof(LoadContent))]
        Object m_dropZone;

        void LoadContent()
        {
            try
            {
                if (m_dropZone is GameObject go)
                {

                    if(go.transform.childCount>0 && UnityEditor.EditorUtility.DisplayDialog("Search Option","Search child components","Yes","No"))
                        SetInstaller(go.GetComponentsInChildren<Component>(true));
                    else
                        SetInstaller(go.GetComponents<Component>());
                }
                else
                    SetInstaller(m_dropZone);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                m_dropZone = null;
            }
        }


        void EditorCheckAddModule(Object obj)
        {
             if (obj != null)
                m_Modules.CheckAdd(obj);
        }

        [Button(ButtonSizes.Gigantic)]
        void QuickInject()
        {
            var comps = GetComponentsInChildren<Component>(true);

            SetInstaller(comps);

        }

        void SetInstaller(params Object[] objs)
        {
            foreach (var obj in objs)
            {
                if (obj == null || obj ==this)
                    continue;

                Debug.Log($"Add install target : {obj}.");

                switch (obj)
                {
                    case Transform tf:
                        if (tf.parent == null)
                        {
                            // binding root
                            Debug.Log($"Binding root transform {tf}.");
                        }
                        else
                            continue;

                        break;

                    case Animator:

                        break;

                    case Rigidbody:

                        break;

                    case IModularHandle:

                        break;

                    //case TNet.ITNO:

                    //    break;

                    case IModular:

                        break;

                    case ScriptableObject:

                        break;

                    case UnityEngine.EventSystems.UIBehaviour:

                        break;

                    default:

                        if (allowComponent && obj is Component)
                            break;

                        Debug.Log($"Auto inject ignore {objs}");
                        continue;
                }

                EditorCheckAddModule(obj);
            }
        }

        [OnInspectorGUI]
        [ShowInInspector]
        object draw_module;

        int index = -1;

        [OnInspectorGUI]
        void DrawModule()
        {
            GUILayout.BeginHorizontal();

            int newIndex = index;


            //if (Modules != null)
            //{
            //    if (GUILayout.Button("Last"))
            //        index = Modules.Loop(--index);

            //    if (GUILayout.Button("Next"))
            //        index = Modules.Loop(++index);

            //    if (index != newIndex)
            //        draw_module = Modules.ToArray()[index];
            //}
            //else
            {
                if (GUILayout.Button("Last"))
                    index = PresetModules.Loop(--index);

                if (GUILayout.Button("Next"))
                    index = PresetModules.Loop(++index);

                if (index != newIndex)
                    draw_module = PresetModules.ToArray()[index];
            }



            GUILayout.EndHorizontal();

        }

 

#endif

        #endregion

    }


}