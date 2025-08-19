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
using System.Globalization;

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

            int attempts = 0;

            if (options.RetryCount < 0)
            {
                throw new UnityFetchException($"Invalid retry count: {options.RetryCount}");
            }

            bool aborted = false;
            options.AbortController?.AbortSignal.AddListener(() => aborted = true);

            Exception? exception = null;

            do
            {
                UnityFetchResponse<T>? response = null;

                RequestContext context = new()
                {
                    Attempt = ++attempts,
                    Options = globalOptions.Clone()
                };
                optionsCallback?.Invoke(context.Options);

                try
                {
                    response = await RequestOnce<T>(context, uri, method, body, fileName);
                }
                catch (Exception e)
                {
                    exception = e;
                    context.Exception = e;
                }

                if ((context.Exception != null || !response.IsSuccess) && options.ShouldRetryCallback(context))
                {
                    await Task.Delay((int)options.RetryDelay.TotalMilliseconds);
                }
                else if (response != null)
                {
                    return response;
                }
                else throw context.Exception ?? new UnityFetchException("Unexpected Error: Both response and exception are null!");
            } while (attempts <= options.RetryCount && !aborted);

            throw exception ?? new UnityFetchException("Unexpected Error: Failed to process.");
        }

        private async Task<UnityFetchResponse<T>> RequestOnce<T>(
            RequestContext context,
            string uri,
            RequestMethod method,
            object? body = null,
            string? fileName = null)
        {
            GenerateUrl(context, uri);

            context.RequestStrategy = RequestStrategyFactory.Create(body, fileName, context.Options);

            using UnityWebRequest request = body != null
                ? context.RequestStrategy.CreateRequest(context.Url, body, fileName, context.Options.Clone())
                : new UnityWebRequest(context.Url);

            request.method = method.ToString();
            request.timeout = context.Options.Timeout;

            context.Options.AbortController?.AbortSignal.AddListener(() => request.Abort());

            context.RequestProcessor = RequestProcessorFactory.Create(method, context.Options.DownloadHandlerType);

            request.downloadHandler = context.RequestProcessor.GetDownloadHandler(context.Options);

            context.RequestHeaders = context.Options.GetHeaders();
            foreach ((string key, string value) in context.RequestHeaders)
            {
                request.SetRequestHeader(key, value);
            }

            GenerateRequestInfo(context, body);
            UF.NotifyRequestStart(context.RequestInfo);

            float startTime = Time.realtimeSinceStartup;

            request.SendWebRequest();

            while (!request.isDone)
            {
                await Task.Yield();
            }

            context.Result = request.result;
            context.ResponseCode = request.responseCode;
            context.DownloadedBytes = request.downloadedBytes;
            float endTime = Time.realtimeSinceStartup;
            context.TimeElapsedSeconds = endTime - startTime;
            context.Timestamp = DateTime.Now;
            context.ResponseHeaders = request.GetResponseHeaders() ?? new();
            context.RawResponse = context.RequestProcessor.GetRawResponse(request);

            UpdateRequestInfo(context);
            UF.NotifyRequestFinish(context.RequestInfo);

            return ProcessResponse<T>(context, request);
        }

        private void GenerateUrl(RequestContext context, string uri)
        {
            context.Url = Util.UriCombine(
                context.Options.BaseUrl.ToString(),
                uri,
                Util.UriCombine(context.Options.RouteParameters.ConvertAll(p => p.ToString())),
                context.Options.QueryParameters.Any()
                    ? "?" + Util.EncodeUrlBody(context.Options.QueryParameters)
                    : null);
        }

        private void GenerateRequestInfo(RequestContext context, object? body)
        {
            context.RequestInfo = new()
            {
                url = context.Url,
                method = context.Method,
                requestBody = body != null
                    ? context.Options.JsonSerializer.SerializeObject(body, context.Options.ActionFlags)
                    : string.Empty,
                guid = Guid.NewGuid().ToString(),
                requestHeaders = context.RequestHeaders.ToList().ConvertAll(kvp => new Header(kvp.Key, kvp.Value)),
                attempt = context.Attempt,
            };
        }

        private void UpdateRequestInfo(RequestContext context)
        {
            context.RequestInfo.status = context.ResponseCode;
            context.RequestInfo.statusLabel = context.ResponseCode != 0
                ? context.ResponseCode.ToString()
                : "(failed)";
            context.RequestInfo.responseBody = context.RawResponse;
            context.RequestInfo.time = context.TimeElapsedSeconds.ToString("##0.0 s", CultureInfo.InvariantCulture);
            context.RequestInfo.responseHeaders = context.ResponseHeaders.ToList().ConvertAll(kvp => new Header(kvp.Key, kvp.Value));
            context.RequestInfo.size = Util.FormatBytes(context.DownloadedBytes);
            context.RequestInfo.type = ContentTypeMapper.GetFriendlyContentType(
                context.ResponseHeaders.TryGetValue("Content-Type", out string contentType)
                    ? contentType
                    : "N/A");
            context.RequestInfo.finished = true;
        }

        private UnityFetchResponse<T> ProcessResponse<T>(RequestContext context, UnityWebRequest request)
        {
            TimeSpan timeElapsedTimeSpan = TimeSpan.FromSeconds(context.TimeElapsedSeconds);

            if (context.Result == UnityWebRequest.Result.Success)
            {
                T? responseObject = context.RequestProcessor.GenerateResponse<T>(request, context.Options);

                UnityFetchResponse<T> response = new(
                    responseObject,
                    context.ResponseCode,
                    context.RawResponse,
                    context.RequestHeaders,
                    context.ResponseHeaders,
                    context.Method,
                    timeElapsedTimeSpan,
                    context.Timestamp);

                if (response.IsSuccess)
                {
                    context.Options.SuccessHandlers.ForEach(callback => callback.TryHandle(response, context.Options));
                }
                else
                {
                    context.Options.ErrorHandlers.ForEach(callback => callback.TryHandle(response, context.Options));
                }

                return response;
            }

            if (context.ResponseCode == 0)
            {
                throw new UnityFetchTransportException(context.Result);
            }

            UnityFetchResponse<T> failResponse = new(
                default,
                context.ResponseCode,
                context.RawResponse,
                context.RequestHeaders,
                context.ResponseHeaders,
                context.Method,
                timeElapsedTimeSpan,
                context.Timestamp);

            return failResponse;
        }

        public Task<UnityFetchResponse<object>> Request(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null,
            string? fileName = null)
        {
            return Request<object>(uri, method, body, optionsCallback, fileName);
        }

        public async Task<T> RequestSimple<T>(
            string uri,
            RequestMethod method,
            object? body = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null,
            string? fileName = null)
        {
            UnityFetchResponse<T> response = await Request<T>(uri, method, body, optionsCallback, fileName);

            return response.content;
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

        public async Task<T> GetSimple<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Get<T>(uri, optionsCallback)).content;
        }

        public async Task<T> GetSimple<T>(
            string uri,
            object parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Get<T>(uri, parameters, optionsCallback)).content;
        }

        public async Task<T> GetSimple<T>(
            string uri,
            Dictionary<string, object> parameters,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Get<T>(uri, parameters, optionsCallback)).content;
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

        public async Task<Texture2D> GetTextureSimple(
            string uri,
            DownloadedTextureParams downloadedTextureParams = default,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await GetTexture(uri, downloadedTextureParams, optionsCallback)).content;
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

        public async Task<T> PostSimple<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Post<T>(uri, body, optionsCallback)).content;
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

        public async Task<T> PutSimple<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Put<T>(uri, body, optionsCallback)).content;
        }

        public Task<UnityFetchResponse<object>> Put(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Put<object>(uri, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Patch<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.PATCH, body, optionsCallback);
        }

        public async Task<T> PatchSimple<T>(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Patch<T>(uri, body, optionsCallback)).content;
        }

        public Task<UnityFetchResponse<object>> Patch(string uri, object body, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Patch<object>(uri, body, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Delete<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.DELETE, null, optionsCallback);
        }

        public async Task<T> DeleteSimple<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Delete<T>(uri, optionsCallback)).content;
        }

        public Task<UnityFetchResponse<object>> Delete(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Delete<object>(uri, optionsCallback);
        }

        public Task<UnityFetchResponse<object>> Head(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<object>(uri, RequestMethod.HEAD, null, optionsCallback);
        }

        public Task<UnityFetchResponse<T>> Options<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Request<T>(uri, RequestMethod.OPTIONS, null, optionsCallback);
        }

        public async Task<T> OptionsSimple<T>(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return (await Options<T>(uri, optionsCallback)).content;
        }

        public Task<UnityFetchResponse<object>> Options(string uri, Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return Options<object>(uri, optionsCallback);
        }

        public IEnumerator CoroutineRequest<T>(
            Task<UnityFetchResponse<T>> task,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null)
        {
            while (!task.IsCompleted)
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

        public IEnumerator CoroutineDelete<T>(
            string uri,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.DELETE, null, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineDelete(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineDelete(uri, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineHead(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.HEAD, null, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineOptions<T>(
            string uri,
            Action<T>? onSuccess = null,
            Action<UnityFetchResponse<T>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineRequest(uri, RequestMethod.OPTIONS, null, onSuccess, onError, optionsCallback);
        }

        public IEnumerator CoroutineOptions(
            string uri,
            Action<object>? onSuccess = null,
            Action<UnityFetchResponse<object>>? onError = null,
            Action<UnityFetchRequestOptions>? optionsCallback = null)
        {
            return CoroutineOptions(uri, onSuccess, onError, optionsCallback);
        }

        public UnityFetchClient SetAbortController(AbortController? abortController)
        {
            globalOptions.SetAbortController(abortController);

            return this;
        }
    }
}
