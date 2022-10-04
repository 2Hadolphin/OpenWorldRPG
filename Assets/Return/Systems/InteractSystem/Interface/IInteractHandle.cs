namespace Return.InteractSystem
{
    /// <summary>
    /// Interface for selection module to push interaction.
    /// </summary>
    public interface IInteractHandle : IInteractable
    {
        HighLightMode HighLightMode { get; }

        Interact InteractHandle => Interact;
        virtual Cancel CancelHandle => Cancel;
    }

}