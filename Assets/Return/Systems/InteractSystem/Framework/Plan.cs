using Return.Items;

namespace Return.Agents
{
    public class Plan
    {
        public IPickup Item;
        public StrokeType StrokeType;
        public ICoordinate TargetCoordinate;
        public float Time = 0.5f;
        public float TimeLog;
        public Plan Dependent;
        public Set DependType = Set.Intersection;
    }

}
