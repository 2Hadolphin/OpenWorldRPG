using System;
using System.Collections.Generic;
using UnityEngine;

namespace NeoSaveGames.Serialization
{
    public static class NeoSerializationFormatters
    {
        private static Dictionary<Type, INeoSerializationFormatter> s_Formatters = new Dictionary<Type, INeoSerializationFormatter>();

        public static void RegisterFormatter<T>(NeoSerializationFormatter<T> formatter) where T : Component
        {
            if (formatter == null)
                return;

            // Add the formatter if it's not already registered
            var t = typeof(T);
            if (!s_Formatters.ContainsKey(t))
                s_Formatters.Add(t, formatter);
        }

        public static NeoSerializationFormatter<T> GetFormatter<T>() where T : Component
        {
            INeoSerializationFormatter result;
            if (s_Formatters.TryGetValue(typeof(T), out result))
                return result as NeoSerializationFormatter<T>;
            else
                return null;
        }

        public static INeoSerializationFormatter GetFormatter(Component c)
        {
            INeoSerializationFormatter result;
            if (s_Formatters.TryGetValue(c.GetType(), out result))
                return result;
            else
                return null;
        }

        public static bool ContainsFormatter<T>()
        {
            return s_Formatters.ContainsKey(typeof(T));
        }

        public static bool ContainsFormatter(Type t)
        {
            return s_Formatters.ContainsKey(t);
        }

        public static bool ContainsFormatter(Component c)
        {
            return s_Formatters.ContainsKey(c.GetType());
        }
    }
}
