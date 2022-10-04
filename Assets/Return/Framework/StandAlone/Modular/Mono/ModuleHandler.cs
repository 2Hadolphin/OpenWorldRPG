using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;
using Sirenix.OdinInspector;

namespace Return.Modular
{
    /// <summary>
    /// Modular system to manage cycle of modules. **Agent **Motions **Inventory **Interact **Item
    /// </summary>
    public abstract class ModuleHandler : MonoModule, IModularHandle, IResolveHandle, IInjectable
    {
        #region IResolveHandle

        /// <summary>
        /// Binding Resolver or lit-Resolver.
        /// </summary>
        [ShowInInspector,ReadOnly]
        public IModularResolver Resolver { get; protected set; }

        /// <summary>
        /// Install parent container or create custom Resolver.
        /// </summary>
        void IResolveHandle.InstallResolver(IModularResolver resolver)
        {
            // default cache parent Resolver.
            Resolver = resolver;
            InstallResolver(resolver);
        }

        public abstract void InstallResolver(IModularResolver resolver);

        #endregion


        void Awake()
        {
            // register this module to Resolver if dynamic instantiate
            if (Resolver == null)
            {
                var resolver = GetComponentInParent<IModularResolver>(true);
                
                Assert.IsFalse(resolver == null, $"Non valid Resolver found in parent.");

                resolver.RegisterModule(this);
            }
        }


        protected virtual void OnDestroy()
        {
            if (Resolver != null)
                Resolver.UnregisterModule(this);
        }
    }
}