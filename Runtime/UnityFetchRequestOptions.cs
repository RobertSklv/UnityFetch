using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UnityFetch
{
    public class UnityFetchRequestOptions
    {
        public string BaseUrl { get; set; } = string.Empty;
        public List<object> RouteParameters { get; set; } = new();
        public Dictionary<string, object> QueryParameters { get; set; } = new();
        public Dictionary<string, object> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public int Timeout { get; set; } = 5000;
        public IJsonSerializer JsonSerializer { get; set; } = new DefaultUnityJsonSerializer();
        public List<UnityFetchResponseHandler> SuccessHandlers { get; set; } = new();
        public List<UnityFetchResponseHandler> ErrorHandlers { get; set; } = new();
        public AbortController? AbortController { get; set; }
        public DownloadHandlerType DownloadHandlerType { get; set; } = DownloadHandlerType.Json;
        public DownloadedTextureParams DownloadedTextureParams { get; set; } = new();
        public string? DownloadedFileSavePath { get; set; }
        public bool DownloadedFileAppend { get; set; }

        internal UnityFetchRequestOptions Clone()
        {
            UnityFetchRequestOptions clone = new();
            clone.BaseUrl = BaseUrl;
            clone.RouteParameters = new(RouteParameters);
            clone.QueryParameters = new(QueryParameters);
            clone.Headers = new(Headers);
            clone.Timeout = Timeout;
            clone.JsonSerializer = JsonSerializer;
            clone.SuccessHandlers = new(SuccessHandlers);
            clone.ErrorHandlers = new(ErrorHandlers);
            clone.AbortController = AbortController;
            clone.DownloadHandlerType = DownloadHandlerType;
            clone.DownloadedTextureParams = DownloadedTextureParams;
            clone.DownloadedFileSavePath = DownloadedFileSavePath;
            clone.DownloadedFileAppend = DownloadedFileAppend;

            return clone;
        }

        public UnityFetchRequestOptions SetBaseUrl(string url)
        {
            BaseUrl = url;

            return this;
        }

        public UnityFetchRequestOptions SetHeader(string name, string value)
        {
            Headers.Add(name, value);

            return this;
        }

        public UnityFetchRequestOptions SetHeader(string name, Func<string> valueCallback)
        {
            Headers.Add(name, new DynamicValue(valueCallback));

            return this;
        }

        public UnityFetchRequestOptions SetContentType(string contentType)
        {
            return SetHeader("Content-Type", contentType);
        }

        public UnityFetchRequestOptions AddQueryParameter(string name, object value)
        {
            return AddQueryParameter(name, value.ToString());
        }

        public UnityFetchRequestOptions AddQueryParameter(string name, Func<string> valueCallback)
        {
            return AddQueryParameter(name, new DynamicValue(valueCallback));
        }

        public UnityFetchRequestOptions AddQueryParameters(Dictionary<string, object> parameters)
        {
            foreach ((string name, object value) in parameters)
            {
                QueryParameters.Add(name, value);
            }

            return this;
        }

        public UnityFetchRequestOptions AddParameters(object parameters)
        {
            foreach ((string name, object value) in Util.GetAnonymousObjectParameters(parameters))
            {
                QueryParameters.Add(name, value);
            }

            return this;
        }

        public UnityFetchRequestOptions AddRouteParameter(object value)
        {
            RouteParameters.Add(value);

            return this;
        }

        public UnityFetchRequestOptions AddRouteParameters(IEnumerable<object> value)
        {
            RouteParameters.AddRange(value);

            return this;
        }

        public UnityFetchRequestOptions SetTimeout(int milliseconds)
        {
            Timeout = milliseconds;

            return this;
        }

        public UnityFetchRequestOptions SetJsonSerializer(IJsonSerializer jsonSerializer)
        {
            JsonSerializer = jsonSerializer;

            return this;
        }

        public UnityFetchRequestOptions SetAbortController(AbortController? abortController)
        {
            AbortController = abortController;

            return this;
        }

        public UnityFetchRequestOptions SetDownloadHandlerType(DownloadHandlerType downloadHandlerType)
        {
            DownloadHandlerType = downloadHandlerType;

            return this;
        }

        public UnityFetchRequestOptions UseDownloadHandlerTexture(DownloadedTextureParams downloadedTextureParams)
        {
            DownloadHandlerType = DownloadHandlerType.Texture;
            DownloadedTextureParams = downloadedTextureParams;

            return this;
        }

        public UnityFetchRequestOptions UseDownloadHandlerTexture(Action<DownloadedTextureParams> downloadedTextureParamsCallback)
        {
            DownloadHandlerType = DownloadHandlerType.Texture;
            downloadedTextureParamsCallback(DownloadedTextureParams);

            return this;
        }

        public UnityFetchRequestOptions SetDownloadedFileSavePath(string downloadedFileSavePath)
        {
            DownloadedFileSavePath = downloadedFileSavePath;

            return this;
        }

        public UnityFetchRequestOptions SetDownloadedFileAppend(bool downloadedFileAppend)
        {
            DownloadedFileAppend = downloadedFileAppend;

            return this;
        }

        public UnityFetchRequestOptions UseDownloadHandlerFile(string savePath, bool append)
        {
            DownloadHandlerType = DownloadHandlerType.File;
            DownloadedFileSavePath = savePath;
            DownloadedFileAppend = append;

            return this;
        }

        public UnityFetchRequestOptions OnSuccess(Action<UnityFetchResponse<object>> callback)
        {
            UnityFetchResponseHandler<object> handler = new(callback);
            SuccessHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnSuccess<T>(Action<UnityFetchResponse<T>> callback)
        {
            UnityFetchResponseHandler<T> handler = new(callback);
            SuccessHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnSuccess<T>(Action<T> callback)
        {
            UnityFetchResponseHandler<T> handler = new(callback);
            SuccessHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnError(Action<UnityFetchResponse> callback)
        {
            UnityFetchResponseHandler<object> handler = new(callback);
            ErrorHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnError<T>(Action<UnityFetchResponse<T>> callback)
        {
            UnityFetchResponseHandler<T> handler = new(callback);
            ErrorHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnError<T>(Action<T> callback)
        {
            UnityFetchResponseHandler<T> handler = new(callback);
            ErrorHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnApiError(Action<ApiErrorResponse> callback)
        {
            return OnError(callback);
        }

        public Dictionary<string, string> GetHeaders()
        {
            return Util.ConvertParamDictionary(Headers);
        }
    }
}