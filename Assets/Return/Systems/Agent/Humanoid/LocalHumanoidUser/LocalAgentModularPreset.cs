using UnityEngine;

namespace Return.Agents
{
    public class LocalAgentModularPreset : AgentModularPreset
    {
        public override void LoadModule(GameObject character)
        {
            var id = new GameObject(nameof(HumanoidAgent));

            id.SetParent(character);
            var agent = character.InstanceIfNull<HumanoidAgent>();
            agent.Name = "LocalPlayer";
 

        }
    }
}