using UnityEngine;

namespace Return.Items.Gear
{
    public enum GearStyle { Glasses = 0, Hat = 1, Top = 2, Bottom = 3, Shoes = 4, Armor = 5, Inventory = 6, }
    public abstract class Gear : ConfigurableItemModulePreset
    {
        public GearStyle Style { get; protected set; }

        public abstract bool TryWear();
        public abstract Gear Wear();
        //public abstract GearSystem[] System { get; }
        //[SerializeField]
        //protected GearSystem[] systems;


    }
}
