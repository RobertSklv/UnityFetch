using System;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class FileUploadStrategy : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            UnityWebRequest request = new(url);

            if (body is byte[] bytes)
            {
                request.uploadHandler = new UploadHandlerRaw(bytes);
            }
            else if (body is string strBody)
            {
                request.uploadHandler = new UploadHandlerFile(strBody);
            }
            else throw new ArgumentException(
                $"The body must be of byte[] or string type for requests with Content-Type: application/octet-stream");

            return request;
        }
    }
}