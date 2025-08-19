using System;
using System.Collections.Generic;
using Unity.Properties;

namespace UnityFetch.Debugging
{
    [Serializable]
    public class UnityFetchRequestInfo
    {
        public string guid;

        public string url;

        public RequestMethod method;

        public string requestBody;

        public string responseBody;

        public long status = 0;

        public string statusLabel;

        public string size;

        public string type;

        public string time;

        public bool finished;

        public int attempt;

        public List<Header> requestHeaders = new();

        public List<Header> responseHeaders = new();

        [CreateProperty]
        public bool IsFailed => finished && !(status >= 200 && status < 300);
    }
}