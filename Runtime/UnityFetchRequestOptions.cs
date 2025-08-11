using System;
using System.Collections.Generic;
using System.Reflection;

namespace UnityFetch
{
    public class UnityFetchRequestOptions
    {
        public string BaseUrl { get; internal set; } = string.Empty;
        public Dictionary<string, object> QueryParameters { get; internal set; } = new();
        public Dictionary<string, object> Headers { get; internal set; } = new();
        public int Timeout { get; internal set; } = 5000;
        public IJsonSerializer JsonSerializer { get; internal set; } = new DefaultUnityJsonSerializer();
        public List<UnityFetchResponseHandler> SuccessHandlers { get; internal set; } = new();
        public List<UnityFetchResponseHandler> ErrorHandlers { get; internal set; } = new();
        public AbortController? AbortController { get; internal set; }

        internal UnityFetchRequestOptions Clone()
        {
            UnityFetchRequestOptions clone = new();
            clone.BaseUrl = BaseUrl;
            clone.QueryParameters = new(QueryParameters);
            clone.Headers = new(Headers);
            clone.Timeout = Timeout;
            clone.JsonSerializer = JsonSerializer;
            clone.SuccessHandlers = new(SuccessHandlers);
            clone.ErrorHandlers = new(ErrorHandlers);
            clone.AbortController = AbortController;

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

        public UnityFetchRequestOptions AddParameter(string name, object value)
        {
            return AddParameter(name, value.ToString());
        }

        public UnityFetchRequestOptions AddParameter(string name, Func<string> valueCallback)
        {
            return AddParameter(name, new DynamicValue(valueCallback));
        }

        public UnityFetchRequestOptions AddParameters(Dictionary<string, object> parameters)
        {
            foreach ((string name, object value) in parameters)
            {
                QueryParameters.Add(name, value);
            }

            return this;
        }

        public UnityFetchRequestOptions AddParameters(object parameters)
        {
            foreach ((string name, object value) in GetAnonymousObjectParameters(parameters))
            {
                QueryParameters.Add(name, value);
            }

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

        public UnityFetchRequestOptions SetAbortController(AbortController? abortController)
        {
            AbortController = abortController;

            return this;
        }

        private Dictionary<string, string> ConvertParamDictionary(Dictionary<string, object> parameters)
        {
            Dictionary<string, string> converted = new();

            foreach ((string key, object value) in parameters)
            {
                converted.Add(key, value.ToString());
            }

            return converted;
        }

        private Dictionary<string, object> GetAnonymousObjectParameters(object parameters)
        {
            Dictionary<string, object> converted = new();

            PropertyInfo[] properties = parameters.GetType().GetProperties();

            foreach (PropertyInfo property in properties)
            {
                string name = property.Name;
                object value = property.GetValue(parameters, null);

                converted.Add(name, value);
            }

            return converted;
        }
    }
}