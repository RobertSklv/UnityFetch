using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using System.Text;
using System.Reflection;

namespace UnityFetch
{
    public class UnityFetchClient
    {
        private readonly UnityFetchRequestOptions globalOptions = new();

        public UnityFetchClient() { }

        public UnityFetchClient(UnityFetchRequestOptions options)
        {
            globalOptions = options;
        }

        public UnityFetchClient(Action<UnityFetchRequestOptions> optionsCallback)
        {
            optionsCallback(globalOptions);
        }

        public async Task<UnityFetchResponse<T>> Request<T>(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            UnityFetchRequestOptions opts = globalOptions.Clone();
            optionsCallback?.Invoke(opts);

            string url = opts.BaseUrl + uri + BuildQueryParameters(opts.QueryParameters);

            UnityWebRequest request = new(url, method.ToString());
            request.timeout = opts.Timeout;

            opts.AbortController?.AbortSignal.AddListener(() => request.Abort());

            if (body != null)
            {
                string serializedPayload = opts.JsonSerializer.SerializeObject(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(serializedPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            foreach ((string key, object value) in opts.Headers)
            {
                request.SetRequestHeader(key, value.ToString());
            }

            request.SendWebRequest();

            while (!request.isDone)
            {
                await Task.Yield();
            }

            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    UnityFetchResponse<T> response = GenerateResponse<T>(request, opts);

                    if (response.IsSuccess)
                    {
                        opts.SuccessHandlers.ForEach(callback => callback.TryHandle(response, opts.JsonSerializer));
                    }
                    else
                    {
                        opts.ErrorHandlers.ForEach(callback => callback.TryHandle(response, opts.JsonSerializer));
                    }

                    return response;
                default: throw new UnityFetchTransportException(request.result);
            }
        }

        public Task<UnityFetchResponse<object>> Request(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, method, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Get<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.GET, null, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Get<T>(
            string uri,
            object parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            void setParametersCallback(UnityFetchRequestOptions options)
            {
                options.AddParameters(parameters);
                optionsCallback?.Invoke(options);
            }

            return Request<T>(uri, RequestMethod.GET, null, setParametersCallback);
        }

        public Task<UnityFetchResponse<T>> Get<T>(
            string uri,
            Dictionary<string, object> parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            void setParametersCallback(UnityFetchRequestOptions options)
            {
                options.AddParameters(parameters);
                optionsCallback?.Invoke(options);
            }

            return Request<T>(uri, RequestMethod.GET, null, setParametersCallback);
        }

        public Task<UnityFetchResponse<T>> Post<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.POST, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Post(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.POST, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Put<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.PUT, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Put(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.PUT, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Patch<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.PATCH, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Patch(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.PATCH, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Delete(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.DELETE, null, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Head(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.HEAD, null, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Options(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.OPTIONS, null, optionsCallback);
        }

        public UnityFetchClient SetAbortController(AbortController? abortController)
        {
            globalOptions.SetAbortController(abortController);

            return this;
        }

        private static UnityFetchResponse<T> GenerateResponse<T>(UnityWebRequest request, UnityFetchRequestOptions options)
        {
            T? deserializedResponse = options.JsonSerializer.DeserializeObject<T>(request.downloadHandler.text);

            return new UnityFetchResponse<T>(
                deserializedResponse,
                request.responseCode,
                request.downloadHandler.text,
                request.GetResponseHeaders());
        }

        private static string BuildQueryParameters(IDictionary<string, object> queryParameters)
        {
            if (queryParameters.Count == 0)
            {
                return string.Empty;
            }

            List<string> keyValuePairs = new();

            foreach ((string key, object value) in queryParameters)
            {
                foreach (string kvp in BuildQueryParamKeyValuePair(key, value))
                {
                    keyValuePairs.Add(kvp);
                }
            }

            return "?" + string.Join('&', keyValuePairs);
        }

        private static IEnumerable<string> BuildQueryParamKeyValuePair(string key, object value)
        {
            string encodedKey = UnityWebRequest.EscapeURL(key);
            string encodedValue = null;

            if (value != null)
            {
                Type valueType = value.GetType();

                if (valueType.IsValueType)
                {
                    encodedValue = UnityWebRequest.EscapeURL(value.ToString());
                }
                else if (value is string str)
                {
                    encodedValue = UnityWebRequest.EscapeURL(str);
                }
                else if (valueType.IsArray)
                {
                    Array arr = (Array)value;

                    foreach (object element in arr)
                    {
                        if (element != null && !element.GetType().IsValueType)
                        {
                            throw new UnityFetchException("Only value types are supported as query string array elements.");
                        }

                        foreach (string kvp in BuildQueryParamKeyValuePair(key, element))
                        {
                            yield return kvp;
                        }
                    }

                    yield break;
                }
                else if (valueType.IsClass && !valueType.IsAbstract)
                {
                    PropertyInfo[] properties = valueType.GetProperties();

                    foreach (PropertyInfo property in properties)
                    {
                        string name = property.Name;
                        object propValue = property.GetValue(value, null);

                        foreach (string kvp in BuildQueryParamKeyValuePair(encodedKey + '.' + name, propValue))
                        {
                            yield return kvp;
                        }
                    }

                    yield break;
                }
            }

            yield return encodedKey + "=" + encodedValue;
        }
    }
}
