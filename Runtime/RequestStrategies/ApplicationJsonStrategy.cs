using System.Text;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class ApplicationJsonStrategy : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            string serializedPayload = options.JsonSerializer.SerializeObject(body);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(serializedPayload);

            UnityWebRequest request = new(url);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            return request;
        }
    }
}