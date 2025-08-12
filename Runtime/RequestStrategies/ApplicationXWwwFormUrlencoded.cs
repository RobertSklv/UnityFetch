using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class ApplicationXWwwFormUrlencoded : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            string formData;

            if (body is Dictionary<string, object> dict)
            {
                formData = Util.EncodeUrlBody(dict);
            }
            else if (body is Dictionary<string, string> strDict)
            {
                formData = Util.EncodeUrlBody(strDict);
            }
            else
            {
                formData = Util.EncodeUrlBody(Util.GetAnonymousObjectParameters(body));
            }

            byte[] bodyRaw = Encoding.UTF8.GetBytes(formData);
            UnityWebRequest request = new(url);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);

            return request;
        }
    }
}