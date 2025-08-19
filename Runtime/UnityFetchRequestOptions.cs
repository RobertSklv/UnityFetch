using System;
using System.Collections.Generic;
using UnityEngine.Networking;

namespace UnityFetch
{
    public class UnityFetchRequestOptions
    {
        public object BaseUrl { get; set; } = string.Empty;
        public List<object> RouteParameters { get; set; } = new();
        public Dictionary<string, object> QueryParameters { get; set; } = new();
        public Dictionary<string, object> Headers { get; set; } = new(StringComparer.OrdinalIgnoreCase);
        public int Timeout { get; set; } = 5;
        public IJsonSerializer JsonSerializer { get; set; } = new DefaultUnityJsonSerializer();
        public List<UnityFetchResponseHandler> SuccessHandlers { get; set; } = new();
        public List<UnityFetchResponseHandler> ErrorHandlers { get; set; } = new();
        public AbortController? AbortController { get; set; }
        public DownloadHandlerType DownloadHandlerType { get; set; } = DownloadHandlerType.Json;
        public DownloadedTextureParams DownloadedTextureParams { get; set; } = new();
        public string? DownloadedFileSavePath { get; set; }
        public bool DownloadedFileAppend { get; set; }
        public Dictionary<string, object> ActionFlags { get; set; } = new();
        public int RetryCount { get; set; }
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
        public Func<RequestContext, bool> ShouldRetryCallback { get; set; } = Util.IsRequestIdempotent;

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
            clone.ActionFlags = new(ActionFlags);
            clone.RetryCount = RetryCount;
            clone.RetryDelay = RetryDelay;
            clone.ShouldRetryCallback = ShouldRetryCallback;

            return clone;
        }

        public UnityFetchRequestOptions SetBaseUrl(string url)
        {
            BaseUrl = url;

            return this;
        }

        public UnityFetchRequestOptions SetBaseUrl(Func<string> callback)
        {
            BaseUrl = new DynamicValue(callback);

            return this;
        }

        public UnityFetchRequestOptions SetHeader(string name, string value)
        {
            Headers.AddOrUpdate(name, value);

            return this;
        }

        public UnityFetchRequestOptions SetHeader(string name, Func<string> valueCallback)
        {
            Headers.AddOrUpdate(name, new DynamicValue(valueCallback));

            return this;
        }

        public UnityFetchRequestOptions SetContentType(string contentType)
        {
            return SetHeader("Content-Type", contentType);
        }

        public UnityFetchRequestOptions AddQueryParameter(string name, object value)
        {
            QueryParameters.AddOrUpdate(name, value);

            return this;
        }

        public UnityFetchRequestOptions AddQueryParameter(string name, Func<string> valueCallback)
        {
            return AddQueryParameter(name, new DynamicValue(valueCallback));
        }

        public UnityFetchRequestOptions AddQueryParameters(Dictionary<string, object> parameters)
        {
            foreach ((string name, object value) in parameters)
            {
                QueryParameters.AddOrUpdate(name, value);
            }

            return this;
        }

        public UnityFetchRequestOptions AddParameters(object parameters)
        {
            foreach ((string name, object value) in Util.GetAnonymousObjectParameters(parameters))
            {
                QueryParameters.AddOrUpdate(name, value);
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

        public UnityFetchRequestOptions SetTimeout(int seconds)
        {
            Timeout = seconds;

            return this;
        }

        public UnityFetchRequestOptions SetJsonSerializer(IJsonSerializer jsonSerializer)
        {
            JsonSerializer = jsonSerializer;

            return this;
        }

        public UnityFetchRequestOptions ConfigureJsonSerializer<TJsonSerializer>(Action<TJsonSerializer> configure)
            where TJsonSerializer : IJsonSerializer
        {
            if (JsonSerializer is TJsonSerializer s)
            {
                configure(s);
            }
            else throw new UnityFetchException($"JSON Serializer not of type: {typeof(TJsonSerializer).Name}");

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

        public UnityFetchRequestOptions SetFlag(string name, object value)
        {
            ActionFlags.AddOrUpdate(name, value);

            return this;
        }

        public UnityFetchRequestOptions SetRetry(int retryCount = 3, TimeSpan delay = default)
        {
            RetryCount = retryCount;

            if (delay != null)
            {
                RetryDelay = delay;
            }

            return this;
        }

        public UnityFetchRequestOptions ShouldRetry(Func<RequestContext, bool> shouldRetryCallback)
        {
            ShouldRetryCallback = shouldRetryCallback;

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

        public UnityFetchRequestOptions OnSuccessSimple<T>(Action<T> callback)
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

        public UnityFetchRequestOptions OnErrorSimple<T>(Action<T> callback)
        {
            UnityFetchResponseHandler<T> handler = new(callback);
            ErrorHandlers.Add(handler);

            return this;
        }

        public UnityFetchRequestOptions OnApiError(Action<ApiErrorResponse> callback)
        {
            return OnErrorSimple(callback);
        }

        public Dictionary<string, string> GetHeaders()
        {
            return Util.ConvertParamDictionary(Headers);
        }
    }
}