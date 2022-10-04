namespace Return.Items
{
    public interface IConfigurableItemModule :IItemModule
    {
        void LoadModuleData(ConfigurableItemModulePreset preset);
    }


}