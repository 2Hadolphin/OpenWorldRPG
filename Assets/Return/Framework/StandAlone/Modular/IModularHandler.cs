using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Modular
{
    /// <summary>
    /// Interface to register and install modules.
    /// </summary>
    public interface IModularResolver
    {

        /// <summary>
        /// All modules without safe cache.
        /// </summary>
        IEnumerable<object> Modules { get; }

        /// <summary>
        /// Binding instance with custom type.
        /// </summary>
        /// <typeparam name="T">Type to binding.</typeparam>
        void RegisterModule<T>(object obj);

        /// <summary>
        /// Binding instance reference to container.
        /// </summary>
        void RegisterModule(object obj);


        /// <summary>
        /// Remove instance reference to container.
        /// </summary>
        void UnregisterModule(object obj);

        /// <summary>
        /// Get all module with typeof(T).
        /// </summary>
        IEnumerable<T> GetModules<T>();

        /// <summary>
        /// Get all module with type.
        /// </summary>
        IEnumerable<object> GetModules(Type type);

        /// <summary>
        /// Try get module with typeof(T).
        /// </summary>
        bool TryGetModule<T>(out T module) where T : class;
        //bool TryGetModule<T>(out T module, Type defaultTypeToInstance = null) where T : class;

        /// <summary>
        /// Try get module with type.
        /// </summary>
        bool TryGetModule(Type type, out object module);

        /// <summary>
        /// Subscribe module when container update.
        /// </summary>
        /// <typeparam name="T">Type to subscribe.</typeparam>
        /// <param name="callbackExisting">Invoke callback with existing module.</param>
        void SubscribeModule<T>(Action<T> callback, bool callbackExisting = true) where T : class;



        /// <summary>
        /// Copy behaviour from preset and register by reflection.
        /// </summary>
        /// <param name="variant"> preset to copy from</param>
        //T RegisterModuleVariant<T>(T variant, bool singleton = true) where T : Behaviour;

        /// <summary>
        /// Inject parameter with attribute by reflection.
        /// </summary>
        /// <param name="obj">Instance to inject data.</param>
        void Inject(object obj);
    }

    public interface IModularresolverHandle : IModularResolver 
    {
        /// <summary>
        /// Manual inject contents.
        /// </summary>
        void Activate();
    }


    /// <summary>
    /// Interface for handler to identify and register module.
    /// </summary>
    public interface IModularHandle
    {

    }

    /// <summary>
    /// Interface to identify handler and install Resolver. **Agent **Item **Motions **Interact **Inventory **Object
    /// </summary>
    public interface IResolveHandle : IModularHandle
    {
        /// <summary>
        /// Install parent Resolver and register custom interface.
        /// </summary>
        void InstallResolver(IModularResolver resolver);
        //void Unistallresolver(IModularResolver Resolver);

        //#region IResolveHandle

        ///// <summary>
        ///// Binding Resolver or lit-Resolver.
        ///// </summary>
        //public IModularResolver Resolver { get; protected set; }

        ///// <summary>
        ///// Install parent container or create custom Resolver.
        ///// </summary>
        //void IResolveHandle.InstallResolver(IModularResolver Resolver)
        //{
        //    // default cache parent Resolver.
        //    Resolver = Resolver;
        //    InstallResolver(Resolver);
        //}

        //public abstract void InstallResolver(IModularResolver Resolver);

        //#endregion
    }



}