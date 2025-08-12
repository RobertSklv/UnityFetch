using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal abstract class RequestStrategy
    {
        public abstract UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options);
    }
}