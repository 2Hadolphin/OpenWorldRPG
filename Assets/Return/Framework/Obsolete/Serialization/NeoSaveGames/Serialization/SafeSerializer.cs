using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public class SafeSerializer : INeoSerializer
    {
        private SerializerState m_State = SerializerState.Idle;
        private MemoryStream m_MemoryStream = null;
        private BinaryFormatter m_Formatter = null;

        enum SerializerState
        {
            Idle,
            CollectingData,
            WaitingForWrite,
            WritingData
        }

        public bool isSerializing
        {
            get { return m_State != SerializerState.Idle; }
        }

        public int byteLength
        {
            get { return (int)m_MemoryStream.Length; }
        }

        public SafeSerializer()
        {
            m_MemoryStream = new MemoryStream();
            m_Formatter = new BinaryFormatter();
        }

        void ClearBuffers()
        {
            m_MemoryStream.SetLength(0);
        }

        public void BeginSerialization()
        {
            if (m_State != SerializerState.Idle)
                Debug.LogError("Attempting to begin serialization, while serialization is already ongoing");
            else
                m_State = SerializerState.CollectingData;
        }

        public void EndSerialization()
        {
            if (!CheckEndingContext())
                Debug.LogError("Unbalanced push/pop for serialization contexts. Serialization did not end on the root context.");

            switch (m_State)
            {
                case SerializerState.CollectingData:
                    {
                        m_Formatter.Serialize(m_MemoryStream, PropertyType.EndOfData);
                        m_Formatter.Serialize(m_MemoryStream, PropertyType.EndOfData);
                        m_Formatter.Serialize(m_MemoryStream, PropertyType.EndOfData);
                        m_Formatter.Serialize(m_MemoryStream, PropertyType.EndOfData);
                        m_State = SerializerState.WaitingForWrite;
                    }
                    break;
                case SerializerState.Idle:
                    Debug.LogError("Attempting to end serialization when it hasn't been started.");
                    break;
                case SerializerState.WritingData:
                    Debug.LogError("Attempting to end serialization while writing data (has already been ended).");
                    break;
            }
        }
        
        public bool WriteToStream(Stream stream)
        {
            if (m_State == SerializerState.WaitingForWrite)
            {
                m_State = SerializerState.WritingData;

                try
                {
                    m_MemoryStream.WriteTo(stream);
                }
                catch (Exception e)
                {
                    Debug.LogError("Serialization failed with error: " + e.Message);
                    return false;
                }

                // Set state to idle
                m_State = SerializerState.Idle;

                ClearBuffers();

                return true;
            }
            else
            {
                Debug.LogError("Attempting to write serialized data to stream before serialization has ended.");
                return false;
            }
        }
        

        #region CONTEXT

        private Stack<ContextInfo> m_CurrentContext = new Stack<ContextInfo>();

        private class ContextInfo
        {
            public SerializationContext contextType = SerializationContext.Root;
            public int id = -1;

            private int m_PropertiesStart = 0;
            private int m_SubContextsStart = 0;

            static List<int> s_Properties = new List<int>(128);
            static List<int> s_SubContexts = new List<int>(32);

            public ContextInfo(SerializationContext t, int i)
            {
                contextType = t;
                id = i;
                m_PropertiesStart = s_Properties.Count;
                m_SubContextsStart = s_SubContexts.Count;
            }

            public bool AddProperty(int hash)
            {
                // Check if the hash exists
                for (int i = m_PropertiesStart; i < s_Properties.Count; ++i)
                    if (s_Properties[i] == hash)
                        return false;

                // Add the hash
                s_Properties.Add(hash);
                return true;
            }

            public bool AddSubContext(int hash)
            {
                // Check if the hash exists
                for (int i = m_SubContextsStart; i < s_SubContexts.Count; ++i)
                    if (s_SubContexts[i] == hash)
                        return false;

                // Add the hash
                s_SubContexts.Add(hash);
                return true;
            }

            public void Destroy()
            {
                if (s_Properties.Count > m_PropertiesStart)
                    s_Properties.RemoveRange(m_PropertiesStart, s_Properties.Count - m_PropertiesStart);
                if (s_SubContexts.Count > m_SubContextsStart)
                    s_SubContexts.RemoveRange(m_SubContextsStart, s_SubContexts.Count - m_SubContextsStart);
            }
        }

        bool CheckEndingContext()
        {
            // True if all contexts are gone
            if (m_CurrentContext.Count == 0)
                return true;

            // Pop back to first context
            ContextInfo c = null;
            while (m_CurrentContext.Count > 0)
                c = m_CurrentContext.Pop();

            // Destroy the first context (clears subcontext and property stacks)
            c.Destroy();

            return false;
        }

        public void PushContext(SerializationContext context, int id)
        {
            // Write the context push property header
            WriteContextPushHeader(context, id);

            // Check the current context doesn't already contain this one
            if (m_CurrentContext.Count > 0)
            {
                var top = m_CurrentContext.Peek();
                if (!top.AddSubContext(id))
                    Debug.LogError("Pushed serialization context with ID that's already in use.");
            }

            // Track the context
            m_CurrentContext.Push(new ContextInfo(context, id));
        }

        public void PopContext(SerializationContext context)
        {
            if (m_CurrentContext.Count == 0)
            {
                Debug.LogError("All contexts have been popped. Cannot call pop again.");
                return;
            }

            // Write the context push property header
            WriteContextPopHeader(context);

            // Check the popped context matches
            var was = m_CurrentContext.Pop();
            if (was.contextType != context)
                Debug.LogError("Popped serialization context does not match active context. Your save game might be messed up.");

            // Clean up
            was.Destroy();
        }

        unsafe void WriteContextPushHeader(SerializationContext c, int id)
        {
            // Check the write position (and move to next buffer if required)
            //CheckWritePosition(k_HeaderSize);

            //fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            //{
            //    *(int*)ptr = k_HeaderSize; // Total size
            //    *(ptr + 4) = (byte)PropertyType.PushContext; // Property type (push context)
            //    *(ptr + 5) = (byte)c; // Context type
            //    *(int*)(ptr + 6) = id; // ID
            //}

            //m_BufferOffset += k_HeaderSize;

            m_Formatter.Serialize(m_MemoryStream, PropertyType.PushContext);
            m_Formatter.Serialize(m_MemoryStream, c);
            m_Formatter.Serialize(m_MemoryStream, id);
        }

        unsafe void WriteContextPopHeader(SerializationContext c)
        {
            //// Check the write position (and move to next buffer if required)
            //CheckWritePosition(k_HeaderSizeUnnamed);

            //fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            //{
            //    *(int*)ptr = k_HeaderSizeUnnamed; // Total size
            //    *(ptr + 4) = (byte)PropertyType.PopContext; // Property type (pop context)
            //    *(ptr + 5) = (byte)c; // Context type
            //}

            //m_BufferOffset += k_HeaderSizeUnnamed;

            m_Formatter.Serialize(m_MemoryStream, PropertyType.PopContext);
            m_Formatter.Serialize(m_MemoryStream, c);
        }

        #endregion


        #region WRITE HELPERS

        bool CheckPropertyKeyAvailable(int hash)
        {
            if (m_CurrentContext.Count != 0)
            {
                var top = m_CurrentContext.Peek();
                if (!top.AddProperty(hash))
                {
                    Debug.LogError("Attempting to write multiple properties with the same key.");
                    return false;
                }
            }
            return true;
        }
        
        void WriteHeader(PropertyType t, bool isArray, string key, bool isNullOrEmpty = false)
        {
            // Set flags
            PropertyFlags flags = (isArray) ? PropertyFlags.IsArray : PropertyFlags.None;
            if (isNullOrEmpty)
                flags |= PropertyFlags.NullOrEmpty;

            m_Formatter.Serialize(m_MemoryStream, t);
            m_Formatter.Serialize(m_MemoryStream, flags);
            m_Formatter.Serialize(m_MemoryStream, NeoSerializationUtilities.StringToHash(key));
        }

        void WriteHeader(PropertyType t, bool isArray, int hash, bool isNullOrEmpty = false)
        {
            // Set flags
            PropertyFlags flags = (isArray) ? PropertyFlags.IsArray : PropertyFlags.None;
            if (isNullOrEmpty)
                flags |= PropertyFlags.NullOrEmpty;

            m_Formatter.Serialize(m_MemoryStream, t);
            m_Formatter.Serialize(m_MemoryStream, flags);
            m_Formatter.Serialize(m_MemoryStream, hash);
        }

        void WriteHeader(PropertyType t, bool isArray, bool isNullOrEmpty = false)
        {
            // Property flags (unnamed as no key/hash provided)
            PropertyFlags flags = PropertyFlags.Unnammed;
            if (isArray)
                flags |= PropertyFlags.IsArray;
            if (isNullOrEmpty)
                flags |= PropertyFlags.NullOrEmpty;

            m_Formatter.Serialize(m_MemoryStream, t);
            m_Formatter.Serialize(m_MemoryStream, flags);
        }

        #endregion


        #region SERIALIZABLES

        public void WriteSerializable<T>(int hash, T s)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (s == null)
                {
                    WriteHeader(PropertyType.Serializable, false, hash, true);
                }
                else
                {
                    WriteHeader(PropertyType.Serializable, false, hash);
                    m_Formatter.Serialize(m_MemoryStream, s);
                }
            }
        }

        public void WriteSerializables<T>(int hash, ICollection<T> s)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (s == null)
                {
                    WriteHeader(PropertyType.Serializable, true, hash, true);
                }
                else
                {
                    WriteHeader(PropertyType.Serializable, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, s);
                }
            }
        }

        public void WriteSerializable<T>(string key, T s) { WriteSerializable(NeoSerializationUtilities.StringToHash(key), s); }
        public void WriteSerializables<T>(string key, ICollection<T> s) { WriteSerializables(NeoSerializationUtilities.StringToHash(key), s); }

        #endregion


        #region SERIALIZE VALUES (HASH)

        public void WriteValue(int hash, bool value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Bool, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, byte value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Byte, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, sbyte value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.SignedByte, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, char value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Char, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, short value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Short, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, ushort value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.UnsignedShort, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, int value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Int, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, uint value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.UnsignedInt, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, long value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                ;
                WriteHeader(PropertyType.Long, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, ulong value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.UnsignedLong, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, float value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Float, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, double value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Double, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, string value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.String, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, Vector2 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector2, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector2(value));
            }
        }

        public void WriteValue(int hash, Vector3 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector3, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector3(value));
            }
        }

        public void WriteValue(int hash, Vector4 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector4, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector4(value));
            }
        }

        public void WriteValue(int hash, Vector2Int value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector2Int, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector2Int(value));
            }
        }

        public void WriteValue(int hash, Vector3Int value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector3Int, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector3Int(value));
            }
        }

        public void WriteValue(int hash, Quaternion value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Quaternion, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector4(value));
            }
        }

        public void WriteValue(int hash, Color value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Color, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateVector4(value));
            }
        }

        public void WriteValue(int hash, Color32 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Color32, false, hash);
                m_Formatter.Serialize(m_MemoryStream, new IntermediateColor32(value));
            }
        }

        public void WriteValue(int hash, Guid value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Guid, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValue(int hash, DateTime value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.DateTime, false, hash);
                m_Formatter.Serialize(m_MemoryStream, value);
            }
        }

        public void WriteValues(int hash, ICollection<bool> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Bool, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Bool, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<bool>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<byte> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Byte, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Byte, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<byte>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<sbyte> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.SignedByte, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.SignedByte, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<sbyte>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<char> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Char, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Char, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<char>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<short> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Short, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Short, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<short>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<ushort> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.UnsignedShort, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.UnsignedShort, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<ushort>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<int> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Int, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Int, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<int>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<uint> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.UnsignedInt, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.UnsignedInt, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<uint>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<long> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Long, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Long, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<long>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<ulong> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.UnsignedLong, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.UnsignedLong, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<ulong>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<float> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Float, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Float, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<float>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<double> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Double, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Double, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<double>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<string> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.String, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.String, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<string>(value));
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector2> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector2, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Vector2, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector2>(value.Count);
                    foreach (var v in value)
                        intermediates.Add(new IntermediateVector2(v));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector3> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector3, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Vector3, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector3>(value.Count);
                    foreach (var v in value)
                        intermediates.Add(new IntermediateVector3(v));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector4> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector4, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Vector4, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector4>(value.Count);
                    foreach (var v in value)
                        intermediates.Add(new IntermediateVector4(v));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector2Int> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector2Int, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Vector2Int, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector2Int>(value.Count);
                    foreach (var v in value)
                        intermediates.Add(new IntermediateVector2Int(v));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector3Int> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector3Int, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Vector3Int, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector3Int>(value.Count);
                    foreach (var v in value)
                        intermediates.Add(new IntermediateVector3Int(v));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Quaternion> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Quaternion, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Quaternion, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector4>(value.Count);
                    foreach (var q in value)
                        intermediates.Add(new IntermediateVector4(q));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Color> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Color, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Color, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateVector4>(value.Count);
                    foreach (var c in value)
                        intermediates.Add(new IntermediateVector4(c));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Color32> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Color32, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Color32, true, hash);

                    // Build intermediates list
                    var intermediates = new List<IntermediateColor32>(value.Count);
                    foreach (var c in value)
                        intermediates.Add(new IntermediateColor32(c));

                    // Serialize
                    m_Formatter.Serialize(m_MemoryStream, intermediates);
                }
            }
        }

        public void WriteValues(int hash, ICollection<Guid> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Guid, true, hash, true);
                else
                {
                    WriteHeader(PropertyType.Guid, true, hash);
                    m_Formatter.Serialize(m_MemoryStream, new List<Guid>(value));
                }
            }
        }

        #endregion


        #region REFERENCES

        public bool WriteComponentReference<T>(int hash, T value, NeoSerializedGameObject pathFrom) where T : class
        {
            return NeoSerializationUtilities.WriteComponentReference(this, value, pathFrom, hash);
        }

        public bool WriteTransformReference(int hash, Transform value, NeoSerializedGameObject pathFrom)
        {
            return NeoSerializationUtilities.WriteTransformReference(this, value, pathFrom, hash);
        }

        public bool WriteGameObjectReference(int hash, GameObject value, NeoSerializedGameObject pathFrom)
        {
            return NeoSerializationUtilities.WriteGameObjectReference(this, value, pathFrom, hash);
        }

        public bool WriteNeoSerializedGameObjectReference(int hash, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom)
        {
            return NeoSerializationUtilities.WriteNeoSerializedGameObjectReference(this, value, pathFrom, hash);
        }

        #endregion


        #region SERIALIZE VALUES (STRING)

        public void WriteValue(string key, bool value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, byte value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, sbyte value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, char value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, short value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, ushort value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, int value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, uint value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, long value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, ulong value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, float value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, double value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, string value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Vector2 value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Vector3 value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Vector4 value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Vector2Int value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Vector3Int value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Quaternion value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Color value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Color32 value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, Guid value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValue(string key, DateTime value) { WriteValue(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<bool> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<byte> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<sbyte> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<char> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<short> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<ushort> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<int> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<uint> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<long> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<ulong> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<float> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<double> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<string> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Vector2> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Vector3> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Vector4> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Vector2Int> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Vector3Int> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Quaternion> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Color> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Color32> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public void WriteValues(string key, ICollection<Guid> value) { WriteValues(NeoSerializationUtilities.StringToHash(key), value); }
        public bool WriteComponentReference<T>(string key, T value, NeoSerializedGameObject pathFrom) where T : class { return WriteComponentReference(key, value, pathFrom); }
        public bool WriteTransformReference(string key, Transform value, NeoSerializedGameObject pathFrom) { return WriteTransformReference(key, value, pathFrom); }
        public bool WriteGameObjectReference(string key, GameObject value, NeoSerializedGameObject pathFrom) { return WriteGameObjectReference(key, value, pathFrom); }
        public bool WriteNeoSerializedGameObjectReference(string key, NeoSerializedGameObject value, NeoSerializedGameObject pathFrom) { return WriteNeoSerializedGameObjectReference(key, value, pathFrom); }

        #endregion
    }
}
