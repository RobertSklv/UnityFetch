using System;
using System.Text;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class TextPlainStrategy : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            if (body is not string text)
            {
                text = body.ToString();
                //throw new ArgumentException($"The body must be of string type for requests with Content-Type: text/plain");
            }

            byte[] bodyRaw = Encoding.UTF8.GetBytes(text);
            UnityWebRequest request = new(url);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            return request;
        }
    }
}