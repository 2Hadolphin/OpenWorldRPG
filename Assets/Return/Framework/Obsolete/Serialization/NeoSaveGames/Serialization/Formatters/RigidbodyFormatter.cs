using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization.Formatters
{
    public class RigidbodyFormatter : NeoSerializationFormatter<Rigidbody>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#if UNITY_EDITOR
        [UnityEditor.InitializeOnLoadMethod]
#endif
        static void Register()
        {
            NeoSerializationFormatters.RegisterFormatter(new RigidbodyFormatter());
        }

        private static readonly NeoSerializationKey k_VelocityKey = new NeoSerializationKey("velocity");
        private static readonly NeoSerializationKey k_AngularVKey = new NeoSerializationKey("angularV");
        private static readonly NeoSerializationKey k_DetectCollisionsKey = new NeoSerializationKey("detectCollisions");
        private static readonly NeoSerializationKey k_InterpolationKey = new NeoSerializationKey("interpolation");
        private static readonly NeoSerializationKey k_IsKinematicKey = new NeoSerializationKey("isKinematic");
        private static readonly NeoSerializationKey k_MassKey = new NeoSerializationKey("mass");
        private static readonly NeoSerializationKey k_UseGravityKey = new NeoSerializationKey("useGravity");

        protected override void WriteProperties(INeoSerializer writer, Rigidbody from, NeoSerializedGameObject nsgo)
        {
            writer.WriteValue(k_VelocityKey, from.velocity);
            writer.WriteValue(k_AngularVKey, from.angularVelocity);
            writer.WriteValue(k_DetectCollisionsKey, from.detectCollisions);
            writer.WriteValue(k_InterpolationKey, (int)from.interpolation);
            writer.WriteValue(k_IsKinematicKey, from.isKinematic);
            writer.WriteValue(k_MassKey, from.mass);
            writer.WriteValue(k_UseGravityKey, from.useGravity);
        }

        protected override void ReadProperties(INeoDeserializer reader, Rigidbody to, NeoSerializedGameObject nsgo)
        {
            Vector3 vector3Prop;
            bool boolProp;
            int intProp;
            float floatProp;

            if (reader.TryReadValue(k_VelocityKey, out vector3Prop, Vector3.zero))
                to.velocity = vector3Prop;

            if (reader.TryReadValue(k_AngularVKey, out vector3Prop, Vector3.zero))
                to.angularVelocity = vector3Prop;

            if (reader.TryReadValue(k_DetectCollisionsKey, out boolProp, true))
                to.detectCollisions = boolProp;

            if (reader.TryReadValue(k_InterpolationKey, out intProp, 0))
                to.interpolation = (RigidbodyInterpolation)intProp;

            if (reader.TryReadValue(k_IsKinematicKey, out boolProp, false))
                to.isKinematic = boolProp;

            if (reader.TryReadValue(k_MassKey, out floatProp, 0))
                to.mass = floatProp;

            if (reader.TryReadValue(k_UseGravityKey, out boolProp, true))
                to.useGravity = boolProp;

            to.position = to.transform.position;
            to.rotation = to.transform.rotation;
        }
    }
}