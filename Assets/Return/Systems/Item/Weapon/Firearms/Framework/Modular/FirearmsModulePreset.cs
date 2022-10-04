using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;
using Return;
using Return.Humanoid.Motion;
using Return.Agents;
using Return.Items.Weapons.Firearms;

namespace Return.Items.Weapons
{
    public abstract class FirearmsModulePreset : ConfigurableItemModulePreset 
    {
        
    }
    public abstract class FirearmsModulePreset<TFirearmModule> : FirearmsModulePreset where TFirearmModule: FirearmsModule,IFirearmsModule
    {
        public override IConfigurableItemModule  LoadModule(GameObject @object, IAgent agent = null)
        {
            return InstanceModule<TFirearmModule>(@object);
        }
    }
}