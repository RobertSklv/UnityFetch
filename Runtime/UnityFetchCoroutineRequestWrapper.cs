using System;
using System.Collections;

namespace UnityFetch
{
    public class UnityFetchCoroutineRequestWrapper<T>
    {
        public IEnumerator routine;
        public Action<T>? onSuccess;
        public Action<UnityFetchResponse<T>>? onError;
        private UnityFetchResponse<T>? response;

        internal UnityFetchCoroutineRequestWrapper()
        {
        }

        public UnityFetchCoroutineRequestWrapper(IEnumerator routine)
        {
            this.routine = routine;
        }

        public UnityFetchCoroutineRequestWrapper<T> OnSuccess(Action<T> onSuccess)
        {
            this.onSuccess = onSuccess;

            if (response != null && response.IsSuccess)
            {
                onSuccess?.Invoke(response.content);
            }

            return this;
        }

        public UnityFetchCoroutineRequestWrapper<T> OnError(Action<UnityFetchResponse<T>> onError)
        {
            this.onError = onError;

            if (response != null && !response.IsSuccess)
            {
                onError?.Invoke(response);
            }

            return this;
        }

        internal void SetResponse(UnityFetchResponse<T> response)
        {
            this.response = response;
        }
    }
}