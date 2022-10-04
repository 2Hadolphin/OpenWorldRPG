namespace Return.Items.Gear
{
    public interface IWear
    {
        bool TryWear();
        Gear Wear();
    }
}