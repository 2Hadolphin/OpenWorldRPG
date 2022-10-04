using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public interface INeoSerializer
    {
        void BeginSerialization();
        void EndSerialization();
        bool WriteToStream(Stream stream);

        bool isSerializing { get; }
        int byteLength { get; }

        void PushContext(SerializationContext context, int id);
        void PopContext(SerializationContext context);// Use type for error checking

        void WriteValue(string key, bool value);
        void WriteValue(string key, byte value);
        void WriteValue(string key, sbyte value);
        void WriteValue(string key, char value);
        void WriteValue(string key, short value);
        void WriteValue(string key, ushort value);
        void WriteValue(string key, int value);
        void WriteValue(string key, uint value);
        void WriteValue(string key, long value);
        void WriteValue(string key, ulong value);
        void WriteValue(string key, float value);
        void WriteValue(string key, double value);
        void WriteValue(string key, string value);
        void WriteValue(string key, Vector2 value);
        void WriteValue(string key, Vector3 value);
        void WriteValue(string key, Vector4 value);
        void WriteValue(string key, Vector2Int value);
        void WriteValue(string key, Vector3Int value);
        void WriteValue(string key, Quaternion value);
        void WriteValue(string key, Color value);
        void WriteValue(string key, Color32 value);
        void WriteValue(string key, Guid value);
        void WriteValue(string key, DateTime value);
        void WriteValues(string key, ICollection<bool> value);
        void WriteValues(string key, ICollection<byte> value);
        void WriteValues(string key, ICollection<sbyte> value);
        void WriteValues(string key, ICollection<char> value);
        void WriteValues(string key, ICollection<short> value);
        void WriteValues(string key, ICollection<ushort> value);
        void WriteValues(string key, ICollection<int> value);
        void WriteValues(string key, ICollection<uint> value);
        void WriteValues(string key, ICollection<long> value);
        void WriteValues(string key, ICollection<ulong> value);
        void WriteValues(string key, ICollection<float> value);
        void WriteValues(string key, ICollection<double> value);
        void WriteValues(string key, ICollection<string> value);
        void WriteValues(string key, ICollection<Vector2> value);
        void WriteValues(string key, ICollection<Vector3> value);
        void WriteValues(string key, ICollection<Vector4> value);
        void WriteValues(string key, ICollection<Vector2Int> value);
        void WriteValues(string key, ICollection<Vector3Int> value);
        void WriteValues(string key, ICollection<Quaternion> value);
        void WriteValues(string key, ICollection<Color> value);
        void WriteValues(string key, ICollection<Color32> value);
        void WriteValues(string key, ICollection<Guid> value);
        void WriteSerializable<T>(string key, T s);
        void WriteSerializables<T>(string key, ICollection<T> s);
        bool WriteComponentReference<T>(string key, T value, NeoSerializedGameObject pathFrom) where T : class;
        bool WriteTransformReference(string key, Transform value, NeoSerializedGameObject pathFrom);
        bool WriteGameObjectReference(string key, GameObject value, NeoSerializedGameObject pathFrom);
        bool WriteNeoSerializedGameObjectReference(string key, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom);

        void WriteValue(int hash, bool value);
        void WriteValue(int hash, byte value);
        void WriteValue(int hash, sbyte value);
        void WriteValue(int hash, char value);
        void WriteValue(int hash, short value);
        void WriteValue(int hash, ushort value);
        void WriteValue(int hash, int value);
        void WriteValue(int hash, uint value);
        void WriteValue(int hash, long value);
        void WriteValue(int hash, ulong value);
        void WriteValue(int hash, float value);
        void WriteValue(int hash, double value);
        void WriteValue(int hash, string value);
        void WriteValue(int hash, Vector2 value);
        void WriteValue(int hash, Vector3 value);
        void WriteValue(int hash, Vector4 value);
        void WriteValue(int hash, Vector2Int value);
        void WriteValue(int hash, Vector3Int value);
        void WriteValue(int hash, Quaternion value);
        void WriteValue(int hash, Color value);
        void WriteValue(int hash, Color32 value);
        void WriteValue(int hash, Guid value);
        void WriteValue(int hash, DateTime value);
        void WriteValues(int hash, ICollection<bool> value);
        void WriteValues(int hash, ICollection<byte> value);
        void WriteValues(int hash, ICollection<sbyte> value);
        void WriteValues(int hash, ICollection<char> value);
        void WriteValues(int hash, ICollection<short> value);
        void WriteValues(int hash, ICollection<ushort> value);
        void WriteValues(int hash, ICollection<int> value);
        void WriteValues(int hash, ICollection<uint> value);
        void WriteValues(int hash, ICollection<long> value);
        void WriteValues(int hash, ICollection<ulong> value);
        void WriteValues(int hash, ICollection<float> value);
        void WriteValues(int hash, ICollection<double> value);
        void WriteValues(int hash, ICollection<string> value);
        void WriteValues(int hash, ICollection<Vector2> value);
        void WriteValues(int hash, ICollection<Vector3> value);
        void WriteValues(int hash, ICollection<Vector4> value);
        void WriteValues(int hash, ICollection<Vector2Int> value);
        void WriteValues(int hash, ICollection<Vector3Int> value);
        void WriteValues(int hash, ICollection<Quaternion> value);
        void WriteValues(int hash, ICollection<Color> value);
        void WriteValues(int hash, ICollection<Color32> value);
        void WriteValues(int hash, ICollection<Guid> value);
        void WriteSerializable<T>(int hash, T s);
        void WriteSerializables<T>(int hash, ICollection<T> s);
        bool WriteComponentReference<T>(int hash, T value, NeoSerializedGameObject pathFrom) where T : class;
        bool WriteTransformReference(int hash, Transform value, NeoSerializedGameObject pathFrom);
        bool WriteGameObjectReference(int hash, GameObject value, NeoSerializedGameObject pathFrom);
        bool WriteNeoSerializedGameObjectReference(int hash, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom);
    }
}