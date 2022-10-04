using Return.Framework.DI;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using System.Linq;

namespace Return.Modular
{
    /// <summary>
    /// Local Resolver handle with type bindings container.
    /// </summary>
    public class BindingResolver : BaseResolver, IResolver
    {
        public ModularContainer Container { get; protected set; }

        protected override IModularResolver Resolver => Container;

        #region Iresolver

        BindingHandle IResolver.BindInterface<T>()
        {
            return Container.BindInterface<T>();
        }

        BindingHandle IResolver.BindInterface(IEnumerable<Type> types)
        {
            return Container.BindInterface(types);
        }

        BindingHandle IResolver.BindInterface(params Type[] types)
        {
            return Container.BindInterface(types);
        }

        void IResolver.Inject(object obj)
        {
            Container.Inject(obj);
        }

        #endregion

        #region Mono

        protected override void SetupContainer()
        {
            Container = new ModularContainer();
        }


        #endregion

    }

    /// <summary>
    /// Container with modular access.
    /// </summary>
    public class ModularContainer : Container, IModularResolver
    {
        #region Iresolver

        IEnumerable<object> IModularResolver.Modules => GetModules();
        void IModularResolver.Inject(object obj) => Inject(obj);


        #endregion

        public virtual void RegisterModule<T>(object module)
        {
            Assert.IsFalse(module.IsNull());

            var type = typeof(T);

            Register(type, module);
        }

        /// <summary>
        /// Register module to container.
        /// </summary>
        public virtual void RegisterModule(object module)
        {
            var type = module.GetType();
            Register(type, module);
        }

        public virtual void UnregisterModule(object obj)
        {
            Unregister(obj);
        }



        public virtual bool TryGetModule<T>(out T module) where T : class
        {
            var type = typeof(T);

            if (TryResolve(type, out var obj) && obj is T value)
            {
                module = value;
                return true;
            }

            module = null;
            return false;
        }

        IEnumerable<object> all()
        {
            foreach (var module in Singleton.Values)
                yield return module;

            foreach (var modules in InstancePools.Values)
                foreach (var module in modules)
                    yield return module;

        }


        /// <summary>
        /// Return all module instance.
        /// </summary>
        protected virtual IEnumerable<object> GetModules()
        {
            return all().ToHashSet();
        }


        public IEnumerable<T> GetModules<T>()
        {
            var modules = all();

            foreach (var module in modules)
                if (module is T value)
                    yield return value;
        }

        public virtual bool TryGetModule(Type type, out object module)
        {
            return TryResolve(type, out module);
        }

        public IEnumerable<object> GetModules(Type type)
        {
            IEnumerable<object> values()
            {
                foreach (var pair in Singleton)
                {
                    if (pair.Key == type)
                        yield return pair.Value;

                    if (pair.Key.IsSubclassOf(type))
                        yield return pair.Value;

                    if (type.IsAssignableFrom(pair.Key))
                        yield return pair.Value;
                }

                foreach (var pair in InstancePools)
                {
                    if (pair.Key == type)
                        foreach (var value in pair.Value)
                            yield return value;


                    if (pair.Key.IsSubclassOf(type))
                        foreach (var value in pair.Value)
                            yield return value;

                    if (type.IsAssignableFrom(pair.Key))
                        foreach (var value in pair.Value)
                            yield return value;
                }
            }

            return values().ToHashSet();
        }


        protected DictionaryList<Type, ActionWrapper> ModuleCallbacks = new();

        public virtual void SubscribeModule<T>(Action<T> callback, bool searchExisting = true) where T : class
        {
            Assert.IsNotNull(callback);

            Debug.Log($"{callback} subscribe {typeof(T)}.");

            var type = typeof(T);

            ModuleCallbacks.Add(type, new ActionWrapper<T>(callback));

            if (searchExisting)
            {
                var modules = GetModules<T>();

                foreach (var module in modules)
                    callback(module);
            }
        }

        /// <summary>
        /// Proccess module register event.
        /// </summary>
        protected virtual void OnModuleRegister(object module)
        {
            var type = module.GetType();

            var interfaces = type.GetInterfaces();

            foreach (var binding in interfaces)
                CheckBindingModuleRegister(module, binding);

            CheckBindingModuleRegister(module, type);
        }

        protected virtual void CheckBindingModuleRegister(object module, Type type)
        {
            try
            {
                if (ModuleCallbacks.TryGetValues(type, out var list))
                {
                    Debug.Log($"Valid {type.Name} callback nums : {list.Count}.");

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
    }



}