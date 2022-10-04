using UnityEngine;
using System;

namespace Return.Modular
{
    /// <summary>
    /// Interface for handler to control life cycle of module.
    /// </summary>
    public interface IModule: IModular 
    {
        void SetHandler(object handler);
    }


    /// <summary>
    /// Unified modular interface, use to set handler reference and subscribe life cycle.
    /// SetHandler => Inject => Register => Activate => Deactivate => Unregister
    /// </summary>
    /// <typeparam name="T">Handler type.</typeparam>
    public interface IModule<T> : IModule
    {
        void IModule.SetHandler(object handler)
        {
            if (handler is T module)
                SetHandler(module);
            else
                Debug.LogException(new InvalidCastException($"Set [{typeof(T)}] handler failure with {handler}."));
        }

        void SetHandler(T handler);
    }



}