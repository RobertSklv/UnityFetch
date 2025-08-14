using System.Collections.Generic;

namespace UnityFetch
{
    public interface IJsonSerializer
    {
        string SerializeObject(object? value, Dictionary<string, object> actionFlags);

        T? DeserializeObject<T>(string value, Dictionary<string, object> actionFlags);
    }
}