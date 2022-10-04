using UnityEngine;

namespace Return.Humanoid.Character
{
    public class BoxJoint : HumanJointInfo
    {
        public Bounds Bounds;
        public override Vector3 Center { get => Bounds.center; set => Bounds.center = value; }

    }
}
