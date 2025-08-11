using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using System.Text;
using System.Reflection;
using System.Collections;

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

            using UnityWebRequest request = new(url, method.ToString());
            request.timeout = opts.Timeout;

            opts.AbortController?.AbortSignal.AddListener(() => request.Abort());

            if (body != null)
            {
                string serializedPayload = opts.JsonSerializer.SerializeObject(body);
                byte[] bodyRaw = Encoding.UTF8.GetBytes(serializedPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }

            request.downloadHandler = new DownloadHandlerBuffer();

            Dictionary<string, string> requestHeaders = opts.GetHeaders();
            foreach ((string key, string value) in requestHeaders)
            {
                request.SetRequestHeader(key, value);
            }

            float startTime = Time.realtimeSinceStartup;

            request.SendWebRequest();

            while (!request.isDone)
            {
                await Task.Yield();
            }

            float endTime = Time.realtimeSinceStartup;
            float timeElapsedSeconds = endTime - startTime;
            DateTime timestamp = DateTime.Now;

            switch (request.result)
            {
                case UnityWebRequest.Result.Success:
                    T? deserializedResponse = opts.JsonSerializer.DeserializeObject<T>(request.downloadHandler.text);

                    UnityFetchResponse<T> response = new(
                        deserializedResponse,
                        request.responseCode,
                        request.downloadHandler.text,
                        requestHeaders,
                        request.GetResponseHeaders(),
                        method,
                        TimeSpan.FromSeconds(timeElapsedSeconds),
                        timestamp);

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
            return Get<T>(uri, options =>
            {
                options.AddParameters(parameters);
                optionsCallback?.Invoke(options);
            });
        }

        public Task<UnityFetchResponse<T>> Get<T>(
            string uri,
            Dictionary<string, object> parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Get<T>(uri, options =>
            {
                options.AddParameters(parameters);
                optionsCallback?.Invoke(options);
            });
        }

        public Task<UnityFetchResponse<T>> Post<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.POST, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Post(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Post<object>(uri, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Put<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.PUT, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Put(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Put<object>(uri, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Patch<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.PATCH, body, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Patch(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Patch<object>(uri, body, optionsCallback);
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

        public IEnumerator CoroutineRequest<T>(
            Task<UnityFetchResponse<T>> task,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null)
        {
            if (!task.IsCompleted)
            {
                yield return null;
            }

            if (task.Result.IsSuccess)
            {
                onSuccess?.Invoke(task.Result.content);
            }
            else
            {
                onError?.Invoke(task.Result);
            }
        }

        public IEnumerator CoroutineRequest<T>(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            Task<UnityFetchResponse<T>> task = Request<T>(uri, method, body, optionsCallback);

            return CoroutineRequest(task, onSuccess, onError);
        }

        public IEnumerator CoroutineRequest(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest<object>(uri, method, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineGet<T>(
            string uri,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.GET, null, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineGet<T>(
            string uri,
            object parameters,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineGet(uri, onSuccess, onError, options =>
            {
                options.AddParameters(parameters);
                optionsCallback?.Invoke(options);
            });
        }

        public IEnumerator CoroutineGet<T>(
            string uri,
            Dictionary<string, object> parameters,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineGet(uri, onSuccess, onError, options =>
            {
                options.AddParameters(parameters);
                optionsCallback?.Invoke(options);
            });
        }

        public IEnumerator CoroutinePost<T>(
            string uri,
            object body,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.POST, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutinePost(
            string uri,
            object body,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutinePost<object>(uri, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutinePut<T>(
            string uri,
            object body,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.PUT, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutinePut(
            string uri,
            object body,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutinePut<object>(uri, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutinePatch<T>(
            string uri,
            object body,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.PATCH, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutinePatch(
            string uri,
            object body,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutinePatch<object>(uri, body, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineDelete(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.DELETE, null, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineHead(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.HEAD, null, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineOptions(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.OPTIONS, null, onSuccess, onError, optionsCallback);
        }

        public UnityFetchClient SetAbortController(AbortController? abortController)
        {
            globalOptions.SetAbortController(abortController);

            return this;
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
