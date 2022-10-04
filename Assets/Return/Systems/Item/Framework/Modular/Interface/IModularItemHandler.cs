using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Modular;
using Return.Agents;
using System;

namespace Return.Items
{
    /// <summary>
    /// The interface for item modules to access the core. **agent **other modules
    /// </summary>
    [Obsolete]
    public interface IModularItemHandler : IModularResolver
    {
        public IAgent Agent { get; }

        void RegisterModule(object module);

        /// <summary>
        /// Use this method to get module.
        /// </summary>
        bool TryGetModule<T>(out T module, Type defaultType = null) where T : class;

    }
}