using System;
using System.Text;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class ApplicationGraphQlStrategy : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            if (body is not string graphql)
            {
                throw new ArgumentException($"The body must be of string type for requests with Content-Type: application/graphql");
            }

            byte[] bodyRaw = Encoding.UTF8.GetBytes(graphql);
            UnityWebRequest request = new(url);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            return request;
        }
    }
}