using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public class SafeDeserializer : INeoDeserializer
    {
        private Stream m_Stream = null;
        private Property m_LastNamedProperty = null;
        private BinaryFormatter m_BinaryFormatter = new BinaryFormatter();
        private Stack<Context> m_ContextStack = new Stack<Context>();

        public bool isDeserializing
        {
            get;
            private set;
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
            public object data { get; private set; }

            public List<Property> children
            {
                get { return m_Children; }
            }

            public Property(PropertyType t, PropertyFlags f, object o)
            {
                propertyType = t;
                propertyFlags = f;
                data = o;
            }

            public bool isArray
            {
                get { return (propertyFlags & PropertyFlags.IsArray) != PropertyFlags.None; }
            }

            public bool isNullOrEmpty
            {
                get { return (propertyFlags & PropertyFlags.NullOrEmpty) != PropertyFlags.None; }
            }
        }

        public bool ReadFromStream(Stream stream)
        {
            if (stream != null)
            {
                isDeserializing = true;
                m_Stream = stream;

                var rootContext = new Context(SerializationContext.Root, 0);
                m_ContextStack.Push(rootContext);

                // Iterate through properties
                while (ReadElement()) ;

                var closingContext = m_ContextStack.Peek();
                if (closingContext != rootContext)
                    Debug.LogError("Unbalanced push/pop for serialization contexts. Deserialization did not end on the root context.");
                
                m_Stream = null;

                /*
                // Get the length of the data
                var totalLengthBytes = new byte[4];
                stream.Read(totalLengthBytes, 0, 4);
                int totalLength = BitConverter.ToInt32(totalLengthBytes, 0);

                // Read data
                var bytes = new byte[totalLength];
                int totalRead = stream.Read(bytes, 0, totalLength);
                if (totalRead != totalLength)
                {
                    Debug.LogError(string.Format("Save data length mismatch. Could not read from stream. Expected: {0}, Read {1}", totalLength, totalRead));
                    return false;
                }
                m_Stream = new MemoryStream(bytes, false);

                Debug.Log("Total bytes: " + totalLength);
                */

                return true;
            }
            else
                return false;
        }

        public void BeginDeserialization()
        {
        }

        bool ReadElement()
        {
            var pType = (PropertyType)m_BinaryFormatter.Deserialize(m_Stream);
            if (pType == PropertyType.EndOfData)
                return false;

            switch (pType)
            {
                case PropertyType.PushContext:
                    {
                        // Read the relevant context data
                        var contextType = (SerializationContext)m_BinaryFormatter.Deserialize(m_Stream);
                        var key = (int)m_BinaryFormatter.Deserialize(m_Stream);

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
                        var contextType = (SerializationContext)m_BinaryFormatter.Deserialize(m_Stream);

                        var popped = m_ContextStack.Pop();
                        if (popped.contextType != contextType)
                            Debug.LogError("Popped serialization context does not match active context. Your save game might be messed up.");
                    }
                    break;
                default:
                    {
                        // Get property flags
                        var flags = (PropertyFlags)m_BinaryFormatter.Deserialize(m_Stream);

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
                                if ((flags & PropertyFlags.NullOrEmpty) != PropertyFlags.None)
                                    m_LastNamedProperty.children.Add(new Property(pType, flags, null));
                                else
                                    m_LastNamedProperty.children.Add(new Property(pType, flags, m_BinaryFormatter.Deserialize(m_Stream)));
                            }
                        }
                        else
                        {
                            // Get the hash
                            int hash = (int)m_BinaryFormatter.Deserialize(m_Stream);

                            // Create the property
                            if ((flags & PropertyFlags.NullOrEmpty) != PropertyFlags.None)
                                m_LastNamedProperty = new Property(pType, flags, null);
                            else
                                m_LastNamedProperty = new Property(pType, flags, m_BinaryFormatter.Deserialize(m_Stream));

                            // Add to dictionary
                            var context = m_ContextStack.Peek();
                            if (context.properties.ContainsKey(hash))
                                Debug.LogError(string.Format("Key collision: {0}, type: {1}, in context: {2}", hash, pType, context.id));
                            else
                                context.properties.Add(hash, m_LastNamedProperty);
                        }
                    }
                    break;
            }

            return true;
        }

        public void EndDeserialization()
        {
            m_LastNamedProperty = null;
            isDeserializing = false;
            m_ContextStack.Clear();
        }

        public bool PushContext(SerializationContext context, int id)
        {
            Context c;
            if (m_ContextStack.Peek().subContexts.TryGetValue(id, out c))
            {
                m_ContextStack.Push(c);
                return true;
            }
            else
            {
                return false;
            }
        }

        public void PopContext(SerializationContext context, int id)
        {
            var activeContext = m_ContextStack.Pop().contextType;
            if (activeContext != context)
                Debug.LogError(string.Format("Popped serialization context does not match active context. Popped: {0}, Active: {1}", context, activeContext));
        }

        #region READ HELPERS

        bool TryFetchProperty(int hash, PropertyType t, bool isArray)
        {
            if (m_ContextStack.Peek().properties.TryGetValue(hash, out m_LastNamedProperty) &&
                m_LastNamedProperty.propertyType == t &&
                m_LastNamedProperty.isArray == isArray)
                return true;
            else
                return false;
        }

        void ReadList<T>(List<T> output)
        {
            output.Clear();
            if (!m_LastNamedProperty.isNullOrEmpty &&  m_LastNamedProperty.data != null)
                output.AddRange((List<T>)m_LastNamedProperty.data);
        }

        T[] ReadArray<T>()
        {
            if (m_LastNamedProperty.isNullOrEmpty || m_LastNamedProperty.data == null)
            {
                return new T[0];
            }
            else
            {
                // Get the serialized collection
                var collection = (List<T>)m_LastNamedProperty.data;
                return collection.ToArray();
            }
        }

        List<T> ReadIntermediates<T>()
        {
            if (m_LastNamedProperty.isNullOrEmpty || m_LastNamedProperty.data == null)
                return null;
            else
                return (List<T>)m_LastNamedProperty.data;
        }

        #endregion


        #region SINGLE VALUE READERS

        public bool TryReadValue(int hash, out bool output, bool defaultValue)
        {
            if (TryFetchProperty(hash, PropertyType.Bool, false))
            {
                output = (bool)m_LastNamedProperty.data;
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
                output = (byte)m_LastNamedProperty.data;
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
                output = (sbyte)m_LastNamedProperty.data;
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
                output = (char)m_LastNamedProperty.data;
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
                output = (short)m_LastNamedProperty.data;
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
                output = (ushort)m_LastNamedProperty.data;
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
                output = (int)m_LastNamedProperty.data;
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
                output = (uint)m_LastNamedProperty.data;
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
                output = (long)m_LastNamedProperty.data;
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
                output = (ulong)m_LastNamedProperty.data;
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
                output = (float)m_LastNamedProperty.data;
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
                output = (double)m_LastNamedProperty.data;
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
                output = (string)m_LastNamedProperty.data;
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
                output = (IntermediateVector2)m_LastNamedProperty.data;
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
                output = (IntermediateVector3)m_LastNamedProperty.data;
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
                output = (IntermediateVector4)m_LastNamedProperty.data;
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
                output = (IntermediateVector2Int)m_LastNamedProperty.data;
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
                output = (IntermediateVector3Int)m_LastNamedProperty.data;
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
                output = (IntermediateVector4)m_LastNamedProperty.data;
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
                output = (IntermediateVector4)m_LastNamedProperty.data;
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
                output = (IntermediateColor32)m_LastNamedProperty.data;
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
                output = (Guid)m_LastNamedProperty.data;
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
                output = (DateTime)m_LastNamedProperty.data;
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
                output = ReadArray<bool>();
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
                output = ReadArray<byte>();
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
                output = ReadArray<sbyte>();
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
                output = ReadArray<char>();
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
                output = ReadArray<short>();
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
                output = ReadArray<ushort>();
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
                output = ReadArray<int>();
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
                output = ReadArray<uint>();
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
                output = ReadArray<long>();
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
                output = ReadArray<ulong>();
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
                output = ReadArray<float>();
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
                output = ReadArray<double>();
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
                output = ReadArray<string>();
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
                var intermediates = ReadIntermediates<IntermediateVector2>();
                if (intermediates == null)
                    output = new Vector2[0];
                else
                {
                    // Allocate
                    output = new Vector2[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateVector3>();
                if (intermediates == null)
                    output = new Vector3[0];
                else
                {
                    // Allocate
                    output = new Vector3[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateVector4>();
                if (intermediates == null)
                    output = new Vector4[0];
                else
                {
                    // Allocate
                    output = new Vector4[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateVector2Int>();
                if (intermediates == null)
                    output = new Vector2Int[0];
                else
                {
                    // Allocate
                    output = new Vector2Int[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateVector3Int>();
                if (intermediates == null)
                    output = new Vector3Int[0];
                else
                {
                    // Allocate
                    output = new Vector3Int[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateVector4>();
                if (intermediates == null)
                    output = new Quaternion[0];
                else
                {
                    // Allocate
                    output = new Quaternion[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateVector4>();
                if (intermediates == null)
                    output = new Color[0];
                else
                {
                    // Allocate
                    output = new Color[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                var intermediates = ReadIntermediates<IntermediateColor32>();
                if (intermediates == null)
                    output = new Color32[0];
                else
                {
                    // Allocate
                    output = new Color32[intermediates.Count];
                    // Transfer across
                    int i = 0;
                    foreach (var intermediate in intermediates)
                        output[i++] = intermediate;
                }

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
                output = ReadArray<Guid>();
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
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<byte> output)
        {
            if (TryFetchProperty(hash, PropertyType.Byte, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<sbyte> output)
        {
            if (TryFetchProperty(hash, PropertyType.SignedByte, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<char> output)
        {
            if (TryFetchProperty(hash, PropertyType.Char, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<short> output)
        {
            if (TryFetchProperty(hash, PropertyType.Short, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<ushort> output)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedShort, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<int> output)
        {
            if (TryFetchProperty(hash, PropertyType.Int, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<uint> output)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedInt, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<long> output)
        {
            if (TryFetchProperty(hash, PropertyType.Long, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<ulong> output)
        {
            if (TryFetchProperty(hash, PropertyType.UnsignedLong, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<float> output)
        {
            if (TryFetchProperty(hash, PropertyType.Float, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<double> output)
        {
            if (TryFetchProperty(hash, PropertyType.Double, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<string> output)
        {
            if (TryFetchProperty(hash, PropertyType.String, true))
            {
                ReadList(output);
                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector2> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector2>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector3> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector3>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector4> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector4, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector4>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector2Int> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector2Int, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector2Int>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Vector3Int> output)
        {
            if (TryFetchProperty(hash, PropertyType.Vector3Int, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector3Int>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Quaternion> output)
        {
            if (TryFetchProperty(hash, PropertyType.Quaternion, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector4>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Color> output)
        {
            if (TryFetchProperty(hash, PropertyType.Color, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateVector4>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Color32> output)
        {
            if (TryFetchProperty(hash, PropertyType.Color32, true))
            {
                output.Clear();
                var intermediates = ReadIntermediates<IntermediateColor32>();
                if (intermediates != null)
                {
                    foreach (var intermediate in intermediates)
                        output.Add(intermediate);
                }

                return true;
            }
            else
                return false;
        }

        public bool TryReadValues(int hash, List<Guid> output)
        {
            if (TryFetchProperty(hash, PropertyType.Guid, true))
            {
                ReadList(output);
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
                if (m_LastNamedProperty.isNullOrEmpty)
                    output = default(T);
                else
                    output = (T)m_BinaryFormatter.Deserialize(m_Stream);

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
                if (m_LastNamedProperty.isNullOrEmpty)
                    output = new T[0];
                else
                {
                    var collection = (ICollection < T >)m_BinaryFormatter.Deserialize(m_Stream);
                    output = new T[collection.Count];
                    collection.CopyTo(output, 0);
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
                output.Clear();
                if (!m_LastNamedProperty.isNullOrEmpty)
                    output.AddRange((ICollection<T>)m_BinaryFormatter.Deserialize(m_Stream));

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
