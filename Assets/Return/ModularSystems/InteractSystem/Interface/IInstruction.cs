using Return.Agents;

namespace Return.InteractSystem
{
    /// <summary>
    /// Interfac to invoke instruction during interaction.
    /// </summary>
    public interface IInstruction
    {
        /// <summary>
        /// Process instruction to agent.
        /// </summary>
        void Instruction(IAgent agent, object sender);
    }

}