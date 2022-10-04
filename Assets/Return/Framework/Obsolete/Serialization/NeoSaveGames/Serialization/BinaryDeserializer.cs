using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public class BinaryDeserializer : INeoDeserializer
    {
        const int k_HeaderSize = 10; // length (int), type (byte), flags (byte), hash (int)
        const int k_HeaderSizeUnnamed = 6; // length (int), type (byte), flags (byte)

        private byte[] m_Data = null;
        private Property m_LastNamedProperty = null;
        private StringBuilder m_StringBuilder = new StringBuilder(256);
        private BinaryFormatter m_BinaryFormatter = new BinaryFormatter();
        private Stack<Context> m_ContextStack = new Stack<Context>();

        public bool isDeserializing
        {
            get { return m_Data != null; }
        }

        public Context currentContext
        {
            get { return m_ContextStack.Peek(); }
        }

        public class Context
        {
            private Dictionary<int, Property> m_Properties = new Dictionary<int, Property>();
            private Dictionary<int, Context> m_SubContexts = new Dictionary<int, Context>();

            public int id { get; private set; }
            public SerializationContext contextType { get; private set; }

            public Dictionary<int, Property> properties
            {
                get { return m_Properties; }
            }

            public Dictionary<int, Context> subContexts
            {
                get { return m_SubContexts; }
            }

            public Context(SerializationContext t, int key)
            {
                id = key;
                contextType = t;
            }
        }

        public class Property
        {
            private List<Property> m_Children = new List<Property>();

            public PropertyType propertyType { get; private set; }
            public PropertyFlags propertyFlags { get; private set; }
            public int startIndex { get; private set; }
            public int length { get; private set; }

            public List<Property> children
            {
                get { return m_Children; }
            }

            public Property(PropertyType t, PropertyFlags f, int i, int l)
            {
                propertyType = t;
                propertyFlags = f;
                startIndex = i;
                length = l;
            }

            public bool isArray
            {
                get { return (propertyFlags & PropertyFlags.IsArray) != PropertyFlags.None;  }
            }

            public bool isNullOrEmpty
            {
                get { return (propertyFlags & PropertyFlags.NullOrEmpty) != PropertyFlags.None; }
            }
        }

        public bool ReadFromStream(Stream stream)
        {
            try
            {
                // Get the length of the data
                var totalLengthBytes = new byte[4];
                stream.Read(totalLengthBytes, 0, 4);
                int totalLength = BitConverter.ToInt32(totalLengthBytes, 0);
                
                // Read data
                m_Data = new byte[totalLength];
                int totalRead = stream.Read(m_Data, 0, totalLength);
                if (totalRead != totalLength)
                {
                    Debug.LogError(string.Format("Save data length mismatch. Could not read from stream. Expected: {0}, Read {1}", totalLength, totalRead));
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to read data from stream due to error: " + e.Message);
                return false;
            }

            return true;
        }
        
        public void BeginDeserialization()
        {
            var rootContext = new Context(SerializationContext.Root, 0);
            m_ContextStack.Push(rootContext);

            // Iterate through properties
            int offset = 0;
            while (offset != -1)
                offset = ReadElement(m_Data, offset);

            var closingContext = m_ContextStack.Peek();
            if (closingContext != rootContext)
                Debug.LogError("Unbalanced push/pop for serialization contexts. Deserialization did not end on the root context.");

            //Debug.Log("Built deserialization map");
        }

        int ReadElement(byte[] data, int offset)
        {
            if (offset >= data.Length)
                return -1;

            // Get total length
            int elementSize = BitConverter.ToInt32(data, offset);
            if (elementSize <= 0)
            {
                Debug.LogError("Read save game property length cannot be negative");
                return -1;
            }
            if (offset + elementSize > data.Length)
            {
                Debug.LogError("Read save game property length that does not match file length");
                return -1;
            }

            // Get the position of the next element
            int next = offset + elementSize;

            // Get property type
            offset += 4;
            var t = (PropertyType)data[offset];
            if (t == PropertyType.EndOfData)
                return -1;
            
            switch (t)
            {
                case PropertyType.PushContext:
                    {
                        // Read the relevant context data
                        var contextType = (SerializationContext)data[++offset];
                        int key = BitConverter.ToInt32(data, ++offset);

                        // Create the new context
                        var newContext = new Context(contextType, key);

                        // Add new context to old
                        var currentContext = m_ContextStack.Peek();
                        if (currentContext.subContexts.ContainsKey(key))
                            Debug.LogError("Context key collision: " + key);
                        else
                            currentContext.subContexts.Add(key, newContext);

                        // Push new context
                        m_ContextStack.Push(newContext);
                    }
                    break;
                case PropertyType.PopContext:
                    {
                        var contextType = (SerializationContext)data[++offset];

                        var popped = m_ContextStack.Pop();
                        if (popped.contextType != contextType)
                            Debug.LogError("Popped serialization context does not match active context. Your save game might be messed up.");
                    }
                    break;
                default:
                    {
                        // Get property flags
                        var flags = (PropertyFlags)data[++offset];

                        // Check if array
                        bool isUnnamed = (flags & PropertyFlags.Unnammed) != PropertyFlags.None;

                        // Add the property (to the context or parent depending if named)
                        if (isUnnamed)
                        {
                            if (m_LastNamedProperty == null)
                                Debug.LogError("Attempting to read child property when no parent set");
                            else
                            {
                                // Add as a child property of the last named property
                                m_LastNamedProperty.children.Add(
                                    new Property(t, flags, ++offset, elementSize - k_HeaderSizeUnnamed)
                                    );
                            }
                        }
                        else
                        {
                            // Get the hash
                            int hash = BitConverter.ToInt32(data, ++offset);

                            // Create the property
                            m_LastNamedProperty = new Property(t, flags, offset + 4, elementSize - k_HeaderSize);

                            // Add to dictionary
                            var context = m_ContextStack.Peek();
                            if (context.properties.ContainsKey(hash))
                                Debug.LogError(string.Format("Key collision: {0}, type: {1}, in context: {2}", hash, t, context.id));
                            else
                                context.properties.Add(hash, m_LastNamedProperty);
                        }
                    }
                    break;
            }

            return next;
        }

        public void EndDeserialization()
        {
            m_LastNamedProperty = null;
            m_Data = null;
            m_ContextStack.Clear();
        }
        
        public bool PushContext(SerializationContext context, int id)
        {
            Context c;
            if (m_ContextStack.Peek().subContexts.TryGetValue(id, out c))
            {
                //Debug.Log(string.Format("Pushing context: {0}, id: {1}", context, id));
                m_ContextStack.Push(c);
                return true;
            }
            else
            {
                //Debug.Log(string.Format("Failed to find context: {0}, id: {1}", context, id));
                return false;
            }
        }

        public void PopContext(SerializationContext context, int id)
        {
            var activeContext = m_ContextStack.Pop();
            var activeContextType = activeContext.contextType;
            if (activeContextType != context)
                Debug.LogError(string.Format("Popped serialization context does not match active context. Popped: {0} ({1}), Active: {2} ({3})", context, id, activeContextType, activeContext.id));
        }

        #region READ HELPERS

        delegate T BinaryValueReader<T>(int index);

        bool TryFetchProperty(int hash, PropertyType t, bool isArray)
        {
            if (m_ContextStack.Peek().properties.TryGetValue(hash, out m_LastNamedProperty) &&
                m_LastNamedProperty.propertyType == t &&
                m_LastNamedProperty.isArray == isArray)
                return true;
            else
                return false;
        }

        void ReadList<T>(List<T> output, int valueSize, BinaryValueReader<T> valueReader)
        {
            int length = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);

            output.Clear();
            if (output.Capacity < length)
                output.Capacity = length;

            int index = m_LastNamedProperty.startIndex + 4;
            for (int i = 0; i < length; ++i)
            {
                output.Add(valueReader(index));
                index += valueSize;
            }
        }
        
        void ReadArray<T>(out T[] output, int valueSize, BinaryValueReader<T> valueReader)
        {
            int length = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);

            output = new T[length];
            int index = m_LastNamedProperty.startIndex + 4;
            for (int i = 0; i < length; ++i)
            {
                output[i] = valueReader(index);
                index += valueSize;
            }
        }

        Guid ReadGuid(int startIndex)
        {
            int byteIndex = startIndex + 8;
            return new Guid(
                     BitConverter.ToUInt32(m_Data, startIndex),
                     BitConverter.ToUInt16(m_Data, startIndex + 4),
                     BitConverter.ToUInt16(m_Data, startIndex + 6),
                     m_Data[byteIndex++],
                     m_Data[byteIndex++],
                     m_Data[byteIndex++],
                     m_Data[byteIndex++],
                     m_Data[byteIndex++],
                     m_Data[byteIndex++],
                     m_Data[byteIndex++],
                     m_Data[byteIndex++]
                     );
        }

        #endregion

        #region SINGLE VALUE READERS

        public bool TryReadValue(int hash, out bool output, bool defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Bool, false))
            {
                output = BitConverter.ToBoolean(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out byte output, byte defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Byte, false))
            {
                output = m_Data[m_LastNamedProperty.startIndex];
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out sbyte output, sbyte defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.SignedByte, false))
            {
                output = (sbyte)m_Data[m_LastNamedProperty.startIndex];
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out char output, char defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Char, false))
            {
                output = BitConverter.ToChar(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out short output, short defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Short, false))
            {
                output = BitConverter.ToInt16(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out ushort output, ushort defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedShort, false))
            {
                output = BitConverter.ToUInt16(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out int output, int defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Int, false))
            {
                output = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out uint output, uint defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedInt, false))
            {
                output = BitConverter.ToUInt32(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out long output, long defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Long, false))
            {
                output = BitConverter.ToInt64(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out ulong output, ulong defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedLong, false))
            {
                output = BitConverter.ToUInt64(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out float output, float defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Float, false))
            {
                output = BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out double output, double defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Double, false))
            {
                output = BitConverter.ToDouble(m_Data, m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out string output, string defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.String, false))
            {
                int length = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);
                int index = m_LastNamedProperty.startIndex + 4;
                for (int i = 0; i < length; ++i)
                {
                    m_StringBuilder.Append(BitConverter.ToChar(m_Data, index));
                    index += 2;
                }
                output = m_StringBuilder.ToString();
                m_StringBuilder.Length = 0;
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Vector2 output, Vector2 defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2, false))
            {
                output = new Vector2(
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 4)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Vector3 output, Vector3 defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3, false))
            {
                output = new Vector3(
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 4),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 8)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Vector4 output, Vector4 defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Vector4, false))
            {
                output = new Vector4(
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 4),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 8),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 12)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Vector2Int output, Vector2Int defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2Int, false))
            {
                output = new Vector2Int(
                    BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex + 4)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Vector3Int output, Vector3Int defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3Int, false))
            {
                output = new Vector3Int(
                    BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex + 4),
                    BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex + 8)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Quaternion output, Quaternion defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Quaternion, false))
            {
                output = new Quaternion(
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 4),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 8),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 12)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Color output, Color defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Color, false))
            {
                output = new Color(
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 4),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 8),
                    BitConverter.ToSingle(m_Data, m_LastNamedProperty.startIndex + 12)
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Color32 output, Color32 defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Color32, false))
            {
                int index = m_LastNamedProperty.startIndex;
                output = new Color32(
                    m_Data[index++],
                    m_Data[index++],
                    m_Data[index++],
                    m_Data[index]
                    );
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadValue(int hash, out Guid output)
        {
            if (TryFetchProperty(hash, PropertyType.Guid, false))
            {
                output = ReadGuid(m_LastNamedProperty.startIndex);
                return true;
            }
            else
            {
                output = new Guid();
                return false;
            }
        }

        public bool TryReadValue(int hash, out DateTime output, DateTime defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.DateTime, false))
            {
                output = DateTime.FromBinary(BitConverter.ToInt64(m_Data, m_LastNamedProperty.startIndex));
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        #endregion

        #region ARRAY READERS

        public bool TryReadValues(int hash, out bool[] output, bool[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Bool, true))
            {
                ReadArray(out output, 1, (index) => { return BitConverter.ToBoolean(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out byte[] output, byte[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Byte, true))
            {
                ReadArray(out output, 1, (index) => { return m_Data[index]; });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out sbyte[] output, sbyte[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.SignedByte, true))
            {
                ReadArray(out output, 1, (index) => { return (sbyte)m_Data[index]; });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out char[] output, char[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Char, true))
            {
                ReadArray(out output, 2, (index) => { return BitConverter.ToChar(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out short[] output, short[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Short, true))
            {
                ReadArray(out output, 2, (index) => { return BitConverter.ToInt16(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out ushort[] output, ushort[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedShort, true))
            {
                ReadArray(out output, 2, (index) => { return BitConverter.ToUInt16(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out int[] output, int[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Int, true))
            {
                ReadArray(out output, 4, (index) => { return BitConverter.ToInt32(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out uint[] output, uint[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedInt, true))
            {
                ReadArray(out output, 4, (index) => { return BitConverter.ToUInt32(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out long[] output, long[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Long, true))
            {
                ReadArray(out output, 8, (index) => { return BitConverter.ToInt64(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out ulong[] output, ulong[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedLong, true))
            {
                ReadArray(out output, 8, (index) => { return BitConverter.ToUInt64(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out float[] output, float[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Float, true))
            {
                ReadArray(out output, 4, (index) => { return BitConverter.ToSingle(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out double[] output, double[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Double, true))
            {
                ReadArray(out output, 8, (index) => { return BitConverter.ToDouble(m_Data, index); });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out string[] output, string[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.String, true))
            {
                // Create string array
                int numStrings = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);
                output = new string[numStrings];

                // Read strings
                int index = m_LastNamedProperty.startIndex + 4;
                for (int i = 0; i < numStrings; ++i)
                {
                    int length = BitConverter.ToInt32(m_Data, index);
                    index += 4;
                    for (int j = 0; j < length; ++j)
                    {
                        m_StringBuilder.Append(BitConverter.ToChar(m_Data, index));
                        index += 2;
                    }
                    output[i] = m_StringBuilder.ToString();
                    m_StringBuilder.Length = 0;
                }
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Vector2[] output, Vector2[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2, true))
            {
                ReadArray(out output, 8, (index) => 
                {
                    return new Vector2(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index + 4)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Vector3[] output, Vector3[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3, true))
            {
                ReadArray(out output, 12, (index) =>
                {
                    return new Vector3(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index + 4),
                        BitConverter.ToSingle(m_Data, index + 8)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Vector4[] output, Vector4[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Vector4, true))
            {
                ReadArray(out output, 16, (index) =>
                {
                    return new Vector4(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index + 4),
                        BitConverter.ToSingle(m_Data, index + 8),
                        BitConverter.ToSingle(m_Data, index + 12)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Vector2Int[] output, Vector2Int[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2Int, true))
            {
                ReadArray(out output, 8, (index) =>
                {
                    return new Vector2Int(
                        BitConverter.ToInt32(m_Data, index),
                        BitConverter.ToInt32(m_Data, index + 4)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Vector3Int[] output, Vector3Int[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3Int, true))
            {
                ReadArray(out output, 12, (index) =>
                {
                    return new Vector3Int(
                        BitConverter.ToInt32(m_Data, index),
                        BitConverter.ToInt32(m_Data, index + 4),
                        BitConverter.ToInt32(m_Data, index + 8)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Quaternion[] output, Quaternion[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Quaternion, true))
            {
                ReadArray(out output, 16, (index) =>
                {
                    return new Quaternion(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index + 4),
                        BitConverter.ToSingle(m_Data, index + 8),
                        BitConverter.ToSingle(m_Data, index + 12)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Color[] output, Color[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Color, true))
            {
                ReadArray(out output, 16, (index) =>
                {
                    return new Color(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index + 4),
                        BitConverter.ToSingle(m_Data, index + 8),
                        BitConverter.ToSingle(m_Data, index + 12)
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }


        public bool TryReadValues(int hash, out Color32[] output, Color32[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Color32, true))
            {
                ReadArray(out output, 4, (index) =>
                {
                    return new Color32(
                        m_Data[index],
                        m_Data[index + 1],
                        m_Data[index + 2],
                        m_Data[index + 3]
                        );
                });
                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadValues(int hash, out Guid[] output)
        {
            if (TryFetchProperty(hash, PropertyType.Guid, true))
            {
                ReadArray(out output, 16, (index) => { return ReadGuid(index); });
                return true;
            }
            else
            {
                output = new Guid[0];
                return false;
            }
        }

        #endregion

        #region LIST READERS

        public bool TryReadValues(int hash, List<bool> output)
        {
            if (TryFetchProperty(hash, PropertyType.Bool, true))
            {
                ReadList(output, 1, (index) => { return BitConverter.ToBoolean(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<byte> output)
        {
            if (TryFetchProperty(hash, PropertyType.Byte, true))
            {
                ReadList(output, 1, (index) => { return m_Data[index]; });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<sbyte> output)
        {
            if (TryFetchProperty(hash, PropertyType.SignedByte, true))
            {
                ReadList(output, 1, (index) => { return (sbyte)m_Data[index]; });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<char> output)
        {
            if (TryFetchProperty(hash, PropertyType.Char, true))
            {
                ReadList(output, 2, (index) => { return BitConverter.ToChar(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<short> output)
        {
            if (TryFetchProperty(hash, PropertyType.Short, true))
            {
                ReadList(output, 2, (index) => { return BitConverter.ToInt16(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<ushort> output)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedShort, true))
            {
                ReadList(output, 2, (index) => { return BitConverter.ToUInt16(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<int> output)
        {
            if (TryFetchProperty(hash, PropertyType.Int, true))
            {
                ReadList(output, 4, (index) => { return BitConverter.ToInt32(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<uint> output)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedInt, true))
            {
                ReadList(output, 4, (index) => { return BitConverter.ToUInt32(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<long> output)
        {
            if (TryFetchProperty(hash, PropertyType.Long, true))
            {
                ReadList(output, 8, (index) => { return BitConverter.ToInt64(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<ulong> output)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedLong, true))
            {
                ReadList(output, 8, (index) => { return BitConverter.ToUInt64(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<float> output)
        {
            if (TryFetchProperty(hash, PropertyType.Float, true))
            {
                ReadList(output, 4, (index) => { return BitConverter.ToSingle(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<double> output)
        {
            if (TryFetchProperty(hash, PropertyType.Double, true))
            {
                ReadList(output, 8, (index) => { return BitConverter.ToDouble(m_Data, index); });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<string> output)
        {
            if (TryFetchProperty(hash, PropertyType.String, true))
            {
                // Prep string list
                int numStrings = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);
                output.Clear();
                if (output.Capacity < numStrings)
                    output.Capacity = numStrings;

                // Read strings
                int index = m_LastNamedProperty.startIndex + 4;
                for (int i = 0; i < numStrings; ++i)
                {
                    int length = BitConverter.ToInt32(m_Data, index);
                    index += 4;
                    for (int j = 0; j < length; ++j)
                    {
                        m_StringBuilder.Append(BitConverter.ToChar(m_Data, index));
                        index += 2;
                    }
                    output.Add(m_StringBuilder.ToString());
                    m_StringBuilder.Length = 0;
                }
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector2> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2, true))
            {
                ReadList(output, 8, (index) =>
                {
                    return new Vector2(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector3> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3, true))
            {
                ReadList(output, 12, (index) =>
                {
                    return new Vector3(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector4> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector4, true))
            {
                ReadList(output, 16, (index) =>
                {
                    return new Vector4(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector2Int> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2Int, true))
            {
                ReadList(output, 8, (index) =>
                {
                    return new Vector2Int(
                        BitConverter.ToInt32(m_Data, index),
                        BitConverter.ToInt32(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector3Int> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3Int, true))
            {
                ReadList(output, 12, (index) =>
                {
                    return new Vector3Int(
                        BitConverter.ToInt32(m_Data, index),
                        BitConverter.ToInt32(m_Data, index),
                        BitConverter.ToInt32(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Quaternion> output)
        {
            if (TryFetchProperty(hash, PropertyType.Quaternion, true))
            {
                ReadList(output, 16, (index) =>
                {
                    return new Quaternion(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Color> output)
        {
            if (TryFetchProperty(hash, PropertyType.Color, true))
            {
                ReadList(output, 16, (index) =>
                {
                    return new Color(
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index),
                        BitConverter.ToSingle(m_Data, index)
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Color32> output)
        {
            if (TryFetchProperty(hash, PropertyType.Color32, true))
            {
                ReadList(output, 4, (index) =>
                {
                    return new Color32(
                        m_Data[index],
                        m_Data[index + 1],
                        m_Data[index + 2],
                        m_Data[index + 3]
                    );
                });
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Guid> output)
        {
            if (TryFetchProperty(hash, PropertyType.Guid, true))
            {
                ReadList(output, 16, (index) => { return ReadGuid(index); });
                return true;
            }
            else
                return false;
        }

        #endregion

        #region SERIALIZABLES

        public bool TryReadSerializable<T>(int hash, out T output, T defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Serializable, false))
            {
                // Check for null (length 0)
                if (m_LastNamedProperty.length == 0)
                    output = default(T);
                else
                {
                    var memStream = new MemoryStream(m_Data, m_LastNamedProperty.startIndex, m_LastNamedProperty.length);
                    output = (T)m_BinaryFormatter.Deserialize(memStream);
                }
                return true;
            }
            else
            {
                output = defaultValue;
                return false;
            }
        }

        public bool TryReadSerializables<T>(int hash, out T[] output, T[] defaultValues)
        {
            if (TryFetchProperty(hash, PropertyType.Serializable, true))
            {
                int length = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);

                output = new T[length];
                for (int i = 0; i < length; ++i)
                {
                    var childProp = m_LastNamedProperty.children[i];

                    // Check for null (length 0)
                    if (childProp.length == 0)
                        output[i] = default(T);
                    else
                    {
                        var memStream = new MemoryStream(m_Data, childProp.startIndex, childProp.length);
                        output[i] = (T)m_BinaryFormatter.Deserialize(memStream);
                    }
                }

                return true;
            }
            else
            {
                output = defaultValues;
                return false;
            }
        }

        public bool TryReadSerializables<T>(int hash, List<T> output)
        {
            if (TryFetchProperty(hash, PropertyType.Serializable, true))
            {
                int length = BitConverter.ToInt32(m_Data, m_LastNamedProperty.startIndex);

                output.Clear();
                if (output.Capacity < length)
                    output.Capacity = length;

                for (int i = 0; i < length; ++i)
                {
                    var childProp = m_LastNamedProperty.children[i];

                    // Check for null (length 0)
                    if (childProp.length == 0)
                        output.Add(default(T));
                    else
                    {
                        var memStream = new MemoryStream(m_Data, childProp.startIndex, childProp.length);
                        output.Add((T)m_BinaryFormatter.Deserialize(memStream));
                    }
                }

                return true;
            }
            else
                return false;
        }

        #endregion

        #region REFERENCES

        public bool TryReadComponentReference<T>(int hash, out T output, NeoSerializedGameObject pathFrom) where T : class
        {
            return NeoSerializationUtilities.TryReadComponentReference(this, out output, pathFrom, hash);
        }

        public bool TryReadTransformReference(int hash, out Transform output, NeoSerializedGameObject pathFrom)
        {
            return NeoSerializationUtilities.TryReadTransformReference(this, out output, pathFrom, hash);
        }

        public bool TryReadGameObjectReference(int hash, out GameObject output, NeoSerializedGameObject pathFrom)
        {
            return NeoSerializationUtilities.TryReadGameObjectReference(this, out output, pathFrom, hash);
        }

        public bool TryReadNeoSerializedGameObjectReference(int hash, out NeoSerializedGameObject output, NeoSerializedGameObject pathFrom)
        {
            return NeoSerializationUtilities.TryReadNeoSerializedGameObjectReference(this, out output, pathFrom, hash);
        }

        #endregion

        #region STRING KEYS

        public bool TryReadValue(string key, out bool output, bool defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out byte output, byte defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out sbyte output, sbyte defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out char output, char defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out short output, short defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out ushort output, ushort defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out int output, int defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out uint output, uint defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out long output, long defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out ulong output, ulong defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out float output, float defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out double output, double defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Vector2 output, Vector2 defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Vector3 output, Vector3 defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Vector4 output, Vector4 defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Vector2Int output, Vector2Int defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Vector3Int output, Vector3Int defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Quaternion output, Quaternion defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Color output, Color defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Color32 output, Color32 defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out Guid output) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output); }
        public bool TryReadValue(string key, out DateTime output, DateTime defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadValue(string key, out string output, string defaultValue) { return TryReadValue(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        
        public bool TryReadValues(string key, out bool[] output, bool[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }        
        public bool TryReadValues(string key, out byte[] output, byte[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out sbyte[] output, sbyte[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out char[] output, char[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out short[] output, short[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out ushort[] output, ushort[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out int[] output, int[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out uint[] output, uint[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out long[] output, long[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out ulong[] output, ulong[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out float[] output, float[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out double[] output, double[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out string[] output, string[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Vector2[] output, Vector2[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Vector3[] output, Vector3[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Vector4[] output, Vector4[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Vector2Int[] output, Vector2Int[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Vector3Int[] output, Vector3Int[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Quaternion[] output, Quaternion[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Color[] output, Color[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Color32[] output, Color32[] defaultValues) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadValues(string key, out Guid[] output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), out output); }

        public bool TryReadValues(string key, List<bool> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<byte> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<sbyte> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<char> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<short> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<ushort> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<int> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<uint> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<long> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<ulong> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<float> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<double> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<string> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Vector2> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Vector3> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Vector4> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Vector2Int> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Vector3Int> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Quaternion> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Color> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Color32> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }
        public bool TryReadValues(string key, List<Guid> output) { return TryReadValues(NeoSerializationUtilities.StringToHash(key), output); }

        public bool TryReadSerializable<T>(string key, out T output, T defaultValue) { return TryReadSerializable(NeoSerializationUtilities.StringToHash(key), out output, defaultValue); }
        public bool TryReadSerializables<T>(string key, out T[] output, T[] defaultValues) { return TryReadSerializables(NeoSerializationUtilities.StringToHash(key), out output, defaultValues); }
        public bool TryReadSerializables<T>(string key, List<T> output) { return TryReadSerializables(NeoSerializationUtilities.StringToHash(key), output); }

        public bool TryReadComponentReference<T>(string key, out T output, NeoSerializedGameObject pathFrom) where T : class
        {
            return TryReadComponentReference(NeoSerializationUtilities.StringToHash(key), out output, pathFrom);
        }

        public bool TryReadTransformReference(string key, out Transform output, NeoSerializedGameObject pathFrom)
        {
            return TryReadTransformReference(NeoSerializationUtilities.StringToHash(key), out output, pathFrom);
        }

        public bool TryReadGameObjectReference(string key, out GameObject output, NeoSerializedGameObject pathFrom)
        {
            return TryReadGameObjectReference(NeoSerializationUtilities.StringToHash(key), out output, pathFrom);
        }

        public bool TryReadNeoSerializedGameObjectReference(string key, out NeoSerializedGameObject output, NeoSerializedGameObject pathFrom)
        {
            return TryReadNeoSerializedGameObjectReference(NeoSerializationUtilities.StringToHash(key), out output, pathFrom);
        }

        #endregion
    }
}
