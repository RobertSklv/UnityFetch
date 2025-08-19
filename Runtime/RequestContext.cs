using System;
using System.Collections.Generic;
using UnityEngine.Networking;
using UnityFetch.Debugging;
using UnityFetch.RequestProcessing;
using UnityFetch.RequestStrategies;

namespace UnityFetch
{
    public class RequestContext
    {
        public string Url { get; set; }

        public RequestMethod Method { get; set; }

        public UnityFetchRequestOptions Options { get; set; }

        internal RequestStrategy RequestStrategy { get; set; }

        internal RequestProcessor RequestProcessor { get; set; }

        public long ResponseCode { get; set; }

        public ulong DownloadedBytes { get; set; }

        public UnityWebRequest.Result Result { get; set; }

        public string? RawResponse { get; set; }

        public float TimeElapsedSeconds { get; set; }

        public DateTime Timestamp { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; }

        public Dictionary<string, string> ResponseHeaders { get; set; }

        public Exception? Exception { get; set; }

        public int Attempt { get; set; } = 1;

        public UnityFetchRequestInfo RequestInfo { get; set; }
    }
}