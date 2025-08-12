using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFetch.RequestStrategies
{
    internal class MultiparyFormDataStrategy : RequestStrategy
    {
        public override UnityWebRequest CreateRequest(string url, object body, string? fileName, UnityFetchRequestOptions options)
        {
            if (body is WWWForm formData)
            {
                return UnityWebRequest.Post(url, formData);
            }
            else if (body is List<IMultipartFormSection> multipartFormSections)
            {
                return UnityWebRequest.Post(url, multipartFormSections);
            }

            throw new UnityFetchException($"Unsupported body type: {body.GetType()}");
        }
    }
}