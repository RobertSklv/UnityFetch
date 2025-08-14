using System.Collections.Generic;
using UnityEngine;

namespace UnityFetch
{
    public class DefaultUnityJsonSerializer : IJsonSerializer
    {
        public T? DeserializeObject<T>(string value, Dictionary<string, object> actionFlags)
        {
            return JsonUtility.FromJson<T>(value);
        }

        public string SerializeObject(object value, Dictionary<string, object> actionFlags)
        {
            return JsonUtility.ToJson(value);
        }
    }
}