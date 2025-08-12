using System.Text;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class ApplicationXmlStrategy : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            if (body is not string xml)
            {
                xml = body.ToString();
            }

            byte[] bodyRaw = Encoding.UTF8.GetBytes(xml);
            UnityWebRequest request = new(url);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            return request;
        }
    }
}