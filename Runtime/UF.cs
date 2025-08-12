using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace UnityFetch
{
    public static class UF
    {
        private static readonly UnityFetchClient DefaultClient = new();

        public static Task<UnityFetchResponse<object>> Request(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Request(uri, method, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<T>> Get<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Get<T>(uri, optionsCallback);
        }

        public static Task<UnityFetchResponse<T>> Get<T>(
            string uri,
            object parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Get<T>(uri, parameters, optionsCallback);
        }

        public static Task<UnityFetchResponse<T>> Get<T>(
            string uri,
            Dictionary<string, object> parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Get<T>(uri, parameters, optionsCallback);
        }

        public static Task<UnityFetchResponse<Texture2D>> GetTexture(
            string uri,
            DownloadedTextureParams downloadedTextureParams = default,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.GetTexture(uri, downloadedTextureParams, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> GetFile(
            string uri,
            string savePath,
            bool append = false,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.GetFile(uri, savePath, append, optionsCallback);
        }

        public static Task<UnityFetchResponse<T>> Post<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Post<T>(uri, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> Post(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Post(uri, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> UploadFile(
            string uri,
            byte[] bytes,
            string mimeType,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.UploadFile(uri, bytes, mimeType, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> UploadPng(
            string uri,
            Texture2D texture,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.UploadPng(uri, texture, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> UploadJpeg(
            string uri,
            Texture2D texture,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.UploadJpeg(uri, texture, optionsCallback);
        }

        public static Task<UnityFetchResponse<T>> Put<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Put<T>(uri, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> Put(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Put(uri, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<T>> Patch<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Patch<T>(uri, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> Patch(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Patch(uri, body, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> Delete(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Delete(uri, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> Head(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Head(uri, optionsCallback);
        }

        public static Task<UnityFetchResponse<object>> Options(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.Options(uri, optionsCallback);
        }

        public static IEnumerator CoroutineRequest<T>(
            Task<UnityFetchResponse<T>> task,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null)
        {
            return DefaultClient.CoroutineRequest(task, onSuccess, onError);
        }

        public static IEnumerator CoroutineRequest<T>(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineRequest(uri, method, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineRequest(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineRequest(uri, method, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineGet<T>(
            string uri,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineGet(uri, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineGet<T>(
            string uri,
            object parameters,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineGet(uri, parameters, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineGet<T>(
            string uri,
            Dictionary<string, object> parameters,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineGet(uri, parameters, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineGetTexture(
            string uri,
            Action<Texture2D>? onSuccess = null,
            Action<UnityFetchResponse<Texture2D>>? onError = null,
            DownloadedTextureParams downloadedTextureParams = default,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineGetTexture(uri, onSuccess, onError, downloadedTextureParams, optionsCallback);
        }

        public static IEnumerator CoroutineGetFile(
            string uri,
            string savePath,
            bool append = false,
            Action<Texture2D>? onSuccess = null,
            Action<UnityFetchResponse<Texture2D>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineGetFile(uri, savePath, append, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutinePost<T>(
            string uri,
            object body,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutinePost(uri, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutinePost(
            string uri,
            object body,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutinePost(uri, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineUploadFile(
            string uri,
            byte[] bytes,
            string mimeType,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineUploadFile(uri, bytes, mimeType, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineUploadPng(
            string uri,
            Texture2D texture,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineUploadPng(uri, texture, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineUploadJpeg(
            string uri,
            Texture2D texture,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineUploadJpeg(uri, texture, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutinePut<T>(
            string uri,
            object body,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutinePut(uri, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutinePut(
            string uri,
            object body,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutinePut(uri, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutinePatch<T>(
            string uri,
            object body,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutinePatch(uri, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutinePatch(
            string uri,
            object body,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutinePatch(uri, body, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineDelete(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineDelete(uri, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineHead(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineHead(uri, onSuccess, onError, optionsCallback);
        }

        public static IEnumerator CoroutineOptions(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return DefaultClient.CoroutineOptions(uri, onSuccess, onError, optionsCallback);
        }
    }
}