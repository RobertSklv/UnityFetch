using UnityEngine;

namespace UnityFetch
{
    public class DefaultUnityJsonSerializer : IJsonSerializer
    {
        public T? DeserializeObject<T>(string value)
        {
            return JsonUtility.FromJson<T>(value);
        }

        public string SerializeObject(object value)
        {
            return JsonUtility.ToJson(value);
        }
    }
}