namespace Return.Items
{
    /// <summary>
    /// Mono 
    /// </summary>
    public abstract class ItemModuleHandle : MonoItemModule, IItemModuleHandle
    {
        public virtual void SetModule(IItem item, object module)
        {
            base.Activate();
        }
    }

}