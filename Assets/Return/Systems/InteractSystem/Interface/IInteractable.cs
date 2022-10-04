using Cysharp.Threading.Tasks;
using Return.Agents;
using Return.Items;
using System;
using UnityEngine;

namespace Return.InteractSystem
{

    public delegate void Interact(IAgent agent, object sender = null);

    public delegate void Cancel(IAgent agent, object sender = null);


    /// <summary>
    /// Provide agent interaction interface, but has lower targeted order than interactable.
    /// </summary>
    public interface IInteractable
    {
        /// <summary>
        /// Execute interact behaviour.
        /// </summary>
        /// <param name="agent">Sender</param>
        /// <param name="sender">Interactor</param>
        void Interact(IAgent agent, object sender = null);


        /// <summary>
        /// Cancel interact behaviour.
        /// </summary>
        /// <param name="agent">Sender</param>
        /// <param name="sender">Interactor</param>
        void Cancel(IAgent agent, object sender = null);
    }

    /// <summary>
    /// MonoModule to manage interact between game object.
    /// </summary>
    public interface IInteractHandler
    {
        /// <summary>
        /// Invoke while interact new target. 
        /// </summary>
        event Action<InteractWrapper> OnInteractMission;

        /// <summary>
        /// Invoke while interact cancel.
        /// </summary>
        event Action<InteractWrapper> OnCancelMission;


        UniTask<bool> Interact(object obj);
    }


    public interface ISelectionHandler<T>
    {
        /// <summary>
        /// Invoke while selection collection changed.
        /// </summary>
        event Action OnSelectedChanged;

        /// <summary>
        /// Invoke while new target selected.
        /// </summary>
        event Action<InteractWrapper> OnSelected;

        /// <summary>
        /// Invoke while exist target deselect.
        /// </summary>
        event Action<InteractWrapper> OnDeselected;
    }


    /// <summary>
    /// Interface to subscribe selection result.    **Sensor => Interact handler
    /// </summary>
    public interface ISelectionHandle<T>
    {
        /// <summary>
        /// RegisterHandler selection handler.
        /// </summary>
        void SetHandler(ISelectionHandler<T> handler);
    }


    /// <summary>
    /// ???? delete?
    /// </summary>
    [Obsolete]
    public interface IHumanoidInteractSystem : IInteractHandler
    {

        /// <summary>
        /// Search most closed interactor
        /// </summary>
        Symmetry Evalute(ICoordinate coordinates);

        /// <summary>
        /// CheckAdd hand enable
        /// </summary>
        bool ValidMission(Symmetry requireHand, out Symmetry validHandle);

        /// <summary>
        /// Apply for a task request
        /// </summary>
        /// <param name="sender">Who can control the IK handle</param>
        /// <param name="requireHand">Inputs ''Deny'' option will return true always</param>
        /// <param name="adapter">Choses to </param>
        /// <returns>Return true if all request been apply</returns>
        InteractAdapter SetupMission(object sender, Symmetry requireHand);




        UniTask<bool> PickUp(IPickup pickup, object plan=null);
    }

}