using UnityEngine;

namespace Return.Items.Weapons
{
    public interface IWeaponCaculator
    {
        bool Initialization(IWeaponBase central);
        void Single();
        void Multi();
        void CheckMotion();
        void MathfOperate();
    }
}