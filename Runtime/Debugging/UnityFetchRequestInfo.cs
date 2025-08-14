using System;
using System.Collections.Generic;

namespace UnityFetch.Debugging
{
    [Serializable]
    public class UnityFetchRequestInfo
    {
        public string url;

        public RequestMethod method;

        public string remoteAddress;

        public string referrerPolicy;

        public string requestBody;

        public string responseBody;

        public long status = -1;

        public string size;

        public string type;

        public string time;

        public List<Header> requestHeaders = new();

        public List<Header> responseHeaders = new();
    }
}