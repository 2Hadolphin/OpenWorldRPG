using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public class BinarySerializer : INeoSerializer
    {
        const int k_HeaderSize = 10; // length (int), type (byte), flags (byte), hash (int)
        const int k_HeaderSizeUnnamed = 6; // length (int), type (byte), flags (byte)
        const int k_BufferSize = 4194304;
        const int k_MemoryStreamBufferSize = 1024;

        enum SerializerState
        {
            Idle,
            CollectingData,
            WaitingForWrite,
            WritingData
        }
        
        private List<byte[]> m_Buffers = null;
        private int m_BufferIndex = 0;
        private int m_BufferOffset = 4;
        private int m_TotalLength = 0;
        private SerializerState m_State = SerializerState.Idle;
        private byte[] m_MemoryStreamBuffer = null;
        private MemoryStream m_MemoryStream = null;
        private BinaryFormatter m_Formatter = null;

        public bool isSerializing
        {
            get { return m_State != SerializerState.Idle; }
        }

        public int byteLength
        {
            get { return m_TotalLength; }
        }

        public BinarySerializer()
        {
            m_Buffers = new List<byte[]>(16);
            m_Buffers.Add(new byte[k_BufferSize]);
            m_MemoryStreamBuffer = new byte[k_MemoryStreamBufferSize];
            m_MemoryStream = new MemoryStream(m_MemoryStreamBuffer);
            m_Formatter = new BinaryFormatter();
            ClearBuffers();
        }

        void ClearBuffers()
        {
            for (int i = 0; i < m_Buffers.Count; ++i)
                Array.Clear(m_Buffers[i], 0, k_BufferSize);
            m_BufferIndex = 0;
            m_BufferOffset = 4;
            m_TotalLength = 0;
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
                        // Write the last index for the current buffer
                        WriteBufferLength();
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
                    var totalLengthBytes = BitConverter.GetBytes(m_TotalLength);
                    stream.Write(totalLengthBytes, 0, 4);
                    for (int i = 0; i <= m_BufferIndex; ++i)
                    {
                        int length = BitConverter.ToInt32(m_Buffers[i], 0);
                        stream.Write(m_Buffers[i], 4, length);
                    }
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
            CheckWritePosition(k_HeaderSize);

            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            {
                *(int*)ptr = k_HeaderSize; // Total size
                *(ptr + 4) = (byte)PropertyType.PushContext; // Property type (push context)
                *(ptr + 5) = (byte)c; // Context type
                *(int*)(ptr + 6) = id; // ID
            }

            m_BufferOffset += k_HeaderSize;
        }

        unsafe void WriteContextPopHeader(SerializationContext c)
        {
            // Check the write position (and move to next buffer if required)
            CheckWritePosition(k_HeaderSizeUnnamed);

            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            {
                *(int*)ptr = k_HeaderSizeUnnamed; // Total size
                *(ptr + 4) = (byte)PropertyType.PopContext; // Property type (pop context)
                *(ptr + 5) = (byte)c; // Context type
            }

            m_BufferOffset += k_HeaderSizeUnnamed;
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

        void CheckWritePosition(int size)
        {
            if (size > k_BufferSize - 4)
                Debug.LogError("Attempting to write data beyond buffer size");

            if (m_BufferOffset + size >= k_BufferSize)
            {
                // Write "next buffer" marker to remainder of current buffer
                byte nextBufferByte = (byte)PropertyType.NextBuffer;
                for (int i = m_BufferOffset; i < k_BufferSize; ++i)
                    m_Buffers[m_BufferIndex][i] = nextBufferByte;

                // Write the end point to the start of the buffer
                WriteBufferLength();

                // Set buffer index and offset
                m_BufferOffset = 4;
                ++m_BufferIndex;

                // Allocate new buffer if required
                if (m_BufferIndex >= m_Buffers.Count)
                    m_Buffers.Add(new byte[k_BufferSize]);
            }
        }

        unsafe void WriteBufferLength()
        {
            m_TotalLength += m_BufferOffset - 4;
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][0])
                *(int*)ptr = m_BufferOffset - 4;
        }

        void IncrementBufferOffset(int size)
        {
            m_BufferOffset += size;
            if (m_BufferOffset >= k_BufferSize)
            {
                m_BufferOffset = 4;
                ++m_BufferIndex;
                if (m_BufferIndex >= m_Buffers.Count)
                    m_Buffers.Add(new byte[k_BufferSize]);
            }
        }

        unsafe void WriteHeader(PropertyType t, bool isArray, int size, string key)
        {
            // Check the write position (and move to next buffer if required)
            CheckWritePosition(size + k_HeaderSize);

            // Set flags
            PropertyFlags flags = (isArray) ? PropertyFlags.IsArray : PropertyFlags.None;
            if (size == 0)
                flags |= PropertyFlags.NullOrEmpty;

            // Write data
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            {
                *(int*)ptr = size + k_HeaderSize; // Total size
                *(ptr + 4) = (byte)t; // Property type
                *(ptr + 5) = (byte)flags;  // Property flags
                *(int*)(ptr + 6) = NeoSerializationUtilities.StringToHash(key); // Property key hash
            }

            m_BufferOffset += k_HeaderSize;
        }

        unsafe void WriteHeader(PropertyType t, bool isArray, int size, int hash)
        {
            // Check the write position (and move to next buffer if required)
            CheckWritePosition(size + k_HeaderSize);

            // Set flags
            PropertyFlags flags = (isArray) ? PropertyFlags.IsArray : PropertyFlags.None;
            if (size == 0)
                flags |= PropertyFlags.NullOrEmpty;

            // Write data
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            {
                *(int*)ptr = size + k_HeaderSize; // Total size
                *(ptr + 4) = (byte)t; // Property type
                *(ptr + 5) = (byte)flags; // Property flags
                *(int*)(ptr + 6) = hash; // Property key hash
            }

            m_BufferOffset += k_HeaderSize;
        }

        unsafe void WriteHeader(PropertyType t, bool isArray, int size)
        {
            // Check the write position (and move to next buffer if required)
            CheckWritePosition(size + k_HeaderSizeUnnamed);

            // Property flags (unnamed as no key/hash provided)
            PropertyFlags flags = PropertyFlags.Unnammed;
            if (isArray)
                flags |= PropertyFlags.IsArray;
            if (size == 0)
                flags |= PropertyFlags.NullOrEmpty;

            // Write data
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            {
                *(int*)ptr = size + k_HeaderSizeUnnamed; // Total size
                *(ptr + 4) = (byte)t; // Property type
                *(ptr + 5) = (byte)flags; // Property flags
            }

            m_BufferOffset += k_HeaderSizeUnnamed;
        }
        
        #endregion

        #region SERIALIZABLES

        unsafe void WriteSerializableBytesInternal (int count)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
            {
                // Loop through bytes in stream and write to buffer
                byte* itr = ptr;
                for (int i = 0; i < count; ++i)
                    *(itr++) = m_MemoryStreamBuffer[i];
            }
            m_BufferOffset += count;
        }

        public void WriteSerializable<T>(int hash, T s)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (s == null)
                {
                    WriteHeader(PropertyType.Serializable, false, 0, hash);
                }
                else
                {
                    // Write serializable to memory stream
                    m_Formatter.Serialize(m_MemoryStream, s);
                    int size = (int)m_MemoryStream.Position;

                    // Write memory stream to proper buffer
                    WriteHeader(PropertyType.Serializable, false, size, hash);
                    WriteSerializableBytesInternal(size);

                    // Reset memory stream
                    m_MemoryStream.Position = 0;
                }
            }
        }

        public void WriteSerializables<T>(int hash, ICollection<T> s)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (s == null)
                {
                    WriteHeader(PropertyType.Serializable, true, 0, hash);
                }
                else
                {
                    int count = s.Count;
                    if (count == 0)
                    {
                        WriteHeader(PropertyType.Serializable, true, 4, hash);
                        WriteValueInternal(0);
                    }
                    else
                    {
                        int[] endPoints = new int[s.Count];
                        int i = 0;

                        foreach (var serializable in s)
                        {
                            // Write serializable to memory stream
                            if (serializable != null)
                                m_Formatter.Serialize(m_MemoryStream, serializable);

                            endPoints[i] = (int)m_MemoryStream.Position;
                            ++i;
                        }

                        // Write header
                        int byteCount = endPoints[count - 1];
                        WriteHeader(PropertyType.Serializable, true, byteCount + 4 + (4 * count), hash);

                        // Write endpoints
                        WriteValueInternal(count);
                        for (i = 0; i < count; ++i)
                            WriteValueInternal(endPoints[i]);

                        // Write serialized data
                        WriteSerializableBytesInternal(byteCount);

                        //// Reset memory stream
                        m_MemoryStream.Position = 0;
                    }
                }
            }
        }

        public void WriteSerializable<T>(string key, T s) { WriteSerializable(NeoSerializationUtilities.StringToHash(key), s); }
        public void WriteSerializables<T>(string key, ICollection<T> s) { WriteSerializables(NeoSerializationUtilities.StringToHash(key), s); }

        #endregion

        #region BINARY WRITERS

        unsafe void WriteValueInternal(bool value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(bool*)ptr = value;
            m_BufferOffset += 1;
        }

        unsafe void WriteValueInternal(byte value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *ptr = value;
            m_BufferOffset += 1;
        }

        unsafe void WriteValueInternal(sbyte value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(sbyte*)ptr = value;
            m_BufferOffset += 1;
        }

        unsafe void WriteValueInternal(char value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(char*)ptr = value;
            m_BufferOffset += 2;
        }

        unsafe void WriteValueInternal(short value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(short*)ptr = value;
            m_BufferOffset += 2;
        }

        unsafe void WriteValueInternal(ushort value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(ushort*)ptr = value;
            m_BufferOffset += 2;
        }

        unsafe void WriteValueInternal(int value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(int*)ptr = value;
            m_BufferOffset += 4;
        }

        unsafe void WriteValueInternal(uint value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(uint*)ptr = value;
            m_BufferOffset += 4;
        }

        unsafe void WriteValueInternal(long value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(long*)ptr = value;
            m_BufferOffset += 8;
        }

        unsafe void WriteValueInternal(ulong value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(ulong*)ptr = value;
            m_BufferOffset += 8;
        }

        unsafe void WriteValueInternal(float value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(float*)ptr = value;
            m_BufferOffset += 4;
        }

        unsafe void WriteValueInternal(double value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(double*)ptr = value;
            m_BufferOffset += 8;
        }

        unsafe void WriteValueInternal(Guid value)
        {
            fixed (byte* ptr = &m_Buffers[m_BufferIndex][m_BufferOffset])
                *(Guid*)ptr = value;
            m_BufferOffset += 16;
        }

        #endregion

        #region SERIALIZE VALUES (HASH)

        public void WriteValue(int hash, bool value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Bool, false, 1, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, byte value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Byte, false, 1, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, sbyte value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.SignedByte, false, 1, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, char value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Char, false, 2, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, short value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Short, false, 2, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, ushort value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.UnsignedShort, false, 2, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, int value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Int, false, 4, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, uint value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.UnsignedInt, false, 4, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, long value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {;
                WriteHeader(PropertyType.Long, false, 8, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, ulong value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.UnsignedLong, false, 8, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, float value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Float, false, 4, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, double value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Double, false, 8, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, string value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                {
                    WriteHeader(PropertyType.String, false, 0, hash);
                    WriteValueInternal(0);
                }
                else
                {
                    int size = 4 + (value.Length * 2);
                    WriteHeader(PropertyType.String, false, size, hash);
                    WriteValueInternal(value.Length);
                    foreach (var c in value)
                        WriteValueInternal(c);
                }
            }
        }

        public void WriteValue(int hash, Vector2 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector2, false, 8, hash);
                WriteValueInternal(value.x);
                WriteValueInternal(value.y);
            }
        }

        public void WriteValue(int hash, Vector3 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector3, false, 12, hash);
                WriteValueInternal(value.x);
                WriteValueInternal(value.y);
                WriteValueInternal(value.z);
            }
        }

        public void WriteValue(int hash, Vector4 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector4, false, 16, hash);
                WriteValueInternal(value.x);
                WriteValueInternal(value.y);
                WriteValueInternal(value.z);
                WriteValueInternal(value.w);
            }
        }

        public void WriteValue(int hash, Vector2Int value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector2Int, false, 8, hash);
                WriteValueInternal(value.x);
                WriteValueInternal(value.y);
            }
        }

        public void WriteValue(int hash, Vector3Int value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Vector3Int, false, 12, hash);
                WriteValueInternal(value.x);
                WriteValueInternal(value.y);
                WriteValueInternal(value.z);
            }
        }

        public void WriteValue(int hash, Quaternion value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Quaternion, false, 16, hash);
                WriteValueInternal(value.x);
                WriteValueInternal(value.y);
                WriteValueInternal(value.z);
                WriteValueInternal(value.w);
            }
        }

        public void WriteValue(int hash, Color value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Color, false, 16, hash);
                WriteValueInternal(value.r);
                WriteValueInternal(value.g);
                WriteValueInternal(value.b);
                WriteValueInternal(value.a);
            }
        }

        public void WriteValue(int hash, Color32 value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Color32, false, 4, hash);
                WriteValueInternal(value.r);
                WriteValueInternal(value.g);
                WriteValueInternal(value.b);
                WriteValueInternal(value.a);
            }
        }

        public void WriteValue(int hash, Guid value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.Guid, false, 16, hash);
                WriteValueInternal(value);
            }
        }

        public void WriteValue(int hash, DateTime value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                WriteHeader(PropertyType.DateTime, false, 8, hash);
                WriteValueInternal(value.ToBinary());
            }
        }
        
        public void WriteValues(int hash, ICollection<bool> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Bool, true, 0, hash);
                else
                {
                    int size = 4 + value.Count;
                    WriteHeader(PropertyType.Bool, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<byte> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Byte, true, 0, hash);
                else
                {
                    int size = 4 + value.Count;
                    WriteHeader(PropertyType.Byte, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<sbyte> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.SignedByte, true, 0, hash);
                else
                {
                    int size = 4 + value.Count;
                    WriteHeader(PropertyType.SignedByte, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<char> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Char, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 2);
                    WriteHeader(PropertyType.Char, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<short> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Short, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 2);
                    WriteHeader(PropertyType.Short, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<ushort> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.UnsignedShort, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 2);
                    WriteHeader(PropertyType.UnsignedShort, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<int> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Int, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 4);
                    WriteHeader(PropertyType.Int, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<uint> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.UnsignedInt, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 4);
                    WriteHeader(PropertyType.UnsignedInt, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<long> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Long, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 8);
                    WriteHeader(PropertyType.Long, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<ulong> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.UnsignedLong, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 8);
                    WriteHeader(PropertyType.UnsignedLong, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<float> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Float, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 4);
                    WriteHeader(PropertyType.Float, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<double> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Double, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 8);
                    WriteHeader(PropertyType.Double, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
                }
            }
        }

        public void WriteValues(int hash, ICollection<string> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.String, true, 0, hash);
                else
                {
                    if (value.Count == 0)
                    {
                        WriteHeader(PropertyType.String, true, 4, hash);
                        WriteValueInternal(0);
                    }
                    else
                    {
                        // Get total size
                        int size = 4 + (value.Count * 4);
                        foreach (var s in value)
                        {
                            if (s != null)
                                size += s.Length * 2;
                        }

                        // Write header
                        WriteHeader(PropertyType.String, true, size, hash);

                        // Write strings
                        WriteValueInternal(value.Count);
                        foreach (var s in value)
                        {
                            if (s != null)
                            {
                                WriteValueInternal(s.Length);
                                foreach (var c in s)
                                    WriteValueInternal(c);
                            }
                            else
                                WriteValueInternal(0);
                        }
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector2> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector2, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 8);
                    WriteHeader(PropertyType.Vector2, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.x);
                        WriteValueInternal(v.y);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector3> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector3, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 12);
                    WriteHeader(PropertyType.Vector3, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.x);
                        WriteValueInternal(v.y);
                        WriteValueInternal(v.z);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector4> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector4, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 16);
                    WriteHeader(PropertyType.Vector4, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.x);
                        WriteValueInternal(v.y);
                        WriteValueInternal(v.z);
                        WriteValueInternal(v.w);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector2Int> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector2Int, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 8);
                    WriteHeader(PropertyType.Vector2Int, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.x);
                        WriteValueInternal(v.y);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Vector3Int> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Vector3Int, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 12);
                    WriteHeader(PropertyType.Vector3Int, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.x);
                        WriteValueInternal(v.y);
                        WriteValueInternal(v.z);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Quaternion> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Quaternion, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 16);
                    WriteHeader(PropertyType.Quaternion, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.x);
                        WriteValueInternal(v.y);
                        WriteValueInternal(v.z);
                        WriteValueInternal(v.w);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Color> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Color, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 16);
                    WriteHeader(PropertyType.Color, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.r);
                        WriteValueInternal(v.g);
                        WriteValueInternal(v.b);
                        WriteValueInternal(v.a);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Color32> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Color32, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 4);
                    WriteHeader(PropertyType.Color32, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                    {
                        WriteValueInternal(v.r);
                        WriteValueInternal(v.g);
                        WriteValueInternal(v.b);
                        WriteValueInternal(v.a);
                    }
                }
            }
        }

        public void WriteValues(int hash, ICollection<Guid> value)
        {
            if (CheckPropertyKeyAvailable(hash))
            {
                if (value == null)
                    WriteHeader(PropertyType.Guid, true, 0, hash);
                else
                {
                    int size = 4 + (value.Count * 16);
                    WriteHeader(PropertyType.Guid, true, size, hash);
                    WriteValueInternal(value.Count);
                    foreach (var v in value)
                        WriteValueInternal(v);
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
