using Return.Agents;
using System.Collections;
using System.Collections.Generic;

namespace Return.InteractSystem
{
    /// <summary>
    /// Interface for component to interact.    **Agent can do behaviour.
    /// </summary>
    public abstract class BaseInterable : BaseComponent, IInteractHandle
    {

        public abstract HighLightMode HighLightMode { get; set; }


        /// <summary>
        /// Invoke this function to start interact.
        /// </summary>
        /// <param name="agent">Agent sent this interact msg.</param>
        /// <param name="sender">MonoModule to interact with.</param>
        public abstract void Interact(IAgent agent,object sender = null);

        public virtual void Cancel(IAgent agent,object sender = null) { }
    }
}