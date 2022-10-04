using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Return.Agents;
using Return.Humanoid;
using Return.Modular;

namespace Return.Gear
{
    /// <summary>
    /// ???
    /// </summary>
    public abstract class GearSystem : ModuleHandler
    {
        protected HashSet<EquipmentSlot> Slots = new ();

        public virtual bool LoadGear()
        {
            foreach (var slot in Slots)
            {

            }
            return false;
        }

        public override void InstallResolver(IModularResolver resolver)
        {
            resolver.RegisterModule(this);
        }
    }

    public class CharacterGearSystem : GearSystem
    {

    }

    

}