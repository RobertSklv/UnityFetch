using System.Collections.Generic;

namespace UnityFetch
{
    public abstract class UnityFetchResponse
    {
        public readonly long statusCode;

        public readonly string rawContent;

        public readonly Dictionary<string, string> headers = new();

        public bool IsSuccess => statusCode >= 200 && statusCode < 300;

        public UnityFetchResponse(long statusCode, string rawContent, Dictionary<string, string> headers)
        {
            this.statusCode = statusCode;
            this.rawContent = rawContent;
            this.headers = headers;
        }

        public UnityFetchResponse<T> ToGeneric<T>(T content)
        {
            return new UnityFetchResponse<T>(content, statusCode, rawContent, headers);
        }
    }

    public class UnityFetchResponse<T> : UnityFetchResponse
    {
        public readonly T? content;

        public UnityFetchResponse(T content, long statusCode, string rawContent, Dictionary<string, string> headers)
            : base(statusCode, rawContent, headers)
        {
            this.content = content;
        }
    }
}