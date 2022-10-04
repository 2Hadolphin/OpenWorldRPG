


namespace Return
{

    public class Modifier : Entity<string> , IModifier
    {
        public Modifier(string id, ModifierType type, int amount) : base(id)
        {
            Amount = amount;
            Type = type;
        }

        public int          Amount { get; }
        public ModifierType Type   { get; }




    }
}