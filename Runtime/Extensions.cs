using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UnityFetch
{
    public static class Extensions
    {
        public static Coroutine StartCoroutine<T>(
            this MonoBehaviour monoBehaviour,
            UnityFetchCoroutineRequestWrapper<T> requestWrapper)
        {
            Coroutine coroutine = monoBehaviour.StartCoroutine(requestWrapper.routine);

            return coroutine;
        }

        public static Coroutine StartCoroutine<T>(
            this MonoBehaviour monoBehaviour,
            IEnumerator routine,
            Action<T> onSuccess = null,
            Action<UnityFetchResponse<T>> onError = null)
        {
            UnityFetchCoroutineRequestWrapper<T> requestWrapper = new UnityFetchCoroutineRequestWrapper<T>(routine)
                .OnSuccess(onSuccess)
                .OnError(onError);
            Coroutine coroutine = monoBehaviour.StartCoroutine(requestWrapper);

            return coroutine;
        }

        public static Coroutine StartCoroutine<T>(
            this MonoBehaviour monoBehaviour,
            UnityFetchCoroutineRequestWrapper<T> requestWrapper,
            Action<T> onSuccess = null,
            Action<UnityFetchResponse<T>> onError = null)
        {
            requestWrapper
                .OnSuccess(onSuccess)
                .OnError(onError);
            Coroutine coroutine = monoBehaviour.StartCoroutine(requestWrapper);

            return coroutine;
        }

        public static void AddOrUpdate<T, U>(this Dictionary<T, U> dict, T key, U value)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = value;
            }
            else
            {
                dict.Add(key, value);
            }
        }
    }
}