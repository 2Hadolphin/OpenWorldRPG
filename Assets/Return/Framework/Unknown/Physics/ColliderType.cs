using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return.Physical
{
    public enum ColliderType
    {
        Null,
        Sphere,
        Capsule,
        Box,
        Mesh,
    }

    public static class mExtension
    {
        public static ColliderType GetColType(this Collider col)
        {
            if (col is SphereCollider)
                return ColliderType.Sphere;
            else if(col is CapsuleCollider)
                return ColliderType.Capsule;
            else if (col is BoxCollider)
                return ColliderType.Box;
            else if (col is MeshCollider)
                return ColliderType.Mesh;
            else 
                return ColliderType.Null;
        }

        public static Collider Create(this ColliderType colliderType,GameObject @object)
        {
            return colliderType switch
            {
                ColliderType.Null => null,
                ColliderType.Sphere => @object.AddComponent<SphereCollider>(),
                ColliderType.Capsule => @object.AddComponent<SphereCollider>(),
                ColliderType.Box => @object.AddComponent<SphereCollider>(),
                ColliderType.Mesh => @object.AddComponent<SphereCollider>(),
                _ => throw new System.NotImplementedException(),
            };
        }
    }
}
