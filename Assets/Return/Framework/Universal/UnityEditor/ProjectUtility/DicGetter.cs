using System.Collections.Generic;
using System;
//using Cysharp.Threading.Tasks;

namespace Return.Editors
{
    [Serializable]
    public class DicGetter
    {
        public string[] keys;
        public string[] values;

        public static implicit operator Dictionary<string, string>(DicGetter getter)
        {
            var dic = new Dictionary<string, string>(getter.keys.Length);

            for (int i = 0; i < getter.keys.Length; i++)
            {
                dic.Add(getter.keys[i], getter.values[i]);
            }

            return dic;
        }

        public DicGetter(Dictionary<string, string> dic)
        {
            keys = new string[dic.Count];
            values = new string[dic.Count];

            int i = 0;

            foreach (var pair in dic)
            {
                keys[i] = pair.Key;
                values[i] = pair.Value;
                i++;
            }
        }
    }

}
