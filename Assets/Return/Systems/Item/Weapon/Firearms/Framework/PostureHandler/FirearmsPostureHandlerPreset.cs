using Return.Agents;
using UnityEngine;

namespace Return.Items.Weapons.Firearms
{
    public class FirearmsPostureHandlerPreset : ItemPostureHandlerPreset
    {
        public override IConfigurableItemModule LoadModule(GameObject @object, IAgent agent = null)
        {
            return InstanceModule<FirearmsPostureHandlerModule>(@object);
        }
    }
}