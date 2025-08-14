using System;

namespace UnityFetch
{
    public abstract class UnityFetchResponseHandler
    {
        internal abstract bool TryHandle(UnityFetchResponse errorResponse, UnityFetchRequestOptions options);
    }

    public class UnityFetchResponseHandler<T> : UnityFetchResponseHandler
    {
        private readonly Action<T>? simpleCallback;
        private readonly Action<UnityFetchResponse<T>>? fullResponseCallback;

        public UnityFetchResponseHandler(Action<T> simpleCallback)
        {
            this.simpleCallback = simpleCallback;
        }

        public UnityFetchResponseHandler(Action<UnityFetchResponse<T>> callback)
        {
            fullResponseCallback = callback;
        }

        internal override bool TryHandle(UnityFetchResponse errorResponse, UnityFetchRequestOptions options)
        {
            if (errorResponse is UnityFetchResponse<T> genericResponse)
            {
                fullResponseCallback?.Invoke(genericResponse);
                simpleCallback?.Invoke(genericResponse.content);

                return true;
            }

            try
            {
                T? obj = options.JsonSerializer.DeserializeObject<T>(errorResponse.rawContent, options.ActionFlags);

                if (simpleCallback != null)
                {
                    simpleCallback(obj);
                }
                else
                {
                    fullResponseCallback(errorResponse.ToGeneric(obj));
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}