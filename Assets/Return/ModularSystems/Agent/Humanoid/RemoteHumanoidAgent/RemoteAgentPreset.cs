using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace Return.Agents
{
    public class RemoteAgentPreset : AgentModularPreset
    {
        public override void LoadModule(GameObject @object)
        {
            var agent = @object.InstanceIfNull<HumanoidAgent>();
            agent.Name = "RemotePlayer";
        }
    }
}
