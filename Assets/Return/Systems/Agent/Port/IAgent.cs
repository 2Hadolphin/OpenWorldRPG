using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Return.Modular;
using Return.Framework.Parameter;

namespace Return.Agents
{
    /// <summary>
    /// Interface of autonomous unit
    /// </summary>
    public interface IAgent //: IModularResolver
    {
        bool isMine { get; }

        #region Mono
        /// <summary>
        /// Agent transform.
        /// </summary>
        Transform transform { get; }

        /// <summary>
        /// Agent gameObject.
        /// </summary>
        GameObject GameObject { get; }

        #endregion

        #region Resolver

        /// <summary>
        /// Container to hold all agent module. **Motions **Inventory **Interact
        /// </summary>
        IModularResolver Resolver { get; }

        #endregion


        /// <summary>
        /// Request system module and register self
        /// </summary>
        //[Obsolete]
        //bool Authorize<T>(out T system, bool fuzzySearch = true);
    }

    //public interface IAIHumanoidAgent : IHumanoidAgent
    //{


    //}

    //[Obsolete]
    //public interface ILocalHumanoidAgent : IAgent
    //{


    //}
}