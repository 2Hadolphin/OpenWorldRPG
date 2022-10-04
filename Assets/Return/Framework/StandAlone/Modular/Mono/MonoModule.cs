using System;
using UnityEngine;

namespace Return.Modular
{


    /// <summary>
    /// Base component for handler to control life cycle.
    /// </summary>
    public abstract class MonoModule : AbstractMonoModule, IModule
    {
        #region Hanlder

        void IModule.SetHandler(object handler) => SetHandler(handler);

        public virtual void SetHandler(object handler)
        {
            Debug.LogException(new NotImplementedException("Module handler setter not finish."));
        }

        #endregion
    }


   
}