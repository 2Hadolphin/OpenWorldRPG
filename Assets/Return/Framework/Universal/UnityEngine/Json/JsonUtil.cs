using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Return
{
    public class JsonUtil
    {
        //Usage:
        //YouObject[] objects = JsonHelper.getJsonArray<YouObject> (jsonString);
        public static T[] getJsonArray<T>(string json)
        {
            var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
            return wrapper.array;
        }

        //Usage:
        //string jsonString = JsonHelper.arrayToJson<YouObject>(objects);
        public static string arrayToJson<T>(T[] array, bool pretty = false)
        {
            var wrapper = new Wrapper<T>
            {
                array = array
            };
            return JsonUtility.ToJson(wrapper,pretty);
        }

        [System.Serializable]
        private class Wrapper<T>
        {
            public T[] array;
        }
    }
}
