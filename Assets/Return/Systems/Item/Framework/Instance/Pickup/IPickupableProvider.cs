namespace Return.Items
{
    /// <summary>
    /// Interface of custom loot processor. **drop position **loot pool **network sync **debris
    /// </summary>
    public interface IPickupableProvider
    {
        /// <summary>
        /// Set pickupable item with custom module.
        /// </summary>
        /// <param name="pickup">Item to drop.</param>
        void SetPickupable(IPickup pickup);
    }
}