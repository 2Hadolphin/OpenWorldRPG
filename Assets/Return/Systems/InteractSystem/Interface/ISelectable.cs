using Return.Agents;

namespace Return.InteractSystem
{
    public interface ISelectable
    {
        void OnSelected(IAgent agent,object sender);
        void OnDeselect(IAgent agent, object sender);
    }
}