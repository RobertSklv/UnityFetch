using System;
using System.Collections.Generic;

namespace UnityFetch
{
    public abstract class UnityFetchResponse
    {
        public readonly long statusCode;

        public readonly string rawContent;

        public readonly Dictionary<string, string> requestHeaders = new();

        public readonly Dictionary<string, string> responseHeaders = new();

        public readonly RequestMethod method;

        public readonly TimeSpan duration;

        public readonly DateTime timestamp;

        public bool IsSuccess => statusCode >= 200 && statusCode < 300;

        public UnityFetchResponse(
            long statusCode,
            string rawContent,
            Dictionary<string, string> requestHeaders,
            Dictionary<string, string> responseHeaders,
            RequestMethod method,
            TimeSpan duration,
            DateTime timestamp)
        {
            this.statusCode = statusCode;
            this.rawContent = rawContent;
            this.requestHeaders = requestHeaders;
            this.responseHeaders = responseHeaders;
            this.duration = duration;
            this.timestamp = timestamp;
            this.method = method;
        }

        public UnityFetchResponse<T> ToGeneric<T>(T content)
        {
            return new UnityFetchResponse<T>(
                content,
                statusCode,
                rawContent,
                requestHeaders,
                responseHeaders,
                method,
                duration,
                timestamp);
        }
    }

    public class UnityFetchResponse<T> : UnityFetchResponse
    {
        public readonly T? content;
        private readonly Dictionary<string, string> requestHeaders;

        public UnityFetchResponse(
            T content,
            long statusCode,
            string rawContent,
            Dictionary<string, string> requestHeaders,
            Dictionary<string, string> responseHeaders,
            RequestMethod method,
            TimeSpan duration,
            DateTime timestamp)
            : base(statusCode, rawContent, requestHeaders, responseHeaders, method, duration, timestamp)
        {
            this.content = content;
            this.requestHeaders = requestHeaders;
        }
    }
}