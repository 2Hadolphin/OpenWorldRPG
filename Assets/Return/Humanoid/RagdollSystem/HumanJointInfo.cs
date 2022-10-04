using UnityEngine;
using System;
using Return.Physical;

namespace Return.Humanoid.Character
{
    /// <summary>
    /// Basic ragdoll element data
    /// </summary>
    [Serializable]
    public abstract class HumanJointInfo
    {
        /// <summary>
        /// Attach human bone
        /// </summary>
        public HumanBodyBones bone;
        public HumanBodyBones parent;
        public float minLimit;
        public float maxLimit;
        public float swingLimit;

        public virtual Vector3 Center { get; set; }
        public Vector3 axis;
        public Vector3 normalAxis;

        public float BoundsScale;
        public ColliderType ColliderType;

        public float density;
        public float summedMass;// The mass of this and all children bodies

    }
}
