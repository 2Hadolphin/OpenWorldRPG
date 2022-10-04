using UnityEngine;
using System;
using Newtonsoft.Json;
using Sirenix.OdinInspector;

namespace Return
{
    [Serializable]
    public struct PR:IEquatable<PR>
    {
        public static PR Default => new(Vector3.zero, Quaternion.identity);

        public PR(Vector3 position,Quaternion rotation)
        {
            Position = position;
            Rotation = rotation;
        }

        public PR(Vector3 position)
        {
            Position = position;
            Rotation = Quaternion.identity;
        }

        public PR(Quaternion rotation)
        {
            Position = Vector3.zero;
            Rotation = rotation;
        }

        [SerializeField]
        public Vector3 Position;

        [HideInInspector]
        [SerializeField]
        public Quaternion Rotation;

        [JsonIgnore]
        [ShowInInspector]
        [LabelText("Rotation")]
        public Vector3 eulerAngles
        {
            get => Rotation.eulerAngles;
            set => Rotation = Quaternion.Euler(value);
        }

        public Vector3 WrapRotation
        {
            get => Rotation.eulerAngles.WrapAngle();
        }

        public static PR operator +(PR a,PR b)
        {
            return new PR()
            {
                Position = a.Position + b.Position,
                Rotation = Quaternion.Euler(a.Rotation.eulerAngles + b.Rotation.eulerAngles),
            };
        }
        public static implicit operator Vector3(PR value)
        {
            return value.Position;
        }
        public static implicit operator Quaternion(PR value)
        {
            return value.Rotation;
        }

        /// <summary>
        /// Get world PR.
        /// </summary>
        public static implicit operator PR(Transform transform)
        {
            return new PR(transform.position, transform.rotation);
        }

        public bool Equals(PR other)
        {
            return other.Position.Equals(Position) && other.Rotation.Equals(Rotation);
        }

#if UNITY_EDITOR
        public override string ToString()
        {
            return string.Format("Position : {0} Rotation : {1}", Position, eulerAngles.WrapAngle());
        }
#endif
    }
}
