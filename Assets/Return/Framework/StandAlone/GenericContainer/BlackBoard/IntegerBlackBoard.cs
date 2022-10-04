using Sirenix.OdinInspector;

namespace Return
{
    /// <summary>
    /// Integer blackboard with preset value
    /// </summary>
    public class IntegerBlackBoard : GenericBlackBoard<int>
    {
        public override VirtualValue ValueType => VirtualValue.Int;
    }
}