using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using UnityEngine.Networking;
using System.Collections;
using UnityFetch.RequestStrategies;
using UnityFetch.RequestProcessing;
using UnityFetch.Debugging;
using System.Linq;

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
            Action<UnityFetchRequestOptions>? optionsCallback = null,
            string? fileName = null)
        {
            UnityFetchRequestOptions options = globalOptions.Clone();
            optionsCallback?.Invoke(options);

            string url = Util.UriCombine(
                options.BaseUrl.ToString(),
                uri,
                Util.UriCombine(options.RouteParameters.ConvertAll(p => p.ToString())),
                options.QueryParameters.Any()
                    ? "?" + Util.EncodeUrlBody(options.QueryParameters)
                    : null);

            RequestStrategy requestStrategy = RequestStrategyFactory.Create(body, fileName, options);

            using UnityWebRequest request = body != null
                ? requestStrategy.CreateRequest(url, body, fileName, options.Clone())
                : new UnityWebRequest(url);

            request.method = method.ToString();
            request.timeout = options.Timeout;

            options.AbortController?.AbortSignal.AddListener(() => request.Abort());

            RequestProcessor requestProcessor = RequestProcessorFactory.Create(method, options.DownloadHandlerType);

            request.downloadHandler = requestProcessor.GetDownloadHandler(options);

            UnityFetchRequestInfo requestInfo = new()
            {
                url = url,
                method = method,
                requestBody = body?.ToString(),
            };
            UF.NotifyRequestStart(requestInfo);

            Dictionary<string, string> requestHeaders = options.GetHeaders();
            foreach ((string key, string value) in requestHeaders)
            {
                request.SetRequestHeader(key, value);
            }

            requestInfo.requestHeaders = requestHeaders.ToList().ConvertAll(kvp => new Header(kvp.Key, kvp.Value));

            float startTime = Time.realtimeSinceStartup;

            request.SendWebRequest();

            while (!request.isDone)
            {
                await Task.Yield();
            }

            float endTime = Time.realtimeSinceStartup;
            float timeElapsedSeconds = endTime - startTime;
            TimeSpan timeElapsedTimeSpan = TimeSpan.FromSeconds(timeElapsedSeconds);
            DateTime timestamp = DateTime.Now;
            Dictionary<string, string> responseHeaders = request.GetResponseHeaders();
            string rawResponse = requestProcessor.GetRawResponse(request);

            requestInfo.status = request.responseCode;
            requestInfo.responseBody = rawResponse;
            requestInfo.time = timeElapsedSeconds.ToString("##0.0 s");
            requestInfo.responseHeaders = responseHeaders.ToList().ConvertAll(kvp => new Header(kvp.Key, kvp.Value));

            if (request.result == UnityWebRequest.Result.Success)
            {
                T? responseObject = requestProcessor.GenerateResponse<T>(request, options);

                UnityFetchResponse<T> response = new(
                    responseObject,
                    request.responseCode,
                    rawResponse,
                    requestHeaders,
                    responseHeaders,
                    Enum.Parse<RequestMethod>(request.method),
                    timeElapsedTimeSpan,
                    timestamp);

                if (response.IsSuccess)
                {
                    options.SuccessHandlers.ForEach(callback => callback.TryHandle(response, options));
                }
                else
                {
                    options.ErrorHandlers.ForEach(callback => callback.TryHandle(response, options));
                }

                UF.NotifyRequestFinish(requestInfo);

                return response;
            }

            UF.NotifyRequestFinish(requestInfo);

            throw new UnityFetchTransportException(request.result);
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
                options.AddQueryParameters(parameters);
                optionsCallback?.Invoke(options);
            });
        }

        public Task<UnityFetchResponse<Texture2D>> GetTexture(
            string uri,
            DownloadedTextureParams downloadedTextureParams = default,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Get<Texture2D>(uri, options =>
            {
                options.UseDownloadHandlerTexture(downloadedTextureParams);
                optionsCallback?.Invoke(options);
            });
        }

        public Task<UnityFetchResponse<object>> GetFile(
            string uri,
            string savePath,
            bool append = false,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Get<object>(uri, options =>
            {
                options.UseDownloadHandlerFile(savePath, append);
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

        public Task<UnityFetchResponse<object>> UploadFile(
            string uri,
            byte[] bytes,
            string mimeType,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Post<object>(uri, bytes, options =>
            {
                options.SetContentType(mimeType);
                optionsCallback?.Invoke(options);
            });
        }

        public Task<UnityFetchResponse<object>> UploadPng(
            string uri,
            Texture2D texture,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            byte[] bytes = ImageConversion.EncodeToPNG(texture);

            return UploadFile(uri, bytes, "image/png", optionsCallback);
        }

        public Task<UnityFetchResponse<object>> UploadJpeg(
            string uri,
            Texture2D texture,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            byte[] bytes = ImageConversion.EncodeToJPG(texture);

            return UploadFile(uri, bytes, "image/jpeg", optionsCallback);
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
                options.AddQueryParameters(parameters);
                optionsCallback?.Invoke(options);
            });
        }

        public IEnumerator CoroutineGetTexture(
            string uri,
            Action<Texture2D>? onSuccess = null,
            Action<UnityFetchResponse<Texture2D>>? onError = null,
            DownloadedTextureParams downloadedTextureParams = default,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineGet(uri, onSuccess, onError, options =>
            {
                options.UseDownloadHandlerTexture(downloadedTextureParams);
                optionsCallback?.Invoke(options);
            });
        }

        public IEnumerator CoroutineGetFile(
            string uri,
            string savePath,
            bool append = false,
            Action<Texture2D>? onSuccess = null,
            Action<UnityFetchResponse<Texture2D>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineGet(uri, onSuccess, onError, options =>
            {
                options.UseDownloadHandlerFile(savePath, append);
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

        public IEnumerator CoroutineUploadFile(
            string uri,
            byte[] bytes,
            string mimeType,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutinePost(uri, bytes, onSuccess, onError, options =>
            {
                options.SetContentType(mimeType);
                optionsCallback?.Invoke(options);
            });
        }

        public IEnumerator CoroutineUploadPng(
            string uri,
            Texture2D texture,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            byte[] bytes = ImageConversion.EncodeToPNG(texture);

            return CoroutineUploadFile(uri, bytes, "image/png", onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineUploadJpeg(
            string uri,
            Texture2D texture,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            byte[] bytes = ImageConversion.EncodeToJPG(texture);

            return CoroutineUploadFile(uri, bytes, "image/jpeg", onSuccess, onError, optionsCallback);
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
    }
}
